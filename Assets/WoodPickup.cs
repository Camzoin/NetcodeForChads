using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class WoodPickup : NetworkBehaviour
{
    public List<PlayerController> players = new List<PlayerController>();
    public float attractSpeed = 5;
    public float attractRange = 5;
    public float pickupRange = 1;

    public override void OnNetworkSpawn()
    {
        players = FindObjectsOfType<PlayerController>().ToList();


        base.OnNetworkSpawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        foreach(PlayerController pc in players)
        {
            float distanceFromPlayer = Vector3.Distance(pc.transform.position, transform.position);

            if (distanceFromPlayer < attractRange)
            {
                transform.position += (pc.transform.position - transform.position).normalized * Time.deltaTime * attractSpeed;

                if (distanceFromPlayer < pickupRange)
                {
                    DespawnMeServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnMeServerRpc()
    {
        this.GetComponent<NetworkObject>().Despawn(true);
    }
}
