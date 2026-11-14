using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.Control.Replays;
using Xclna.Xna.Animation;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class MechanicalAnimationTests
{
    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private static AnimationController Controller(string name = "gear", long elapsed = 0)
    {
        var info = (AnimationInfo)RuntimeHelpers.GetUninitializedObject(typeof(AnimationInfo));
        SetField(info, "animationName", name);
        SetField(info, "duration", 1000u);
        info.StartOffset = 100;
        return new InterpolationController(info) { ElapsedTime = elapsed, SpeedFactor = 0 };
    }

    private static ModelAnimator Animator(params AnimationController[] controllers)
    {
        var animator = (ModelAnimator)RuntimeHelpers.GetUninitializedObject(typeof(ModelAnimator));
        var bones = (BonePoseCollection)Activator.CreateInstance(typeof(BonePoseCollection), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { new List<BonePose>() }, null)!;
        SetField(animator, "bonePoses", bones);
        animator.AnimationControllers = controllers.ToDictionary(c => c.AnimationInfo.Name);
        return animator;
    }

    private static AnimationTrack Track(ModelAnimator animator) => new(animator, AnimationTrack.TrackType.Additional);

    private static void Start(AnimationTrack track, AnimationController controller, Playback playback = Playback.Forwards,
        StartingPoint start = StartingPoint.Current, BlendMode mode = BlendMode.Normal, Looping looping = Looping.No,
        long? offset = null, EventHandler? callback = null) =>
        track.StartAnimation(controller, playback, start, mode, out _, 1f, looping, offset, callback != null, callback!, new Random(7));

    private static GameTime Elapsed(long ticks) => new(TimeSpan.Zero, TimeSpan.FromTicks(ticks));

    [TestMethod]
    public void FirstCurrentStartKeepsFrameZero()
    {
        var controller = Controller();
        var track = Track(Animator(controller));
        Start(track, controller);
        Assert.AreSame(controller, track.currentController);
        Assert.AreEqual(0L, controller.ElapsedTime);
    }

    [DataTestMethod]
    [DataRow(BlendMode.Normal)]
    [DataRow(BlendMode.NoBlending)]
    public void CurrentReversesSameControllerWithoutResetting(BlendMode mode)
    {
        var controller = Controller(elapsed: 500);
        var track = Track(Animator(controller));
        Start(track, controller, mode: mode);
        Start(track, controller, Playback.Backwards, mode: mode);
        Assert.AreEqual(500L, controller.ElapsedTime);
        Assert.AreEqual(-1d, controller.SpeedFactor);
        track.Update(Elapsed(75));
        Assert.AreEqual(425L, controller.ElapsedTime);
        Start(track, controller, Playback.Forwards, mode: mode);
        Assert.AreEqual(425L, controller.ElapsedTime);
        track.Update(Elapsed(25));
        Assert.AreEqual(450L, controller.ElapsedTime);
    }

    [TestMethod]
    public void CurrentReverseCrossesZeroAndFiresCompletionOnce()
    {
        var controller = Controller(elapsed: 50);
        var track = Track(Animator(controller));
        int ended = 0;
        Start(track, controller, Playback.Backwards, callback: (_, _) => ended++);
        track.Update(Elapsed(75));
        track.Update(Elapsed(75));
        Assert.AreEqual(0L, controller.ElapsedTime);
        Assert.AreEqual(1, ended);
    }

    [TestMethod]
    public void CurrentReverseAtZeroStopsWithoutCompletion()
    {
        var controller = Controller();
        var track = Track(Animator(controller));
        Start(track, controller);
        Start(track, controller, Playback.Backwards);
        track.Update(Elapsed(50));
        Assert.AreEqual(0L, controller.ElapsedTime);
        Assert.AreEqual(0d, controller.SpeedFactor);
    }

    [TestMethod]
    public void CurrentForwardReachesEndAndFiresCompletionOnce()
    {
        var controller = Controller(elapsed: 950);
        var track = Track(Animator(controller));
        int ended = 0;
        Start(track, controller, callback: (_, _) => ended++);
        track.Update(Elapsed(100));
        track.Update(Elapsed(100));
        Assert.AreEqual(1000L, controller.ElapsedTime);
        Assert.AreEqual(1, ended);
        Start(track, controller, Playback.Backwards);
        Assert.AreEqual(1000L, controller.ElapsedTime);
        track.Update(Elapsed(50));
        Assert.AreEqual(950L, controller.ElapsedTime);
    }

    [TestMethod]
    public void CurrentPreservesTimeWhenChangingBlendTargets()
    {
        var old = Controller("old", 250);
        var target = Controller("target", 450);
        var other = Controller("other", 750);
        var animator = Animator(old, target, other);
        var track = Track(animator);
        track.currentController = old;
        Start(track, target);
        Assert.AreSame(target, track.controllerBeingBlendedTo);
        Assert.AreEqual(450L, target.ElapsedTime);
        Start(track, target, Playback.Backwards);
        Assert.AreEqual(450L, target.ElapsedTime);
        Assert.AreEqual(-1d, target.SpeedFactor);
        Start(track, other);
        Assert.AreSame(other, track.controllerBeingBlendedTo);
        Assert.AreEqual(750L, other.ElapsedTime);
        Start(track, old, Playback.Backwards);
        Assert.AreSame(old, track.currentController);
        Assert.IsNull(track.controllerBeingBlendedTo);
        Assert.AreEqual(250L, old.ElapsedTime);
    }

    [TestMethod]
    public void CurrentCancelsBlendToNullWithoutResettingPosition()
    {
        var controller = Controller(elapsed: 500);
        var track = Track(Animator(controller));
        Start(track, controller);
        track.StartAnimation(null!, Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal, out _);
        Start(track, controller, Playback.Backwards);
        Assert.AreSame(controller, track.currentController);
        Assert.AreEqual(500L, controller.ElapsedTime);
        Assert.AreEqual(-1d, controller.SpeedFactor);
    }

    [TestMethod]
    public void OtherStartingModesRetainTheirPositions()
    {
        var controller = Controller(elapsed: 500);
        var track = Track(Animator(controller));
        Start(track, controller, start: StartingPoint.FromBeginning);
        Assert.AreEqual(100L, controller.ElapsedTime);
        Start(track, controller, start: StartingPoint.Specified, offset: 75);
        Assert.AreEqual(175L, controller.ElapsedTime);
        Start(track, controller, start: StartingPoint.Random);
        Assert.IsTrue(controller.ElapsedTime >= 100 && controller.ElapsedTime < 1000);
        Start(track, controller, playback: Playback.Manual, start: StartingPoint.Current, offset: 25);
        Assert.AreEqual(125L, controller.ElapsedTime);
        Assert.AreEqual(0d, controller.SpeedFactor);
        Start(track, controller, playback: Playback.Backwards, start: StartingPoint.FromBeginning);
        Assert.AreEqual(1000L, controller.ElapsedTime);
    }

    [TestMethod]
    public void CurrentLoopingForwardRetainsPosition()
    {
        var controller = Controller(elapsed: 500);
        var track = Track(Animator(controller));
        Start(track, controller, looping: Looping.Yes);
        Assert.AreEqual(500L, controller.ElapsedTime);
        Assert.IsTrue(controller.IsLooping);
        track.Update(Elapsed(50));
        Assert.AreEqual(550L, controller.ElapsedTime);
    }

    [TestMethod]
    public void AdditionalCurrentUsesOneTrackAndReusesItForReversal()
    {
        var controller = Controller();
        var other = Controller("door");
        var animator = Animator(controller, other);
        var first = Track(animator);
        var second = Track(animator);
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        var model = new RenderAsModel(null!, renderable)
        {
            AnimatedModel = new StiffAnimatedModel { ModelAnimator = animator }
        };
        SetField(model, "additionalAnimationTracks", new List<AnimationTrack> { first, second });
        var previousClient = The.Client;
        try
        {
            The.Client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
            The.Client.ClientRandomGenerator = new RandomGenerator(7, RandomGenerator.GeneratorType.Client);
            model.StartAdditionalAnimation("gear", Playback.Forwards, StartingPoint.Current, BlendMode.Normal);
            Assert.AreSame(controller, first.currentController);
            Assert.IsNull(second.currentController);
            first.Update(Elapsed(500));
            model.StartAdditionalAnimation("gear", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);
            Assert.AreSame(controller, first.currentController);
            Assert.IsNull(second.currentController);
            Assert.AreEqual(500L, controller.ElapsedTime);
            Assert.AreEqual(-1d, controller.SpeedFactor);
            model.StartAdditionalAnimation("door", Playback.Forwards, StartingPoint.Current, BlendMode.Normal);
            Assert.AreSame(other, second.currentController);

            second.currentController = other;
            second.controllerBeingBlendedTo = controller;
            first.currentController = null;
            model.StartAdditionalAnimation("gear", Playback.Forwards, StartingPoint.Current, BlendMode.Normal);
            Assert.IsNull(first.currentController);
            Assert.AreSame(controller, second.controllerBeingBlendedTo);
            Assert.AreEqual(500L, controller.ElapsedTime);
            Assert.AreEqual(1d, controller.SpeedFactor);
        }
        finally
        {
            The.Client = previousClient;
        }
    }

    [DataTestMethod]
    [DataRow(Playback.Forwards, StartingPoint.FromBeginning)]
    [DataRow(Playback.Manual, StartingPoint.Current)]
    public void OtherAdditionalModesKeepExistingFreeTrackRouting(Playback playback, StartingPoint startingPoint)
    {
        var controller = Controller(elapsed: 500);
        var animator = Animator(controller);
        var first = Track(animator);
        var second = Track(animator);
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        var model = new RenderAsModel(null!, renderable)
        {
            AnimatedModel = new StiffAnimatedModel { ModelAnimator = animator }
        };
        SetField(model, "additionalAnimationTracks", new List<AnimationTrack> { first, second });
        var previousClient = The.Client;
        try
        {
            The.Client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
            The.Client.ClientRandomGenerator = new RandomGenerator(7, RandomGenerator.GeneratorType.Client);
            model.StartAdditionalAnimation("gear", playback, startingPoint, BlendMode.Normal);
            Assert.AreSame(controller, first.currentController);
            Assert.AreSame(controller, second.currentController);
            Assert.AreEqual(100L, controller.ElapsedTime);
        }
        finally
        {
            The.Client = previousClient;
        }
    }
}
