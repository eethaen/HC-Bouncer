using DG.Tweening;
using UnityEngine;
using Zenject;

public class Border : MonoBehaviour
{
    public Transform Transform { get; private set; }

    private SpriteRenderer _renderer;
    private Game _game;
    private BoxCollider2D _collider;
    private Vector2 _colliderSize;
    private float _colliderMinWidth;

    [Inject]
    public void Construct(Game game, SpriteRenderer renderer, BoxCollider2D collider)
    {
        _game = game;
        _renderer = renderer;
        _collider = collider;
        Transform = transform;

        _colliderSize = _collider.size;
        _colliderMinWidth = _colliderSize.x;
    }

    public void SetColor(Color color)
    {
        _renderer.DOColor(color, 0.3f);
    }

    private void Update()
    {
        _colliderSize.x = Mathf.Lerp(_colliderMinWidth, _colliderMinWidth * 50.0f, Mathf.Abs(_game.rotationalSpeed / 400f));
        _collider.size = _colliderSize;
    }

    public class Factory : PlaceholderFactory<float, Border>
    {
    }
}

public class BorderFactory : IFactory<float, Border>
{
    private DiContainer _container;
    private readonly MainSetting _mainSetting;
    private readonly BorderSetting _bordersetting;
    private readonly World _world;
    private Border _instance;

    public BorderFactory(DiContainer container, MainSetting mainSetting, BorderSetting borderSettings, World world)
    {
        _container = container;
        _mainSetting = mainSetting;
        _bordersetting = borderSettings;
        _world = world;
    }

    public Border Create(float theta)
    {
        _instance = _container.InstantiatePrefabForComponent<Border>(_bordersetting.prefab, _world.Transform);
        _instance.Transform.localPosition = new Vector2(_mainSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _mainSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta - 90.0f);

        return _instance;
    }
}