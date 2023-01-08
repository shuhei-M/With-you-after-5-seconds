using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//=== 梯子ギミックのひな型 ===//
// インタフェイスの使い方を説明するためのモノなので好きな形に変更してください
public class LadderGimmick : MonoBehaviour
{
    enum Direction { N = 0, E = 1, S = 2, W = 3 };
    public GameObject forLadder;
    GameObject player;
    GameObject mainCamera;
    IPlayGimmick playGimmick;
    PlayerBehaviour playerController;
    [Header("プレイヤーが梯子を登る時の速度")]
    public float speed = 5f;
    [Header("梯子を登る時にプレイヤーが向いている方向(どれか1つだけ必ずチェック入れる)")]
    public bool X;
    public bool Z, negativeX, negativeZ;

    string axisName;
    float code,angle, directionX = 0, directionZ = 0, time;
    bool climb = false, timeCount = false;
    Direction cameraDirection,  ladderDirection;
    KeyCode up, down;
    
    public enum LadderType : int
    {
        Start,
        End,
    }

    public LadderType _ladderType;
    
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        mainCamera = GameObject.Find("Main Camera");

        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerBehaviour>();

        if(X)
        {
            ladderDirection = Direction.E;

            axisName = "L_Horizontal";
            code = 1;
            up = KeyCode.D;
            down = KeyCode.A;
            directionX = -1;
        }
        else if(Z)
        {
            ladderDirection = Direction.N;

            axisName = "L_Vertical";
            code = 1;
            up = KeyCode.W;
            down = KeyCode.S;
            directionZ = -1;
        }
        else if(negativeX)
        {
            ladderDirection = Direction.W;

            axisName = "L_Horizontal";
            code = -1;
            up = KeyCode.A;
            down = KeyCode.D;
            directionX = 1;
        }
        else if(negativeZ)
        {
            ladderDirection = Direction.S;

            axisName = "L_Vertical";
            code = -1;
            up = KeyCode.S;
            down = KeyCode.W;
            directionZ = 1;
        }
        else 
        {
            print("梯子の向きを設定してください。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        angle = mainCamera.transform.eulerAngles.y;

        if ((315 <= angle && angle < 360) || (0 <= angle && angle < 45))
            cameraDirection = Direction.S;
        else if (45 <= angle && angle < 135)
            cameraDirection = Direction.W;
        else if (135 <= angle && angle < 225)
            cameraDirection = Direction.N;
        else
            cameraDirection = Direction.E;

        //if (cameraDirection != oldCameraDirection)
        switch ((Direction)((4 + (int)cameraDirection - (int)ladderDirection) % 4))
        {
            case Direction.S:
                axisName = "L_Vertical";
                code = 1;
                up = KeyCode.W;
                down = KeyCode.S;
                break;
            case Direction.E:
                axisName = "L_Horizontal";
                code = 1;
                up = KeyCode.D;
                down = KeyCode.A;
                break;
            case Direction.N:
                axisName = "L_Vertical";
                code = -1;
                up = KeyCode.S;
                down = KeyCode.W;
                break;
            case Direction.W:
                axisName = "L_Horizontal";
                code = -1;
                up = KeyCode.A;
                down = KeyCode.D;
                break;
        }

        if (timeCount)
            time += Time.deltaTime;
        else
            time = 0;
        if (time > 0.1f)
            climb = true;
    }

    private void OnTriggerStay(Collider other)
    { 
        if (other.tag == "Player")
        {
            // 接触したオブジェクトから IPlayGimmick インターフェイスを取得する
            playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            
            _ladderType = LadderType.Start;

            if (!climb)
            {
                if (code * Input.GetAxis(axisName) >= 0.7)
                    timeCount = true;
                else
                    timeCount = false;
            }

            if (playGimmick != null && climb)
            {
                // 取り出せていれば、キャラクターコントローラーをオフにする
                playGimmick.OFF_CharacterController();
                playerController.OFF_PlayerRotate();

                forLadder.transform.position = other.transform.position - Vector3.up * 1 / 8;
                forLadder.SetActive(true);
                other.gameObject.transform.position = new Vector3(transform.position.x, other.gameObject.transform.position.y, transform.position.z) + directionX * Vector3.right * 0.51f + directionZ * Vector3.forward * 0.51f;
            }
        }

        if (other.tag == "ForLadder" && playGimmick != null)
        {
            if (_ladderType == LadderType.Start)
            {
                player.transform.LookAt(new Vector3(transform.position.x, player.transform.position.y, transform.position.z));
                if (Input.GetKey(up) || code * Input.GetAxis(axisName) >= 0.2)
                {
                    playGimmick.Get_Transform().Translate(Vector3.up * speed * Time.deltaTime);
                    forLadder.transform.Translate(Vector3.up * speed * Time.deltaTime);
                }
                if(Input.GetKey(down) || code * Input.GetAxis(axisName) <= -0.2)
                {
                    playGimmick.Get_Transform().Translate(Vector3.up * -speed * Time.deltaTime);
                    forLadder.transform.Translate(Vector3.up * -speed * Time.deltaTime);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ForLadder")
        {
            _ladderType = LadderType.End;
            forLadder.SetActive(false);
            climb = false;
            timeCount = false;
            if (playGimmick != null)
            {
                playGimmick.ON_CharacterController();
                playerController.ON_PlayerRotate();
            }
        }
    }
}
