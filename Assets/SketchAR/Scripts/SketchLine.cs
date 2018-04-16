using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SketchLine : MonoBehaviour {

    private List<Vector3> points = new List<Vector3>();
    public LineRenderer LineRenderer;
    public Color Color
    {
        get
        {
            return LineRenderer.startColor;
        }
        set
        {
            LineRenderer.startColor = value;
            LineRenderer.endColor = value;
        }
    }

    private void Start()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }

    public void AddPoint(Vector3 point)
    {
        var localPos = transform.worldToLocalMatrix* point;
        points.Add(localPos);
        print(points.Count);
        LineRenderer.positionCount = points.Count;
        LineRenderer.SetPositions(points.ToArray());
    }
}
