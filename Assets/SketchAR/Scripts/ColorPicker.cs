using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPicker : MonoBehaviour {

    public void SetColorBlue()
    {
        Drawer.Instance.sketch.SendMessage("SetColor", Color.blue);
    }
    public void SetColorWhite()
    {
        Drawer.Instance.sketch.SendMessage("SetColor", Color.white);
    }
    public void SetColorGreen()
    {
        Drawer.Instance.sketch.SendMessage("SetColor", Color.green);
    }
    public void SetColorRed()
    {
        Drawer.Instance.sketch.SendMessage("SetColor", Color.red);
    }
    public void SetColorYellow()
    {
        Drawer.Instance.sketch.SendMessage("SetColor", Color.yellow);
    }
}
