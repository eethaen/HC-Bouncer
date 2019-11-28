using UnityEngine;
using Zenject;

public class World : MonoBehaviour
{
    public Transform Transform { get; private set; }

    [Inject]
    public void Construct()
    {
        Transform = transform;
    }
}
