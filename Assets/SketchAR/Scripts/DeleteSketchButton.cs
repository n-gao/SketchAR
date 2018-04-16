using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DeleteSketchButton : MonoBehaviour, IInputHandler
{

    public void OnInputDown(InputEventData eventData)
    {
        Destroy(transform.parent.gameObject);
    }

    public void OnInputUp(InputEventData eventData)
    {
    }
}
