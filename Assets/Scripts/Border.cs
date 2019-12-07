using UnityEngine;
using Zenject;

public class Border : MonoBehaviour
{
    public Transform Transform { get; private set; }

    private SpriteRenderer _renderer;

    [Inject]
    public void Construct(SpriteRenderer renderer)
    {
        _renderer = renderer;
        Transform = transform;
    }

    public void Init(Color color)
    {
        _renderer.color = color;
    }

    public class Factory : PlaceholderFactory<float, bool, Level, Border>
    {
    }
}

public class BorderFactory : IFactory<float, bool, Level, Border>
{
    private DiContainer _container;
    private readonly MainSetting _mainSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly LevelSetting _levelSetting;
    private readonly BorderSetting _bordersetting;
    private Border _instance;

    public BorderFactory(DiContainer container, MainSetting mainSetting, ThematicSetting thematicSetting, LevelSetting levelSetting, BorderSetting settings)
    {
        _container = container;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _levelSetting = levelSetting;
        _bordersetting = settings;
    }

    public Border Create(float theta, bool starter, Level level)
    {
        _instance = _container.InstantiatePrefabForComponent<Border>(_bordersetting.prefab, level.Transform);
        _instance.Init(_thematicSetting.ChapterPalletes[level.Index / _mainSetting.levelsPerChapter].borderColor);
        _instance.Transform.localPosition = new Vector2(_mainSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _mainSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta - 90.0f);

        return _instance;
    }
}