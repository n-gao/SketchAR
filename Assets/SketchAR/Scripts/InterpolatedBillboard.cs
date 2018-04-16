using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// Adding this component to GameObject forces it to behave like a billboard,
/// but instead of instantly adnusting its rotation it will slowly rotate 
/// towards that target rotation.
/// </summary>
public class InterpolatedBillboard : MonoBehaviour
{
    /// <summary>
    /// Use this to override the point the billboard rotate towards.
    /// </summary>
    public static Vector3? OverrideCenter;
    /// <summary>
    /// Velocity of the rotation.
    /// </summary>
    public float Velocity = 10;

    private Transform _cameraTransform;

    void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        Vector3 target = OverrideCenter ?? _cameraTransform.position;
        // Get a Vector that points from the target to the main camera.
        Vector3 directionToTarget = target - transform.position;

        // If we are right next to the camera the rotation is undefined. 
        if (directionToTarget == transform.forward || directionToTarget.sqrMagnitude < 0.001f)
            return;

        // Calculate and apply the rotation required to reorient the object
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-directionToTarget), Time.deltaTime * Velocity);
    }
}
