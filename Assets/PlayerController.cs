using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Linq;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private Vector3 input;
    public float moveSpeed = 3;
    public float acceleration = 3;
    public float runMoveSpeedMulti = 2;
    private bool isRunning = false;
    public float cameraTrackingSpeed = 5;
    public GameObject playerAttackProj;
    public GameObject playerServerAttackProj;
    public float timeToShoot = 0.2f, curTimeToShoot = 0;
    public bool isShooting = false;
    public NetworkVariable<float> ping = new NetworkVariable<float>(0);

    private Plane plane = new Plane(Vector3.down, 0);

    public Vector3 oldPos;

    public float localCurHP = 100;
    public float localCurMana = 100;
    public float localCurStam = 100;

    public NetworkVariable<float> curHP = new NetworkVariable<float>(100);
    public NetworkVariable<float> maxHP = new NetworkVariable<float>(100);

    public NetworkVariable<float> curMana = new NetworkVariable<float>(100);
    public NetworkVariable<float> maxMana = new NetworkVariable<float>(100);

    public NetworkVariable<float> curStam = new NetworkVariable<float>(100);
    public NetworkVariable<float> maxStam = new NetworkVariable<float>(100);

    public Renderer hpBarRend;
    private Material hpBarMat;

    public Transform crosshairTransform;

    public GameObject playerHud;


    public Image myHPBarRend, myManaBarRend, myStamBarRend;
    private Material myHPBarMat, myManaBarMat, myStamBarMat;

    public List<Image> activeAbilityIcons = new List<Image>();
    public List<AbilitySO> activeAbilities = new List<AbilitySO>();
    public List<float> curCooldowns = new List<float>();
    public List<bool> isChannelingThisAbility = new List<bool>();

    float timeSinceDirectionTap = 0;

    float lookingThisWayTime = 0;

    public List<GameObject> abilityPrefabs = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        hpBarMat = new Material(hpBarRend.material);

        if (IsOwner)
        {
            playerHud.transform.SetParent(null);

            playerHud.GetComponent<Canvas>().worldCamera = Camera.main;

            Cursor.visible = false;

            Cursor.lockState = CursorLockMode.Confined;

            hpBarRend.gameObject.SetActive(false);

            playerHud.SetActive(true);


            myHPBarMat = new Material(myHPBarRend.material);

            myHPBarRend.material = myHPBarMat;

            myManaBarMat = new Material(myManaBarRend.material);

            myManaBarRend.material = myManaBarMat;

            myStamBarMat = new Material(myStamBarRend.material);

            myStamBarRend.material = myStamBarMat;

            for (int i = 0; i < activeAbilityIcons.Count; i++)
            {
                activeAbilityIcons[i].material = new Material(activeAbilityIcons[i].material);

                activeAbilityIcons[i].material.SetTexture("_AbilityIcon", activeAbilities[i].abilityIcon);
            }
        }
        else
        {
            crosshairTransform.gameObject.SetActive(false);

            hpBarRend.gameObject.SetActive(true);

            playerHud.SetActive(false);

            hpBarRend.material = hpBarMat;
        }





    }

    // Update is called once per frame
    void Update()
    {
        if (lookingThisWayTime > 0)
        {
            lookingThisWayTime -= Time.deltaTime;
        }
        else
        {
            transform.LookAt(transform.position + input);
        }


        hpBarMat.SetFloat("_curHP", curHP.Value);
        hpBarMat.SetFloat("_maxHP", maxHP.Value);

        if (!IsOwner) return;

        myHPBarMat.SetFloat("_Fill", localCurHP / maxHP.Value);
        myManaBarMat.SetFloat("_Fill", localCurMana / maxMana.Value);
        myStamBarMat.SetFloat("_Fill", localCurStam / maxStam.Value);

        SetPingServerRpc(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId));

        //ping.Value = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);

        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));



        Vector3 lookPos = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            lookPos = ray.GetPoint(distance);

        }








        for (int i = 0; i < activeAbilityIcons.Count; i++)
        {
            activeAbilityIcons[i].material.SetFloat("_Fill", 1 - (curCooldowns[i] / activeAbilities[i].cooldown));
        }


        for (int i = 0; i < curCooldowns.Count; i++)
        {
            curCooldowns[i] -= Time.deltaTime;

            if (curCooldowns[i] < 0)
            {
                curCooldowns[i] = 0;
            }
        }

        if (Input.GetMouseButtonDown(0) || (isChannelingThisAbility[0] && activeAbilities[0].canBeChannelled))
        {
            if (curCooldowns[0] == 0)
            {
                transform.LookAt(lookPos);

                CheckIDandCast(0);

                isChannelingThisAbility[0] = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isChannelingThisAbility[0] = false;
        }




        if (Input.GetMouseButtonDown(1) || (isChannelingThisAbility[1] && activeAbilities[1].canBeChannelled))
        {
            if (curCooldowns[1] == 0)
            {
                transform.LookAt(lookPos);

                CheckIDandCast(1);

                isChannelingThisAbility[1] = true;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isChannelingThisAbility[1] = false;
        }





        if (Input.GetKeyDown(KeyCode.Space) || (isChannelingThisAbility[2] && activeAbilities[2].canBeChannelled))
        {
            if (curCooldowns[2] == 0)
            {
                transform.LookAt(lookPos);

                CheckIDandCast(2);

                isChannelingThisAbility[2] = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isChannelingThisAbility[2] = false;
        }



        //if (Input.GetMouseButton(0))
        //{
        //    //fire
        //    isShooting = true;




        //    transform.LookAt(lookPos);

        //    if (curTimeToShoot <= 0)
        //    {
        //        ServerInstantiateObjectServerRpc(transform.position + Vector3.up, transform.rotation);

        //        //SpawnBulletClientRpc(transform.position + Vector3.up, transform.rotation, NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);

        //        GameObject newBullet = Instantiate(playerAttackProj, transform.position + Vector3.up, transform.rotation);

        //        newBullet.GetComponent<Projectile>().realProj = true;

        //        curTimeToShoot += timeToShoot;
        //    }

        //    if (curTimeToShoot > 0)
        //    {
        //        curTimeToShoot -= Time.deltaTime;
        //    }
        //}
        //else
        //{
        //    isShooting = false;
        //}


        //if (Input.GetMouseButton(0))
        //{

        //}

        //    if (Input.GetKey(KeyCode.Space))
        //{
        //    //Do movemnet
        //}



        if (timeSinceDirectionTap < 0.2f && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S)))
        {
            isRunning = true;
        }

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A)  || Input.GetKeyDown(KeyCode.S)))
        {
            timeSinceDirectionTap = 0;
        }

        timeSinceDirectionTap += Time.deltaTime;


        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }

        if (input.magnitude < 0.01f || localCurStam <= 0)
        {
            isRunning = false;
        }


        if (isRunning)
        {
            localCurStam -= Time.deltaTime * 2f;

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 6, Time.deltaTime * 5f);
        }
        else
        {
            localCurStam += Time.deltaTime;

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 5, Time.deltaTime);

            if (localCurStam > maxStam.Value)
            {
                localCurStam = maxStam.Value;
            }
        }

        crosshairTransform.position = lookPos;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;



        float modifiedMoveSpeed = moveSpeed;






        if (isRunning)
        {
            modifiedMoveSpeed = moveSpeed * runMoveSpeedMulti;
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


        //Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId));

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //Debug.Log(unityTransport.ConnectionData.Address + " " + unityTransport.ConnectionData.Port);

        Vector3 lookPos = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            lookPos = ray.GetPoint(distance);

        }

        crosshairTransform.position = lookPos;

        Vector3 lerpedCameraPos = Vector3.Lerp(transform.position, crosshairTransform.position, 0.2f);

        Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, new Vector3(lerpedCameraPos.x,0, lerpedCameraPos.z), cameraTrackingSpeed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        oldPos = transform.position;

        hpBarRend.transform.LookAt(Camera.main.transform.position);

        hpBarRend.transform.Rotate(Vector3.up * 180);

        if (!IsOwner) return;




    }

    public override void OnNetworkSpawn()
    {
        List<WoodPickup> wps = FindObjectsOfType<WoodPickup>().ToList();

        foreach(WoodPickup wp in wps)
        {
            wp.players.Add(this);
        }


        List<DefaultEnemy> des = FindObjectsOfType<DefaultEnemy>().ToList();

        foreach (DefaultEnemy de in des)
        {
            de.players.Add(this);
        }


        base.OnNetworkSpawn();
    }





    public void CheckIDandCast(int abilityIndex)
    {
        AbilitySO ability = activeAbilities[abilityIndex];


        if ((ability.stamCost > 0 && localCurStam <= ability.stamCost))
        {
            //Failed to stam cast

            return;
        }

        if ((ability.manaCost > 0 && localCurMana <= ability.manaCost))
        {
            //Failed to mana cast
            return;
        }




        if (ability.manaCost > 0)
        {
            myManaBarMat.SetFloat("_Ping", Time.time);
        }

        if (ability.stamCost > 0)
        {
            myStamBarMat.SetFloat("_Ping", Time.time);
        }



        if (ability.abilityID == 0)
        {
            CastMagicMissile(abilityIndex);
        }

        if (ability.abilityID == 1)
        {
            CastMagicMissile(abilityIndex);
        }
    }

    public void CastMagicMissile(int abilityIndex)
    {
        ServerInstantiateObjectServerRpc(transform.position + Vector3.up, transform.rotation, activeAbilities[abilityIndex].abiltiyScale, activeAbilities[abilityIndex].projectileSpeed, activeAbilities[abilityIndex].timeLookingAfterCast, activeAbilities[abilityIndex].abilityID);

        curCooldowns[abilityIndex] += activeAbilities[abilityIndex].cooldown;

        Projectile mm = Instantiate(activeAbilities[abilityIndex].spellPrefab, transform.position, transform.rotation).GetComponent<Projectile>();

        mm.pc = this;

        mm.ownerTransform = transform;

        mm.knockBackTime = activeAbilities[abilityIndex].knockBackTime;

        mm.knockBackVelocity = activeAbilities[abilityIndex].knockBackVelocity;

        mm.transform.localScale = Vector3.one * activeAbilities[abilityIndex].abiltiyScale;

        mm.damage = activeAbilities[abilityIndex].damage;

        mm.speed = activeAbilities[abilityIndex].projectileSpeed;

        lookingThisWayTime = activeAbilities[abilityIndex].timeLookingAfterCast;

        //mm.impactAnimatonDuration = activeAbilities[abilityIndex].impactAnimationSpeed;

        //mm.toPos = toPos;

        localCurMana -= activeAbilities[abilityIndex].manaCost;
    }





    [ServerRpc]
    public void SetPingServerRpc(float thisPing)
    {
        ping.Value = thisPing;
    }


    [ServerRpc]
    public void ServerInstantiateObjectServerRpc(Vector3 position, Quaternion rotation, float scale, float speed, float lookTime, int abilityID)
    {
        // Trigger instantiation on all clients
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            ClientRpcInstantiateObjectClientRpc(position, rotation, scale, speed, lookTime, abilityID, clientId);
        }
    }

    [ClientRpc]
    void ClientRpcInstantiateObjectClientRpc(Vector3 position, Quaternion rotation,float scale, float speed, float lookTime, int abilityID, ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (IsOwner) return;

            GameObject attackProj = abilityPrefabs[abilityID];



            Projectile proj = Instantiate(attackProj, position, rotation).GetComponent<Projectile>();

            proj.ownerTransform = transform;

            proj.transform.localScale = Vector3.one * scale;

            proj.speed = speed;

            proj.realProj = false;

            lookingThisWayTime = lookTime;
        }
    }
}
