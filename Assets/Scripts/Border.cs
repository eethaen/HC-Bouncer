using DG.Tweening;
using UnityEngine;
using Zenject;

public class Border : MonoBehaviour
{
    public Transform Transform { get; private set; }

    private SpriteRenderer _renderer;

    [Inject]
    public void Construct(Game game, SpriteRenderer renderer)
    {
        _renderer = renderer;
        Transform = transform;
    }

    public void SetColor(Color color)
    {
        _renderer.DOColor(color, 0.3f);
    }

    public class Factory : PlaceholderFactory<float, Border>
    {
    }
}

public class BorderFactory : IFactory<float, Border>
{
    private readonly DiContainer _container;
    private readonly MainSetting _mainSetting;
    private readonly BorderSetting _borderSetting;
    private readonly World _world;
    private Border _instance;

    public BorderFactory(DiContainer container, MainSetting mainSetting, BorderSetting borderSettings, World world)
    {
        _container = container;
        _mainSetting = mainSetting;
        _borderSetting = borderSettings;
        _world = world;
    }

    public Border Create(float theta)
    {
        _instance = _container.InstantiatePrefabForComponent<Border>(_borderSetting.prefab, _world.Transform);
        _instance.Transform.localPosition = new Vector2(_mainSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _mainSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta - 90.0f);

        return _instance;
    }
}