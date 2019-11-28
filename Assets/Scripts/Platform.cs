using DG.Tweening;
using System.Collections;
using UnityEngine;
using Zenject;

public class Platform : MonoBehaviour
{
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _hollowSprite;
    [SerializeField] private Sprite _whiteSprite;
    [SerializeField] private SpriteRenderer _glowRenderer;
    [SerializeField] private SpriteRenderer _hollowRenderer;

    public Transform Transform { get; private set; }

    private Game _game;
    private Level.Setting _levelSetting;
    private Setting _setting;
    private SpriteRenderer _renderer;

    private float _angle;
    private float _radius;

    [Inject]
    public void Construct(Game game, Level.Setting levelSetting, Setting setting, SpriteRenderer renderer)
    {
        Transform = transform;
        _setting = setting;
        _renderer = renderer;
    }

    private void OnEnable()
    {
        StartCoroutine(CustomTick());
    }

    private void OnDisable()
    {
        StopCoroutine(CustomTick());
    }

    private IEnumerator CustomTick()
    {
        while (true)
        {
            Transform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 0.9f * _setting.customTickInterval, RotateMode.Fast).SetEase(_setting.rotateCurve);
            yield return new WaitForSeconds(_setting.customTickInterval);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Ball>())
        {
            if (Transform.position.y > collision.GetContact(0).point.y)
            {
                return;
            }

            ApplyDisplacementFX();
            ApplyColorShiftFX();
            ApplyGlowFX();
            ApplyHollowExpandFX();
        }
    }

    private void ApplyHollowExpandFX()
    {
        _hollowRenderer.gameObject.SetActive(true);

        _hollowRenderer.DOFade(_setting.onHitHollowExpandFXTargetAlpha, _setting.onHitHollowExpandFXDuration)
                       .From(_setting.onHitHollowExpandFXStartAlpha)
                       .SetEase(_setting.onHitHollowExpandFadeCurve)
                       .OnComplete(() =>
                       {
                           _hollowRenderer.gameObject.SetActive(false);
                       });

        _hollowRenderer.transform.DOScale(_setting.onHitHollowExpandFXTargetScale * new Vector3(1.0f, 1.0f, 0.0f), _setting.onHitHollowExpandFXDuration)
                                 .From(_setting.onHitHollowExpandFXStartScale * new Vector3(1.0f, 1.0f, 0.0f))
                                 .SetEase(_setting.onHitHollowExpandScaleCurve);

        _hollowRenderer.DOColor(Color.white, _setting.onHitHollowExpandFXDuration)
                       .From(_levelSetting.colorPalletes[_game.Level.Index].ballColor)
                       .SetEase(_setting.onHitHollowExpandColorShiftCurve);
    }

    private void ApplyGlowFX()
    {
        _glowRenderer.gameObject.SetActive(true);

        _glowRenderer.DOFade(_setting.onHitGlowFXTargetAlpha, _setting.onHitGlowFXDuration)
                     .From(_setting.onHitGlowFXStartAlpha)
                     .SetEase(_setting.onHitGlowFadeCurve)
                     .OnComplete(() =>
                     {
                         _glowRenderer.gameObject.SetActive(false);
                     });

        _glowRenderer.transform.DOScale(_setting.onHitGlowFXTargetScale * new Vector3(1.0f, 1.0f, 0.0f), _setting.onHitGlowFXDuration)
                    .From(_setting.onHitGlowFXStartScale * new Vector3(1.0f, 1.0f, 0.0f))
                    .SetEase(_setting.onHitGlowScaleCurve);
    }

    private void ApplyColorShiftFX()
    {
        _renderer.sprite = _whiteSprite;

        _renderer.DOColor(_levelSetting.colorPalletes[_game.Level.Index].ballColor, _setting.onHitColorShiftFXDuration)
                 .From(Color.white)
                 .SetEase(_setting.onHitOutboardColorShiftCurve)
                 .OnComplete(() =>
                 {
                     _renderer.DOColor(_levelSetting.colorPalletes[_game.Level.Index].platformColor, _setting.onHitColorShiftFXDuration)
                              .SetEase(_setting.onHitInboardColorShiftCurve)
                              .OnComplete(() =>
                              {
                                  _renderer.sprite = _defaultSprite;
                              });
                 });
    }

    private void ApplyDisplacementFX()
    {
        var yPos = Transform.localPosition.y;
        Transform.DOLocalMoveY(yPos - _setting.onHitDisplacement, _setting.onHitDisplacementDuration, false)
                 .SetEase(_setting.onHitOutboardDisplacementCurve)
                 .OnComplete(() =>
                 {
                     Transform.DOLocalMoveY(yPos, _setting.onHitDisplacementDuration, false)
                              .SetEase(_setting.onHitInboardDisplacementCurve);
                 });
    }

    [System.Serializable]
    public class Setting
    {
        [Header("General Properties")]
        [Space]
        public Platform[] prefabs;
        public AnimationCurve minLengthCurve;
        public AnimationCurve maxLengthCurve;
        public float customTickInterval;

        [Space]
        [Header("Rotate Upward Properties")]
        [Space]
        public AnimationCurve rotateCurve;

        [Space]
        [Header("On Hit Displacement Properties")]
        [Space]
        public float onHitDisplacement;
        public float onHitDisplacementDuration;
        public AnimationCurve onHitOutboardDisplacementCurve;
        public AnimationCurve onHitInboardDisplacementCurve;

        [Space]
        [Header("On Hit Color Shift FX Properties")]
        [Space]
        public float onHitColorShiftFXDuration;
        public AnimationCurve onHitOutboardColorShiftCurve;
        public AnimationCurve onHitInboardColorShiftCurve;

        [Space]
        [Header("On Hit Glow FX Properties")]
        [Space]
        public float onHitGlowFXDuration;
        public float onHitGlowFXStartAlpha;
        public float onHitGlowFXTargetAlpha;
        public AnimationCurve onHitGlowFadeCurve;
        public float onHitGlowFXStartScale;
        public float onHitGlowFXTargetScale;
        public AnimationCurve onHitGlowScaleCurve;

        [Space]
        [Header("On Hit Hollow Expand FX Properties")]
        [Space]
        public float onHitHollowExpandFXDuration;
        public float onHitHollowExpandFXStartAlpha;
        public float onHitHollowExpandFXTargetAlpha;
        public AnimationCurve onHitHollowExpandFadeCurve;
        public float onHitHollowExpandFXStartScale;
        public float onHitHollowExpandFXTargetScale;
        public AnimationCurve onHitHollowExpandScaleCurve;
        public AnimationCurve onHitHollowExpandColorShiftCurve;
    }

    public class Factory : PlaceholderFactory<float, float, float, Transform, int, Platform>
    {
    }
}

