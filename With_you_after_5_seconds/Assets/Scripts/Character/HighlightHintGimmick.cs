using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightHintGimmick : MonoBehaviour
{
    GameObject hintArea;
    HintGimmick hint;

    bool canUseHint, isStartHint;
    float timeCount;

    [SerializeField] Material highlightMat;
    [SerializeField] GameObject[] highlightGameObjects;

    Renderer[] renderers;
    Material[] materials;
    // Start is called before the first frame update
    void Start()
    {
        hintArea = GameObject.Find("HintArea");
        hint = hintArea.GetComponent<HintGimmick>();
        materials = new Material[highlightGameObjects.Length];
        renderers = new Renderer[highlightGameObjects.Length];

        canUseHint = false;
        timeCount = 0;

        for(int i = 0; i < highlightGameObjects.Length; i++)
        {
            materials[i] = highlightGameObjects[i].GetComponent<Renderer>().material;
            renderers[i] = highlightGameObjects[i].GetComponent<Renderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;

        if (hint.State == HintGimmick.StateEnum.Useable)
        {
            canUseHint = true;
        }

        if(canUseHint && Input.GetButtonDown("Hint"))
        {
            isStartHint = true;
            timeCount = 0;
            
            for(int i = 0; i < highlightGameObjects.Length; i++)
            {
                renderers[i].material = highlightMat;
            }
        }

        if (isStartHint)
        {           
            if (timeCount >= 5f)
            {
                isStartHint = false;
                for (int i = 0; i < highlightGameObjects.Length; i++)
                {
                    renderers[i].material = materials[i];
                }
            }
        }
    }
}
