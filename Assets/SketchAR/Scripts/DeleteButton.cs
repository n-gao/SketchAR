using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DeleteButton : MonoBehaviour, IInputHandler
{

    public void OnInputDown(InputEventData eventData)
    {
        Drawer.Instance.EndSketch();
    }

    public void OnInputUp(InputEventData eventData)
    {
    }
}
