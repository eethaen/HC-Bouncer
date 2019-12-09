using DG.Tweening;
using System.Collections;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class Platform : MonoBehaviour
{
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _hollowSprite;
    [SerializeField] private SpriteRenderer _whiteRenderer;
    [SerializeField] private SpriteRenderer _glowRenderer;
    [SerializeField] private SpriteRenderer _hollowRenderer;

    private SignalBus _signalBus;
    private Game _game;
    private int _colorDuoIndex;
    private MainSetting _mainSetting;
    private ThematicSetting _thematicSetting;
    private VFXSetting _vfxSetting;
    private PlatformSetting _platformSetting;
    private Level _level;

    private int _colorAID;
    private int _colorBID;

    public Transform Transform { get; private set; }
    public bool ColorChanger { get; private set; }
    public bool Transient { get; private set; }

    private SpriteRenderer _renderer;

    [Inject]
    public void Construct(SignalBus signalBus, Game game, MainSetting mainSetting, ThematicSetting thematicSetting, VFXSetting vfxSetting, PlatformSetting platformSetting, SpriteRenderer renderer)
    {
        Transform = transform;

        _signalBus = signalBus;
        _game = game;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _vfxSetting = vfxSetting;
        _platformSetting = platformSetting;
        _renderer = renderer;

        _colorAID = Shader.PropertyToID("_ColorA");
        _colorBID = Shader.PropertyToID("_ColorB");

        _signalBus.Subscribe<LevelLoaded>(OnLevelLoaded);
    }

    public void Init(Level level)
    {
        _level = level;

        ColorChanger = false;
        Transient = false;

        _renderer.material.SetColor(_colorAID, _thematicSetting.ChapterPalletes[_level.Index / _mainSetting.levelsPerChapter].platformColor.colorA);
        _renderer.material.SetColor(_colorBID, _thematicSetting.ChapterPalletes[_level.Index / _mainSetting.levelsPerChapter].platformColor.colorB);
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

    public void SetAsColorChanger(int colorDuoIndex)
    {
        ColorChanger = true;

        _colorDuoIndex = colorDuoIndex;

        _renderer.material.SetColor(_colorAID, _thematicSetting.platformColorSequence[_colorDuoIndex].colorA);
        _renderer.material.SetColor(_colorBID, _thematicSetting.platformColorSequence[_colorDuoIndex].colorB);
    }

    public void SetAsTransient()
    {
        Transient = true;
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
        //ApplyColorShiftFX();
        ApplyGlowFX();
        ApplyHollowExpandFX();
    }

    public void ShiftColor()
    {
        CustomDebug.Assert(ColorChanger);

        if (Transient)
        {
            CancelInvoke("ShiftColor");
        }

        _colorDuoIndex = (short)((_colorDuoIndex + 1) % _thematicSetting.platformColorSequence.Length);

        _renderer.material.DOColor(_thematicSetting.platformColorSequence[_colorDuoIndex].colorA, _colorAID, 0.3f)
                         .OnComplete(() =>
                         {
                             _game.AssessLevel();

                             if (Transient)
                             {
                                 Invoke("ShiftColor", _platformSetting.TransientColorChangeInterval);
                             }
                         });

        _renderer.material.DOColor(_thematicSetting.platformColorSequence[_colorDuoIndex].colorB, _colorBID, 0.3f);
    }

    private void ApplyHollowExpandFX()
    {
        _hollowRenderer.gameObject.SetActive(true);

        _hollowRenderer.color = _thematicSetting.ChapterPalletes[_level.Index / _mainSetting.levelsPerChapter].ballColor;

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

    public Color GetColorA()
    {
        return _renderer.material.GetColor(_colorAID);
    }

    public Color GetColorB()
    {
        return _renderer.material.GetColor(_colorBID);
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

        _whiteRenderer.DOColor(_thematicSetting.ChapterPalletes[_level.Index / _mainSetting.levelsPerChapter].ballColor, _vfxSetting.OnBallHitPlatformSetting.onHitColorShiftFXDuration)
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

    private void OnLevelLoaded(LevelLoaded msg)
    {
        _level = msg.level;
    }

    public struct Coord
    {
        public float radius;
        public float theta;
    }

    public class Factory : PlaceholderFactory<Platform.Coord, Level, Platform>
    {
    }
}

public class PlatformFactory : IFactory<Platform.Coord, Level, Platform>
{
    private readonly DiContainer _container;
    private readonly LevelSetting _levelSetting;
    private readonly PlatformSetting _platfromSetting;
    private Platform _instance;

    public PlatformFactory(DiContainer container, LevelSetting levelSetting, PlatformSetting platformSetting)
    {
        _container = container;
        _levelSetting = levelSetting;
        _platfromSetting = platformSetting;
    }

    public Platform Create(Platform.Coord state, Level level)
    {
        CustomDebug.Assert(_platfromSetting.maxLengthCurve.Evaluate(level.Index) > _platfromSetting.minLengthCurve.Evaluate(level.Index));

        var rnd = Random.Range(0.0f, 1.0f);
        var maxLength = 0.8f * state.radius * level.ChannelSpan * Mathf.Deg2Rad;
        var length = Mathf.Lerp(_platfromSetting.minLengthCurve.Evaluate(level.Index), Mathf.Min(_platfromSetting.maxLengthCurve.Evaluate(level.Index), maxLength), rnd);
        state.theta += (rnd * 2.0f - 1.0f) * (maxLength - length) / 2.0f / state.radius * Mathf.Rad2Deg;

        var prefabIndex = (int)((1.0f - length) * 5.0f);

        CustomDebug.Assert(prefabIndex < _platfromSetting.prefabs.Length);

        _instance = _container.InstantiatePrefabForComponent<Platform>(_platfromSetting.prefabs[prefabIndex], level.Transform);
        _instance.Init(level);
        level.Platforms.Add(_instance);
        _instance.Transform.localPosition = new Vector2(state.radius * Mathf.Cos(state.theta * Mathf.Deg2Rad), state.radius * Mathf.Sin(state.theta * Mathf.Deg2Rad));

        return _instance;
    }
}
