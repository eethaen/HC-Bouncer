// Author: Eric Hodgson 2017

// NOTE: This script should be placed on the parent of the object that's moving.
//  It should be drawn in the parent's local space, and move/rotate with the parent.
//  If the trail script is on the moving object, the entire trail will move and
//   rotate with the object itself, instead of leaving a trail behind in it's
//   parent's coordinate system.

using UnityEngine;
using Zenject;

public class Trail : MonoBehaviour
{
    public float threshod = 0.1f;      // How far should the object move before leaving a new trail point
    public bool limitTrailLength = false;   // Toggle this to make trail be a finite number of segments
    public int maxCount = 10;           // Set the number of segments here

    private Game _game;
    private SignalBus _signalBus;
    private World _world;
    private Transform _transform;
    private Transform _parent;
    private Transform _objToFollow;   //  Object that is leaving the trail
    private LineRenderer _renderer;    //  internal pointer to LineRenderer Component
    private Vector3 _lastPosition;     // internal log of last trail point... could also use myLine.GetPosition(myLine.numPositions)
    private Vector3[] _tempArray;
    private Vector3 _position;

    [Inject]
    public void Construct(Game game, SignalBus signalBus, World world,LineRenderer renderer)
    {
        _game = game;
        _signalBus = signalBus;
        _world = world;
        _renderer = renderer;
        _transform = transform;
    }

    void Start()
    {
        // ....and make sure it's set to use local space
        _renderer.useWorldSpace = false;
        // Reset the trail
        Reset();
    }

    private void OnEnable()
    {
        _objToFollow = _game.Ball.Transform;
        _parent = _world.Transform;
        _transform.SetParent(_parent);

        Reset();
    }

    private void OnDisable()
    {
        Reset();
    }

    public void Reset()
    {
        // Wipe out any old positions in the LineRenderer
        _renderer.positionCount = 0;
        // Then set the first position to our object's current local position
        //AddPoint(_objToFollow.localPosition);
    }

    // Add a new point to the line renderer on demand
    void AddPoint(Vector3 newPosition)
    {
        // Increase the number of positions to render by 1
        _renderer.positionCount++;
        // Set the new, last item in the Vector3 list to our new point
        _renderer.SetPosition(_renderer.positionCount - 1, newPosition);

        // Check to see if the list is too long
        if (limitTrailLength && _renderer.positionCount > maxCount)
        {
            // ...and discard old positions if necessary
            TruncatePositions(maxCount);
        }

        // Log this position as the last one logged
        _lastPosition = newPosition;
    }


    // Shorten position list to the desired amount, discarding old values
    void TruncatePositions(int newCount)
    {
        // Create a temporary list of the desired length
        _tempArray = new Vector3[newCount];
        // Calculate how many extra items will need to be cut out from the original list
        int extraCount = _renderer.positionCount - newCount;
        // Loop through original list and add newest X items to temp list
        for (var i = 0; i < newCount; i++)
        {
            // shift index by nExtraItems... e.g., if 2 extras, start at index 2 instead of index 0
            _tempArray[i] = _renderer.GetPosition(i + extraCount);
        }

        // Set the LineRenderer's position list length to the appropriate amount
        _renderer.positionCount = newCount;
        // ...and use our tempList to fill it's positions appropriately
        _renderer.SetPositions(_tempArray);
    }

    void Update()
    {
        // Get the current position of the object in local space
         _position = _parent.InverseTransformPoint(_objToFollow.position);
       
        // Check to see if object has moved far enough
        if (Vector3.Distance(_position, _lastPosition) > threshod)
        {
            // ..and add the point to the trail if so
            AddPoint(_position);
        }
    }
}
