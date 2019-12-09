using UnityEngine;
using Zenject;

public class Background : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private int _colorAID;
    private int _colorBID;

    [Inject]
    public void Construct(SpriteRenderer renderer)
    {
        _renderer = renderer;

        _colorAID = Shader.PropertyToID("_ColorA");
        _colorBID = Shader.PropertyToID("_ColorB");
    }

    public void SetColor(Color colorA, Color colorB)
    {
        _renderer.material.SetColor(_colorAID, colorA);
        _renderer.material.SetColor(_colorBID, colorB);
    }

}
