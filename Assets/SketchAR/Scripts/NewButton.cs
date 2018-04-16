using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class NewButton : MonoBehaviour, IInputHandler {

    public void OnInputDown(InputEventData eventData)
    {
        Drawer.Instance.StartSketch();
    }

    public void OnInputUp(InputEventData eventData)
    {
    }
}
