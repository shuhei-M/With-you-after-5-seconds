using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    GameObject torch;
    bool hold, isCount;
    float coolTime;
    // Start is called before the first frame update
    void Start()
    {
        torch = transform.parent.gameObject;
        hold = false;
        coolTime = 0f;
        isCount = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCount)
        {
            coolTime += Time.deltaTime;
            if (coolTime >= 1f)
            {
                coolTime = 0f;
                isCount = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            var playerState = other.GetComponent<PlayerBehaviour>().State;
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Default;
            if (playerState == PlayerState.Action && !isCount)
            {
                if (!hold)
                {
                    torch.transform.parent = other.transform;
                    torch.transform.position = other.transform.position + other.transform.forward / 2 + Vector3.up / 2;
                }
                else
                    torch.transform.parent = null;

                hold = !hold;
                isCount = true;
            }
        }
    }
}
