using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;

public class ComicSubTitle : MonoBehaviour
{
    [System.Serializable]
    class SubTitle
    {
        [SerializeField]
        [Header("表示する文")]
        string text;
        [SerializeField]
        [Header("表示し始めるPathPos")]
        float startTime;
        [SerializeField]
        [Header("表示を終えるPathPos")]
        float endTime;

        public string Text() { return text; }
        public float StartTime() { return startTime; }
        public float EndTime() { return endTime; }
    }

    [SerializeField] SubTitle[] subTitles;
    [SerializeField] float speed = 1f;
    [SerializeField] CinemachineVirtualCamera vcam;
    CinemachineTrackedDolly dolly;

    bool printing;
    int subTitleNum;

    TextMeshProUGUI subTitleObj;

    // Start is called before the first frame update
    void Start()
    {
        dolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        subTitleObj = GameObject.Find("SubTitle").GetComponent<TextMeshProUGUI>();
        subTitleNum = 0;
        printing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (dolly.m_PathPosition >= subTitles[subTitleNum].StartTime() && !printing)
        {
            printing = true;
            subTitleObj.text = subTitles[subTitleNum].Text();
        }

        if (dolly.m_PathPosition >= subTitles[subTitleNum].EndTime())
        {
            printing = false;
            if(subTitleNum < subTitles.Length - 1)
                subTitleNum++;
        }


        if (printing && subTitleObj.color.a <= 1.1f)
            subTitleObj.color += new Color(0, 0, 0, speed) * Time.deltaTime;
        else if (!printing && subTitleObj.color.a >= -0.1f)
            subTitleObj.color -= new Color(0, 0, 0, speed) * Time.deltaTime;
    }
}
