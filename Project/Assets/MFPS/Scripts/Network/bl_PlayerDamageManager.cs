//////////////////////////////////////////////////////////////////////////////
// bl_PlayerDamageManager.cs
//
// this contains all the logic of the player health
// This is enabled locally or remotely
//                      Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using UnityEngine.UI;

public class bl_PlayerDamageManager : bl_MonoBehaviour
{

    [HideInInspector] public bool DamageEnabled = true;
    [Header("Settings")]
    //Current Player Health
    [Range(0,100)] public float health = 100;
	[Range(1, 100)] public float maxHealth = 100;
    [Range(1, 10)] public float StartRegenerateIn = 4f;
    [Range(1, 10)] public float RegenerationSpeed = 3f;

    [Header("GUI")]
    public Texture2D Blood;
    public Texture2D PaintFlash;
    /// <summary>
    /// Color of vignette effect when take damage
    /// </summary>
    public Color PaintFlashColor;
    /// <summary>
    /// Color of UI when player health is low
    /// </summary>
    public Color LowHealtColor = new Color(0.9f, 0, 0);
    private Color CurColor = new Color(0, 0, 0);
    private float m_alpha = 0.0f;
    /// <summary>
    /// Blood Screen Fade speed
    /// </summary>
    public float m_FadeSpeed = 3;
    [Range(0.0f, 2.0f)]
    public float m_UIIntensity = 0.2f;
    [Header("Shake")]
    [Range(0.0f, 1.0f)]
    public float ShakeTime = 0.07f;
    [Range(0.01f, 1f)]
    public float ShakeAmount = 2.5f;

    [Header("Effects")]
    public AudioClip[] HitsSound;

    [Header("References")]
    public GameObject m_Ragdoll;
    public bl_BodyPartManager mBodyManager;

