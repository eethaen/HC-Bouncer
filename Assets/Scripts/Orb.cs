using DG.Tweening;
using UnityEngine;
using Zenject;

public class Orb : MonoBehaviour
{
    public Transform Transform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    private SignalBus _signalBus;
    private Ball _ball;
    private Tween _tween;

    [Inject]
    public void Construct(SignalBus signalBus, Rigidbody2D rigidbody)
    {
        _signalBus = signalBus;
        Rigidbody = rigidbody;
        Transform = transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CustomDebug.Log($"{name} Collider triggered");

        _ball = collision.GetComponent<Ball>();

        if (null != _ball)
        {
            if (null != _tween && _tween.IsPlaying())
            {
                return;
            }

            var targetPos = Transform.localPosition + Transform.InverseTransformDirection(_ball.Transform.up);
            _tween = Transform.DOLocalMove(targetPos, 2.0f, false)
                              .SetEase(Ease.InOutExpo);

            _signalBus.Fire(new BallHitOrb() { orb = this });
        }

        _ball = null;
    }
}
