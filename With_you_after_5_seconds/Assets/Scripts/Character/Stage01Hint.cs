using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Stage01Hint : MonoBehaviour
{
    Animator animator;
    bool isStartHint, canUseHint;
    float timeCount;
    CinemachineDollyCart dolly;
    [SerializeField] GameObject hintPlayer;
    [SerializeField] float speed = 1;

    GameObject hintArea;
    HintGimmick hint;
    // Start is called before the first frame update
    void Start()
    {
        hintArea = GameObject.Find("HintArea");
        hint = hintArea.GetComponent<HintGimmick>();

        animator = hintPlayer.GetComponent<Animator>();
        hintPlayer.SetActive(false);
        isStartHint = false;
        canUseHint = false;
        timeCount = 0;

        dolly = hintPlayer.GetComponent<CinemachineDollyCart>();
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;
        animator.SetFloat("DeltaTime", 1f);
        animator.SetFloat("MoveBlend", 1f);

        if(hint.State == HintGimmick.StateEnum.Useable)
        {
            canUseHint = true;  
        }

        if (Input.GetButtonDown("Hint") && canUseHint)
        {
            isStartHint = true;
            hintPlayer.SetActive(true);
            timeCount = 0;
            dolly.m_Position = 0;
        }

        if (isStartHint)
        {
            dolly.m_Position += speed * Time.deltaTime;
            if (timeCount >= 5f)
            {
                hintPlayer.SetActive(false);
                isStartHint = false;
            }
        }
    }
}
