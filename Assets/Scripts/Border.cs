using UnityEngine;
using Zenject;

public class Border : MonoBehaviour
{
    public Transform Transform { get; private set; }

    private SpriteRenderer[] _renderers;

    [Inject]
    public void Construct(SpriteRenderer[] renderers)
    {
        _renderers = renderers;
        Transform = transform;
    }

    public void Init(Color color)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].color = color;
        }
    }

    public class Factory : PlaceholderFactory<float, bool, Transform, int, Border>
    {
    }
}

public class BorderFactory : IFactory<float, bool, Transform, int, Border>
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

    public Border Create(float theta, bool starter, Transform parent, int level)
    {
        _instance = _container.InstantiatePrefabForComponent<Border>(_bordersetting.prefabs[0], parent);

        _instance.Init(_thematicSetting.ChapterPalletes[level / _mainSetting.levelsPerChapter].borderColor);

        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta);
        _instance.Transform.Translate(_levelSetting.coreRadius * Vector3.up, Space.Self);

        return _instance;
    }
}