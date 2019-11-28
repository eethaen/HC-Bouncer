using TMPro;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] Game.Setting _gameSetting;
    [SerializeField] Level.Setting _levelSetting;
    [SerializeField] Segment.Setting _segmentSetting;
    [SerializeField] Platform.Setting _platformSetting;
    [SerializeField] Border.Setting _borderSetting;

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);

        Container.BindInterfacesAndSelfTo<Game>().AsSingle();
        Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Orb>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<Ball>().FromComponentInHierarchy().AsSingle();
        Container.Bind<World>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SpriteRenderer>().FromComponentInChildren(true);
        //Container.Bind<Trail>().FromComponentInHierarchy(true).AsSingle();
        Container.Bind<LineRenderer>().FromComponentInChildren(true);
        Container.Bind<EdgeCollider2D>().FromComponentInChildren(true);
        Container.Bind<Rigidbody2D>().FromComponentInChildren(true);
        Container.Bind<AsyncProcessor>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<Canvas>().FromComponentInChildren(true);
        Container.Bind<TextMeshProUGUI>().FromComponentInChildren(true);

        Container.BindExecutionOrder<Game>(-100);

        Container.BindInstance(_gameSetting).AsSingle();
        Container.BindInstance(_levelSetting).AsSingle();
        Container.BindInstance(_segmentSetting).AsSingle();
        Container.BindInstance(_platformSetting).AsSingle();
        Container.BindInstance(_borderSetting).AsSingle();

        Container.BindFactory<int, Level, Level.Factory>().FromFactory<LevelFactory>();
        Container.BindFactory<int, Level, Segment, Segment.Factory>().FromFactory<SegmentFactory>();
        Container.BindFactory<float, bool, Transform, int, Border, Border.Factory>().FromFactory<BorderFactory>();
        Container.BindFactory<float, float, float, Transform, int, Platform, Platform.Factory>().FromFactory<PlatformFactory>();

        //Container.Bind<State>().FromComponentsInChildren(false, null, true);

        //Container.Bind<Enemy>().FromComponentsInHierarchy(null, true).AsSingle();

        //Container.BindMemoryPool<Projectile, Projectile.Pool>()
        //         .WithInitialSize(100)
        //         .FromComponentInNewPrefab(prefab/* Container.Resolve<Player>().ProjectilePrefab */)
        //         .UnderTransformGroup("Projectiles");

        Container.DeclareSignal<BallHitOrb>();
        Container.DeclareSignal<BallHitBorder>();
        Container.DeclareSignal<BallHitCore>();
        Container.DeclareSignal<LevelLoaded>();
        Container.DeclareSignal<LevelPassed>();
    }
}