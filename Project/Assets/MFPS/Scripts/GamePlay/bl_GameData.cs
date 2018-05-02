using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class bl_GameData : ScriptableObject
{

    [Header("Game Settings")]
    public int ScorePerKill = 50;
    public int HeadShotScoreBonus = 25;
    public bool SelfGrenadeDamage = true;
    public bool UseLobbyChat = true;
    public bool BulletTracer = false;
    public bool DropGunOnDeath = true;
    public bool HealthRegeneration = true;
    [Range(1, 10)] public float PlayerRespawnTime = 5.0f;
    public bool CanChangeTeam = false;
    public int MaxChangeTeamTimes = 3;
    public bool KillCamStatic = true;

    [Header("Teams")]
    public string Team1Name = "Delta";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Recon";
    public Color Team2Color = Color.green;

    [Header("Weapons")]
    public List<bl_GunInfo> AllWeapons = new List<bl_GunInfo>();

    [Header("Players")]
    public GameObject Player1;
    public GameObject Player2;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();

   [HideInInspector] public GameTeamInfo CurrentTeamUser = null;

    public bl_GunInfo GetWeapon(int ID)
    {
        if (ID < 0 || ID > AllWeapons.Count - 1)
            return AllWeapons[0];

        return AllWeapons[ID];
    }

    public string[] AllWeaponStringList()
    {
        return AllWeapons.Select(x => x.Name).ToList().ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    public int CheckPlayerName(string pName)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (pName == GameTeam[i].UserName)
            {
                return 1;
            }
        }
        if (pName.Contains('[') || pName.Contains('{'))
        {
            return 2;
        }
        CurrentTeamUser = null;
        return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CheckPasswordUse(string PName, string Pass)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (PName == GameTeam[i].UserName)
            {
               if(Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public string RolePrefix
    {
        get
        {
            if (CurrentTeamUser != null)
            {
                return string.Format(" <color=#{1}>[{0}]</color>", CurrentTeamUser.m_Role.ToString(), ColorUtility.ToHtmlStringRGBA(CurrentTeamUser.m_Color));
            }
            else
            {
                return string.Empty;
            }
        }
    }

    [System.Serializable]
    public class GameTeamInfo
    {
        public string UserName;
        public Role m_Role = Role.Moderator;
        public string Password;
        public Color m_Color;

        public enum Role
        {
            Admin = 0,
            Moderator = 1,
        }
    }

    private static bl_GameData m_Data;
    public static bl_GameData Instance
    {
        get
        {
            if(m_Data == null)
            {
                m_Data = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }
            return m_Data;
        }
    }
}