using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public float pushingtime = 1f;
    public bool isPushing;

    bool action;
    bool playSound;
    bool isTimeCounting;
    float time;

    GameObject buttonSwitch, pushedButtonSwitch, canPushArea;
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        isPushing = false;
        isTimeCounting = false;
        playSound = false;
        action = false;
        buttonSwitch = transform.GetChild(0).gameObject;
        pushedButtonSwitch = transform.GetChild(1).gameObject;

        //スイッチを押す音
        Sound.LoadSE("Switch", "Switch");
    }

    // Update is called once per frame
    void Update()
    {
        if (action)
        {
            isTimeCounting = true;
        }
        else
        {
            pushedButtonSwitch.SetActive(false);
            buttonSwitch.SetActive(true);
        }
        if (isTimeCounting)
            time += Time.deltaTime;

        if (time >= 0.03f && !playSound)
        {
            playSound = true;
            Sound.PlaySE("Switch", 2);
            PushButton();
        }
        else if (time >= pushingtime)
        {
            isTimeCounting = false;
            isPushing = false;
            playSound = false;
            action = false;
            time = 0;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            var playerState = other.GetComponent<PlayerBehaviour>().State;
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Button;
            //if (playGimmick != null) playGimmick.Set_ActionType(ActionType.Torch);   // デバッグ用消してよし
            if (playerState == PlayerState.Action)
            {
                action = true;
                time = 0;
            }
        }

        if (other.tag == "Afterimage")
        {
            if (other.GetComponent<AfterimageBehaviour>().AfterimageState == PlayerState.Action)
            {
                action = true;
                time = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Default;
        }
    }

    void PushButton()
    {            
        isPushing = true;
        pushedButtonSwitch.SetActive(true);
        buttonSwitch.SetActive(false);
        isTimeCounting = true;
    }
}
