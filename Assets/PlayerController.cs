using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PlayerController : NetworkBehaviour
{
    private Vector3 input;
    public float moveSpeed = 3;
    public float acceleration = 3;
    public float runMoveSpeedMulti = 2;
    private bool isRunning = false;
    public float cameraTrackingSpeed = 5;
    public GameObject playerAttackProj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetMouseButton(0))
        {
            //fire

            SpawnBulletServerRPC(transform.position + Vector3.up, transform.rotation);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float modifiedMoveSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            modifiedMoveSpeed = moveSpeed * runMoveSpeedMulti;
        }
        else
        {
            isRunning = false;
        }





        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 6;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward), out hit, (input.magnitude * Time.fixedDeltaTime * modifiedMoveSpeed) * 5, layerMask))
        {
            Debug.DrawRay(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit " + hit.distance);

            if (hit.distance > 0.4f)
            {
                transform.position = Vector3.MoveTowards(transform.position, hit.point - Vector3.up, Time.fixedDeltaTime * modifiedMoveSpeed);
            }
            
        }
        else
        {
            Debug.DrawRay(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //Debug.Log("Did not Hit");

            transform.position = Vector3.MoveTowards(transform.position, transform.position + input, Time.fixedDeltaTime * modifiedMoveSpeed);
        }



        

        transform.LookAt(transform.position + input);

        //Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId));

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //Debug.Log(unityTransport.ConnectionData.Address + " " + unityTransport.ConnectionData.Port);

        Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, transform.position, cameraTrackingSpeed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;




    }

    [ServerRpc]
    private void SpawnBulletServerRPC(Vector3 position, Quaternion rotation)
    {
        GameObject newBullet = Instantiate(playerAttackProj, position, rotation);

        newBullet.GetComponent<NetworkObject>().Spawn();
    }
}
