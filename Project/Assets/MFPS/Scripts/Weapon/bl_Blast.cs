//////////////////////////////////////////////////////////////////////////////
// bl_Blast.cs
//
// This contain the logic of the explosions
// determines the objectives that are punished,
// and calculates the precise damage
//                       LovattoStudio
//////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_Blast : bl_PhotonHelper {
	 /// <summary>
	 /// This is assigned auto
	 /// </summary>
    public float explosionDamage = 50f;
    /// <summary>
    /// range of the explosion generates damage
    /// </summary>
    public float explosionRadius = 50f;
    /// <summary>
    /// the time fire particles disappear
    /// </summary>
    public float DisappearIn = 3f;

    [HideInInspector]
    public int WeaponID;
    [HideInInspector]
    public string WeaponName;
    [HideInInspector]
    public bool isNetwork = false;

    /// <summary>
    /// is not remote take damage
    /// </summary>
    void Start()
    {
        if (!isNetwork)
        {
            ApplyDamage();
            ApplyShake();
        }
        StartCoroutine(Init());
    }
    /// <summary>
    /// applying impact damage from the explosion to enemies
    /// </summary>
    private void ApplyDamage()
    {
        List<PhotonPlayer> playersInRange = this.GetPlayersInRange();
        if (playersInRange != null && playersInRange.Count > 0)
        {
            foreach (PhotonPlayer player in playersInRange)
            {
                if (player != null)
                {
                    GameObject p = FindPhotonPlayer(player);
                    if (p != null)
                    {
                        bl_PlayerDamageManager pdm = p.transform.root.GetComponent<bl_PlayerDamageManager>();
                        bl_OnDamageInfo odi = new bl_OnDamageInfo();
                        odi.mDamage = CalculatePlayerDamage(player);
                        odi.mDirection = this.transform.position;
                        odi.mFrom = PhotonNetwork.player.NickName;
                        odi.mHeatShot = false;
                        odi.mWeapon = WeaponName;
                        odi.mWeaponID = WeaponID;
                        odi.mActor = PhotonNetwork.player;

                        pdm.GetDamage(odi);
                    }
                    else
                    {
                        Debug.LogError("This Player " + player.NickName + " is not found");
                    }
                }
            }
        }
    }
    /// <summary>
    /// When Explosion is local, and take player hit
    /// Send only shake movement
    /// </summary>
    void ApplyShake()
    {
        if (isMyInRange() == true)
        {
            bl_EventHandler.PlayerLocalShakeEvent(0.3f, 1, 0.25f);
        }
    }

    /// <summary>
    /// calculate the damage it generates, based on the distance
    /// between the player and the explosion
    /// </summary>
    private int CalculatePlayerDamage(PhotonPlayer p)
    {
        if (GetGameMode != GameMode.FFA)
        {
            if (bl_GameData.Instance.SelfGrenadeDamage && p == PhotonNetwork.player)
            {

            }
            else
            {
                if ((string)p.CustomProperties[PropertiesKeys.TeamKey] == myTeam)
                {
                    return 0;
                }
            }
        }

        float distance = Vector3.Distance(base.transform.position, FindPhotonPlayer(p).transform.position);
        return Mathf.Clamp((int)(this.explosionDamage * ((this.explosionRadius - distance) / this.explosionRadius)), 0, (int)this.explosionDamage);
    }

    /// <summary>
    /// get players who are within the range of the explosion
    /// </summary>
    /// <returns></returns>
    private List<PhotonPlayer> GetPlayersInRange()
    {
        List<PhotonPlayer> list = new List<PhotonPlayer>();
        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            GameObject player = FindPhotonPlayer(p);
            if(player == null)
                return null;

            if (GetGameMode != GameMode.FFA)
            {
                if (p != PhotonNetwork.player)
                {
                    if ((string)p.CustomProperties[PropertiesKeys.TeamKey] != myTeam && (Vector3.Distance(base.transform.position, player.transform.position) <= this.explosionRadius))
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (bl_GameData.Instance.SelfGrenadeDamage)
                    {
                        if ((Vector3.Distance(base.transform.position, player.transform.position) <= this.explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                }
            }
            else
            {
                if (p != PhotonNetwork.player)
                {
                    if ((Vector3.Distance(transform.position, player.transform.position) <= explosionRadius))
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (bl_GameData.Instance.SelfGrenadeDamage)
                    {
                        if ((Vector3.Distance(base.transform.position, player.transform.position) <= this.explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                }
            }
        }
        return list;
    }
    /// <summary>
    /// Calculate if player local in explosion radius
    /// </summary>
    /// <returns></returns>
    private bool isMyInRange()
    {
        GameObject p = FindPhotonPlayer(PhotonNetwork.player);

        if (p == null)
        {
            return false;
        }
        if ((Vector3.Distance(this.transform.position, p.transform.position) <= this.explosionRadius))
        {
            return true;
        }
        return false;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Init()
    {      
        yield return new WaitForSeconds(DisappearIn / 2);
        Destroy(gameObject);
    }
}