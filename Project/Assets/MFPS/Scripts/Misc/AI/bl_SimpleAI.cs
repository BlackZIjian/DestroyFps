using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class bl_SimpleAI : bl_MonoBehaviour {

    public Transform Target;
    [Space(5)]
    [Header("AI")]     
    public float PatrolRadius = 20f; //Radius for get the random point
    public float LookRange = 25.0f;   //when the AI starts to look at the player
    public float FollowRange = 10.0f;       //when the AI starts to chase the player
    public float AttackRange = 2;         // when the AI stars to attack the player
    public float LosseRange = 50f; 
    public float RotationLerp = 6.0f;
    [Range(10, 500)] public int Health = 100;

    [Space(5)]
    [Header("Attack")] 
    public float AttackRate = 3;
    public float Damage = 20; // The damage AI give
    [Space(5)]
    [Header("AutoTargets")] 
    public List<Transform> PlayersInRoom = new List<Transform>();//All Players in room
    public float UpdatePlayerEach = 5f;

    [Header("References")]
    [SerializeField] private GameObject DeathEffect;

    //Privates
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this
    private float attackTime;
    private UnityEngine.AI.NavMeshAgent Agent = null;
    private bool death = false;
    private bool personal = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        InvokeRepeating("UpdateList", 1, UpdatePlayerEach);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting && PhotonNetwork.isMasterClient)//only masterclient can send information
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Network player, receive data
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        attackTime = Time.time;
    }

    public void DoDamage(int damage, int viewID)
    {
        if (death)
            return;

        photonView.RPC("RpcDoDamage", PhotonTargets.All, damage, viewID);
    }

    [PunRPC]
    void RpcDoDamage(int damage,int viewID)
    {
        if (death)
            return;

        Health -= damage;

        if(viewID == bl_GameManager.m_view)//if was me that make damage
        {
            bl_UCrosshair.Instance.OnHit();
        }

        if(Health > 0)
        {
            if(Target == null)
            {
                personal = true;
                Target = FindPlayerRoot(viewID).transform;
                if (PhotonNetwork.isMasterClient) { Agent.speed = Agent.speed * 5; }
                /*PhotonView view = PhotonView.Find(viewID);
                if (view != null)
                {
                    photonView.RPC("SyncTargetAI", PhotonTargets.OthersBuffered, view.viewID);
                }
                else
                {
                    personal = false;
                    Target = null;
                }*/
            }
        }
        else
        {
            death = true;
            if (DeathEffect != null) { Instantiate(DeathEffect, transform.position, Quaternion.identity); }
            this.photonView.RPC("DestroyRpc", PhotonTargets.AllBuffered);
        }
    }

    [PunRPC]
    public IEnumerator DestroyRpc()
    {
        GameObject.Destroy(this.gameObject);
        yield return 0; // if you allow 1 frame to pass, the object's OnDestroy() method gets called and cleans up references.
        PhotonNetwork.UnAllocateViewID(this.photonView.viewID);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!PhotonNetwork.isMasterClient)//if not master client, then get position from server
        {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 7);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 7);
        }

        if (PhotonNetwork.isMasterClient)//All AI logic only master client can make it functional
        {
            if (Target == null)
            {
                //Get the player most near
                for (int i = 0; i < PlayersInRoom.Count; i++)
                {
                    if (PlayersInRoom[i] != null)
                    {
                        float Distance = Vector3.Distance(PlayersInRoom[i].position, transform.position);//if a player in range, get this

                        if (Distance < LookRange)//if in range
                        {
                            GetTarget(PlayersInRoom[i]);//get this player
                        }
                    }
                }
                //if target null yet, the patrol
                if (!Agent.hasPath)
                {
                    RandomPatrol();
                }
            }
        }

        if (Target != null)
        {
            float Distance = Vector3.Distance(Target.position, transform.position);        
            if (Distance < LookRange)//if in range
            {
                Look();
            }

            if (Distance > LookRange)//if the target not in the range, then patrol
            {
                GetComponent<Renderer>().material.color = Color.green;     // if you want the AI to be green when it not can see you.
                if (PhotonNetwork.isMasterClient)
                {
                    if (!Agent.hasPath)
                    {
                        RandomPatrol();
                    }
                }
            }

            if (PhotonNetwork.isMasterClient)
            {
                if (Distance < AttackRange)
                {
                    Attack();
                }
            }

            if (Distance < FollowRange || personal)
            {
                Follow();
            }

            if (PhotonNetwork.isMasterClient)
            {
                if (Distance >= LosseRange && !personal)
                {
                    photonView.RPC("ResetTarget", PhotonTargets.AllBuffered);
                    if (!Agent.hasPath)
                    {
                        RandomPatrol();
                    }
                }
            }
        }
        
    }
    /// <summary>
    /// If player not in range then the AI patrol in map
    /// </summary>
    void RandomPatrol()
    {
        Vector3 randomDirection = Random.insideUnitSphere * PatrolRadius;
        randomDirection += transform.position;
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, PatrolRadius, 1);
        Vector3 finalPosition = hit.position;
        Agent.SetDestination(finalPosition);
    }
/// <summary>
/// 
/// </summary>
    void Look()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
        if (PhotonNetwork.isMasterClient)
        {
            Quaternion rotation = Quaternion.LookRotation(Target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationLerp);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void Follow()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Agent.destination = Target.transform.position;
        }
        GetComponent<Renderer>().material.color = Color.red;     
    }
    /// <summary>
    /// 
    /// </summary>
    void Attack()
    {
        if (Time.time > attackTime)
        {
            bl_PlayerDamageManager pdm = Target.root.GetComponent<bl_PlayerDamageManager>();
            if (pdm != null)
            {
                bl_OnDamageInfo di = new bl_OnDamageInfo();
                di.mActor = null;
                di.mDamage = Damage;
                di.mDirection = this.transform.position;
                di.mFrom = "AI";
                pdm.GetDamage(di);
                attackTime = Time.time + AttackRate;
                Debug.Log("Send Damage!");
            }
            else
            {
                Debug.Log("Can't found pdm in: " + Target.gameObject.name);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void GetTarget(Transform t)
    {
            Target = t;
            PhotonView view = GetPhotonView(Target.gameObject);
            if (view != null)
            {
                photonView.RPC("SyncTargetAI", PhotonTargets.OthersBuffered, view.viewID);
            }
            else
            {
                Debug.Log("This Target " + Target.name + "no have photonview");
            }
    }


    [PunRPC]
    void SyncTargetAI(int view)
    {
        Transform t = FindPlayerRoot(view).transform;
        if (t != null)
        {
            Target = t;
        }
    }

    [PunRPC]
    void ResetTarget()
    {
        Target = null;
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateList()
    {
        PlayersInRoom = AllPlayers;
    }
    /// <summary>
    /// 
    /// </summary>
    protected List<Transform> AllPlayers
    {
        get
        {
            List<Transform> list = new List<Transform>();
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                GameObject g = FindPhotonPlayer(p);
                if (g != null)
                {
                    list.Add(g.transform);
                }
            }
            return list;
        }
    }

    [PunRPC]
    void RpcSync(int _health)
    {
        Health = _health;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);
        if (PhotonNetwork.isMasterClient && newPlayer.ID != PhotonNetwork.player.ID)
        {
            photonView.RPC("RpcSync", newPlayer, Health);
        }
    }
}