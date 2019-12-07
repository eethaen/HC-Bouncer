using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public partial class Game : IInitializable, ITickable, IFixedTickable
{
    private readonly DiContainer _container;
    private readonly SignalBus _signalBus;
    private readonly AsyncProcessor _asyncProcessor;
    private readonly MainSetting _mainSetting;
    private readonly Level.Factory _levelFactory;
    private readonly World _world;
    private readonly List<Level> _levels;

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

    public Ball Ball { get; private set; }
    public Trail Trail { get; private set; }

    public Game(DiContainer container, SignalBus signalBus, AsyncProcessor asyncProcessor, MainSetting mainSetting, Level.Factory levelFactory, World world)
    {
        _container = container;
        _signalBus = signalBus;
        _asyncProcessor = asyncProcessor;
        _mainSetting = mainSetting;
        _levelFactory = levelFactory;
        _world = world;
        _levels = new List<Level>();

        _signalBus.Subscribe<BallHitBorder>(OnBallHitBorder);
        _signalBus.Subscribe<BallHitCore>(OnBallHitCore);
        _signalBus.Subscribe<BallHitObstacle>(OnBallHitBorder);
    }

    public void Initialize()
    {
        LoadLevel(0);

        ConstructBall();
        ConstructTrail();

        Input.multiTouchEnabled = false;
        _acceptInput = true;
        _screenWidth = Screen.width;
    }

    private void LoadLevel(int index)
    {
        _level = _levelFactory.Create(index);
        _signalBus.Fire(new LevelLoaded { level = _level });
        _levels.Add(_level);
        CustomDebug.Log($"Level {_level.Index} loaded");
    }

    private void UnloadLevel(int index)
    {
        if (!_levels.Any(l => l.Index == index))
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
        Ball.Transform.position = _world.Transform.position + _level.Transform.up * (_mainSetting.coreRadius + _mainSetting.spawnPoinOffset);
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
            _world.Transform.Rotate(Vector3.forward, _rotationalSpeed * Time.unscaledDeltaTime);

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

    public void AssessLevel()
    {
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
            _asyncProcessor.StartCoroutine(RevealPlatforms());
        }
    }

    private IEnumerator RevealPlatforms()
    {
        for (var i = 0; i < _level.Platforms.Count; i++)
        {
            _level.Platforms[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        for (var i = 0; i < _level.Obstacles.Count; i++)
        {
            _level.Obstacles[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        _level.Revealed = true;
    }

    private void LevelPassed()
    {
        //GameObject.Destroy(Level.gameObject);
        UnloadLevel(_level.Index - 1);
        LoadLevel(_level.Index + 1);
        ReorientLevel();
    }
}
