using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.AI;

public class DefaultEnemy : NetworkBehaviour
{
    public NetworkVariable<float> curHP = new NetworkVariable<float>(25);

    public float curLocalHP = 25;

    public NetworkVariable<bool> isAggro = new NetworkVariable<bool>(false);

    public float maxHP = 25;

    public List<PlayerController> players = new List<PlayerController>();

    public float aggroRange = 50;

    public float moveSpeed = 10;

    public float acceleration = 10;

    public float angularSpeed = 120;

    public Renderer hpBarRend;
    private Material hpBarMat;

    bool isDead = false;

    public PlayerController targetPlayer;

    Collider col;

    float attackRange = 5;

    private NavMeshAgent agent;

    public bool isStunned = false;
    public bool isKnockedBack = false, isKnockedBackPlus = false;

    public float knockBackTime = 0, knockBackSpeed = 30;

    public Vector3 knockbBackDirection = new Vector3();

    public Renderer rend;

    public Material myMat;

    public float attackTime = 3f;

    public float curAttackCooldown = 0f;


    bool isMoving = false;

    float timeSinceSync = 0;

    public NetworkVariable<Vector3> curRealPosition = new NetworkVariable<Vector3>();

    private Vector3 oldPos;

    float timeIwasAggroed = Mathf.Infinity;

    bool hasAggroSynced = false;

    public override void OnNetworkSpawn()
    {
        players = FindObjectsOfType<PlayerController>().ToList();


        base.OnNetworkSpawn();
    }

    public void LateUpdate()
    {
        oldPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        myMat = new Material(rend.material);

        rend.material = myMat;

        col = GetComponent<Collider>();

        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;

        agent.acceleration = acceleration;

        hpBarMat = new Material(hpBarRend.material);

        hpBarRend.material = hpBarMat;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAggroSynced == false && Time.time - timeIwasAggroed > 1)
        {
            hasAggroSynced = true;

            timeSinceSync = 0;
            SetMyPositionServerRpc(transform.position, NetworkManager.Singleton.LocalClientId);
        }



        if (curHP.Value < curLocalHP)
        {
            DamageFlash();

            FindClosestPlayer();

            curLocalHP = curHP.Value;
        }


        hpBarRend.transform.LookAt(Camera.main.transform.position);

        hpBarRend.transform.Rotate(Vector3.up * 180);

        hpBarMat.SetFloat("_curHP", curLocalHP);
        hpBarMat.SetFloat("_maxHP", maxHP);




        if (targetPlayer)
        {
            if (Vector3.Distance(transform.position, targetPlayer.transform.position) < attackRange)
            {
                if (curAttackCooldown <= 0)
                {
                    //SetMyPositionServerRpc(transform.position, NetworkManager.Singleton.LocalClientId);
                    Debug.Log("Hit");

                    curAttackCooldown = attackTime;
                }


            }
        }

        if (curAttackCooldown > 0)
        {
            curAttackCooldown -= Time.deltaTime;
        }

        if (curLocalHP <= 0 && isDead == false)
        {
            isDead = true;

            transform.GetChild(0).gameObject.SetActive(false);



            DespawnMeServerRpc();
        }



        foreach (PlayerController pc in players)
        {
            float distanceFromplayer = Vector3.Distance(pc.transform.position, transform.position);

            if (distanceFromplayer < aggroRange)
            {      
                SetAggroServerRPC(true);

                targetPlayer = pc;
            }
        }








    }

    public void FixedUpdate()
    {
        if (isKnockedBack || isKnockedBackPlus)
        {
            if (knockBackTime > 0)
            {
                //agent.SetDestination(transform.position);

                agent.ResetPath();

                agent.speed = 0;
                agent.angularSpeed = 0;
                agent.acceleration = 100;
                agent.velocity = knockbBackDirection * knockBackSpeed;
            }
            else
            {

                agent.speed = moveSpeed;
                agent.angularSpeed = angularSpeed;
                isKnockedBack = false;
            }


            knockBackTime -= Time.fixedDeltaTime;


            if (knockBackTime < -0.2f)
            {


                timeSinceSync = -2;
                //ServerRPC to set the networked position on knockback end
                SetMyPositionServerRpc(transform.position, NetworkManager.Singleton.LocalClientId);

                //ClientRPC to set my position to my networked position

                isKnockedBackPlus = false;

            }
        }


        //if (!IsServer) return;

        if (targetPlayer && !isStunned && !isKnockedBack)
        {
            Vector3 targetPos = targetPlayer.transform.position + ((targetPlayer.transform.position - targetPlayer.oldPos) * (targetPlayer.ping.Value / 1000f));

            if (isAggro.Value)
            {
                agent.SetDestination(targetPos);
            }
        }

        if (isStunned)
        {
            agent.SetDestination(transform.position);
        }
    }

    public void FindClosestPlayer()
    {
        List<PlayerController> pcs = players;


        pcs = pcs.OrderBy(
   x => Vector3.Distance(transform.position, x.transform.position)
  ).ToList();

        targetPlayer = pcs[0];
    }

    public void TakeKnockBack(Vector3 dir, float duration, float velocity)
    {
        isKnockedBack = true;

        isKnockedBackPlus = true;

        knockbBackDirection = dir;

        knockBackTime = duration;

        knockBackSpeed = velocity;


        //TakeKnockBackServerRpc(dir, duration, velocity);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMyPositionServerRpc(Vector3 pos, ulong clientId)
    {
        curRealPosition.Value = pos;

        SyncToNetworkPositionClientRpc(pos, clientId);
    }



    [ClientRpc]
    public void SyncToNetworkPositionClientRpc(Vector3 pos, ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {

            if (Vector3.Distance(transform.position, pos)> 2f)
            {
                StartCoroutine(CatchUpMyPosition(pos));
            }




            //transform.position = pos;
        }

        timeSinceSync = 0;
    }



    public void TakeDamage(float damage)
    {
        curLocalHP -= damage;

        DamageFlash();

        TakeDamageServerRpc(damage);
    }

    public void DamageFlash()
    {
        myMat.SetFloat("_HitTime", Time.time);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        curHP.Value -= damage;

        isAggro.Value = true;

        timeIwasAggroed = Time.time;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAggroServerRPC(bool aggroState)
    {
        isAggro.Value = aggroState;

        timeIwasAggroed = Time.time;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnMeServerRpc()
    {
        this.GetComponent<NetworkObject>().Despawn(true);
    }

    IEnumerator CatchUpMyPosition(Vector3 finalPos)
    {
        float totalTime = 0.05f;
        float curTime = 0;
        //Vector3 startingPos = transform.position;

        //Vector3 diff = finalPos - startingPos;

        while (curTime < totalTime)
        {
            curTime += Time.deltaTime;

            transform.position = Vector3.Lerp(transform.position, finalPos, Mathf.Clamp01(curTime / totalTime)) ;

            yield return null;
        }
    }
}