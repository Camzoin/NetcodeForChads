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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));




        transform.position = Vector3.MoveTowards(transform.position, transform.position + input, Time.deltaTime * modifiedMoveSpeed);

        transform.LookAt(transform.position + input);

        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId));

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        Debug.Log(unityTransport.ConnectionData.Address + " " + unityTransport.ConnectionData.Port);
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;



        Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, transform.position, cameraTrackingSpeed * Time.deltaTime);
    }
}
