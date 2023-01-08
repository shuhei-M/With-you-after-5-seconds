using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FallPushObject : MonoBehaviour
{
    bool isFall = true;
    GameObject parent;
    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFall)
            Fall();
    }
    private void OnTriggerStay(Collider other)
    {
        isFall = false;
    }
    private void OnTriggerExit(Collider other)
    {
        isFall = true;
    }

    void Fall()
    {
        parent.transform.Translate(Vector3.down * speed * Time.deltaTime);
    }
}

