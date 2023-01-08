using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSoundObj : MonoBehaviour
{
    GameObject player;
    [SerializeField]
    [Header("freezingPos")]
    bool  x, y, z;
    int cX, cY, cZ;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        if (x) cX = 0;
        else cX = 1;
        if (y) cY = 0;
        else cY = 1;
        if (z) cZ = 0;
        else cZ = 1;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = (new Vector3(player.transform.position.x * cX, player.transform.position.y * cY, player.transform.position.z * cZ));
    }
}
