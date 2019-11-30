using DG.Tweening;
using System.Collections;
using UnityEngine;
using Zenject;

public partial class Game : IInitializable, ITickable, IFixedTickable
{
    private readonly SignalBus _signalBus;
    private readonly AsyncProcessor _asyncProcessor;
    private readonly MainSetting _mainSetting;
    private readonly Level.Factory _levelFactory;
    private readonly World _world;

    private Vector2 _touch;
    private Vector2 _lastTouch;
    private float _touchTime;
    private float _lastTouchTime;

    //#if UNITY_ANDROID || UNITY_IOS

    //    private Touch _touch;

    //#endif

    private float _rotationalSpeed;
    private float _touchDir;
    private float _touchSpeed;
    private float _screenWidth;
    private bool _acceptInput;
    private int _collectedOrbsCount = 0;
    private int _score = 0;

    public Level Level { get; private set; }

    public Game(SignalBus signalBus, AsyncProcessor asyncProcessor, MainSetting mainSetting, Level.Factory levelFactory, World world)
    {
        _signalBus = signalBus;
        _asyncProcessor = asyncProcessor;
        _mainSetting = mainSetting;
        _levelFactory = levelFactory;
        _world = world;
        _signalBus.Subscribe<BallHitBorder>(OnBallHitBorder);
        _signalBus.Subscribe<LevelPassed>(OnLevelPassed);
        _signalBus.Subscribe<BallHitCore>(OnBallHitCore);
        _signalBus.Subscribe<BallHitOrb>(OnBallHitOrb);
    }

    public void Initialize()
    {
        Level = _levelFactory.Create(0);

        CustomDebug.Log($"Level {Level.Index} loaded");

        _signalBus.Fire<LevelLoaded>();

        Input.multiTouchEnabled = false;
        _acceptInput = true;
        _screenWidth = Screen.width;
    }

    public void Tick()
    {
        if (!_acceptInput)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _lastTouch = Input.mousePosition;
            _lastTouchTime = Time.unscaledTime;
        }
        else if (Input.GetMouseButton(0))
        {
            _touch = Input.mousePosition;
            _touchTime = Time.unscaledTime;

            var swipeSpeed = -Mathf.Sign(_touch.x - _lastTouch.x) * (_touch - _lastTouch).magnitude / (_touchTime - _lastTouchTime);
            _rotationalSpeed = Mathf.Lerp(_rotationalSpeed, swipeSpeed / (0.02f * _screenWidth), 40.0f * Time.unscaledDeltaTime);
            Level.Transform.Rotate(Vector3.forward, _rotationalSpeed * Time.unscaledDeltaTime);

            _lastTouch = _touch;
            _lastTouchTime = _touchTime;
        }
        else
        {
            _rotationalSpeed = 0.0f;
        }

        //#if UNITY_ANDROID || UNITY_IOS

        //        _touch = Input.touches[0];

        //        if (_touch.phase == TouchPhase.Moved)
        //        {
        //            var rotationalSpeed = Vector2.SignedAngle((_touch.position - _touch.deltaPosition - _worldScreenPosition), (_touch.position - _worldScreenPosition)) / /*Time.unscaledDeltaTime*/_touch.deltaTime;
        //            rotationalSpeed = Mathf.Clamp(rotationalSpeed, -_mainSetting.maxRotationaSpeed, _mainSetting.maxRotationaSpeed);
        //            _rotationalSpeed = Mathf.Lerp(_rotationalSpeed, rotationalSpeed, 0.5f);
        //            Level.Transform.Rotate(Vector3.forward, _rotationalSpeed * Time.unscaledDeltaTime);
        //        }

        //#endif
    }

    public void FixedTick()
    {
    }

    private void OnBallHitBorder()
    {
        _acceptInput = false;

        Level.Ball.Rigidbody.simulated = false;
        Level.Ball.Renderer.DOFade(0.0f, _mainSetting.respawnTime / 30.0f);

        Level.Trail.gameObject.SetActive(false);

        var segment = Utility.DetermineSegmentByPosition(Level, Level.Ball.Transform.localPosition);
        var angle = Vector2.SignedAngle(Vector2.up, Level.Segments[segment].Transform.up);

        Level.Transform.DORotate(new Vector3(0.0f, 0.0f, -angle), _mainSetting.respawnTime, RotateMode.WorldAxisAdd)
                      .SetEase(Ease.InOutQuad)
                      .SetUpdate(UpdateType.Normal, true)
                      .OnComplete(() =>
                      {
                          Level.Ball.Rigidbody.simulated = true;
                          Level.Ball.Renderer.DOFade(1.0f, _mainSetting.respawnTime / 10.0f);
                          _acceptInput = true;
                          Level.Trail.gameObject.SetActive(true);
                      });
    }

    private void OnBallHitCore(BallHitCore msg)
    {
        if (!Level.Segments[msg.segment].Revealed)
        {
            _asyncProcessor.StartCoroutine(RevealPlatforms(msg.segment));
            Level.Segments[msg.segment].Revealed = true;
        }
    }

    private void OnBallHitOrb()
    {
        _collectedOrbsCount++;
    }

    private IEnumerator RevealPlatforms(int segment)
    {
        for (int i = 0; i < Level.Segments[segment].Platforms.Count; i++)
        {
            Level.Segments[segment].Platforms[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnLevelPassed()
    {
        GameObject.Destroy(Level.Trail.gameObject);
        GameObject.Destroy(Level.Ball.gameObject);
        GameObject.Destroy(Level.gameObject);

        Level = _levelFactory.Create(Level.Index + 1);

        _signalBus.Fire<LevelLoaded>();

        CustomDebug.Log($"Level {Level.Index} loaded");
    }
}
