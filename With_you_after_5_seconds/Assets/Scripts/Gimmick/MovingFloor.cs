using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingFloor : MonoBehaviour
{
    public GameObject rootButton;
    public float speed = 5f;
    public bool upperStart;
    public bool x, z;
    bool isUpperPos;
    ButtonScript[] Buttons = new ButtonScript[2];
    GameObject[] door = new GameObject[2];
    Vector3[] floorMovePos = new Vector3[2];
    float[] floorCheckPoint = new float[2];
    Vector3[] moveVec = new Vector3[2];
    bool[] oldIsPushing = new bool[2];
    float nowPoint;

    // Start is called before the first frame update
    void Start()
    {
        Buttons[0] = rootButton.transform.GetChild(0).GetComponent<ButtonScript>();
        Buttons[1] = rootButton.transform.GetChild(1).GetComponent<ButtonScript>();

        door[0] = transform.GetChild(0).gameObject;
        door[1] = transform.GetChild(1).gameObject;

        floorMovePos[0] = transform.GetChild(0).position;
        floorMovePos[1] = transform.GetChild(1).position;

        moveVec[0] = (floorMovePos[0] - floorMovePos[1]).normalized;
        moveVec[1] = -moveVec[0];
        isUpperPos = upperStart;

        oldIsPushing[0] = Buttons[0].isPushing;
        oldIsPushing[1] = Buttons[1].isPushing;

        Sound.LoadSE("MoveFence","MoveFence");
    }

    // Update is called once per frame
    void Update()
    {
        if (Buttons[0].isPushing && Buttons[1].isPushing && !(oldIsPushing[0] && oldIsPushing[1]))
        {
            if (isUpperPos)
                isUpperPos = false;
            else
                isUpperPos = true;

            Sound.PlaySE("MoveFence", 1f, 3);
        }
       
        if (x)
        {
            nowPoint = transform.position.x;
            floorCheckPoint[0] = floorMovePos[0].x;
            floorCheckPoint[1] = floorMovePos[1].x;
        }
        else if (z)
        {
            nowPoint = transform.position.z;
            floorCheckPoint[0] = floorMovePos[0].z;
            floorCheckPoint[1] = floorMovePos[1].z;
        }
        else
        {
            nowPoint = transform.position.y;
            floorCheckPoint[0] = floorMovePos[0].y;
            floorCheckPoint[1] = floorMovePos[1].y;
        } 
        
        Move();

        oldIsPushing[0] = Buttons[0].isPushing;
        oldIsPushing[1] = Buttons[1].isPushing;
    }

    void Move()
    {
        if (nowPoint < floorCheckPoint[0] && isUpperPos)
            transform.Translate(moveVec[0] * speed * Time.deltaTime);
        if (nowPoint > floorCheckPoint[1] && !isUpperPos)
            transform.Translate(moveVec[1] * speed * Time.deltaTime);
    }
}
