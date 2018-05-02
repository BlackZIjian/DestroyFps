//////////////////////////////////////////////////////////////////////////////
// PlayerProperties.cs
//
// this facilitates access to properties 
// more authoritatively for each photon player, ex: PhotonNetwork.player.GetKills();
//
//                       LovattoStudio
//////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

static class PlayerProperties
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="ScoreToAdd"></param>
    public static void PostScore(this PhotonPlayer player, int ScoreToAdd = 0)
    {
        int current = player.GetPlayerScore();
        current = current + ScoreToAdd;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.ScoreKey] = current;

        player.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static int GetPlayerScore(this PhotonPlayer player)
    {
        int s = 0;

        if (player.CustomProperties.ContainsKey(PropertiesKeys.ScoreKey))
        {
            s = (int)player.CustomProperties[PropertiesKeys.ScoreKey];
            return s;
        }

        return s;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static int GetKills(this PhotonPlayer p)
    {
        int k = 0;
        if (p.CustomProperties.ContainsKey(PropertiesKeys.KillsKey))
        {
            k = (int)p.CustomProperties[PropertiesKeys.KillsKey];
            return k;
        }
        return k;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static int GetDeaths(this PhotonPlayer p)
    {
        int d = 0;
        if (p.CustomProperties.ContainsKey(PropertiesKeys.DeathsKey))
        {
            d = (int)p.CustomProperties[PropertiesKeys.DeathsKey];
            return d;
        }
        return d;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <param name="kills"></param>
    public static void PostKill(this PhotonPlayer p, int kills)
    {
        int current = p.GetKills();
        current = current + kills;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.KillsKey] = current;

        p.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <param name="deaths"></param>
    public static void PostDeaths(this PhotonPlayer p, int deaths)
    {
        int current = p.GetDeaths();
        current = current + deaths;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.DeathsKey] = current;

        p.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }

    public static int GetRoomScore(this Room room,Team team)
    {
        object teamId;
        if (team == Team.Delta)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team1Score, out teamId))
            {
                return (int)teamId;
            }
        }else if(team == Team.Recon)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team2Score, out teamId))
            {
                return (int)teamId;
            }
        }

        return 0;
    }

    public static Team GetPlayerTeam(this PhotonPlayer p)
    {
        object teamId;
        string t = (string)p.CustomProperties[PropertiesKeys.TeamKey];
        if (p.CustomProperties.TryGetValue(PropertiesKeys.TeamKey, out teamId))
        {
            switch ((string)teamId)
            {
                case "Recon":
                    return Team.Recon;
                case "Delta":
                    return Team.Delta;
                case "All":
                    return Team.All;
                case "None":
                    return Team.None;

            }
        }
        return Team.None;
    }

    public static string GetTeamName(this Team t)
    {
        switch (t)
        {
            case Team.Delta:
                return bl_GameData.Instance.Team1Name;
            case Team.Recon:
                return bl_GameData.Instance.Team2Name;
            default:
                return "Global";
        }
    }

    public static Color GetTeamColor(this Team t)
    {
        switch (t)
        {
            case Team.Delta:
                return bl_GameData.Instance.Team1Color;
            case Team.Recon:
                return bl_GameData.Instance.Team2Color;
            default:
                return Color.white;
        }
    }

    private const string PLAYER_CLASS_KEY = "{0}.playerclass";
    public static void SavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        PlayerPrefs.SetInt(key, (int)pc);
    }

    public static PlayerClass GetSavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        int id = PlayerPrefs.GetInt(key, 0);
        PlayerClass pclass = PlayerClass.Assault;
        switch (id)
        {
            case 1:
                pclass = PlayerClass.Recon;
                break;
            case 2:
                pclass = PlayerClass.Support;
                break;
            case 3:
                pclass = PlayerClass.Engineer;
                break;
        }
        return pclass;
    }

    public static string NickNameAndRole(this PhotonPlayer p)
    {
        object role = 0;
        if (p.CustomProperties.TryGetValue(PropertiesKeys.UserRole, out role))
        {
            return string.Format("{0}{1}", p.NickName, (string)role);
        }
        return p.NickName;
    }
}