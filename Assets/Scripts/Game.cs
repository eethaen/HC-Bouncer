using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class Game : IInitializable, ITickable, IFixedTickable
{
    private readonly SignalBus _signalBus;
    private readonly AsyncProcessor _asyncProcessor;
    private readonly Setting _setting;
    private readonly Level.Factory _levelFactory;

    private Vector2 _touch;
    private Vector2 _lastTouch;
    private float _touchTime;
    private float _lastTouchTime;
    private float _touchDir;
    private float _touchSpeed;
    private float _screenWidth;
    private bool _acceptInput;

    public Level Level { get; private set; }

    public Game(SignalBus signalBus, AsyncProcessor asyncProcessor,Setting setting, Level.Factory levelFactory)
    {
        _signalBus = signalBus;
        _asyncProcessor = asyncProcessor;
        _setting = setting;
        _levelFactory = levelFactory;

        _signalBus.Subscribe<BallHitBorder>(OnBallHitBorder);
        _signalBus.Subscribe<LevelPassed>(OnLevelPassed);
        _signalBus.Subscribe<BallHitCore>(OnBallHitCore);
    }

    public void Initialize()
    {
        Level = _levelFactory.Create(0);

        CustomDebug.Log($"Level {Level.Index} loaded");

        _signalBus.Fire<LevelLoaded>();

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
            _lastTouchTime = Time.time;

            Time.timeScale *= 0.8f;
            Time.fixedDeltaTime *= 0.8f;
        }
        else if (Input.GetMouseButton(0))
        {
            _touch = Input.mousePosition;
            _touchTime = Time.time;

            if (_touch.x > _lastTouch.x)
            {
                _touchDir = 1.0f;
            }
            else if (_touch.x < _lastTouch.x)
            {
                _touchDir = -1.0f;
            }
            else if (_touch.y > _lastTouch.y)
            {
                _touchDir = 1.0f;
            }
            else if (_touch.y < _lastTouch.y)
            {
                _touchDir = -1.0f;
            }
            else
            {
                _touchDir = 0.0f;
            }

            _touchSpeed = _touchDir * Mathf.Clamp((_touch - _lastTouch).magnitude / (_touchTime - _lastTouchTime) / _screenWidth, 0.0f, _setting.maxTouchSpeed);
            Level.Transform.Rotate(Vector3.forward, -_setting.rotationalSpeed * _touchSpeed * Time.deltaTime);

            //CustomDebug.Log($"Mathf.Abs(_touchSpeed): {Mathf.Abs(_touchSpeed)}");

            _lastTouch = _touch;
            _lastTouchTime = _touchTime;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = .02f;
        }
    }

    public void FixedTick()
    {
    }

    private void OnBallHitBorder()
    {
        _acceptInput = false;

        Time.timeScale *= 0.1f;
        Time.fixedDeltaTime *= 0.1f;

        Level.Ball.Rigidbody.simulated = false;
        Level.Ball.Renderer.DOFade(0.0f, _setting.respawnTime / 30.0f);

        Level.Trail.gameObject.SetActive(false);

        var segment = Utility.DetermineSegmentByPosition(Level, Level.Ball.Transform.localPosition);
        CustomDebug.Log($"Segment determined to be {segment}");
        var angle = Vector2.SignedAngle(Vector2.up, Level.Segments[segment].Transform.up);
        CustomDebug.Log($"Angle {angle}");

        //Debug.Break();

        Level.Transform.DORotate(new Vector3(0.0f, 0.0f, -angle), _setting.respawnTime, RotateMode.WorldAxisAdd)
                      .SetEase(Ease.InOutQuad)
                      .SetUpdate(UpdateType.Normal, true)
                      .OnComplete(() =>
                      {
                          Time.timeScale = 1.0f;
                          Time.fixedDeltaTime = .02f;
                          Level.Ball.Rigidbody.simulated = true;
                          Level.Ball.Renderer.DOFade(1.0f, _setting.respawnTime / 10.0f);
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

    [System.Serializable]
    public class Setting
    {
        public float rotationalSpeed = 45.0f;
        public float respawnTime = 1.5f;
        public float maxTouchSpeed = 20.0f;
    }
}
