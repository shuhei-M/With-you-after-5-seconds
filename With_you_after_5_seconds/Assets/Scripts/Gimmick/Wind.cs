using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    private float rayDistance;

    void Start()
    {
        rayDistance = 3.0f;
    }

    void Update()
    {
        //Raycastを左右に出す
        Vector3 rayPosition = transform.position + new Vector3(0.0f, 0.0f, 0.0f);
        Ray ray1 = new Ray(rayPosition, Vector3.right);
        Ray ray2 = new Ray(rayPosition, Vector3.left);
        Debug.DrawRay(rayPosition, Vector3.right * rayDistance, Color.red);
        //Debug.DrawRay(rayPosition, Vector3.left * rayDistance, Color.red);

        //オブジェクトに衝突したときの処理
        RaycastHit hit;
        if (Physics.Raycast(ray1, out hit, rayDistance))
        {
            if (hit.collider.name == "Player")
            {
                //Destroy(hit.collider.gameObject);
                Debug.Log("風");
            }
        }

        if (Physics.Raycast(ray2, out hit, rayDistance))
        {
            if (hit.collider.name == "Cylinder")
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
