using System.Reflection;
using System.Runtime.CompilerServices;
using GameStateManagement;
using UWGame;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

/// <summary>
/// SAVE is a save followed by a load of the same file into the running game
/// (SaveLoadMessageBox.LoadGameDirectlyInGame), and Snapshotter.Load starts that load from the game
/// it replaces, emptied by Sim.FreeMemoryBeforeLoad. A game with a vehicle - a trader's barge, the
/// skimmers and harpies of Muckroot Research Station - died there on a duplicate slot ID, and the
/// failure was swallowed (Kastuk, 2026-11-17). The whole round trip on a real save is
/// dataexport --save-reload; these are the two steps that went wrong.
/// Not run in parallel: the lookups and Sim.LoadException are static.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class InGameReloadTests
{
    private static VehicleContainer NewVehicleWithSeats()
    {
        var type = new VehicleContainerType
        {
            PassengerOrCargoSlotTypes = new[]
            {
                new PassengerOrCargoSlotType { PassengerSlotType = new PassengerSlotType { IsDriversSeat = true } },
                new PassengerOrCargoSlotType { PassengerSlotType = new PassengerSlotType() },
            }
        };
        type.Initialize();
        type.ItemStorageType = new ItemStorageType();
        var parent = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        parent.Components = new();
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(parent, (EntityID)100);
        typeof(Entity).GetProperty("EntityType")!.SetValue(parent, new EntityType { ContainerType = type });
        var vehicle = new VehicleContainer(parent);
        parent.Contains = vehicle;
        return vehicle;
    }

    private static int RegisteredSlots()
    {
        int count = 0;
        LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.IterateMembers(_ => count++);
        return count;
    }

    [TestMethod]
    public void VehicleSlotsReadBackIntoTheGameTheyReplace()
    {
        // The running game: a Sim with its collections, and a vehicle whose seats registered themselves.
        var sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(sim, null);
        sim.FreeMemoryBeforeLoad(); // no seats left over from another test
        The.Sim = sim;
        List<PassengerOrCargoSlot> seats = NewVehicleWithSeats().Slots;
        Assert.AreEqual(2, RegisteredSlots());

        // SAVE writes the seats...
        var stream = new MemoryStream();
        var sn = new Snapshotter();
        FieldInfo writer = typeof(Snapshotter).GetField("m_writer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo reader = typeof(Snapshotter).GetField("m_reader", BindingFlags.Instance | BindingFlags.NonPublic)!;
        sn.mode = Snapshotter.Mode.Save;
        writer.SetValue(sn, new BinaryWriter(stream));
        foreach (PassengerOrCargoSlot seat in seats)
        {
            sn.DoISnapshot(seat, verifyField: false);
        }

        // ...and reads them straight back into the same session, as Snapshotter.Load does.
        List<PassengerOrCargoSlotID> ids = seats.Select(seat => seat.ID).ToList();
        sim.FreeMemoryBeforeLoad();
        stream.Position = 0;
        sn.mode = Snapshotter.Mode.Load;
        writer.SetValue(sn, null);
        reader.SetValue(sn, new BinaryReader(stream));
        var read = new List<PassengerOrCargoSlot>();
        for (int i = 0; i < seats.Count; i++)
        {
            read.Add((PassengerOrCargoSlot)sn.DoISnapshot<PassengerOrCargoSlot>(null!, verifyField: false));
        }

        for (int i = 0; i < seats.Count; i++)
        {
            Assert.AreEqual(ids[i], read[i].ID);
            Assert.AreSame(read[i], LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(read[i].ID),
                "the seat read back is the one the loaded game finds by its ID");
        }
        Assert.AreEqual(2, RegisteredSlots(), "no seat of the replaced game is left registered");
        sim.FreeMemoryBeforeLoad();
        Assert.AreEqual(0, RegisteredSlots());
    }

    [TestMethod]
    public void SeatsAreClearedWithTheGameButNotSavedAsACollection()
    {
        var sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(sim, null);
        var all = (Collections)typeof(Sim).GetField("CollectionOfCollections", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        ILookUpCollectible seats = all[typeof(PassengerOrCargoSlot)];
        Assert.IsTrue(seats.ClearedWithGame, "a load empties the seats of the game it replaces");
        Assert.IsFalse(seats.SnapshotThis, "each vehicle saves its own seats; the save format is unchanged");
    }

    [TestMethod]
    public void AFailedInGameLoadIsRethrownOnTheMainThread()
    {
        MethodInfo check = typeof(Controller).GetMethod("ThrowFailedInGameLoad", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var controller = (Controller)RuntimeHelpers.GetUninitializedObject(typeof(Controller));
        // A game component has a finalizer, and this one was never constructed.
        GC.SuppressFinalize(controller);
        LoadingScreen previousLoadScreen = The.LoadScreen;
        The.LoadScreen = (LoadingScreen)RuntimeHelpers.GetUninitializedObject(typeof(LoadingScreen));
        The.LoadScreen.IsLoadFinished = true;
        try
        {
            Sim.LoadException = null;
            check.Invoke(controller, null);

            var failure = new UWException("This Save game failed to load.", new ArgumentException("the real cause"));
            Sim.LoadException = failure;
            var thrown = Assert.ThrowsExactly<TargetInvocationException>(() => check.Invoke(controller, null));
            Assert.AreSame(failure, thrown.InnerException, "the load thread's own exception, with its cause inside");

            // The LOAD menu's loading screen reports it itself (LoadingScreen.VerifyLoadSaveGame).
            The.LoadScreen.IsLoadFinished = false;
            check.Invoke(controller, null);
        }
        finally
        {
            Sim.LoadException = null;
            The.LoadScreen = previousLoadScreen;
        }
    }
}
