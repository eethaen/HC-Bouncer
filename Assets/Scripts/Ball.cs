using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class Ball : MonoBehaviour
{
    private SignalBus _signalBus;
    private MainSetting _mainSetting;
    private World _world;
    private CircleCollider2D _collider;
    private Platform _hitPlatform;
    private Border _hitBorder;
    private World _hitWorld;
    private Obstacle _hitObstacle;
    private ThematicSetting _thematicSetting;
    private List<RaycastHit2D> _results;
    private ContactFilter2D _filter;

    public Transform Transform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer Renderer { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus, MainSetting mainSetting, ThematicSetting thematicSetting, World world, Rigidbody2D rigidbody, CircleCollider2D collider, SpriteRenderer renderer)
    {
        _signalBus = signalBus;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _world = world;
        _collider = collider;

        Rigidbody = rigidbody;
        Transform = transform;
        Renderer = renderer;

        _results = new List<RaycastHit2D>();
        _filter = new ContactFilter2D();
        _filter.NoFilter();

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _hitPlatform = collision.GetComponent<Platform>();
        _hitBorder = collision.GetComponentInParent<Border>();
        _hitWorld = collision.GetComponent<World>();
        _hitObstacle = collision.GetComponent<Obstacle>();

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
        else if (null != _hitPlatform && Physics2D.Raycast(Transform.position, Vector2.down, _filter, _results, _collider.radius * 1.5f) > 1)
        {
            if (!_results.Any(c => c.point.y <= Transform.position.y && c.collider == collision))
            {
                for (int i = 0; i < _results.Count; i++)
                {
                    CustomDebug.Log($"_results[{i}].point {_results[i].point.y}");
                }
                
                return;
            }

            //Debug.Break();

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