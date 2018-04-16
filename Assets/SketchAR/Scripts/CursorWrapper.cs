using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorWrapper : MonoBehaviour {

    public static GameObject Cursor { get; set; }

	// Use this for initialization
	void Start () {
        Cursor = gameObject;
	}
}
