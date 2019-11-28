using UnityEngine;
using Zenject;

public class Ball : MonoBehaviour
{
    private SignalBus _signalBus;
    private Game _game;
    private float _angle;
    private Platform _hitPlatform;
    private Border _hitBorder;
    private Level _hitCore;
    private EndPoint _hitEndPoint;

    public Transform Transform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer Renderer { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus, Game game, Rigidbody2D rigidbody, SpriteRenderer renderer)
    {
        _signalBus = signalBus;
        _game = game;
        Rigidbody = rigidbody;
        Transform = transform;
        Renderer = renderer;
    }

    private void FixedUpdate()
    {
        //_angle = Vector2.SignedAngle(Vector2.right, Transform.position);
        //Transform.Rotate(Vector3.forward, );
        //Rigidbody.rotation= _angle - 90.0f;
        Rigidbody.AddForce(-5.0f * Transform.position.normalized);

        //for (var i = 0; i < _dots.Length; i++)
        //{
        //    var position = (_dots[i].Transform.position - Transform.position);
        //    var r = position.magnitude;

        //    if (r > 0.2f)
        //    {
        //        //continue;
        //    }

        //    var direction = position.normalized;
        //    var force = Mathf.Clamp(constant /** _bouncyDot.Rigidbody.mass * _dots[i].Rigidbody.mass*/ / (r * r), 0.0f, 2.5f);
        //    CustomDebug.Log($"force {force}");
        //    Rigidbody.AddForce(force * direction);
        //}

        _hitBorder = null;
        _hitPlatform = null;
        _hitCore = null;
        _hitEndPoint = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _hitPlatform = collision.gameObject.GetComponent<Platform>();
        _hitBorder = collision.gameObject.GetComponentInParent<Border>();
        _hitCore = collision.gameObject.GetComponent<Level>();

        if (null != _hitBorder)
        {
            CustomDebug.Log("Ball hit border");
            _signalBus.Fire<BallHitBorder>();
            return;
        }

        if (null != _hitCore)
        {
            _signalBus.Fire(new BallHitCore { segment = Utility.DetermineSegmentByPosition(_game.Level, Transform.localPosition) });
            Rigidbody.velocity = 5.0f * Vector2.up;
        }

        if (null != _hitPlatform && Transform.position.y > collision.GetContact(0).point.y)
        {
            Rigidbody.velocity = 5.0f * Vector2.up;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _hitEndPoint = collision.GetComponent<EndPoint>();

        if (null != _hitEndPoint)
        {
            CustomDebug.Log($"Level passed");
            _signalBus.Fire<LevelPassed>();
        }
    }
}
