using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
public class EndImage : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera vcam;
    CinemachineTrackedDolly dolly;

    [SerializeField]
    float fadeInPos,fadeOutPos, speed;

    bool initColor;

    [SerializeField] Image image;
    // Start is called before the first frame update
    void Start()
    {
        dolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        initColor = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(dolly.m_PathPosition >= fadeInPos && !initColor)
        {
            if (image.color.a < 1)
                image.color += new Color(0, 0, 0, 1) * speed * Time.deltaTime;
            else            {
                image.color = new Color(1, 1, 1, 1);
                initColor = true;
            }
        }

        if (dolly.m_PathPosition >= fadeOutPos)
                image.color -= new Color(0, 0, 0, 1) * speed * Time.deltaTime;
    }
}