    private Text HealthTextUI;
    const string FallMethod = "FallDown";
    private CharacterController m_CharacterController;
    private bool dead = false;
    private string m_LastShot;
    private bl_KillFeed KillFeed;
    private int ScorePerKill, ScorePerHeatShot;
    private bl_GameData GameData;
    private bl_DamageIndicator Indicator;
    private bl_PlayerSync PlayerSync;
    private bool HealthRegeneration = false;
    private float TimeToRegenerate = 4;
    private bl_GunManager GunManager;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_CharacterController = GetComponent<CharacterController>();
        KillFeed = FindObjectOfType<bl_KillFeed>();
        GameData = bl_GameData.Instance;
        Indicator = GetComponent<bl_DamageIndicator>();
        PlayerSync = GetComponent<bl_PlayerSync>();
        GunManager = transform.GetComponentInChildren<bl_GunManager>(true);
        HealthRegeneration = GameData.HealthRegeneration;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!isConnected)
            return;

        if (isMine)
        {
            bl_GameManager.isAlive = true;
            gameObject.name = PhotonNetwork.playerName;
            HealthTextUI = bl_UIReferences.Instance.HealthText;
        }

        health = maxHealth;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.Awake();
        if (this.isMine)
        {
            bl_GameManager.m_view = this.photonView.viewID;
            bl_EventHandler.OnPickUp += this.OnPickUp;
            bl_EventHandler.OnRoundEnd += this.OnRoundEnd;
            bl_EventHandler.OnDamage += this.GetDamage;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (this.isMine)
        {
            bl_EventHandler.OnPickUp -= this.OnPickUp;
            bl_EventHandler.OnRoundEnd -= this.OnRoundEnd;
            bl_EventHandler.OnDamage -= this.GetDamage;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (base.isMine)
        {
            if (m_alpha > 0.0f)
            {
                m_alpha = Mathf.Lerp(m_alpha, 0.0f, Time.deltaTime * m_FadeSpeed);
            }
            if (health <= 15)
            {
                CurColor = Color.Lerp(CurColor, LowHealtColor, Time.deltaTime * 8);
            }
            else
            {
                CurColor = Color.Lerp(CurColor, Color.white, Time.deltaTime * 8);
            }
            if (HealthTextUI != null)
            {
                HealthTextUI.text = Mathf.FloorToInt(health) + "<size=12>/" + maxHealth + "</size>";
            }
            if (HealthRegeneration)
            {
                RegenerateHealth();
            }
        }
    }

    /// <summary>
    /// Call this to make a new damage to the player
    /// </summary>
	//public void GetDamage (float t_damage,string t_from,string t_weapon,Vector3 t_direction,bool isHeatShot,int weapon_ID)
    public void GetDamage(bl_OnDamageInfo e)
    {
        if (!DamageEnabled)
            return;

        Debug.Log(" I been hit by " + e.mFrom + " for " + e.mDamage + " damage");
        photonView.RPC("SyncDamage", PhotonTargets.AllBuffered, e.mDamage, e.mFrom, e.mWeapon, e.mDirection, e.mHeatShot, e.mWeaponID, PhotonNetwork.player);

    }

    /// <summary>
    /// Call this when Player Take Damage From fall impact
    /// </summary>
    /// <param name="t_damage"></param>
    public void GetFallDamage(float t_damage)
    {
        Vector3 downpos = this.transform.TransformDirection(Vector3.down);
        photonView.RPC("SyncDamage", PhotonTargets.AllBuffered, t_damage, PhotonNetwork.player.NickName, FallMethod, -downpos, false, 5, PhotonNetwork.player);
    }

    /// <summary>
    /// Sync the Health of player
    /// </summary>
    [PunRPC]
    void SyncDamage(float t_damage, string t_from, string t_weapon, Vector3 m_direction, bool isHeatShot, int weaponID, PhotonPlayer m_sender)
    {
        if (dead)
            return;

        if (DamageEnabled)
        {
            if (health > 0)
            {
                if (isMine)
                {
                    m_alpha += (t_damage * m_UIIntensity);
                    bl_EventHandler.OnLocalPlayerShake(ShakeAmount, ShakeTime);
                    if (Indicator != null)
                    {
                        Indicator.AttackFrom(m_direction);
                    }
                    TimeToRegenerate = StartRegenerateIn;
                }
                else
                {
                    if (m_sender != null)
                    {
                        if (m_sender.NickName == base.LocalName)
                        {
                            bl_UCrosshair.Instance.OnHit();
                        }
                    }

                }
            }
            if (HitsSound.Length > 0 && t_weapon != FallMethod)//Audio effect of hit
            {
                AudioSource.PlayClipAtPoint(HitsSound[Random.Range(0, HitsSound.Length)], this.transform.position, 1.0f);
            }
        }
        if (health > 0)
        {
            m_LastShot = t_from;
            health -= t_damage;
        }

        if (health <= 0)
        {
            health = 0.0f;
            Die(m_LastShot, isHeatShot, t_weapon, weaponID, m_direction);

            if (isMine)
            {
                bl_GameManager.isAlive = false;
                bl_EventHandler.PlayerLocalDeathEvent();
            }

        }
    }

    /// <summary>
    /// Sync Health when pick up a med kit.
    /// </summary>
    [PunRPC]
    void PickUpHealth(float t_amount)
    {
        this.health = t_amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    /// <summary>
    /// Called This when player Die Logic
    /// </summary>
	void Die(string t_from, bool isHeat, string t_weapon, int w_id, Vector3 hitPos)
    {
        dead = true;
        m_CharacterController.enabled = false;
        if (!isMine)
        {
            mBodyManager.Ragdolled(hitPos);// convert into ragdoll the remote player
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        //Spawn ragdoll
        if (!isMine)// when player is not ours
        {
            if (m_LastShot == base.LocalName)
            {
                AddKill(isHeat, t_weapon, w_id);
            }
        }
        else//when is our
        {
            AddDeath();
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - 0.96f, transform.position.z);
            GameObject ragdoll = Instantiate(m_Ragdoll, pos, transform.rotation) as GameObject;
            Transform ngr = (bl_GameData.Instance.DropGunOnDeath) ? null : PlayerSync.NetGunsRoot;
            ragdoll.GetComponent<bl_Ragdoll>().RespawnAfter(GameData.PlayerRespawnTime, m_LastShot, ngr);
            if (t_from == base.LocalName)
            {
                bl_EventHandler.KillEvent(base.LocalName, "", bl_GameTexts.CommittedSuicide, myTeam, 5, 20);
            }
            if (bl_GameData.Instance.DropGunOnDeath)
            {
                GunManager.ThrwoCurrent();
            }
            StartCoroutine(DestroyThis());
        }
    }

    /// <summary>
    /// when we get a new kill, synchronize and add points to the player
    /// </summary>
    public void AddKill(bool m_heat, string m_weapon, int W_id)
    {
        //Send a new event kill feed
        string killfeedText = "";
        if (!m_heat)
        {
            killfeedText = string.Format("{0} [{1}]", bl_GameTexts.Killed, m_weapon);
            KillFeed.OnKillFeed(base.LocalName, this.gameObject.name, killfeedText, myTeam, W_id, 30);
            #if KILL_STREAK
            bl_KillNotifierUtils.GetManager.NewKill();
            #endif
        }
        else
        {
            killfeedText = string.Format("{0} [{1}]", bl_GameTexts.HeadShot, m_weapon);
            KillFeed.OnKillFeed(base.LocalName, this.gameObject.name, killfeedText, myTeam, 6, 30);
#if KILL_STREAK
            bl_KillNotifierUtils.GetManager.NewKill(true);
#endif
        }

        //Add a new kill and update information
        PhotonNetwork.player.PostKill(1);//Send a new kill
        //Add xp for score and update
        int score;
        //If heat shot will give you double experience
        if (m_heat)
        {
            bl_EventHandler.OnKillEvent(bl_GameTexts.KilledEnemy, GameData.ScorePerKill);
            bl_EventHandler.OnKillEvent(bl_GameTexts.HeatShotBonus, GameData.HeadShotScoreBonus);
            score = GameData.ScorePerKill + GameData.HeadShotScoreBonus;
        }
        else
        {
            bl_EventHandler.OnKillEvent(bl_GameTexts.KilledEnemy, GameData.ScorePerKill);
            score = GameData.ScorePerKill;
        }
        //Send to update score to player
        PhotonNetwork.player.PostScore(score);

        //TDM only if the score is updated
        if (GetGameMode == GameMode.TDM)
        {
            //Update ScoreBoard
            if (myTeam == Team.Delta.ToString())
            {
                int CurrentScore = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.Team1Score];
                CurrentScore++;
                Hashtable setTeamScore = new Hashtable();
                setTeamScore.Add(PropertiesKeys.Team1Score, CurrentScore);
                PhotonNetwork.room.SetCustomProperties(setTeamScore);
            }
            else if (myTeam == Team.Recon.ToString())
            {
                int CurrentScore = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.Team2Score];
                CurrentScore++;
                Hashtable setTeamScore = new Hashtable();
                setTeamScore.Add(PropertiesKeys.Team2Score, CurrentScore);
                PhotonNetwork.room.SetCustomProperties(setTeamScore);
            }
        }

    }
    /// <summary>
    /// When Player take a new Death synchronize Die Point
    /// </summary>
    public void AddDeath()
    {
        PhotonNetwork.player.PostDeaths(1);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        if (isMine)
        {
            DamageHUD();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RegenerateHealth()
    {
        if(health < maxHealth)
        {
            if (TimeToRegenerate <= 0)
            {
                health += Time.deltaTime * RegenerationSpeed;
            }
            else
            {
                TimeToRegenerate -= Time.deltaTime * 1.15f;
            }
            photonView.RPC("PickUpHealth", PhotonTargets.Others, health);
        }
    }

    /// <summary>
    /// Suicide player
    /// </summary>
    public void Suicide()
    {
        if (isMine && bl_GameManager.isAlive)
        {
            bl_OnDamageInfo e = new bl_OnDamageInfo();
            e.mDamage = 500;
            e.mFrom = base.LocalName;
            e.mDirection = transform.position;
            e.mHeatShot = false;
            e.mWeaponID = 5;
            GetDamage(e);
        }
    }

    /// <summary>
    /// show interface damage indicating that our player received damage
    /// </summary>
    void DamageHUD()
    {
        if (PaintFlash == null)
            return;
        if (Blood == null)
            return;

        GUI.color = new Color(PaintFlashColor.r, PaintFlashColor.g, PaintFlashColor.b, m_alpha);
        if (m_alpha > 0.0f)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), PaintFlash);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Blood);
        }
        GUI.color = Color.white;
    }  

    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(0.15f);
        PhotonNetwork.Destroy(this.gameObject);
    }

    /// <summary>
    /// This event is called when player pick up a med kit
    /// use PhotonTarget.OthersBuffered to save bandwidth
    /// </summary>
    /// <param name="amount"> amount for sum at current health</param>
    void OnPickUp(int amount)
    {
        if (photonView.isMine)
        {
            float newHealth = health + amount;
            health = newHealth;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            photonView.RPC("PickUpHealth", PhotonTargets.OthersBuffered, newHealth);

        }
    }

    [PunRPC]
    void RpcSyncHealth(float _h, PhotonMessageInfo info)
    {
        if (info.photonView.viewID == photonView.viewID)
        {
            health = _h;
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);
        photonView.RPC("RpcSyncHealth", newPlayer, health);

    }
    /// <summary>
    /// When round is end 
    /// desactive some functions
    /// </summary>
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}