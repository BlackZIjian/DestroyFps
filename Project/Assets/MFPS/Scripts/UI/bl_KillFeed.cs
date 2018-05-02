using UnityEngine;
using UnityEngine.UI;

public class bl_KillFeed : bl_PhotonHelper
{

    //private
    private bl_RoomSettings setting;
    private bl_UIReferences UIReference;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        setting = this.GetComponent<bl_RoomSettings>();
        UIReference = FindObjectOfType<bl_UIReferences>();
        if (PhotonNetwork.room != null)
        {
            OnJoined();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.OnKillFeed += this.OnKillFeed;
        bl_EventHandler.OnKill += this.NewKill;
    }
    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.OnKillFeed -= this.OnKillFeed;
        bl_EventHandler.OnKill -= this.NewKill;
    }

    /// <summary>
    /// Called this when a new kill event 
    /// </summary>
    public void OnKillFeed(string t_Killer, string t_Killed, string t_HowKill, string t_team, int t_GunID, int isHeatShot)
    {
        photonView.RPC("AddNewKillFeed", PhotonTargets.All, t_Killer, t_Killed, t_HowKill, t_team.ToString(), t_GunID, isHeatShot);
    }
    /// <summary>
    /// Player Joined? sync
    /// </summary>
    void OnJoined()
    {
        photonView.RPC("AddNewKillFeed", PhotonTargets.Others, PhotonNetwork.player.NickName, bl_GameTexts.JoinedInMatch, "", "", 777, 20);
    }

    [PunRPC]
    void AddNewKillFeed(string t_Killer, string t_Killed, string t_HowKill, string m_team, int t_GunID, int isHeatShot)
    {
        Color KillerColor = new Color(1, 1, 1, 1);
        Color KilledColor = new Color(1, 1, 1, 1);
        if (setting.m_GameMode != GameMode.FFA)
        {
            if (m_team == Team.Delta.ToString())
            {
                KillerColor = isMy(t_Killer) ? ColorKeys.MineColor : bl_GameData.Instance.Team1Color;
                KilledColor = isMy(t_Killed) ? ColorKeys.MineColor : bl_GameData.Instance.Team2Color;
            }
            else if (m_team == Team.Recon.ToString())
            {
                KillerColor = isMy(t_Killer) ? ColorKeys.MineColor : bl_GameData.Instance.Team2Color;
                KilledColor = isMy(t_Killed) ? ColorKeys.MineColor : bl_GameData.Instance.Team1Color;
            }
            else
            {
                KilledColor = Color.white;
                KillerColor = Color.white;
            }
        }

        if (string.IsNullOrEmpty(t_Killer)) { t_Killer = bl_GameTexts.ServerMesagge; }

        KillFeed newKillFeed = new KillFeed();
        newKillFeed.Killer = t_Killer;
        newKillFeed.Killed = t_Killed;
        newKillFeed.HowKill = t_HowKill;
        newKillFeed.KilledColor = KilledColor;
        newKillFeed.KillerColor = KillerColor;
        newKillFeed.HeatShot = (isHeatShot == 1) ? true : false;
        newKillFeed.GunID = t_GunID;

        UIReference.SetKillFeed(newKillFeed);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        AddNewKillFeed(otherPlayer.NickName, "", bl_GameTexts.LeftOfMatch, "", 777, 20);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private bool isMy(string n)
    {
        bool b = false;
        if (n == LocalName)
        {
            b = true;
        }
        return b;
    }

    /// <summary>
    /// Show a local ui when out killed other player
    /// </summary>
    protected virtual void NewKill(string m_type, float t_amount)
    {

    }
}