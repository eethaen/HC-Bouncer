using DG.Tweening;
using System;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class Orb : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _glowRenderer;
    [SerializeField] private SpriteRenderer _hollowRenderer;

    private Game _game;
    private OrbSetting _setting;

    public Transform Transform { get; private set; }

    [Inject]
    public void Construct(Game game, OrbSetting setting)
    {
        Transform = transform;
        _game = game;
        _setting = setting;
    }

    private void OnEnable()
    {
        Transform.DOLocalRotate(new Vector3(0.0f, 0.0f, _setting.rotationalSpeed), 1.0f)
                     .SetEase(Ease.Linear)
                     .SetLoops(-1, LoopType.Incremental);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Ball>())
        {
            ApplyGlowFX();
            ApplyHollowEmergeFX();
        }
    }

    private void ApplyHollowEmergeFX()
    {
        //throw new NotImplementedException();
    }

    private void ApplyGlowFX()
    {
        //throw new NotImplementedException();
    }

    public class Factory : PlaceholderFactory<float, float, Transform, int, Orb> { }
}

public class OrbFactory : IFactory<float, float, Transform, int, Orb>
{
    private readonly DiContainer _container;
    private readonly OrbSetting _setting;
    private Orb _instance;

    public OrbFactory(DiContainer container, OrbSetting setting)
    {
        _container = container;
        _setting = setting;
    }

    public Orb Create(float r, float theta, Transform parent, int level)
    {
        _instance = _container.InstantiatePrefabForComponent<Orb>(_setting.prefab, parent);
        _instance.Transform.localPosition = new Vector2(r * Mathf.Cos(theta * Mathf.Deg2Rad), r * Mathf.Sin(theta * Mathf.Deg2Rad));
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0f, 120f));

        return _instance;
    }
}
