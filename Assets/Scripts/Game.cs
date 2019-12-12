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
    private readonly World.Factory _worldFactory;
    private readonly Level.Factory _levelFactory;
    private readonly Camera _camera;
    private readonly TextMeshProUGUI _scoreText;
    private readonly List<Level> _levels;
    private Vector3 _touchWorld;
    private Vector3 _lastTouchWorld;

    private World _world;
    private float _touchDir;
    private float _touchSpeed;
    private float _screenWidth;
    private bool _acceptInput;
    private int _score = 0;

    public Ball Ball { get; private set; }
    public Trail Trail { get; private set; }
    public float RotationalSpeed { get; private set; }
    public Level Level { get; private set; }

    public Game(DiContainer container, SignalBus signalBus, AsyncProcessor asyncProcessor, MainSetting mainSetting, ThematicSetting thematicSetting, World.Factory worldFactory, Level.Factory levelFactory, TextMeshProUGUI scoreText)
    {
        _container = container;
        _signalBus = signalBus;
        _asyncProcessor = asyncProcessor;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _worldFactory = worldFactory;
        _levelFactory = levelFactory;
        _camera = Camera.main;
        _scoreText = scoreText;
        _levels = new List<Level>();

        _signalBus.Subscribe<BallHitBorder>(OnBallHitBorder);
        _signalBus.Subscribe<BallHitCore>(OnBallHitCore);
        _signalBus.Subscribe<BallHitObstacle>(OnBallHitBorder);
        _signalBus.Subscribe<ResetGame>(OnResetGame);
    }

    private void OnResetGame(ResetGame msg)
    {
        GameObject.Destroy(_world.gameObject);
        GameObject.Destroy(Ball.gameObject);
        Initialize();
    }

    public void Initialize()
    {
        LoadWorld();
        LoadLevel(0);

        ConstructBall();
        ConstructTrail();

        Input.multiTouchEnabled = false;
        _acceptInput = true;
        _screenWidth = Screen.width;
    }

    private void LoadWorld()
    {
        _world = _worldFactory.Create();
    }

    private void LoadLevel(int index)
    {
        Level = _levelFactory.Create(index);
        _levels.Add(Level);

        if (Level.Index % _mainSetting.levelsPerChapter == 0)
        {
            _signalBus.Fire(new ThemeUpdated() { levelIndex = Level.Index });

            var pallete = _thematicSetting.ChapterPalletes[Level.Index / _mainSetting.levelsPerChapter];
            _scoreText.DOColor(pallete.fontColor, 0.3f);
        }

        CustomDebug.Log($"Level {Level.Index} loaded");
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
        Ball.Transform.position = _world.Transform.position + Level.Transform.up * (_mainSetting.coreRadius + _mainSetting.jumpHeight * 1.7f);
    }

    public void Tick()
    {
        if (!_acceptInput)
        {
            return;
        }

        if (_mainSetting.inputMode == InputMode.Swipe)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastTouchWorld = _camera.ScreenToWorldPoint(Input.mousePosition);

                Time.timeScale = 0.80f;
                Time.fixedDeltaTime = 0.80f * 0.02f;
            }
            else if (Input.GetMouseButton(0))
            {
                _touchWorld = _camera.ScreenToWorldPoint(Input.mousePosition);

                var dAlpha = Vector3.SignedAngle((_lastTouchWorld - _world.Transform.position),
                    (_touchWorld - _world.Transform.position), Vector3.forward);
                RotationalSpeed = dAlpha / Time.unscaledDeltaTime;
                _world.Transform.Rotate(Vector3.forward, RotationalSpeed * Time.unscaledDeltaTime);

                _lastTouchWorld = _touchWorld;
            }
            else
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
            }
        }
        else if (_mainSetting.inputMode == InputMode.Tap)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePosition.x >= _screenWidth / 2.0f)
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, _mainSetting.rotationalSpeed, 15.0f * Time.unscaledDeltaTime);
            }
            else if (Input.mousePosition.x < _screenWidth / 2.0f)
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, -_mainSetting.rotationalSpeed,
                    15.0f * Time.unscaledDeltaTime);
            }
            else
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, 0.0f, 15.0f * Time.unscaledDeltaTime);
            }
#elif UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, _mainSetting.rotationalSpeed, 15.0f * Time.unscaledDeltaTime);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, -_mainSetting.rotationalSpeed,
                    15.0f * Time.unscaledDeltaTime);
            }
            else
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, 0.0f,
                    15.0f * Time.unscaledDeltaTime);
            }
#endif

            _world.Transform.Rotate(Vector3.forward, RotationalSpeed * Time.unscaledDeltaTime);
        }
        else if (_mainSetting.inputMode == InputMode.TapHold)
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.RightArrow))
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, _mainSetting.rotationalSpeed / 5.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, -_mainSetting.rotationalSpeed / 5.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
            else
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, 0.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.mousePosition.x >= _screenWidth / 2.0f)
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, _mainSetting.rotationalSpeed / 5.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
            else if (Input.mousePosition.x < _screenWidth / 2.0f)
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, -_mainSetting.rotationalSpeed / 5.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
            else
            {
                RotationalSpeed = Mathf.Lerp(RotationalSpeed, 0.0f,
                    30.0f * Time.unscaledDeltaTime);
            }
#endif
            _world.Transform.Rotate(Vector3.forward, RotationalSpeed * Time.unscaledDeltaTime);
        }



        if (Mathf.Abs(Vector3.Angle(Level.Transform.InverseTransformPoint(Ball.Transform.position), Level.Transform.up)) > Level.Span /*/ 2.0f*/)
        {
            OnBallHitBorder();
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

        if (Level.Platforms.Where(p => p.ColorChanger).All(p => Level.Platforms.First().GetColorA() == p.GetColorA()))
        {
            CustomDebug.Log($"Level {Level.Index} passed");
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

        var angle = Vector2.SignedAngle(Vector2.up, Level.Transform.up);

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
        if (!Level.Revealed)
        {
            _asyncProcessor.StartCoroutine(RevealLevel());
        }
    }

    private IEnumerator RevealLevel()
    {
        for (var i = 0; i < Level.Platforms.Count; i++)
        {
            Level.Platforms[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        CustomDebug.Log($"_level.Obstacles.Count {Level.Obstacles.Count}");

        for (var i = 0; i < Level.Obstacles.Count; i++)
        {
            Level.Obstacles[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        Level.Revealed = true;
    }

    private void LevelPassed()
    {
        UpdateScore(_score + 1);
        //GameObject.Destroy(Level.gameObject);
        UnloadLevel(Level.Index - 3);
        LoadLevel(Level.Index + 1);
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