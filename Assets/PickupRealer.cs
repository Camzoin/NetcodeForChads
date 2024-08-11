using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickupRealer : MonoBehaviour
{
    public float lifeTime = 1, curlifeTime = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        curlifeTime += Time.deltaTime;

        if(curlifeTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
