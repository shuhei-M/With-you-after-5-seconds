using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Stage01LookAtPlayerCameraController : MonoBehaviour
{
    CinemachineVirtualCamera vcam;
    CinemachineTrackedDolly dolly;
    bool increasingPathPos, endPathPos;
    
    [SerializeField] float increaseSpeed = 1;
    [SerializeField] float pathSize = 6;
    // Start is called before the first frame update
    void Start()
    {
        increasingPathPos = false;
        endPathPos = false;
        vcam = this.GetComponent<CinemachineVirtualCamera>();
        dolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    // Update is called once per frame
    void Update()
    {
        if(increasingPathPos)
        {
            dolly.m_PathPosition+= increaseSpeed * Time.deltaTime;
            if (dolly.m_PathPosition >= pathSize)
                endPathPos = true;
        }
    }

    public bool EndPathPos() { return endPathPos; }
    public void LookAtPlayerCameraStart()
    {
        vcam.Priority = 100;
        increasingPathPos = true;
    }
}
