using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions;


/// <summary>
/// only one target can be active at a time
/// </summary>
public class ProductionOrder : ISnapshot
{
    /// <summary>
    /// for direct ordering from the inventory panel only - not gathering yet!
    /// Currently: counts jobs - not amount of products.
    ///      
    ///     
    /// 
    /// this number is reduced when the job starts, not when it completes. Not the best design...
    /// 
    /// 
    /// LATER: if this is set higher than the amount to keep in store, the surplus can be moved/traded to other expeditions
    /// It doesn't make sense to set this value lower than KeepInStore (unless we want to disable production of the item?)
    /// </summary>
    public int? ProductionJobsToComplete;


    /// <summary>
    /// if filled, the job manager will create jobs to to fulfill the order at all times.
    /// 
    /// NEW: -1 means unlimited order
    /// </summary>
    public int? AmountToKeepInStore;





    #region ISnapshot

    public ISnapshot DoSnapshot(Snapshotter sn)
    {
        ProductionJobsToComplete = sn.DoInt32Nullable(ProductionJobsToComplete);
        AmountToKeepInStore = sn.DoInt32Nullable(AmountToKeepInStore);

        return this;
    }


    public void LoadPostProcess(Snapshotter sn)
    {
        sn.RegisterLoadPostProcessCall(this);

    }


    /// <summary>
    /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
    /// </summary>
    Snapshotter.Version version = Snapshotter.Version.Original;
    public Snapshotter.Version DoVersion(Snapshotter sn)
    {
        version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
        return version;
    }

    public bool IsSnapshotted { get; set; }

    #endregion
}
