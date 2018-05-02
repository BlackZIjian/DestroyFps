using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables

public class bl_PhotonGame : bl_PhotonHelper
{

    [HideInInspector] public bool hasPingKick = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PhotonNetwork.OnEventCall += OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEventCustom(byte eventCode, object content, int senderID)
    {
     /*   Hashtable hash = new Hashtable();
        hash = (Hashtable)content;*/
        switch (eventCode)
        {
            case PropertiesKeys.KickPlayerEvent:
                OnKick();
                break;
            case 1:

                break;
        }
    }

    public void OnPingKick()
    {
        hasPingKick = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void KickPlayer(PhotonPlayer p)
    {
        PhotonNetwork.RaiseEvent(PropertiesKeys.KickPlayerEvent, null, true, new RaiseEventOptions() { TargetActors = new int[] { p.ID } });
    }

    /// <summary>
    /// 
    /// </summary>
    void OnKick()
    {
        if (PhotonNetwork.inRoom)
        {
            PlayerPrefs.SetInt(PropertiesKeys.KickKey, 1);
            PhotonNetwork.LeaveRoom();
        }
    }

    private static bl_PhotonGame _instance;
    public static bl_PhotonGame Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PhotonGame>(); }
            return _instance;
        }
    }
}