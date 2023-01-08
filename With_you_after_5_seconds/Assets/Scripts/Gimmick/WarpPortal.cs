using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPortal : MonoBehaviour
{
    Vector3 inPortal, outPortal;
    GameObject player;
    PlayerBehaviour playerController;
    bool startWarp;
    float timeCount;

    // Start is called before the first frame update
    void Start()
    {
        inPortal = transform.GetChild(0).position;
        outPortal = transform.GetChild(1).position;
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerBehaviour>();
        startWarp = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(inPortal.x - 1 <= player.transform.position.x && inPortal.x + 1 >= player.transform.position.x && 
            inPortal.y - 2 <= player.transform.position.y && inPortal.y + 2 >= player.transform.position.y&&
            inPortal.z - 1 <= player.transform.position.z && inPortal.z + 1 >= player.transform.position.z && !startWarp)
        {
            startWarp = true;
            playerController.OFF_CharacterController();
            playerController.OFF_PlayerRotate();
            player.transform.position = new Vector3(inPortal.x, player.transform.position.y, inPortal.z);
            player.transform.LookAt(inPortal + Vector3.back - new Vector3(0, 0.7f, 0));
            timeCount = 0;    
        }
        if (startWarp)
        {
            timeCount += Time.deltaTime;

            if(timeCount >= 2f)
            {
                player.transform.position = new Vector3(outPortal.x, player.transform.position.y, outPortal.z);
                playerController.ON_CharacterController();
                playerController.ON_PlayerRotate();
                startWarp = false;
            }
        }
    }
}
