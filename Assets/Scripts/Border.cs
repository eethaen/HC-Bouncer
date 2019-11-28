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

    [System.Serializable]
    public class Setting
    {
        public Border[] prefabs;
        public float offset;
    }

    public class Factory : PlaceholderFactory<float, bool, Transform, int, Border>
    {
    }
}

public class BorderFactory : IFactory<float, bool, Transform, int, Border>
{
    private DiContainer _container;
    private readonly Level.Setting _levelSetting;
    private readonly Border.Setting _bordersetting;
    private Border _instance;

    public BorderFactory(DiContainer container, Level.Setting levelSetting,Border.Setting settings)
    {
        _container = container;
        _levelSetting = levelSetting;
        _bordersetting = settings;
    }

    public Border Create(float theta, bool starter, Transform parent, int level)
    {
        _instance = starter
            ? _container.InstantiatePrefabForComponent<Border>(_bordersetting.prefabs[0], parent)
            : _container.InstantiatePrefabForComponent<Border>(_bordersetting.prefabs[Random.Range(1, _bordersetting.prefabs.Length)], parent);

        _instance.Init(_levelSetting.colorPalletes[level].environmentColor);

        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta);
        _instance.Transform.Translate(_bordersetting.offset * Vector3.up, Space.Self);

        return _instance;
    }
}