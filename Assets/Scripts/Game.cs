using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

public class Game : IInitializable, ITickable, IFixedTickable
{
    private readonly DiContainer _container;
    private readonly SignalBus _signalBus;
    private readonly AsyncProcessor _asyncProcessor;
    private readonly MainSetting _mainSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly Level.Factory _levelFactory;
    private readonly World _world;
    private readonly TextMeshProUGUI _scoreText;
    private readonly List<Level> _levels;
    private readonly Camera _camera;

    private Level _level;
    private Vector2 _touch;
    private Vector2 _lastTouch;
    private float _touchTime;
    private float _lastTouchTime;

    private float _rotationalSpeed;
    private float _touchDir;
    private float _touchSpeed;
    private float _screenWidth;
    private bool _acceptInput;
    private int _score = 0;
    private float _dpi;

    public Ball Ball { get; private set; }
    public Trail Trail { get; private set; }

    public Game(DiContainer container, SignalBus signalBus, AsyncProcessor asyncProcessor, MainSetting mainSetting, ThematicSetting thematicSetting, Level.Factory levelFactory, World world, TextMeshProUGUI scoreText)
    {
        _container = container;
        _signalBus = signalBus;
        _asyncProcessor = asyncProcessor;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _levelFactory = levelFactory;
        _world = world;
        _scoreText = scoreText;
        _levels = new List<Level>();
        _camera = Camera.main;

        _signalBus.Subscribe<BallHitBorder>(OnBallHitBorder);
        _signalBus.Subscribe<BallHitCore>(OnBallHitCore);
        _signalBus.Subscribe<BallHitObstacle>(OnBallHitBorder);
    }

    public void Initialize()
    {
        _world.Construct();

        LoadLevel(0);

        ConstructBall();
        ConstructTrail();

        Input.multiTouchEnabled = false;
        _acceptInput = true;
        _screenWidth = Screen.width;

#if UNITY_ANDROID && !UNITY_EDITOR
        _dpi = GetDPI();
#else
        _dpi = Screen.dpi;
#endif
    }

    private void LoadLevel(int index)
    {
        _level = _levelFactory.Create(index);
        _signalBus.Fire(new LevelLoaded { level = _level });
        _levels.Add(_level);

        if (_level.Index % _mainSetting.levelsPerChapter == 0)
        {
            _signalBus.Fire(new ThemeUpdated() { levelIndex = _level.Index });

            var pallete = _thematicSetting.ChapterPalletes[_level.Index / _mainSetting.levelsPerChapter];
            _scoreText.DOColor(pallete.fontColor, 0.3f);
        }

        CustomDebug.Log($"Level {_level.Index} loaded");
    }

    private void UnloadLevel(int index)
    {
        if (_levels.All(l => l.Index != index))
        {
            return;
        }

        var level = _levels.Single(l => l.Index == index);
        _levels.Remove(level);
        GameObject.Destroy(level.gameObject);
        CustomDebug.Log($"Level {index} Unloaded");
    }

    private void ConstructTrail()
    {
        Trail = _container.InstantiatePrefabForComponent<Trail>(_mainSetting.trailPrefab, _world.Transform);
    }

    private void ConstructBall()
    {
        Ball = _container.InstantiatePrefabForComponent<Ball>(_mainSetting.ballPrefab);
        Ball.Transform.position = _world.Transform.position + _level.Transform.up * (_mainSetting.coreRadius + _mainSetting.jumpHeight * 1.7f);
    }

    public void Tick()
    {
        if (!_acceptInput)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _lastTouch = _camera.ScreenToWorldPoint(Input.mousePosition);
            _lastTouchTime = Time.unscaledTime;

            Time.timeScale = 0.90f;
            Time.fixedDeltaTime = 0.90f * 0.02f;
        }
        else if (Input.GetMouseButton(0))
        {
            _touch = _camera.ScreenToWorldPoint(Input.mousePosition);
            _touchTime = Time.unscaledTime;

            var swipeSpeed = -Mathf.Sign(_touch.x - _lastTouch.x) * (_touch - _lastTouch).magnitude / (_touchTime - _lastTouchTime);
            _rotationalSpeed = Mathf.Lerp(_rotationalSpeed, swipeSpeed * _mainSetting.swipeSpeedMultiplier, 40.0f * Time.unscaledDeltaTime);
            _world.Transform.Rotate(Vector3.forward, _rotationalSpeed * Time.unscaledDeltaTime);

            _lastTouch = _touch;
            _lastTouchTime = _touchTime;
        }
        else
        {
            _rotationalSpeed = 0.0f;

            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
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

    public void AssessLevel()
    {
        CustomDebug.Log($"Assess Level invoked");

        if (_level.Platforms.Where(p => p.ColorChanger).All(p => _level.Platforms.First().GetColorA() == p.GetColorA()))
        {
            CustomDebug.Log($"Level {_level.Index} passed");
            LevelPassed();
        }
    }

    public void FixedTick()
    {
    }

    private void OnBallHitBorder()
    {
        UpdateScore(0);
        ReorientLevel();
    }

    private void ReorientLevel()
    {
        _acceptInput = false;

        Ball.Rigidbody.simulated = false;
        Ball.Renderer.DOFade(0.0f, _mainSetting.respawnTime / 30.0f);

        Trail.gameObject.SetActive(false);

        var angle = Vector2.SignedAngle(Vector2.up, _level.Transform.up);

        _world.Transform.DORotate(new Vector3(0.0f, 0.0f, -angle), _mainSetting.respawnTime, RotateMode.WorldAxisAdd)
                      .SetEase(Ease.InOutQuad)
                      .SetUpdate(UpdateType.Normal, true)
                      .OnComplete(() =>
                      {
                          Ball.Rigidbody.simulated = true;
                          Ball.Renderer.DOFade(1.0f, _mainSetting.respawnTime / 10.0f);
                          _acceptInput = true;
                          Trail.gameObject.SetActive(true);
                      });
    }

    private void OnBallHitCore()
    {
        if (!_level.Revealed)
        {
            _asyncProcessor.StartCoroutine(RevealLevel());
        }
    }

    private IEnumerator RevealLevel()
    {
        for (var i = 0; i < _level.Platforms.Count; i++)
        {
            _level.Platforms[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        CustomDebug.Log($"_level.Obstacles.Count {_level.Obstacles.Count}");

        for (var i = 0; i < _level.Obstacles.Count; i++)
        {
            _level.Obstacles[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        _level.Revealed = true;
    }

    private void LevelPassed()
    {
        UpdateScore(_score + 1);
        //GameObject.Destroy(Level.gameObject);
        UnloadLevel(_level.Index - 3);
        LoadLevel(_level.Index + 1);
        ReorientLevel();
    }

    private void UpdateScore(int score)
    {
        _score = score;
        _scoreText.text = _score.ToString();
    }

    float GetDPI()
    {
        var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

        var metrics = new AndroidJavaObject("android.util.DisplayMetrics");
        activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

        return (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
    }
}