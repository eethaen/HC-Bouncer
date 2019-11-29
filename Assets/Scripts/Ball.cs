using System.Linq;
using UnityEngine;
using Zenject;

public class Ball : MonoBehaviour
{
    private SignalBus _signalBus;
    private Game _game;
    private World _world;
    private Platform _hitPlatform;
    private Border _hitBorder;
    private Level _hitCore;
    private EndPoint _hitEndPoint;
    private Orb _hitOrb;

    public Transform Transform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer Renderer { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus, Game game, World world, Rigidbody2D rigidbody, SpriteRenderer renderer)
    {
        _signalBus = signalBus;
        _game = game;
        _world = world;
        Rigidbody = rigidbody;
        Transform = transform;
        Renderer = renderer;
    }

    private void FixedUpdate()
    {
        Rigidbody.AddForce(-5.0f * (Transform.position - _world.Transform.position).normalized);

        _hitBorder = null;
        _hitPlatform = null;
        _hitCore = null;
        _hitEndPoint = null;
        _hitOrb = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _hitPlatform = collision.gameObject.GetComponent<Platform>();
        _hitBorder = collision.gameObject.GetComponentInParent<Border>();
        _hitCore = collision.gameObject.GetComponent<Level>();

        if (null != _hitBorder)
        {
            _signalBus.Fire<BallHitBorder>();
        }
        else if (null != _hitCore)
        {
            _signalBus.Fire(new BallHitCore { segment = Utility.DetermineSegmentByPosition(_game.Level, Transform.localPosition) });
            Rigidbody.velocity = 5.0f * Vector2.up;
        }
        else if (null != _hitPlatform && collision.contacts.Any(c => c.point.y < Transform.position.y))
        {
            _hitPlatform.ShowOnBallCollisionFX();
            Rigidbody.velocity = 5.0f * Vector2.up;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _hitEndPoint = collision.GetComponent<EndPoint>();
        _hitOrb = collision.GetComponent<Orb>();

        if (null != _hitEndPoint)
        {
            CustomDebug.Log($"Level passed");
            _signalBus.Fire<LevelPassed>();
        }
        else if (null != _hitOrb)
        {
            _signalBus.Fire<BallHitOrb>();
        }
    }
}
