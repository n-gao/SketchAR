using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class EditButton : MonoBehaviour, IInputHandler
{

    public void OnInputDown(InputEventData eventData)
    {
        Drawer.Instance.EditSketch(transform.parent.gameObject);
    }

    public void OnInputUp(InputEventData eventData)
    {
    }
}
