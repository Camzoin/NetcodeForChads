using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class ResourceEntity : NetworkBehaviour
{
    public float maxHP = 15, localCurHP = 15;

    public NetworkVariable<float> curHP = new NetworkVariable<float>(15);

    public Renderer rend;

    private Material hpBarMat;

    bool isDead = false;

    public GameObject pickupDrop, fakePickupdrop;

    public Vector2 dropMinMax = new Vector2(1f, 4f);

    // Start is called before the first frame update
    void Start()
    {
        hpBarMat = new Material(rend.material);

        rend.material = hpBarMat;
    }

    // Update is called once per frame
    void Update()
    {
        hpBarMat.SetFloat("_curHP", localCurHP);
        hpBarMat.SetFloat("_maxHP", maxHP);

        rend.transform.LookAt(Camera.main.transform.position);

        rend.transform.Rotate(Vector3.up * 180);


        if (curHP.Value <= localCurHP)
        {
            localCurHP = curHP.Value;
        }

        if (localCurHP <= 0 && isDead == false)
        {
            isDead = true;

            transform.GetChild(0).gameObject.SetActive(false);



            int dropCount = (int)Random.Range(dropMinMax.x, dropMinMax.y);

            for (int i = 0; i < dropCount; i++)
            {
                GameObject newDrop = Instantiate(fakePickupdrop, transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), Quaternion.identity);

                newDrop.GetComponent<PickupRealer>().lifeTime = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId) / 1000f;

                SpawnDropsServerRpc(newDrop.transform.position, newDrop.transform.rotation);
            }




            DespawnMeServerRpc();
        }
    }

    public void TakeDamage(float damage)
    {
        localCurHP -= damage;

        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        curHP.Value -= damage;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnMeServerRpc()
    {
        this.GetComponent<NetworkObject>().Despawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDropsServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject newDrop = Instantiate(pickupDrop, position, rotation);

        newDrop.GetComponent<NetworkObject>().Spawn();
    }
}
