using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindGenerator : MonoBehaviour
{
    Vector3[] areas = new Vector3[3];
    Vector3 rayDirection;
    GameObject wind;
    [SerializeField]float rayDistance = 15f;
    [SerializeField]float windPower = 3f;
    CharacterController player;
    bool inWind;
    int rayNum;
    public Material noneMaterial;
    Ray[] rays;
    // Start is called before the first frame update
    void Start()
    {
        areas[0] = transform.GetChild(0).position;
        areas[1] = transform.GetChild(1).position;
        areas[2] = transform.GetChild(2).position;

        player = GameObject.Find("Player").GetComponent<CharacterController>();

        rayDirection = areas[2] - transform.position;
        rayDirection = rayDirection.normalized;
        wind = transform.GetChild(2).gameObject;
        rayNum = (int)((areas[0].x - areas[1].x) / 0.1f);
        if (rayNum < 0)
            rayNum = -rayNum;
        rays = new Ray[rayNum];
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        inWind = false;
        for(int i = 0;i < rays.Length; i++)
        {     
            rays[i] = new Ray(areas[0] + (areas[1] - areas[0]) / rayNum * (i + 1), rayDirection);
            if (Physics.Raycast(rays[i], out hit, rayDistance))
            {
                if (hit.collider.tag == "Torch")
                {
                    hit.collider.GetComponent<Renderer>().material = noneMaterial;
                    if (hit.collider.transform.root.tag == "Player")
                        inWind = true;
                }

                if (hit.collider.transform.tag == "Player")
                    inWind = true; 
            }
            if (hit.distance != 0)
                Debug.DrawRay(rays[i].origin, rays[i].direction * hit.distance, Color.blue, 0.1f);
            else
                Debug.DrawRay(rays[i].origin, rays[i].direction * 15, Color.blue, 0.1f);
        }

        if (inWind)
            player.Move(rayDirection * Time.deltaTime * windPower);
        //float rnum = Random.Range(areas[0].x, areas[1].x);
        //GameObject obj = Instantiate(wind,new Vector3(rnum, areas[0].y,areas[0].z), wind.transform.localRotation);
        //obj.SetActive(true);
    }

    private void FixedUpdate()
    {
        
    }
}
