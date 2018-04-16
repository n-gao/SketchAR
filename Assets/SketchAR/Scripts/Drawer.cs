using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class Drawer : MonoBehaviour {

    public static Drawer Instance { get; private set; }

    public GameObject SketchPrefab;
    public float MinDistance = 0.05f;
    public Vector3 lastPoint = Vector3.zero;

    private uint sourceId;

    private GameObject _sketch;
    private GameObject sketch
    {
        get
        {
            return _sketch;
        }
        set
        {
            if (_sketch != null)
            {
                _sketch.SendMessage("EditStatusChanged", false);
            }
            if (value != null)
            {
                value.SendMessage("EditStatusChanged", true);
            }
            CursorWrapper.Cursor.SetActive(value == null);
            _sketch = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        InteractionManager.InteractionSourcePressed += InteractionManagerOnSourcePressed;
        InteractionManager.InteractionSourceReleased += InteractionManagerOnSourceReleased;
        InteractionManager.InteractionSourceUpdated += InteractionManagerOnSourceUpdated;
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

    public void EditSketch(GameObject sketch)
    {
        EndSketch();
        this.sketch = sketch;
    }

    public void StartSketch()
    {
        EndSketch();
        sketch = Instantiate(SketchPrefab);
        sketch.transform.position = transform.position + transform.forward * 1f;
    }

    public void EndSketch()
    {
        sketch = null;
    }
}
