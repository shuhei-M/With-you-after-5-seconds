using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPlayerTriggerScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            DirectingScript.playerOnTrigger = gameObject;
            Debug.Log(gameObject.name + " ‚ª Player ‚ÆÚG");
        }
    }
}
