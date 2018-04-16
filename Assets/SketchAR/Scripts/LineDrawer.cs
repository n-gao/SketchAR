using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using System;
#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

public class LineDrawer : MonoBehaviour
{
    /// <summary>
    /// Width of the line
    /// </summary>
    public float LineWidth = 0.02f;

    [SerializeField] private Color _color = Color.red;
    public Color Color
    {
        get { return _color; }
        set
        {
            Renderer.material.color = value;
            _color = value;
        }
    }
    private Mesh _mesh;

    [SerializeField] private List<Vector2> _points = new List<Vector2>();
    /// <summary>
    /// Points of the line.
    /// </summary>
    public List<Vector2> Points
    {
        get { return _points; }
        set { _points = value ?? new List<Vector2>(); }
    }

    private readonly List<Vector2> _pointsCpy = new List<Vector2>();

    public MeshRenderer Renderer;

    // Use this for initialization
    void Start()
    {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        Renderer = GetComponent<MeshRenderer>();
        Color = _color;
        StartThread();
    }

    void OnEnable()
    {
        if (Renderer == null)
            Renderer = GetComponent<MeshRenderer>();
        Renderer.enabled = true;
    }

    void OnDisable()
    {
        if (Renderer == null)
            Renderer = GetComponent<MeshRenderer>();
        Renderer.enabled = false;
    }

    private int _highest = 0;
    private Vector3[] _verticiesArr = new Vector3[0];
    private Vector2[] _uvArr = new Vector2[0];
    private int[] _triangleArr = new int[0];

    private bool _visible = false;
    private bool _locked = false;
    private bool _changed = false;
    private Vector3 _lossyScale = new Vector3(1, 1, 1);

#if WINDOWS_UWP
        private static CancellationTokenSource Token;
        
