using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.Control.Replays;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using Xclna.Xna.Animation;

namespace UnclaimedWorld.Test;

[TestClass]
// Some playback tests replace the global client; keep them isolated from other tests.
[DoNotParallelize]
public sealed class AdditiveAnimationTests
{
    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private sealed class FixedController(AnimationInfo info, Matrix transform) : AnimationController(info)
    {
        public override Matrix GetCurrentBoneTransform(BonePose pose) => transform;
    }

    // Synthetic clips provide deterministic bone transforms without loading model assets.
    private static AnimationController Controller(string name, Matrix transform, string bone = "bone", long elapsed = 500)
    {
        var frame = new BoneKeyFrame(Quaternion.Identity, Vector3.Zero, Vector3.One, 10_000_000);
        var channel = (BoneKeyFrameCollection)Activator.CreateInstance(typeof(BoneKeyFrameCollection), BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { bone, new List<BoneKeyFrame> { frame } }, null)!;
        var channels = (AnimationChannelCollection)Activator.CreateInstance(typeof(AnimationChannelCollection), BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { new List<BoneKeyFrameCollection> { channel } }, null)!;
        var info = (AnimationInfo)Activator.CreateInstance(typeof(AnimationInfo), BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { name, channels }, null)!;
        return new FixedController(info, transform) { ElapsedTime = elapsed, SpeedFactor = 0 };
    }

    private static BonePose Bone(Matrix bind)
    {
        var bone = (BonePose)RuntimeHelpers.GetUninitializedObject(typeof(BonePose));
        SetField(bone, "name", "bone");
        SetField(bone, "additionalAnimationTracks", new List<AnimationTrack>());
        SetField(bone, "additionalAnimAffectsBone", new List<bool>());
        SetField(bone, "additionalBlendAnimAffectsBone", new List<bool>());
        bone.DefaultTransform = bind;
        return bone;
    }

    private static ModelAnimator Animator(BonePose[] bones, params AnimationController[] controllers)
    {
        var animator = (ModelAnimator)RuntimeHelpers.GetUninitializedObject(typeof(ModelAnimator));
        var collection = (BonePoseCollection)Activator.CreateInstance(typeof(BonePoseCollection), BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { bones }, null)!;
        SetField(animator, "bonePoses", collection);
        animator.AnimationControllers = controllers.ToDictionary(c => c.AnimationInfo.Name);
        return animator;
    }

    private static void Start(AnimationTrack track, AnimationController controller, BlendMode mode) =>
        track.StartAnimation(controller, Playback.Forwards, StartingPoint.Current, mode, out _, looping: Looping.No);

