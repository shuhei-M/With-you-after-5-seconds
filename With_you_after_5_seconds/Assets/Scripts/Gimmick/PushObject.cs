using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushObject : MonoBehaviour
{
    GameObject parent, child, player;
    PlayerBehaviour playerController;

    [SerializeField] float power = 3000;
    [SerializeField] bool X;
    [SerializeField] LayerMask layerMask;

    float calcX = 0, calcZ = 0;
    bool  canHold, oldCanHold;

    Rigidbody parentRb;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
        child = transform.GetChild(0).gameObject;
        parentRb = parent.GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerBehaviour>();

        if (X)
            calcX = 1;
        else
            calcZ = 1;

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray;
        for (int i = 0; i < 3; i++)
        {
            //X軸とZ軸が逆になるので調整
            float calcRayX = transform.localScale.x * 2 / 3 * (i - 1) * calcZ;
            float calcRayZ = transform.localScale.x * 2 / 3 * (i - 1) * calcX;
            Vector3 calcRayOrigin = new Vector3(calcRayX, - 0.6f, calcRayZ);

            ray = new Ray(transform.parent.position +　calcRayOrigin, -transform.forward);

            //オブジェクトがつかみたい面の近くにある時はつかめないように
            if (Physics.Raycast(ray , 1.3f, layerMask))
            {
                canHold = false;
            } 
            Debug.DrawRay(ray.origin, ray.direction * 1.3f);
        }
        ray = new Ray(transform.position + new Vector3(transform.forward.x / 7f, -transform.localScale.y * 3 / 4, transform.forward.z / 7f)  , -transform.up);
        if (!Physics.Raycast(ray, 0.5f, layerMask))
        {
            canHold = false;
        }
        Debug.DrawRay(ray.origin, ray.direction * 0.5f);
        //つかめる
        if (canHold)
        {
            var cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1));
            var moveVec =  cameraForward * Input.GetAxis("Vertical") + mainCamera.transform.right * Input.GetAxis("Horizontal");
            IPlayGimmick playGimmick = player.gameObject.GetComponent<IPlayGimmick>();
            if (playGimmick != null)
            {
                playGimmick.ActionTypeP = ActionType.PushOrPull;
            }

            //ボタンを押した最初だけプレイヤーの向きや位置を調整
            if (Input.GetButtonDown("Action"))
            {
                playerController.OFF_CharacterController();
                if(X)
                    player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
                else 
                    player.transform.position = new Vector3(transform.position.x, player.transform.position.y, player.transform.position.z);
                player.transform.LookAt(new Vector3(parent.transform.position.x, player.transform.position.y, parent.transform.position.z));
                playerController.ON_CharacterController();

            }

            //押す処理
            if (Input.GetButton("Action"))
            {
                playerController.OFF_PlayerRotate();
                parentRb.AddForce(new Vector3(calcX * moveVec.x, 0, calcZ * moveVec.z) * power * Time.deltaTime);
                child.SetActive(true);
            }
            else
            {
                playerController.ON_PlayerRotate();
                child.SetActive(false);
            }
        }
        else if(oldCanHold)
        {
            playerController.ON_PlayerRotate();
            child.SetActive(false);
        }

        oldCanHold = canHold;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
             canHold = true;
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        canHold = false;

        if (other.tag == "Player")
        {
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Default;
            playerController.ON_PlayerRotate();
            child.SetActive(false);
        }
    }
}
