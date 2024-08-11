using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TreeSpawner : NetworkBehaviour
{
    public GameObject treePrefab;
    public float spawnDistance = 10;
    public float spawnTime = 5, curSpawnTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (curSpawnTime <= 0)
        {
            //spawn tree

            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

            SpawnTreeServerRpc(transform.position + (offset.normalized * Random.Range(0f, spawnDistance)), Quaternion.identity);

            curSpawnTime += spawnTime;
        }

        curSpawnTime -= Time.deltaTime;

        //Debug.Log("I am the server now");
    }

    [ServerRpc]
    private void SpawnTreeServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject newTree = Instantiate(treePrefab, position, rotation);

        newTree.GetComponent<NetworkObject>().Spawn();
    }
}