    private static GameTime Seconds(double seconds) => new(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));

    private static AnimConditions Condition(AnimAction? action, params AnimModifier[] modifiers)
    {
        var result = new AnimConditions { Action = action, Modifiers = new BitMask64(typeof(AnimModifier)) };
        foreach (var modifier in modifiers)
            result.Modifiers.Set(modifier);
        return result;
    }

    // Two deltas compose over the main pose; weights and unrelated bone channels are respected.
    [TestMethod]
    public void AdditiveTracksComposeRelativeToBindPoseAndRespectWeights()
    {
        var bone = Bone(Matrix.CreateTranslation(1, 0, 0));
        var main = Controller("main", Matrix.CreateTranslation(5, 0, 0));
        var first = Controller("first", Matrix.CreateTranslation(3, 0, 0));
        var second = Controller("second", Matrix.CreateTranslation(4, 0, 0));
        var unrelated = Controller("unrelated", Matrix.CreateTranslation(100, 0, 0), "other");
        var animator = Animator(new[] { bone }, main, first, second, unrelated);
        var mainTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Main);
        var firstTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 0 };
        var secondTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 1, WeightFactor = 0.5f };
        var unrelatedTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 2 };
        Start(mainTrack, main, BlendMode.Normal);
        Start(firstTrack, first, BlendMode.Additive);
        Start(secondTrack, second, BlendMode.Additive);
        Start(unrelatedTrack, unrelated, BlendMode.Additive);
        Assert.AreEqual(8.5f, bone.GetCurrentTransform().Translation.X, 0.0001f);
        secondTrack.StopAnimation();
        Assert.AreEqual(7f, bone.GetCurrentTransform().Translation.X, 0.0001f);
    }

    // A normal override remains the base pose while a fading additive contribution disappears.
    [TestMethod]
    public void NormalOverrideKeepsPriorityAndAdditiveFadeReturnsToIt()
    {
        var bone = Bone(Matrix.Identity);
        var main = Controller("main", Matrix.CreateTranslation(1, 0, 0));
        var normal = Controller("normal", Matrix.CreateTranslation(3, 0, 0));
        var additive = Controller("additive", Matrix.CreateTranslation(4, 0, 0));
        var animator = Animator(new[] { bone }, main, normal, additive);
        var mainTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Main);
        var normalTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 0 };
        var additiveTrack = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 1 };
        Start(mainTrack, main, BlendMode.Normal);
        Start(normalTrack, normal, BlendMode.Normal);
        Start(additiveTrack, additive, BlendMode.Additive);
        Assert.AreEqual(7f, bone.GetCurrentTransform().Translation.X, 0.0001f);
        additiveTrack.StartAnimation(null!, Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Additive, out _);
        additiveTrack.Update(Seconds(0.25));
        Assert.AreEqual(5f, bone.GetCurrentTransform().Translation.X, 0.0001f);
        additiveTrack.Update(Seconds(0.25));
        Assert.AreEqual(3f, bone.GetCurrentTransform().Translation.X, 0.0001f);
    }

    // Switching to a clip without this bone fades the old delta back to the default pose.
    [TestMethod]
    public void AdditiveTransitionFadesBonesMissingFromTheNextClip()
    {
        var bone = Bone(Matrix.CreateTranslation(1, 0, 0));
        var first = Controller("first", Matrix.CreateTranslation(5, 0, 0));
        var next = Controller("next", Matrix.CreateTranslation(9, 0, 0), "other");
        var animator = Animator(new[] { bone }, first, next);
        _ = new AnimationTrack(animator, AnimationTrack.TrackType.Main);
        var track = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 0 };
        Start(track, first, BlendMode.Additive);
        Start(track, next, BlendMode.Additive);
        track.Update(Seconds(0.25));
        Assert.AreEqual(3f, bone.GetCurrentTransform().Translation.X, 0.0001f);
        track.Update(Seconds(0.25));
        Assert.AreEqual(1f, bone.GetCurrentTransform().Translation.X, 0.0001f);
    }

    // Rotation and translation in the bind pose must both be accounted for in the delta.
    [TestMethod]
    public void AdditiveTransformUsesNonIdentityBindPose()
    {
        Matrix bind = Matrix.CreateRotationZ(0.6f) * Matrix.CreateTranslation(2, 3, 0);
        Matrix animated = Matrix.CreateRotationZ(1.1f) * Matrix.CreateTranslation(4, 5, 0);
        var bone = Bone(bind);
        var controller = Controller("rotation", animated);
        var animator = Animator(new[] { bone }, controller);
        _ = new AnimationTrack(animator, AnimationTrack.TrackType.Main);
        var track = new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = 0 };
        Start(track, controller, BlendMode.Additive);
        Matrix actual = bone.GetCurrentTransform();
        Assert.AreEqual(animated.M11, actual.M11, 0.0001f);
        Assert.AreEqual(animated.M12, actual.M12, 0.0001f);
        Assert.AreEqual(animated.Translation.X, actual.Translation.X, 0.0001f);
        Assert.AreEqual(animated.Translation.Y, actual.Translation.Y, 0.0001f);
    }

    // Opposing propeller requests and both doors play together; interrupted doors retain their position.
    [TestMethod]
    public void ParallelIntentStartsIndependentTracksAndHoldsInterruptedDoor()
    {
        var controllers = Enumerable.Range(0, 4).Select(i => Controller("clip" + i, Matrix.Identity)).ToArray();
        var animator = Animator(Array.Empty<BonePose>(), controllers);
        var conditions = new[]
        {
            Condition(null, AnimModifier.Pre, AnimModifier.Right, AnimModifier.Slow),
            Condition(null, AnimModifier.Pre, AnimModifier.Left, AnimModifier.Slow),
            Condition(AnimAction.Containing, AnimModifier.Crew),
            Condition(AnimAction.Containing, AnimModifier.Passenger)
        };
        var entries = controllers.Select((c, i) => new AnimConditionInfo
        {
            ConditionSet = conditions[i],
            SoundAndAnimationSet = new RandomSoundAndAnimationSet { BaseAnimations = new[] { c.AnimationInfo.Name } },
            Playback = i < 2 ? Playback.Forwards : Playback.Backwards,
            StartingPoint = i < 2 ? StartingPoint.FromBeginning : StartingPoint.Current,
            BlendMode = BlendMode.Additive,
            Looping = Looping.No,
            HoldOnExit = i >= 2
        }).ToArray();
        var type = new EntityType();
        typeof(EntityType).GetProperty("RenderableTypeMode")!.SetValue(type, new RenderableType { RenderAsModelType = new RenderAsModelType { ParallelAnimations = entries } });
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty("EntityType")!.SetValue(entity, type);
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        renderable.AnimConditions = new AnimConditions();
        var model = new RenderAsModel(entity, renderable) { AnimatedModel = new StiffAnimatedModel { ModelAnimator = animator } };
        var tracks = new List<AnimationTrack> { new(animator, AnimationTrack.TrackType.Additional) { Index = 0 }, new(animator, AnimationTrack.TrackType.Additional) { Index = 1 } };
        SetField(model, "additionalAnimationTracks", tracks);
        var previousClient = The.Client;
        try
        {
            The.Client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
            The.Client.ClientRandomGenerator = new RandomGenerator(7, RandomGenerator.GeneratorType.Client);
            foreach (var condition in conditions)
                renderable.AddParallelAnimationCondition(condition);
            model.SetAnimFlagsDirty();
            model.UpdateAnimationConditionState();
            Assert.AreEqual(4, tracks.Count(t => t.RunsAnimation()));
            Assert.AreEqual(1d, controllers[0].SpeedFactor);
            Assert.AreEqual(-1d, controllers[2].SpeedFactor);
            model.SetAnimFlagsDirty();
            model.UpdateAnimationConditionState();
            Assert.AreEqual(4, tracks.Count(t => t.RunsAnimation()));
            foreach (var condition in conditions)
                renderable.RemoveParallelAnimationCondition(condition);
            model.SetAnimFlagsDirty();
            model.UpdateAnimationConditionState();
            Assert.AreEqual(0d, controllers[2].SpeedFactor);
            Assert.AreEqual(500L, controllers[2].ElapsedTime);
            Assert.AreEqual(2, tracks.Count(t => t.RunsAnimation()));
            foreach (var condition in conditions)
                renderable.AddParallelAnimationCondition(condition);
            model.SetAnimFlagsDirty();
            model.UpdateAnimationConditionState();
            Assert.AreEqual(4, tracks.Count(t => t.RunsAnimation()));
            Assert.AreEqual(500L, controllers[2].ElapsedTime);
            Assert.AreEqual(-1d, controllers[2].SpeedFactor);
        }
        finally
        {
            The.Client = previousClient;
        }
    }

    // Extra additive tracks must not cause normal landing clips to start repeatedly on every free track.
    [TestMethod]
    public void NormalAnimationsUseOneTrackAfterAdditiveTracksAreAllocated()
    {
        var first = Controller("right_stop", Matrix.Identity);
        var second = Controller("left_stop", Matrix.Identity);
        var animator = Animator(Array.Empty<BonePose>(), first, second);
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        var model = new RenderAsModel(null!, renderable) { AnimatedModel = new StiffAnimatedModel { ModelAnimator = animator } };
        var tracks = Enumerable.Range(0, 5).Select(i => new AnimationTrack(animator, AnimationTrack.TrackType.Additional) { Index = i }).ToList();
        SetField(model, "additionalAnimationTracks", tracks);
        var previousClient = The.Client;
        try
        {
            The.Client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
            The.Client.ClientRandomGenerator = new RandomGenerator(7, RandomGenerator.GeneratorType.Client);
            model.StartAdditionalAnimation("right_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal);
            model.StartAdditionalAnimation("left_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal);
            Assert.AreEqual(1, tracks.Count(track => track.currentController == first));
            Assert.AreEqual(1, tracks.Count(track => track.currentController == second));
            foreach (var track in tracks)
                track.Update(Seconds(0.1));
            Assert.AreEqual(1_000_000L, first.ElapsedTime);
            Assert.AreEqual(1_000_000L, second.ElapsedTime);
        }
        finally
        {
            The.Client = previousClient;
        }
    }

    // The fixed simulation countdown and interruption both clear the wait's parallel requests.
    [TestMethod]
    public void TakeoffWaitClearsIntentAfterThreeSecondsOrInterruption()
    {
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        renderable.AnimConditions = new AnimConditions();
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        entity.Renderable = renderable;
        var conditions = new List<AnimConditions> { Condition(null, AnimModifier.Right), Condition(null, AnimModifier.Left) };
        var wait = new GoalWait { entity = entity, MaxTimeToWait = 3d, ParallelConditionsToSet = conditions };
        wait.EnterIfNew();
        Assert.AreEqual(2, renderable.ParallelAnimationConditions.Count);
        wait.Process(Seconds(2));
        Assert.AreEqual(2, renderable.ParallelAnimationConditions.Count);
        wait.Process(Seconds(1));
        Assert.AreEqual(0, renderable.ParallelAnimationConditions.Count);
        var interrupted = new GoalWait { entity = entity, MaxTimeToWait = 3d, ParallelConditionsToSet = conditions };
        interrupted.EnterIfNew();
        interrupted.Process(Seconds(1));
        interrupted.Terminate();
        Assert.AreEqual(0, renderable.ParallelAnimationConditions.Count);
    }

    // Matching handles actionless sets, required subsets, forbidden bits, and specificity.
    [TestMethod]
    public void ParallelMatchingRespectsActionsModifiersAndForbiddens()
    {
        var broad = new AnimConditionInfo { ConditionSet = Condition(null, AnimModifier.Pre) };
        var right = new AnimConditionInfo { ConditionSet = Condition(null, AnimModifier.Pre, AnimModifier.Right), Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Slow) };
        var crew = new AnimConditionInfo { ConditionSet = Condition(AnimAction.Containing, AnimModifier.Crew) };
        var type = new RenderAsModelType { ParallelAnimations = new[] { broad, right, crew } };
        Assert.AreSame(right, type.FindBestParallelAnimInfo(Condition(null, AnimModifier.Pre, AnimModifier.Right)));
        Assert.AreSame(broad, type.FindBestParallelAnimInfo(Condition(null, AnimModifier.Pre, AnimModifier.Right, AnimModifier.Slow)));
        Assert.AreSame(crew, type.FindBestParallelAnimInfo(Condition(AnimAction.Containing, AnimModifier.Crew)));
        Assert.IsNull(type.FindBestParallelAnimInfo(Condition(AnimAction.Containing, AnimModifier.Passenger)));
        Assert.IsNull(type.FindBestParallelAnimInfo(Condition(null, AnimModifier.Crew)));
    }

    // Leave deliberately retains the request when the goal terminates.
    [TestMethod]
    public void WaitCanLeaveParallelConditionsOnExit()
    {
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        renderable.AnimConditions = new AnimConditions();
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        entity.Renderable = renderable;
        var wait = new GoalWait { entity = entity, MaxTimeToWait = 3d, ParallelConditionsToSet = new List<AnimConditions> { Condition(null, AnimModifier.Left) } };
        SetField(wait, "onExitFlagActionValue", GoalWait.OnExitFlagAction.Leave);
        wait.EnterIfNew();
        wait.Terminate();
        Assert.AreEqual(1, renderable.ParallelAnimationConditions.Count);
    }

    // Removing one identical request must not remove another or alter the main animation flags.
    [TestMethod]
    public void DuplicateParallelConditionsAreIndependentOfMainConditions()
    {
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        renderable.AnimConditions = Condition(AnimAction.Idle, AnimModifier.Crew);
        var request = Condition(null, AnimModifier.Right);
        renderable.AddParallelAnimationCondition(request);
        renderable.AddParallelAnimationCondition(request);
        renderable.RemoveParallelAnimationCondition(request);
        Assert.AreEqual(1, renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(AnimAction.Idle, renderable.AnimConditions.Action);
        Assert.IsTrue(renderable.AnimConditions.Modifiers.Test(AnimModifier.Crew));
        Assert.IsFalse(renderable.AnimConditions.Modifiers.Test(AnimModifier.Right));
        renderable.RemoveParallelAnimationCondition(request);
        Assert.AreEqual(0, renderable.ParallelAnimationConditions.Count);
    }

    // A snapshot round trip preserves both independent requests and the ordinary main conditions.
    [TestMethod]
    public void ParallelConditionsSurviveRenderableSnapshot()
    {
        var source = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        source.AnimConditions = Condition(AnimAction.Idle, AnimModifier.Crew);
        source.AddParallelAnimationCondition(Condition(null, AnimModifier.Left));
        source.AddParallelAnimationCondition(Condition(AnimAction.Containing, AnimModifier.Passenger));
        using var stream = new MemoryStream();
        var snapshotter = new Snapshotter { mode = Snapshotter.Mode.Save };
        SetField(snapshotter, "m_writer", new BinaryWriter(stream));
        snapshotter.DoISnapshot(new Renderable.SnapshotRenderable(source), verifyField: false);
        stream.Position = 0;
        snapshotter.mode = Snapshotter.Mode.Load;
        SetField(snapshotter, "m_reader", new BinaryReader(stream));
        var wasSnapshotting = Snapshotter.IsSnapshotting;
        Renderable.SnapshotRenderable snapshot;
        try
        {
            Snapshotter.IsSnapshotting = true;
            snapshot = (Renderable.SnapshotRenderable)snapshotter.DoISnapshot<Renderable.SnapshotRenderable>(null!, verifyField: false);
        }
        finally
        {
            Snapshotter.IsSnapshotting = wasSnapshotting;
        }
        var restored = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        snapshot.LoadRenderableWithSnapshotData(restored);
        Assert.AreEqual(stream.Length, stream.Position);
        Assert.AreEqual(2, restored.ParallelAnimationConditions.Count);
        Assert.IsTrue(restored.ParallelAnimationConditions[0].Modifiers.Test(AnimModifier.Left));
        Assert.AreEqual(AnimAction.Containing, restored.ParallelAnimationConditions[1].Action);
        Assert.IsTrue(restored.ParallelAnimationConditions[1].Modifiers.Test(AnimModifier.Passenger));
        Assert.AreEqual(AnimAction.Idle, restored.AnimConditions.Action);
    }
}
