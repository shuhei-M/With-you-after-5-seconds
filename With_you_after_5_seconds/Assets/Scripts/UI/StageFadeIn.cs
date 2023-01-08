using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StageFadeIn : MonoBehaviour
{
    Image image;
    [SerializeField] float speed = 1f;
    [SerializeField] float startSec = 0;

    float time;
    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        image.color = new Color(0, 0, 0, 1);
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (image.color.a > 0 && time >= startSec)
            image.color -= new Color(0, 0, 0, 1) * speed * Time.deltaTime;

    }
}
