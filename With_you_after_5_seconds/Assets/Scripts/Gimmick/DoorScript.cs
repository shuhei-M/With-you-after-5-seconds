using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public GameObject rootButton;
    public float speed = 5f;
    bool isOpen;
    ButtonScript[] Buttons = new ButtonScript[2];
    GameObject[] door = new GameObject[2];
    Vector3[] doorMovePos = new Vector3[2];
    Vector3[] moveVec = new Vector3[2];

    // Start is called before the first frame update
    void Start()
    {
        Buttons[0] = rootButton.transform.GetChild(0).GetComponent<ButtonScript>();
        Buttons[1] = rootButton.transform.GetChild(1).GetComponent<ButtonScript>();

        door[0] = transform.GetChild(0).gameObject;
        door[1] = transform.GetChild(1).gameObject;

        doorMovePos[0] = transform.GetChild(2).position;
        doorMovePos[1] = transform.GetChild(3).position;

        moveVec[0] = (doorMovePos[0] - door[0].transform.position).normalized;
        moveVec[1] = (doorMovePos[1] - door[1].transform.position).normalized;
        isOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Buttons[0].isPushing && Buttons[1].isPushing && !isOpen)
            isOpen = true;

        if(isOpen)
        {
            Open();
        }
    }

    void Open()
    {
        if ((doorMovePos[0] - door[0].transform.position).magnitude > 0.5f)
            door[0].transform.Translate(moveVec[0] * speed * Time.deltaTime);
        if ((doorMovePos[1] - door[1].transform.position).magnitude > 0.5f)
            door[1].transform.Translate(moveVec[1] * speed * Time.deltaTime);
    }
}
