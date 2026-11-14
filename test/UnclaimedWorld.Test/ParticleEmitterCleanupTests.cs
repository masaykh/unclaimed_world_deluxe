using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.Client.Particles;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Systems;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class ParticleEmitterCleanupTests
{
    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private static (ParticleSystem System, List<ParticleEmitter> Updating) CreateSystem()
    {
        var system = (ParticleSystem)RuntimeHelpers.GetUninitializedObject(typeof(ParticleSystem));
        var updating = new List<ParticleEmitter>();
        system.ParticleSystemType = new ParticleSystemType { KeyName = "dust" };
        system.FreeParticles = new Queue<Particle>();
        SetField(system, "AllEmitters", new SleepyUpdater<ParticleEmitter>());
        SetField(system, "allEmittersForParticleUpdates", updating);
        return (system, updating);
    }

    private static ParticleEmitter CreateEmitter(Renderable parent, ParticleSystem system, List<ParticleEmitter> updating, Particle particle, ParticleEmitterEffect? effect = null)
    {
        var emitter = (ParticleEmitter)RuntimeHelpers.GetUninitializedObject(typeof(ParticleEmitter));
        emitter.System = system;
        SetField(emitter, "activeParticles", new List<Particle> { particle });
        if (effect != null)
        {
            SetField(emitter, "AnimationEffect", effect);
        }
        parent.Add2WayParticleEmitterReference(emitter);
        updating.Add(emitter);
        return emitter;
    }

    [TestMethod]
    public void ManagerRemovalReturnsParticlesAndUnregistersEmitter()
    {
        var parent = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        var (system, updating) = CreateSystem();
        var particle = new Particle();
        var emitter = CreateEmitter(parent, system, updating, particle);

        new ParticleManager().RemoveEmitter(emitter);

        Assert.AreEqual(0, parent.ParticleEmitters.Count);
        Assert.AreEqual(0, updating.Count);
        Assert.AreSame(particle, system.FreeParticles.Dequeue());
    }

    [TestMethod]
    public void AnimationCleanupPreservesUnrelatedEmittersWithSameSystemKey()
    {
        var parent = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        var (system, updating) = CreateSystem();
        var oldEffect = new ParticleEmitterEffect { ParticleSystemKey = "dust" };
        var otherEffect = new ParticleEmitterEffect { ParticleSystemKey = "dust" };
        var animationParticle = new Particle();
        var animationEmitter = CreateEmitter(parent, system, updating, animationParticle, oldEffect);
        var otherAnimation = CreateEmitter(parent, system, updating, new Particle(), otherEffect);
        var dust = CreateEmitter(parent, system, updating, new Particle());

        parent.AdoptParticleEffects(null!, new[] { oldEffect });

        Assert.AreEqual(2, parent.ParticleEmitters.Count);
        Assert.IsFalse(parent.ParticleEmitters.Contains(animationEmitter));
        Assert.IsTrue(parent.ParticleEmitters.Contains(otherAnimation));
        Assert.IsTrue(parent.ParticleEmitters.Contains(dust));
        Assert.IsFalse(updating.Contains(animationEmitter));
        Assert.IsTrue(updating.Contains(otherAnimation));
        Assert.IsTrue(updating.Contains(dust));
        Assert.AreSame(animationParticle, system.FreeParticles.Dequeue());
    }

    [TestMethod]
    public void RenderableDestructionReturnsAllActiveParticles()
    {
        var previousClient = The.Client;
        var client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
        SetField(client, "renderables", new SleepyUpdater<Renderable>());
        The.Client = client;
        try
        {
            var parent = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
            var (system, updating) = CreateSystem();
            var firstParticle = new Particle();
            var secondParticle = new Particle();
            CreateEmitter(parent, system, updating, firstParticle);
            CreateEmitter(parent, system, updating, secondParticle);

            parent.Destroy();

            Assert.AreEqual(0, parent.ParticleEmitters.Count);
            Assert.AreEqual(0, updating.Count);
            Assert.AreEqual(2, system.FreeParticles.Count);
            CollectionAssert.AreEquivalent(new[] { firstParticle, secondParticle }, system.FreeParticles.ToArray());
        }
        finally
        {
            The.Client = previousClient;
        }
    }
}
