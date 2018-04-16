using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterface : MonoBehaviour {
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
    
    // Use this for initialization
    void Start()
    {
        if (ToFollow == null)
        {
            ToFollow = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = ToFollow.position + Vector3.ProjectOnPlane(ToFollow.forward, Vector3.up) * Distance;
        targetPos.y += YOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);
    }
}
