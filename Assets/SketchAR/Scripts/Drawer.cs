using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class Drawer : MonoBehaviour {

    public GameObject SketchPrefab;
    public float MinDistance = 0.05f;
    public Vector3 lastPoint = Vector3.zero;

    private uint sourceId;

    private GameObject sketch;

	// Use this for initialization
	void Start ()
    {
        InteractionManager.InteractionSourcePressed += InteractionManagerOnSourcePressed;
        InteractionManager.InteractionSourceReleased += InteractionManagerOnSourceReleased;
        InteractionManager.InteractionSourceUpdated += InteractionManagerOnSourceUpdated;
        StartSketch();
    }

    private void InteractionManagerOnSourcePressed(InteractionSourcePressedEventArgs args)
    {
        print("Pressed");
        if (args.state.source.kind == InteractionSourceKind.Hand)
        {
            sourceId = args.state.source.id;
            if (sketch != null)
            {
                sketch.SendMessage("NewLine");
            }
        }
    }

    private void InteractionManagerOnSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        print("Released");
        if (args.state.source.id == sourceId)
        {
            sourceId = 0;
        }
    }
    
    private void InteractionManagerOnSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        if (args.state.source.id != sourceId) return;
        if (sketch == null) return;
        Vector3 newPos;
        if (!args.state.sourcePose.TryGetPosition(out newPos)) return;
        SendAddPoint(newPos);
    }

    private void SendAddPoint(Vector3 point)
    {
        if ((point - lastPoint).magnitude > MinDistance)
        {
            sketch.SendMessage("AddPoint", point);
            lastPoint = point;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sketch.SendMessage("NewLine");
        }
        if (Input.GetKey(KeyCode.Space))
        {
            SendAddPoint(transform.position + transform.forward);
        }
    }

    void StartSketch()
    {
        EndSketch();
        sketch = Instantiate(SketchPrefab);
    }

    void EndSketch()
    {
        sketch = null;
    }
}