        private void StartThread()
        {
            Token = new CancellationTokenSource();
            Task.Factory.StartNew(GenerationLoop, Token.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
        }

        private void OnDestroy()
        {
            Token.Cancel();
        }
#else
    private Thread _thread;

    private void StartThread()
    {
        if (_thread == null)
        {
            _thread = new Thread(GenerationLoop);
            _thread.Start();
        }
    }

    void OnDestroy()
    {
        _thread.Abort();
    }
#endif

    void LateUpdate()
    {
        _lossyScale = transform.lossyScale;
        _visible = Renderer.isVisible;
        if (_visible)
            _waitHandle.Set();
    }

    private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

    private void GenerationLoop()
    {
        while (true)
        {
            GenerateMesh();
            _waitHandle.WaitOne();
        }
    }

    private int _triangleSize = 0;

    // Update is called once per frame
    void Update()
    {
        if (!_visible || _locked || !_changed) return;
        //Assigning to mesh
        if (_triangleArr.Length != _triangleSize)
        {
            _mesh.Clear();
            _triangleSize = _triangleArr.Length;
        }
        _mesh.vertices = _verticiesArr;
        _mesh.triangles = _triangleArr;
        _mesh.uv = _uvArr;
        _changed = false;
    }

    private void GenerateMesh()
    {
        _locked = true;

        _pointsCpy.Clear();
        lock (Points)
            _pointsCpy.AddRange(Points);

        Vector3 lossyScale = _lossyScale;

        float widthMultiplier = (lossyScale.x / lossyScale.y - 1);
        float upWidth = LineWidth * widthMultiplier + LineWidth;
        Vector3 upWidthVector = new Vector3(0, upWidth);

        int maxInd = _pointsCpy.Count - 1;

        if (_pointsCpy.Count > _highest)
        {
            _highest = _pointsCpy.Count;
            Array.Resize(ref _verticiesArr, _highest * 8);
            Array.Resize(ref _uvArr, _highest * 8);
            Array.Resize(ref _triangleArr, _highest * 18);
        }

        //Line which have equal points are not drawn. Therefore we need to create another offset to i.
        int drawnLines = 0;
        for (int i = 0; i < maxInd; i++)
        {
            int triangleOffset = 18 * drawnLines;
            int offset = 8 * drawnLines;
            Vector3 p1 = _pointsCpy[i];
            Vector3 p2 = _pointsCpy[i + 1];

            Vector3 dir = p2 - p1;

            if (dir == Vector3.zero) continue;

            //Rotation of line
            float angle = Mathf.Rad2Deg * Mathf.Atan(dir.x / dir.y);
            angle = (angle < 0 ? -90 : 90) - angle;

            //Line width
            float lineWidth = LineWidth * Mathf.Cos(Mathf.Deg2Rad * angle) * widthMultiplier + LineWidth;
            Vector3 scaledDir = dir;
            scaledDir.y *= lossyScale.y / lossyScale.x;
            Vector3 normal = new Vector3(scaledDir.y, -scaledDir.x).normalized;
            Vector3 widthVector = normal * lineWidth;


            //Line coordinates
            _verticiesArr[offset + 0] = p1 + widthVector;
            _verticiesArr[offset + 1] = p1 - widthVector;
            _verticiesArr[offset + 2] = p2 + widthVector;
            _verticiesArr[offset + 3] = p2 - widthVector;

            _uvArr[offset + 0] = new Vector2(0, 0);
            _uvArr[offset + 1] = new Vector2(1, 0);
            _uvArr[offset + 2] = new Vector2(1, 1);
            _uvArr[offset + 3] = new Vector2(0, 1);

            //Line triangles
            //In order to increase performance we do not correctly sort those triangles
            //Culling should be turned off
            _triangleArr[triangleOffset + 0] = offset + 0;
            _triangleArr[triangleOffset + 1] = offset + 1;
            _triangleArr[triangleOffset + 2] = offset + 2;

            _triangleArr[triangleOffset + 3] = offset + 1;
            _triangleArr[triangleOffset + 4] = offset + 2;
            _triangleArr[triangleOffset + 5] = offset + 3;

            //Filling the room between two lines
            _verticiesArr[offset + 4] = p1 + upWidthVector;
            _verticiesArr[offset + 5] = p1 - upWidthVector;
            _verticiesArr[offset + 6] = p2 + upWidthVector;
            _verticiesArr[offset + 7] = p2 - upWidthVector;

            _uvArr[offset + 4] = new Vector2(0, 0);
            _uvArr[offset + 5] = new Vector2(1, 0);
            _uvArr[offset + 6] = new Vector2(1, 1);
            _uvArr[offset + 7] = new Vector2(0, 1);

            //0 lineLeftTop
            //1 lineLeftBottom
            //2 lineRightTop
            //3 lineRightBottom
            //4 centerLeftTop
            //5 centerLeftBottom
            //6 centerRightTop
            //7 centerRightBottom
            //In order to increase performance we do not correctly sort those triangles,
            //instead this line should use a shader without backculling
            _triangleArr[triangleOffset + 6] = offset + 0;
            _triangleArr[triangleOffset + 7] = offset + 4;
            _triangleArr[triangleOffset + 8] = offset + 5;

            _triangleArr[triangleOffset + 9] = offset + 1;
            _triangleArr[triangleOffset + 10] = offset + 4;
            _triangleArr[triangleOffset + 11] = offset + 5;

            _triangleArr[triangleOffset + 12] = offset + 2;
            _triangleArr[triangleOffset + 13] = offset + 6;
            _triangleArr[triangleOffset + 14] = offset + 7;

            _triangleArr[triangleOffset + 15] = offset + 3;
            _triangleArr[triangleOffset + 16] = offset + 6;
            _triangleArr[triangleOffset + 17] = offset + 7;

            drawnLines++;
        }
        //Filling all unused values
        _verticiesArr.Fill(new Vector3(), drawnLines * 8 + 1);
        _uvArr.Fill(new Vector2(), drawnLines * 8 + 1);
        _triangleArr.Fill(0, drawnLines * 18 + 1);

        _locked = false;
        _changed = true;
    }

    public void AddPoint(Vector3 position)
    {
        _points.Add(position);
    }
}
