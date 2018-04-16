using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class HandParticleManager : MonoBehaviour {

    public GameObject HandParticlesPrefab;
    private Dictionary<uint, GameObject> handParticles = new Dictionary<uint, GameObject>();

    void Start()
    {
        InteractionManager.InteractionSourceLost += InteractionManagerOnSourceLost;
        InteractionManager.InteractionSourceUpdated += InteractionManagerOnSourceUpdated;
        InteractionManager.InteractionSourceDetected += InteractionManagerOnSourceDetected;
    }

    private void InteractionManagerOnSourceLost(InteractionSourceLostEventArgs args)
    {
        uint id = args.state.source.id;
        if (handParticles.ContainsKey(id))
        {
            handParticles[id].SetActive(false);
        }
    }

    private void InteractionManagerOnSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        uint id = args.state.source.id;
        if (handParticles.ContainsKey(id))
        {
            Vector3 pos;
            if (args.state.sourcePose.TryGetPosition(out pos))
                handParticles[id].transform.position = pos;
        }
    }

    private void InteractionManagerOnSourceDetected(InteractionSourceDetectedEventArgs args)
    {
        uint id = args.state.source.id;
        if (handParticles.ContainsKey(id))
        {
            handParticles[id].SetActive(true);
        } else
        {
            handParticles[id] = Instantiate(HandParticlesPrefab);
        }
    }
}
