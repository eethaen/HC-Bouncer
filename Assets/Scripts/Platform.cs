using DG.Tweening;
using System.Collections;
using UnityEngine;
using Zenject;

public class Platform : MonoBehaviour
{
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _hollowSprite;
    [SerializeField] private SpriteRenderer _whiteRenderer;
    [SerializeField] private SpriteRenderer _glowRenderer;
    [SerializeField] private SpriteRenderer _hollowRenderer;

    private Game _game;
    private MainSetting _mainSetting;
    private ThematicSetting _thematicSetting;
    private VFXSetting _vfxSetting;
    private PlatformSetting _platformSetting;
    private SpriteRenderer _renderer;

    private int _colorAID;
    private int _colorBID;
    private float _angle;
    private float _radius;

    public Transform Transform { get; private set; }

    [Inject]
    public void Construct(Game game, MainSetting mainSetting, ThematicSetting thematicSetting, VFXSetting vfxSetting, PlatformSetting platformSetting, SpriteRenderer renderer)
    {
        Transform = transform;
        _game = game;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _vfxSetting = vfxSetting;
        _platformSetting = platformSetting;
        _renderer = renderer;

        _colorAID = Shader.PropertyToID("_ColorA");
        _colorBID = Shader.PropertyToID("_ColorB");
    }

    public void Init(Color colorA, Color colorB)
    {
        _renderer.material.SetColor(_colorAID, colorA);
        _renderer.material.SetColor(_colorBID, colorB);
    }

    private void OnEnable()
    {
        StartCoroutine(CustomTick());

        ApplyGlowFX();
    }

    private void OnDisable()
    {
        StopCoroutine(CustomTick());
    }

    private IEnumerator CustomTick()
    {
        while (true)
        {
            Transform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 0.9f * _platformSetting.customTickInterval, RotateMode.Fast)
                     .SetEase(_platformSetting.rotateCurve)
                     .SetUpdate(UpdateType.Normal, true);

            yield return new WaitForSeconds(_platformSetting.customTickInterval);
        }
    }

    public void ShowOnBallCollisionFX()
    {
        ApplyDisplacementFX();
        ApplyColorShiftFX();
        ApplyGlowFX();
        ApplyHollowExpandFX();
    }

    private void ApplyHollowExpandFX()
    {
        _hollowRenderer.gameObject.SetActive(true);

        _hollowRenderer.color = _thematicSetting.ChapterPalletes[_game.Level.Index / _mainSetting.levelsPerChapter].ballColor;

        _hollowRenderer.DOFade(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXTargetAlpha, _vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXDuration)
                       .From(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXStartAlpha)
                       .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFadeCurve)
                       .SetUpdate(UpdateType.Normal, true)
                       .OnComplete(() =>
                       {
                           _hollowRenderer.gameObject.SetActive(false);
                       });

        _hollowRenderer.transform.DOScale(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXTargetScale * new Vector2(1.0f, 1.0f), _vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXDuration)
                                 .From(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandFXStartScale * new Vector2(1.0f, 1.0f))
                                 .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitHollowExpandScaleCurve)
                                 .SetUpdate(UpdateType.Normal, true);
    }

    private void ApplyGlowFX()
    {
        _glowRenderer.gameObject.SetActive(true);

        _glowRenderer.DOFade(_vfxSetting.OnBallHitPlatformSetting.onHitGlowFXTargetAlpha, _vfxSetting.OnBallHitPlatformSetting.onHitGlowFXDuration)
                     .From(_vfxSetting.OnBallHitPlatformSetting.onHitGlowFXStartAlpha)
                     .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitGlowFadeCurve)
                     .SetUpdate(UpdateType.Normal, true)
                     .OnComplete(() =>
                     {
                         _glowRenderer.gameObject.SetActive(false);
                     });

        _glowRenderer.transform.DOScale(_vfxSetting.OnBallHitPlatformSetting.onHitGlowFXTargetScale * new Vector2(1.0f, 1.0f), _vfxSetting.OnBallHitPlatformSetting.onHitGlowFXDuration)
                               .From(_vfxSetting.OnBallHitPlatformSetting.onHitGlowFXStartScale * new Vector2(1.0f, 1.0f))
                               .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitGlowScaleCurve)
                               .SetUpdate(UpdateType.Normal, true);
    }

    private void ApplyColorShiftFX()
    {
        _whiteRenderer.gameObject.SetActive(true);

        _whiteRenderer.DOColor(_thematicSetting.ChapterPalletes[_game.Level.Index / _mainSetting.levelsPerChapter].ballColor, _vfxSetting.OnBallHitPlatformSetting.onHitColorShiftFXDuration)
                      .From(Color.white)
                      .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitColorShiftCurve)
                      .SetUpdate(UpdateType.Normal, true)
                      .OnComplete(() =>
                      {
                          _whiteRenderer.DOFade(0.0f, _vfxSetting.OnBallHitPlatformSetting.onHitColorShiftFXDuration)
                                        .SetUpdate(UpdateType.Normal, true)
                                        .OnComplete(() =>
                                        {
                                            _whiteRenderer.gameObject.SetActive(false);
                                        });
                      });
    }

    private void ApplyDisplacementFX()
    {
        var yPos = Transform.localPosition.y;
        Transform.DOLocalMoveY(yPos - _vfxSetting.OnBallHitPlatformSetting.onHitDisplacement, _vfxSetting.OnBallHitPlatformSetting.onHitDisplacementDuration, false)
                 .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitOutboardDisplacementCurve)
                 .SetUpdate(UpdateType.Normal, true)
                 .OnComplete(() =>
                 {
                     Transform.DOLocalMoveY(yPos, _vfxSetting.OnBallHitPlatformSetting.onHitDisplacementDuration, false)
                              .SetEase(_vfxSetting.OnBallHitPlatformSetting.onHitInboardDisplacementCurve)
                              .SetUpdate(UpdateType.Normal, true);
                 });
    }

    public class Factory : PlaceholderFactory<float, float, float, Transform, int, Platform>
    {
    }
}

public class PlatformFactory : IFactory<float, float, float, Transform, int, Platform>
{
    private readonly DiContainer _container;
    private readonly PlatformSetting _platfromSetting;
    private Platform _instance;

    public PlatformFactory(DiContainer container, PlatformSetting platformSetting)
    {
        _container = container;
        _platfromSetting = platformSetting;
    }

    public Platform Create(float r, float theta, float maxLength, Transform parent, int level)
    {
        CustomDebug.Assert(maxLength > _platfromSetting.minLengthCurve.Evaluate(level));
        CustomDebug.Assert(_platfromSetting.maxLengthCurve.Evaluate(level) > _platfromSetting.minLengthCurve.Evaluate(level));

        var rnd = Random.Range(0.0f, 1.0f);
        var length = Mathf.Lerp(_platfromSetting.minLengthCurve.Evaluate(level), Mathf.Min(_platfromSetting.maxLengthCurve.Evaluate(level), maxLength), rnd);

        var prefabIndex = (int)((1.0f - length) * 5.0f);

        CustomDebug.Assert(prefabIndex < _platfromSetting.prefabs.Length);

        _instance = _container.InstantiatePrefabForComponent<Platform>(_platfromSetting.prefabs[prefabIndex], parent);

        theta += (rnd > 0.5 ? 1.0f : -1.0f) * rnd * (maxLength - length) / 2.0f / r * Mathf.Rad2Deg;

        _instance.Transform.localPosition = new Vector2(r * Mathf.Cos(theta * Mathf.Deg2Rad), r * Mathf.Sin(theta * Mathf.Deg2Rad));
        //_instance.Transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        return _instance;
    }
}