public class PlatformFactory : IFactory<float, float, float, Transform, int, Platform>
{
    private readonly DiContainer _container;
    private readonly Platform.Setting _setting;
    private Platform _instance;

    public PlatformFactory(DiContainer container, Platform.Setting setting)
    {
        _container = container;
        _setting = setting;
    }

    public Platform Create(float r, float theta, float maxLength, Transform parent, int level)
    {
        CustomDebug.Assert(maxLength > _setting.minLengthCurve.Evaluate(level));
        CustomDebug.Assert(_setting.maxLengthCurve.Evaluate(level) > _setting.minLengthCurve.Evaluate(level));

        var rnd = Random.Range(0.0f, 1.0f);
        var length = Mathf.Lerp(_setting.minLengthCurve.Evaluate(level), Mathf.Min(_setting.maxLengthCurve.Evaluate(level), maxLength), rnd);

        var prefabIndex = (int)((1.0f - length) * 5.0f);
        CustomDebug.Assert(prefabIndex < _setting.prefabs.Length);
        _instance = _container.InstantiatePrefabForComponent<Platform>(_setting.prefabs[prefabIndex], parent);

        theta += (rnd > 0.5 ? 1.0f : -1.0f) * rnd * (maxLength - length) / 2.0f / r * Mathf.Rad2Deg;

        _instance.Transform.localPosition = new Vector2(r * Mathf.Cos(theta * Mathf.Deg2Rad), r * Mathf.Sin(theta * Mathf.Deg2Rad));
        //_instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, theta);
        //_instance.Transform.Translate(r * Vector3.up, Space.Self);

        return _instance;
    }
}
