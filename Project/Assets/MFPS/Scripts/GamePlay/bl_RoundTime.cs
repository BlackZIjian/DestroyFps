/////////////////////////////////////////////////////////////////////////////////
///////////////////////////////bl_RoundTime.cs///////////////////////////////////
///////////////Use this to manage time in rooms//////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Lovatto Studio///////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using UnityEngine.UI;

public class bl_RoundTime : bl_MonoBehaviour
{
    /// <summary>
    /// mode of the round room
    /// </summary>
    public RoundStyle m_RoundStyle;
    /// <summary>
    /// expected duration in round (automatically obtained)
    /// </summary>
    [HideInInspector] public int RoundDuration;
    [HideInInspector]
    public float CurrentTime;

    //private
    private const string StartTimeKey = "RoomTime";       // the name of our "start time" custom property.
    private float m_Reference;
    private int m_countdown = 10;
    private bool isFinish = false;
    private bl_RoomSettings RoomSettings;
    private bl_RoomMenu RoomMenu;
    private bl_GameManager m_Manager = null;
    private Text TimeText;
    private bl_UIReferences UIReferences;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        RoomSettings = this.GetComponent<bl_RoomSettings>();
        RoomMenu = GetComponent<bl_RoomMenu>();
        m_Manager = GetComponent<bl_GameManager>();
        UIReferences = bl_UIReferences.Instance;
        TimeText = UIReferences.TimeText;
        if (!PhotonNetwork.connected)
        {
            bl_UtilityHelper.LoadLevel(RoomMenu.LeftRoomReturnScene);
            return;
        }

        GetTime();
    }

    /// <summary>
    /// get the current time and verify if it is correct
    /// </summary>
    void GetTime()
    {
        RoundDuration = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.TimeRoomKey];
        if (PhotonNetwork.isMasterClient)
        {
            m_Reference = (float)PhotonNetwork.time;

            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.room.SetCustomProperties(startTimeProp);
        }
        else
        {
            m_Reference = (float)PhotonNetwork.room.CustomProperties[StartTimeKey];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        float t_time = RoundDuration - ((float)PhotonNetwork.time - m_Reference);
        if (t_time > 0)
        {
            CurrentTime = t_time;
        }
        else if (t_time <= 0.001 && GetTimeServed == true)//Round Finished
        {
            CurrentTime = 0;
            FinishRound();
        }
        else//even if I do not photonnetwork.time then obtained to regain time
        {
            Refresh();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishRound()
    {
        if (!PhotonNetwork.connected)
            return;

        bl_EventHandler.OnRoundEndEvent();
        if (!isFinish)
        {
            isFinish = true;
            if (RoomMenu) { RoomMenu.isFinish = true; }
            if (m_Manager) { m_Manager.GameFinish = true; }
            UIReferences.SetCountDown(m_countdown);
            InvokeRepeating("countdown", 1, 1);
            UIReferences.SetFinalText(m_RoundStyle, RoomSettings.GetWinnerName);
            bl_UCrosshair.Instance.Show(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        TimeControll();
    }

    /// <summary>
    /// 
    /// </summary>
    void TimeControll()
    {
        int normalSecons = 60;
        float remainingTime = Mathf.CeilToInt(CurrentTime);
        int m_Seconds = Mathf.FloorToInt(remainingTime % normalSecons);
        int m_Minutes = Mathf.FloorToInt((remainingTime / normalSecons) % normalSecons);
        string t_time = bl_UtilityHelper.GetTimeFormat(m_Minutes, m_Seconds);

        if (TimeText != null)
        {
            TimeText.text = t_time;
        }
    }

    /// <summary>
    /// with this fixed the problem of the time lag in the Photon
    /// </summary>
    void Refresh()
    {
        if (PhotonNetwork.room == null)
            return;

        if (PhotonNetwork.isMasterClient)
        {
            m_Reference = (float)PhotonNetwork.time;

            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.room.SetCustomProperties(startTimeProp);
        }
        else
        {
            m_Reference = (float)PhotonNetwork.room.CustomProperties[StartTimeKey];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void countdown()
    {
        m_countdown--;
        UIReferences.SetCountDown(m_countdown);
        if (m_countdown <= 0)
        {
            FinishGame();
            CancelInvoke("countdown");
            m_countdown = 10;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FinishGame()
    {
        bl_UtilityHelper.LockCursor(false);
        if (m_RoundStyle == RoundStyle.OneMacht)
        {
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                bl_UtilityHelper.LoadLevel(0);
            }
        }
        if (m_RoundStyle == RoundStyle.Rounds)
        {
            GetTime();
            if (RoomSettings)
            {
                RoomSettings.ResetRoom();
            }
            isFinish = false;

            if (m_Manager)
            {
                m_Manager.GameFinish = false;
                m_Manager.DestroyPlayer(true);
            }
            if (RoomMenu != null)
            {
                RoomMenu.isFinish = false;
                RoomMenu.isPlaying = false;
                bl_UtilityHelper.LockCursor(false);
            }
            UIReferences.ResetRound();
            m_countdown = 10;
        }
    }

    bool GetTimeServed
    {
        get
        {
            bool m_bool = false;
            if (Time.timeSinceLevelLoad > 7)
            {
                m_bool = true;
            }
            return m_bool;
        }
    }
}