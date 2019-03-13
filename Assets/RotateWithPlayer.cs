using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RotateWithPlayer : MonoBehaviour
{
    public Transform target;

    private CinemachineFreeLook freeLook;
    // Start is called before the first frame update
    private void Start()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = target.forward;
    }
}
