using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{

    [SerializeField] SelectGroundType.GroundType groundType;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float volumeWalk = 0.5f;
    [SerializeField] float volumeWalkWater = 0.05f;
    [SerializeField] float volumeWalkRuin = 0.2f;
    [SerializeField] float volumePushObj = 0.5f;
    [SerializeField] float volumeSwitch = 0.5f;
    [SerializeField] float volumeHint = 0.5f;
    [SerializeField] float footStepInterval = 0.5f;

    IPlayGimmick playGimmick;
    HintGimmick hint;
    GameObject hintArea;
    GameObject player;

    const int walkChannel = 0;
    const int pushAndPullChannel = 1;
    const int hintChannel = 2;

    float stopingTimeCounter;
    float footStepTimeCounter;

    bool isPlayedHint;
    // Start is called before the first frame update
    void Start()
    {
        hintArea = GameObject.Find("HintArea");
        if(hintArea != null)
            hint = hintArea.GetComponent<HintGimmick>();

        player = GameObject.Find("Player");
        playGimmick = player.GetComponent<IPlayGimmick>();

        stopingTimeCounter = 0;
        footStepTimeCounter = 0f;

        isPlayedHint = false;

        LoadSounds();
    }

    // Update is called once per frame
    void Update()
    {
        //ヒントの音
        if (hintArea != null)
            if (hint.State == HintGimmick.StateEnum.Useable && !isPlayedHint)
            {
                Sound.PlaySE("Hint", hintChannel);
                isPlayedHint = true;
            }

        //地面の情報更新
        if (groundType != CheckGroundType(player.transform.position))
        {
            groundType = CheckGroundType(player.transform.position);
            Sound.StopSE(walkChannel);
        }

        //あるく
        if (playGimmick.Get_PlayerState() == PlayerState.Move)
        {
            if (!Sound.IsPlayingSE(walkChannel) && footStepTimeCounter >= footStepInterval)
            {
                switch (groundType)
                {
                    case SelectGroundType.GroundType.@default:
                        Sound.PlaySE("PlayerFootStep_Default", volumeWalk, walkChannel);
                        break;
                    case SelectGroundType.GroundType.stone:
                        Sound.PlaySE("PlayerFootStep_Stone", volumeWalk, walkChannel);
                        break;
                    case SelectGroundType.GroundType.water:
                        Sound.PlaySE("PlayerFootStep_Water", volumeWalkWater, walkChannel);
                        break;
                    case SelectGroundType.GroundType.wood:
                        Sound.PlaySE("PlayerFootStep_Wood", volumeWalk, walkChannel);
                        break;
                    case SelectGroundType.GroundType.ruinDefault:
                        Sound.PlaySE("PlayerFootStep_InsideRuin_Default", volumeWalkRuin, walkChannel);
                        break;
                    case SelectGroundType.GroundType.ruinWater:
                        Sound.PlaySE("PlayerFootStep_InsideRuin_Water", volumeWalkWater, walkChannel);
                        break;
                }
                footStepTimeCounter = 0f;
            }
        }

        footStepTimeCounter += Time.deltaTime;

        //ものを押す
        if (playGimmick.Get_PlayerState() == PlayerState.Action_PushOrPull)
        {
            bool isPlaying = Sound.IsPlayingSE(pushAndPullChannel);

            print(isPlaying);
            if (!isPlaying)
                Sound.PlaySE("PushAndPull", volumePushObj, pushAndPullChannel);
            if (player.GetComponent<CharacterController>().velocity.magnitude == 0f)
                stopingTimeCounter += Time.deltaTime;
            else
                stopingTimeCounter = 0;

            if (stopingTimeCounter > 0.1f)
                Sound.StopSE(pushAndPullChannel);
        }
        else
        {
            Sound.StopSE(pushAndPullChannel);
        }
    }

    /// <summary>
    /// 音声データの取得
    /// </summary>
    void LoadSounds()
    {
        //歩く音
        Sound.LoadSE("PlayerFootStep_Default", "PlayerFootStep_Default");
        Sound.LoadSE("PlayerFootStep_Wood", "PlayerFootStep_Wood");
        Sound.LoadSE("PlayerFootStep_Stone", "PlayerFootStep_Stone");
        Sound.LoadSE("PlayerFootStep_Water", "PlayerFootStep_Water");
        Sound.LoadSE("PlayerFootStep_InsideRuin_Default", "PlayerFootStep_InsideRuin_Default");
        Sound.LoadSE("PlayerFootStep_InsideRuin_Water", "PlayerFootStep_InsideRuin_Water");

        //ものを押す音
        Sound.LoadSE("PushAndPull", "PushAndPull");

        //ワープ音

        //ヒント音
        Sound.LoadSE("Hint", "Hint");

        ////スイッチを押す音
        //Sound.LoadSE("Switch", "Switch");
    }
    /// <summary>
    /// プレイヤーの立っている地面の情報を確認するための関数
    /// </summary>
    /// <param name="playerFootPos">プレイヤーの足の位置</param>
    /// <returns></returns>
    SelectGroundType.GroundType CheckGroundType(Vector3 playerFootPos)
    {
        SelectGroundType.GroundType groundType;
        RaycastHit hit;

        if (Physics.Raycast(playerFootPos, Vector3.down, out hit, 0.3f, layerMask))
        {
            if (hit.collider == null)
                return SelectGroundType.GroundType.none;
            else
            {
                groundType = hit.collider.GetComponent<SelectGroundType>().GetGroundType();
            }
        }
        else
            groundType = SelectGroundType.GroundType.none;

        Debug.DrawRay(playerFootPos, Vector3.down * 0.3f);

        return groundType;
    }
}
