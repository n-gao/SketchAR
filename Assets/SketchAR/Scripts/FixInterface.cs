using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixInterface : MonoBehaviour, IInputHandler {

    public void OnInputDown(InputEventData eventData)
    {
        var ui = transform.GetComponentInParent<UserInterface>();
        ui.Open = !ui.Open;
    }

    public void OnInputUp(InputEventData eventData)
    {
    }
}
