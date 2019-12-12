using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ButtonHandler : MonoBehaviour
{
    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void ResetGame()
    {
        _signalBus.Fire<ResetGame>();
    }
}
