using TMPro;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] MainSetting _mainSetting;
    [SerializeField] ThematicSetting _thematicSetting;
    [SerializeField] VFXSetting _vfxSetting;

    [SerializeField] LevelSetting _levelSetting;
    [SerializeField] PlatformSetting _platformSetting;
    [SerializeField] BorderSetting _borderSetting;
    [SerializeField] ObstacleSetting _obstacleSetting;

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);

        Container.BindInterfacesAndSelfTo<Game>().AsSingle();
        Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Background>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Ball>().FromComponentInHierarchy().AsSingle();
        Container.Bind<World>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SpriteRenderer>().FromComponentInChildren(true);
        Container.Bind<LineRenderer>().FromComponentInChildren(true);
        Container.Bind<EdgeCollider2D>().FromComponentInChildren(true);
        Container.Bind<Rigidbody2D>().FromComponentInChildren(true);
        Container.Bind<AsyncProcessor>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<Canvas>().FromComponentInChildren(true);
        Container.Bind<TextMeshProUGUI>().FromComponentInChildren(true);

        Container.BindExecutionOrder<Game>(-100);

        Container.BindInstance(_mainSetting).AsSingle();
        Container.BindInstance(_thematicSetting).AsSingle();
        Container.BindInstance(_vfxSetting).AsSingle();
        Container.BindInstance(_levelSetting).AsSingle();
        Container.BindInstance(_platformSetting).AsSingle();
        Container.BindInstance(_borderSetting).AsSingle();
        Container.BindInstance(_obstacleSetting).AsSingle();

        Container.BindFactory<int, Level, Level.Factory>().FromFactory<LevelFactory>();
        Container.BindFactory<float, bool, Level, Border, Border.Factory>().FromFactory<BorderFactory>();
        Container.BindFactory<Platform.State, Level, Platform, Platform.Factory>().FromFactory<PlatformFactory>();
        Container.BindFactory<Obstacle.State, Level, Obstacle, Obstacle.Factory>().FromFactory<ObstacleFactory>();

        //Container.Bind<State>().FromComponentsInChildren(false, null, true);

        //Container.Bind<Enemy>().FromComponentsInHierarchy(null, true).AsSingle();

        //Container.BindMemoryPool<Projectile, Projectile.Pool>()
        //         .WithInitialSize(100)
        //         .FromComponentInNewPrefab(prefab/* Container.Resolve<Player>().ProjectilePrefab */)
        //         .UnderTransformGroup("Projectiles");

        Container.DeclareSignal<BallHitBorder>();
        Container.DeclareSignal<BallHitObstacle>();
        Container.DeclareSignal<BallHitCore>();
        Container.DeclareSignal<LevelLoaded>();
    }
}