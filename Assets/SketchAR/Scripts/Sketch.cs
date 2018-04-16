using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sketch : MonoBehaviour {

    public GameObject LinePrefab;
    private List<SketchLine> lines = new List<SketchLine>();
    private SketchLine currentLine;
    private Color _color;
    public Color CurrentColor
    {
        get { return _color; }
        set
        {
            _color = value;
            currentLine.Color = _color;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NewLine()
    {
        print("Created new line.");
        var lineObj = Instantiate(LinePrefab, transform);
        lineObj.transform.localPosition = Vector3.zero;
        currentLine = lineObj.GetComponent<SketchLine>();
        currentLine.Color = CurrentColor;
        lines.Add(currentLine);
    }

    public void AddPoint(Vector3 point)
    {
        if (currentLine != null)
            currentLine.AddPoint(point);
    }
}
