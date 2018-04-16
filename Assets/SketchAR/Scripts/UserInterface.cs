using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class UserInterface : MonoBehaviour
{        
    /// <summary>
    /// Indicates whether the object which is being followed has been overwritten.
    /// </summary>
    private bool IsTransformOverwritten { get { return _overrideTransform != null; } }
    /// <summary>
    /// Use this transform to override the transform which this menu follows.
    /// </summary>
    private Transform _overrideTransform = null;
    /// <summary>
    /// The transform which the menu follows.
    /// Default value is the main camera.
    /// </summary>
    public Transform ToFollow;
    /// <summary>
    /// The distance to the followed object.
    /// </summary>
    public float Distance;
    /// <summary>
    /// An offset in y-Direction which will be applied to the menu.
    /// </summary>
    public float YOffset = -.5f;
    /// <summary>
    /// The angle width of a coloumn of the menu.
    /// </summary>
    public float ColumnAngleWidth = 15;
    /// <summary>
    /// Indicates whether the menu is open.
    /// </summary>
    private bool _open = false;
    public bool Open
    {
        get
        {
            return _open;
        }
        set
        {
            _open = value;
            if (_open)
            {
                LockMenuRotation();
            }
            else
            {
                UnlockMenuRotation();
            }
        }
    }
    /// <summary>
    /// The transform which the menu follows.
    /// </summary>
    public Transform FollowedTransform
    {
        get { return IsTransformOverwritten ? _overrideTransform : Camera.main.transform; }
    }

    private bool _lastState = false;

    // Use this for initialization
    void Start()
    {
        if (ToFollow == null)
        {
            ToFollow = Camera.main.transform;
        }
        _lastState = Open;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Open)
        {
            Vector3 targetPos = ToFollow.position + Vector3.ProjectOnPlane(ToFollow.forward, Vector3.up) * Distance;
            targetPos.y += YOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Unlocks the menu so it continues to follow.
    /// </summary>
    private void UnlockMenuRotation()
    {
        InterpolatedBillboard.OverrideCenter = null;
        Destroy(_overrideTransform.gameObject);
        _overrideTransform = null;
        Destroy(GetComponent<WorldAnchor>());
    }

    /// <summary>
    /// Locks the menu at its place.
    /// </summary>
    private void LockMenuRotation()
    {
        InterpolatedBillboard.OverrideCenter = Camera.main.transform.position;
        _overrideTransform = new GameObject("MenuCameraPosition").transform;
        _overrideTransform.position = Camera.main.transform.position;
        _overrideTransform.rotation = Camera.main.transform.rotation;
        _overrideTransform.gameObject.AddComponent<WorldAnchor>();
        gameObject.AddComponent<WorldAnchor>();
    }
}
