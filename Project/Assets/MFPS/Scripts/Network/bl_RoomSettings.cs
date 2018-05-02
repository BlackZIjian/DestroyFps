using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class bl_RoomSettings : bl_MonoBehaviour
{

    [Header("References")]
    public GameObject CTFObjects;
    [SerializeField] private GameObject TwoTeamsScoreboard;
    [SerializeField] private GameObject FFAScoreboard;

    //Private
    [HideInInspector] public int Team_1_Score = 0;
    [HideInInspector] public int Team_2_Score = 0;
    private bl_RoomMenu RoomMenu;
    private bl_RoundTime TimeManager;
    [HideInInspector] public GameMode m_GameMode = GameMode.FFA;
    private bl_UIReferences UIReferences;
    private int MaxKills = 0;
    private bl_GameData GameData;
#if BDGM
    private int MaxBDRounds;
    private bl_BombDefuse BombDefuse;
#endif
#if CP
        private bl_CoverPointRoom CoverPoint;
#endif

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (!PhotonNetwork.connected || PhotonNetwork.room == null)
            return;

        RoomMenu = base.GetComponent<bl_RoomMenu>();
        TimeManager = base.GetComponent<bl_RoundTime>();
        UIReferences = bl_UIReferences.Instance;
        GameData = bl_GameData.Instance;
#if BDGM
        BombDefuse = FindObjectOfType<bl_BombDefuse>();
#endif
#if CP
         CoverPoint = FindObjectOfType<bl_CoverPointRoom>();
#endif
        ResetRoom();
        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRoom()
    {
        //Initialize new properties where the information will stay Room
        if (PhotonNetwork.isMasterClient)
        {
            Hashtable setTeamScore = new Hashtable();
            setTeamScore.Add(PropertiesKeys.Team1Score, 0);
            PhotonNetwork.room.SetCustomProperties(setTeamScore);

            Hashtable setTeam2Score = new Hashtable();
            setTeam2Score.Add(PropertiesKeys.Team2Score, 0);
            PhotonNetwork.room.SetCustomProperties(setTeam2Score);
        }
        //Initialize new properties where the information will stay Players
        Hashtable PlayerTeam = new Hashtable();
        PlayerTeam.Add(PropertiesKeys.TeamKey, Team.None.ToString());
        PhotonNetwork.player.SetCustomProperties(PlayerTeam);

        Hashtable PlayerKills = new Hashtable();
        PlayerKills.Add(PropertiesKeys.KillsKey, 0);
        PhotonNetwork.player.SetCustomProperties(PlayerKills);

        Hashtable PlayerDeaths = new Hashtable();
        PlayerDeaths.Add(PropertiesKeys.DeathsKey, 0);
        PhotonNetwork.player.SetCustomProperties(PlayerDeaths);

        Hashtable PlayerScore = new Hashtable();
        PlayerScore.Add(PropertiesKeys.ScoreKey, 0);
        PhotonNetwork.player.SetCustomProperties(PlayerScore);

        Hashtable PlayerRole = new Hashtable();
        PlayerRole.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        PhotonNetwork.player.SetCustomProperties(PlayerRole);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        string Mode = GetGameMode.ToString();
        if (Mode == GameMode.FFA.ToString())
        {
            m_GameMode = GameMode.FFA;
            CTFObjects.SetActive(false);
            FFAScoreboard.SetActive(true);
            TwoTeamsScoreboard.SetActive(false);
#if BDGM
            if (BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(false);
            }
#endif
#if CP
        CoverPoint.CPObjects.SetActive(false);
#endif
        }
        else if (Mode == GameMode.TDM.ToString())
        {
            m_GameMode = GameMode.TDM;
            CTFObjects.SetActive(false);
            TwoTeamsScoreboard.SetActive(true);
            FFAScoreboard.SetActive(false);
#if BDGM
            if (BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(false);
            }
#endif
#if CP
        CoverPoint.CPObjects.SetActive(false);
#endif
        }
        else if (Mode == GameMode.CTF.ToString())
        {
            m_GameMode = GameMode.CTF;
            CTFObjects.SetActive(true);
            TwoTeamsScoreboard.SetActive(true);
            FFAScoreboard.SetActive(false);
#if BDGM
            if (BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(false);
            }
#endif
#if CP
        CoverPoint.CPObjects.SetActive(false);
#endif
        }
#if BDGM
        else if (Mode == GameMode.SND.ToString())
        {
            m_GameMode = GameMode.SND;
            CTFObjects.SetActive(false);
           if(BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(true);
            }
#if CP
        CoverPoint.CPObjects.SetActive(false);
#endif
            TwoTeamsScoreboard.SetActive(true);
            FFAScoreboard.SetActive(false);
        }
#endif
#if CP
        else if (Mode == GameMode.CP.ToString())
        {
            m_GameMode = GameMode.CP;
            CTFObjects.SetActive(false);
#if BDGM
           if(BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(false);
            }
#endif
            TwoTeamsScoreboard.SetActive(false);
            FFAScoreboard.SetActive(false);
            CoverPoint.CPObjects.SetActive(true);
            bl_UIReferences.Instance.MaxKillsUI.SetActive(false);
        }
#endif

#if BDGM
                MaxBDRounds = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.BombDefuseRounds];
#endif
        TimeManager.m_RoundStyle = (RoundStyle)PhotonNetwork.room.CustomProperties[PropertiesKeys.RoomRoundKey];
        RoomMenu.AutoTeamSelection = (bool)PhotonNetwork.room.CustomProperties[PropertiesKeys.TeamSelectionKey];
        MaxKills = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.RoomMaxKills];

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(PropertiesKeys.Quality, 3));
        AudioListener.volume = PlayerPrefs.GetFloat(PropertiesKeys.Volume, 1);
        int i = PlayerPrefs.GetInt(PropertiesKeys.Aniso, 2);
        if (i == 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        else if (i == 1)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.Awake();
        bl_EventHandler.OnRoundEnd += this.OnRoundEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnRoundEnd -= this.OnRoundEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (m_GameMode == GameMode.FFA)
        {
            string t_PlayerStart = RoomMenu.PlayerStar;
            UIReferences.FFAScoreText.text = "PLAYER START:\n<size=14><color=black>" + t_PlayerStart + "</color></size>";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        base.OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score))
        {
            Team_1_Score = PhotonNetwork.room.GetRoomScore(Team.Delta);
        }
        else if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            Team_2_Score = PhotonNetwork.room.GetRoomScore(Team.Recon);
        }
        if (UIReferences != null)
        {
            UIReferences.Team1ScoreText.text = Team_1_Score.ToString();
            UIReferences.Team2ScoreText.text = Team_2_Score.ToString();
        }

        CheckScore();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckScore()
    {
        if (MaxKills <= 0) return;

        if (GetGameMode != GameMode.FFA)
        {
            if (Team_1_Score >= MaxKills)
            {
                TimeManager.FinishRound();
            }
            if (Team_2_Score >= MaxKills)
            {
                TimeManager.FinishRound();
            }
        }
        else
        {
            if(RoomMenu.GetStartPlayerScore >= MaxKills)
            {
                TimeManager.FinishRound();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        StartCoroutine(DisableUI());        
    }

    /// <summary>
    /// 
    /// </summary>
    public string GetWinnerName
    {
        get
        {
            if (m_GameMode != GameMode.FFA)
            {
                if (Team_1_Score > Team_2_Score)
                {
                    return GameData.Team1Name;
                }
                else if (Team_1_Score < Team_2_Score)
                {
                    return GameData.Team2Name;
                }
                else
                {
                    return bl_GameTexts.NoOneWonName;
                }
            }
            else
            {
                return RoomMenu.PlayerStar;
            }
        }
    }

#if BDGM
    public void ResetOnDeath()
    {
        if (m_GameMode == GameMode.SND)
        {
            bl_BombDefuse[] bp = FindObjectsOfType<bl_BombDefuse>();
            if (bp.Length > 0)
            {
                for (int i = 0; i < bp.Length; i++)
                {
                    bp[i].PlayerReset();
                }
            }
            Debug.Log("Reset PlantBombs");
        }
    }

    public bool CheckForBDRound
    {
        get
        {
            int deltaScore = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.Team1Score];
            int reconScore = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.Team2Score];

            if (deltaScore >= MaxBDRounds)
            {
                return false;
            }
            if (reconScore >= MaxBDRounds)
            {
                return false;
            }

            return true;
        }
    }
#endif

    IEnumerator DisableUI()
    {
        yield return new WaitForSeconds(10);
    }
}