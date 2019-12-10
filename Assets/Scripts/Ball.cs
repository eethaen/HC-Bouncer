using System.Linq;
using UnityEngine;
using Zenject;
using DG.Tweening;

public class Ball : MonoBehaviour
{
    private SignalBus _signalBus;
    private MainSetting _mainSetting;
    private World _world;
    private Platform _hitPlatform;
    private Border _hitBorder;
    private World _hitWorld;
    private Obstacle _hitObstacle;
    private ThematicSetting _thematicSetting;

    public Transform Transform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer Renderer { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus, Game game, MainSetting mainSetting, ThematicSetting thematicSetting,World world, Rigidbody2D rigidbody, SpriteRenderer renderer)
    {
        _signalBus = signalBus;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _world = world;
        Rigidbody = rigidbody;
        Transform = transform;
        Renderer = renderer;

        _signalBus.Subscribe<ThemeUpdated>(OnThemeUpdated);
    }

    private void FixedUpdate()
    {
        Rigidbody.AddForce(-_mainSetting.gravity * Rigidbody.mass * (Transform.position - _world.Transform.position).normalized);

        _hitBorder = null;
        _hitPlatform = null;
        _hitWorld = null;
        _hitObstacle = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _hitPlatform = collision.gameObject.GetComponent<Platform>();
        _hitBorder = collision.gameObject.GetComponentInParent<Border>();
        _hitWorld = collision.gameObject.GetComponent<World>();
        _hitObstacle = collision.gameObject.GetComponent<Obstacle>();

        if (null != _hitBorder)
        {
            _signalBus.Fire<BallHitBorder>();
        }
        else if (null != _hitObstacle)
        {
            _signalBus.Fire<BallHitObstacle>();
        }
        else if (null != _hitWorld)
        {
            _signalBus.Fire<BallHitCore>();
            Rigidbody.velocity = _mainSetting.VelocityAfterCollision * Vector2.up;
        }
        else if (null != _hitPlatform && collision.contacts.Any(c => c.point.y < Transform.position.y))
        {
            if (_hitPlatform.ColorChanger)
            {
                _hitPlatform.ShiftColor();
            }

            _hitPlatform.ShowOnBallCollisionFX();
            Rigidbody.velocity = _mainSetting.VelocityAfterCollision * Vector2.up;
        }
    }

    private void OnThemeUpdated(ThemeUpdated msg)
    {
        var palette = _thematicSetting.ChapterPalletes[msg.levelIndex / _mainSetting.levelsPerChapter];
        Renderer.DOColor(palette.ballColor, 0.5f);
    }
}