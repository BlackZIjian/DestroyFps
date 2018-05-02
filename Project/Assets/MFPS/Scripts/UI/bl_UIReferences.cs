using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class bl_UIReferences : bl_PhotonHelper
{

    public RoomMenuState State = RoomMenuState.Init;

    [Header("ScoreBoard")]
    public Transform Team1Panel;
    public Transform Team2Panel;
    public Transform TeamFFAPanel;
    [SerializeField] private GameObject PlayerScoreboardPrefab;
    public Graphic[] Team1UI;
    public Graphic[] Team2UI;

    [Header("References")]
    public Text AmmoText;
    public Text ClipText;
    public Text HealthText;
    public Text Team1ScoreText;
    public Text Team2ScoreText;
    public Text FFAScoreText;
    public Text TimeText;
    public Text FireTypeText;
    public Image DamageIndicator;

    [SerializeField]private GameObject TDMScoreboardsRoot;
    [SerializeField]private GameObject FFAScoreboardRoot;
    public GameObject ScoreboardUI;
    public GameObject JoinBusttons;
    public GameObject OptionsUI;
    public GameObject FinalUI;
    [SerializeField]private GameObject LocalKillPrefab;
    [SerializeField]private GameObject KillfeedPrefab;
    [SerializeField]private Transform LocalKillPanel;
    [SerializeField]private Transform KillfeedPanel;
    [SerializeField]private Transform LeftNotifierPanel;
    [SerializeField]private GameObject LeftNotifierPrefab;
    [SerializeField]private GameObject FFAJoinButton;
    [SerializeField]private GameObject BottonMenu;
    [SerializeField]private GameObject TopMenu;
    [SerializeField]private Animator ClassButtonUI;
    [SerializeField]private GameObject SuicideButton;
    [SerializeField]private GameObject KillCamUI;
    [SerializeField] private GameObject AutoTeamUI;
    [SerializeField] private GameObject SpectatorUI;
    [SerializeField] private GameObject SpectatorButton;
    [SerializeField] private GameObject BlackScreen;
    [SerializeField] private GameObject PickUpUI;
    [SerializeField] private GameObject PingUI;
    [SerializeField] private GameObject FrameRateUI;
    public GameObject MaxKillsUI;
    [SerializeField] private GameObject ChangeTeamButton;
    [SerializeField]private Button[] ClassButtons;
    [SerializeField] private GameObject[] TDMJoinButton;
    [SerializeField]private Text FinalUIText;
    [SerializeField]private Text FinalCountText;
    [SerializeField] private Text FinalWinnerText;
    [SerializeField] private Text RoomNameText;
    [SerializeField] private Text GameModeText;
    [SerializeField] private Text MaxPlayerText;
    [SerializeField] private Text MaxKillsText;
    [SerializeField] private Text PickUpText;
    public Text Team1NameText, Team2NameText;
    [SerializeField] private Dropdown QualityDropdown;
    [SerializeField] private Dropdown AnisoDropdown;
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Slider SensitivitySlider;
    [SerializeField] private Slider SensitivityAimSlider;
    [SerializeField] private Slider FovSlider;
    public Canvas PlayerUICanvas;


    public Image SniperScope;

    private List<bl_PlayerScoreboardUI> cachePlayerScoreboard = new List<bl_PlayerScoreboardUI>();
    private bl_RoomMenu RoomMenu;
    private bool ChrAberration = true;
    private bool Antialiasing = true;
    private bool Bloom = true;
    private bool inTeam = false;
    private bool ssao = true;
    private int MaxRoomPing = 2000;
    private bool startKickingByPing = false;
    private int ChangeTeamTimes = 0;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.connected || PhotonNetwork.room == null)
            return;

        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        InvokeRepeating("UpdateScoreboard", 1, 1);
        SetUpUI();
        InvokeRepeating("CheckPing", 5, 5);
        bl_EventHandler.OnLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.OnPickUp += OnPicUpMedKit;
        bl_EventHandler.OnKitAmmo += OnPickUpAmmo;
        bl_EventHandler.OnKill += SetLocalKillFeed;
        bl_EventHandler.OnLocalPlayerDeath += OnLocalPlayerDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpUI()
    {
        ScoreboardUI.SetActive(true);
        OptionsUI.SetActive(false);
        SpectatorUI.SetActive(false);
        BlackScreen.SetActive(false);
        ChangeTeamButton.SetActive(false);
        if (!inTeam)
        {
            if (GetGameMode == GameMode.FFA)
            {
                foreach (GameObject g in TDMJoinButton) { g.SetActive(false); }
                TDMScoreboardsRoot.SetActive(false);
                FFAScoreboardRoot.SetActive(true);
            }
            else
            {
                FFAJoinButton.SetActive(false);
                TDMScoreboardsRoot.SetActive(true);
                FFAScoreboardRoot.SetActive(false);
            }
            JoinBusttons.SetActive(true);
        }

        if (RoomMenu.isPlaying)
        {
            SuicideButton.SetActive(true);
            ClassButtonUI.gameObject.SetActive(false);
        }
        else
        {
            TopMenu.SetActive(false);
            SuicideButton.SetActive(false);
            ClassButtonUI.gameObject.SetActive(true);
            LoadSettings();
        }

        Team1NameText.text = bl_GameData.Instance.Team1Name.ToUpper();
        Team2NameText.text = bl_GameData.Instance.Team2Name.ToUpper();
        if (PhotonNetwork.connected)
        {
            int MaxKills = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.RoomMaxKills];
            MaxKillsText.text = string.Format("{0} KILLS", MaxKills);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        CancelInvoke("UpdateScoreboard");
        bl_EventHandler.OnLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.OnPickUp -= OnPicUpMedKit;
        bl_EventHandler.OnKitAmmo -= OnPickUpAmmo;
        bl_EventHandler.OnKill -= SetLocalKillFeed;
        bl_EventHandler.OnLocalPlayerDeath -= OnLocalPlayerDeath;
    }

    void OnLocalPlayerDeath()
    {
        PickUpUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateScoreboard()
    {
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            if (PhotonNetwork.playerList[i].GetPlayerTeam() != Team.None)
            {
                if (ExistPlayerOnList(PhotonNetwork.playerList[i]))
                {
                    if (GetPlayerScoreboardUI(PhotonNetwork.playerList[i]) != null)
                    {
                        GetPlayerScoreboardUI(PhotonNetwork.playerList[i]).Refresh();
                    }
                }
                else
                {
                    GameObject newUIPS = Instantiate(PlayerScoreboardPrefab) as GameObject;
                    newUIPS.GetComponent<bl_PlayerScoreboardUI>().Init(PhotonNetwork.playerList[i], this);
                    Transform tp = null;
                    if (GetGameMode != GameMode.FFA)
                    {
                        tp = ((string)PhotonNetwork.playerList[i].CustomProperties[PropertiesKeys.TeamKey] == Team.Delta.ToString()) ? Team1Panel : Team2Panel;
                    }
                    else
                    {
                        tp = TeamFFAPanel;
                    }
                    newUIPS.transform.SetParent(tp, false);
                    cachePlayerScoreboard.Add(newUIPS.GetComponent<bl_PlayerScoreboardUI>());
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPing()
    {
        int ping = PhotonNetwork.GetPing();
        if(ping >= MaxRoomPing)
        {
            PingUI.SetActive(true);
            if (!startKickingByPing) { Invoke("StartPingKick", 11); startKickingByPing = true; }
        }
        else
        {
            PingUI.SetActive(false);
            if (startKickingByPing) { CancelInvoke("StartPingKick");  startKickingByPing = false; }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        RoomNameText.text = PhotonNetwork.room.Name.ToUpper();
        GameModeText.text = (string)PhotonNetwork.room.CustomProperties[PropertiesKeys.GameModeKey];
        int vs = (GetGameMode != GameMode.FFA) ? PhotonNetwork.room.MaxPlayers / 2 : PhotonNetwork.room.MaxPlayers - 1;
        MaxPlayerText.text = (GetGameMode != GameMode.FFA) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);
        MaxRoomPing = (int)PhotonNetwork.room.CustomProperties[PropertiesKeys.MaxPing];

        foreach(Graphic g in Team1UI) { g.color = bl_GameData.Instance.Team1Color; }
        foreach (Graphic g in Team2UI) { g.color = bl_GameData.Instance.Team2Color; }
    }

    public void SetKillFeed(KillFeed feed)
    {
        GameObject newkillfeed = Instantiate(KillfeedPrefab) as GameObject;
        newkillfeed.GetComponent<bl_KillFeedUI>().Init(feed);
        newkillfeed.transform.SetParent(KillfeedPanel, false);
        newkillfeed.transform.SetAsFirstSibling();
    }

    public void SetLocalKillFeed(string _type, float amount)
    {
        GameObject newkillfeed = Instantiate(LocalKillPrefab) as GameObject;
        newkillfeed.GetComponent<bl_LocalKillUI>().Init(_type, amount.ToString());
        newkillfeed.transform.SetParent(LocalKillPanel, false);
        newkillfeed.transform.SetAsFirstSibling();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowMenu(bool active)
    {
        ScoreboardUI.SetActive(active);
        BottonMenu.SetActive(active);
        TopMenu.SetActive(active);
        JoinBusttons.SetActive(false);
        if (bl_GameData.Instance.CanChangeTeam && GetGameMode != GameMode.FFA && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes)
        {
            ChangeTeamButton.SetActive(true);
        }
        if (active)
        {
            if (RoomMenu.isPlaying)
            {
                SuicideButton.SetActive(true);
            }
            else
            {
                SuicideButton.SetActive(false);
            }
        }
        else
        {
            OptionsUI.SetActive(false);
        }
        State = (active) ? RoomMenuState.Full : RoomMenuState.Hidde;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Resume()
    {
        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
        State = RoomMenuState.Hidde;
        bl_UCrosshair.Instance.Show(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveOptions()
    {
        ScoreboardUI.SetActive(false);
        OptionsUI.SetActive(true);
        State = RoomMenuState.Options;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveScoreboard()
    {
        ScoreboardUI.SetActive(true);
        OptionsUI.SetActive(false);
        State = RoomMenuState.Scoreboard;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoTeam(bool v)
    {
        AutoTeamUI.SetActive(v);
        if (!v)
        {
            JoinBusttons.SetActive(false);
            ScoreboardUI.SetActive(false);
            BottonMenu.SetActive(false);
            ClassButtonUI.gameObject.SetActive(false);
            SpectatorUI.SetActive(false);
            SpectatorButton.SetActive(false);
            inTeam = true;
            State = RoomMenuState.Hidde;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftRoom()
    {
        //PhotonNetwork.RemoveRPCs(PhotonNetwork.player);
        BlackScreen.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void OnChangeClass(PlayerClass Class)
    {
        foreach (Button b in ClassButtons) { b.interactable = true; }
        ClassButtons[(int)Class].interactable = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        RoomMenu.Suicide();
        bl_UtilityHelper.LockCursor(false);
        ShowMenu(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetFinalText(RoundStyle style, string winner)
    {
        FinalUI.SetActive(true);
        FinalUIText.text = (style == RoundStyle.OneMacht) ? bl_GameTexts.FinalOneMatch : bl_GameTexts.FinalRounds;
        FinalWinnerText.text = string.Format("{0} {1}", winner, bl_GameTexts.FinalWinner);
    }

    public void SetCountDown(int count) { FinalCountText.text = count.ToString(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
        RoomMenu.JoinTeam(id);
        JoinBusttons.SetActive(false);
        ScoreboardUI.SetActive(false);
        BottonMenu.SetActive(false);
        ClassButtonUI.gameObject.SetActive(false);
        inTeam = true;
        State = RoomMenuState.Hidde;
        AutoTeamUI.SetActive(false);
        SpectatorUI.SetActive(false);
        SpectatorButton.SetActive(false);
        RoomMenu.SpectatorMode = false;
        TopMenu.SetActive(false);
        if (bl_GameManager.Joined) { ChangeTeamTimes++; }
        if (bl_GameData.Instance.CanChangeTeam && GetGameMode != GameMode.FFA && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes)
        {
            ChangeTeamButton.SetActive(true);
        }
    }

    public void ActiveChangeTeam()
    {
        FFAJoinButton.SetActive(false);
        FFAScoreboardRoot.SetActive(false);
        JoinBusttons.SetActive(true);
        ChangeTeamButton.SetActive(false);
        TopMenu.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowScoreboard(bool active)
    {
        ScoreboardUI.SetActive(active);
        State = (active) ? RoomMenuState.Scoreboard : RoomMenuState.Hidde;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SpectatorMode(bool active)
    {
        RoomMenu.OnSpectator(active);
        SpectatorUI.SetActive(active);
        JoinBusttons.SetActive(!active);
        ScoreboardUI.SetActive(!active);
        BottonMenu.SetActive(!active);
        ClassButtonUI.gameObject.SetActive(!active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerSpawn()
    {
       if(KillCamUI != null) KillCamUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="m_clips"></param>
    void OnPickUpAmmo(int m_clips)
    {
        AddLeftNotifier(string.Format("+{0} Ammo", m_clips));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_amount"></param>
    void OnPicUpMedKit(int Amount)
    {
        AddLeftNotifier(string.Format("+{0} Health", Amount));
    }

    public void SetPickUp(bool show,int id = 0)
    {
        if (show)
        {
            string t = string.Format(bl_GameTexts.PickUpWeapon, bl_GameData.Instance.GetWeapon(id).Name);
            PickUpText.text = t.ToUpper();
            PickUpUI.SetActive(true);
        }
        else { PickUpUI.SetActive(false); }
    }

    void OnLeftNotifier(string _Text)
    {
        AddLeftNotifier(_Text);
    }

    void AddLeftNotifier(string text)
    {
        GameObject nn = Instantiate(LeftNotifierPrefab) as GameObject;
        nn.GetComponent<bl_UILeftNotifier>().SetInfo(text, 7);
        nn.transform.SetParent(LeftNotifierPanel, false);
    }

    public void OnChangeQuality(int id)
    {
        QualitySettings.SetQualityLevel(id, true);
        PlayerPrefs.SetInt(PropertiesKeys.Quality, id);
    }

    public void OnVolumeChannge(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(PropertiesKeys.Volume, v);
    }

    public void OnSensitivityChannge(float v)
    {
        RoomMenu.m_sensitive = v;
        PlayerPrefs.SetFloat(PropertiesKeys.Sensitivity, v);
    }

    public void OnAimSensitivityChannge(float v)
    {
        RoomMenu.SensitivityAim = v;
        PlayerPrefs.SetFloat(PropertiesKeys.SensitivityAim, v);
    }

    public void OnGunFovChannge(float v)
    {
        RoomMenu.WeaponCameraFog = (int)v;
        PlayerPrefs.SetInt(PropertiesKeys.WeaponFov, (int)v);
    }

    public void OnCrhAb(bool ab)
    {
        ChrAberration = ab;
        SendEffects();
    }

    public void SSAO(bool ab)
    {
        ssao = ab;
        SendEffects();
    }

    public void OnAntialiasing(bool ab)
    {
        Antialiasing = ab;
        SendEffects();
    }

    public void OnBloom(bool ab)
    {
        Bloom = ab;
        SendEffects();
    }

    public void SetFrameActive(bool act)
    {
        int fr = (act == true) ? 1 : 0;
        PlayerPrefs.SetInt(PropertiesKeys.FrameRate, fr);
        FrameRateUI.SetActive(act);
    }

    void SendEffects()
    {
        bl_EventHandler.OnEffectChange(ChrAberration, Antialiasing, Bloom, ssao);
    }

    /// <summary>
    /// 
    /// </summary>
    void LoadSettings()
    {
        int qid = PlayerPrefs.GetInt(PropertiesKeys.Quality, 5);
        int anisoid = PlayerPrefs.GetInt(PropertiesKeys.Aniso, 1);
        float v = PlayerPrefs.GetFloat(PropertiesKeys.Volume, 1);
        float s = PlayerPrefs.GetFloat(PropertiesKeys.Sensitivity, 4);
        float saim = PlayerPrefs.GetFloat(PropertiesKeys.SensitivityAim, 2);
        int fov = PlayerPrefs.GetInt(PropertiesKeys.WeaponFov, 60);
        bool fr = (PlayerPrefs.GetInt(PropertiesKeys.FrameRate, 0) == 1);
        List<Dropdown.OptionData> od = new List<Dropdown.OptionData>();
        for(int i = 0; i < QualitySettings.names.Length; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = QualitySettings.names[i];
            od.Add(data);
        }
        QualityDropdown.AddOptions(od);
        QualityDropdown.value = qid;
        AnisoDropdown.value = anisoid;
        VolumeSlider.value = v;
        SensitivitySlider.value = s;
        SensitivityAimSlider.value = saim;
        FovSlider.value = fov;
        FrameRateUI.SetActive(fr);
        RoomMenu.WeaponCameraFog = fov;
        RoomMenu.m_sensitive = s;
        RoomMenu.SensitivityAim = saim;
    }

    public void OnChangeAniso(int id)
    {
        if (id == 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        else if (id == 1)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
        PlayerPrefs.SetInt(PropertiesKeys.Aniso, id);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRound()
    {
        FinalUI.SetActive(false);
        inTeam = false;
        SetUpUI();
    }

    void StartPingKick()
    {
        bl_PhotonGame.Instance.OnPingKick();
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isMenuActive
    {
        get
        {
            return !(State == RoomMenuState.Hidde);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isOnlyMenuActive
    {
        get
        {
            return (State == RoomMenuState.Options);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isScoreboardActive
    {
        get
        {
            return (State == RoomMenuState.Scoreboard);
        }
    }

    private bl_PlayerScoreboardUI GetPlayerScoreboardUI(PhotonPlayer player)
    {
        foreach (bl_PlayerScoreboardUI p in cachePlayerScoreboard)
        {
            if (p.gameObject.name == (player.NickName + player.ID))
            {
                return p;
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool ExistPlayerOnList(PhotonPlayer player)
    {
        foreach (bl_PlayerScoreboardUI p in cachePlayerScoreboard)
        {
            if (p.gameObject.name == (player.NickName + player.ID))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="psui"></param>
    public void RemoveUIPlayer(bl_PlayerScoreboardUI psui)
    {
        if (cachePlayerScoreboard.Contains(psui))
        {
            cachePlayerScoreboard.Remove(psui);
        }
    }

    public override void OnLeftRoom()
    {
        ShowMenu(false);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        if (ExistPlayerOnList(otherPlayer))
        {
            bl_PlayerScoreboardUI pscui = GetPlayerScoreboardUI(otherPlayer);
            RemoveUIPlayer(pscui);
            pscui.Destroy();
        }
    }

    /// <summary>Refreshes the team lists. It could be a non-team related property change, too.</summary>
    /// <remarks>Called by PUN. See enum PhotonNetworkingMessage for an explanation.</remarks>
    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        UpdateScoreboard();
    }

    [System.Serializable]
    public enum RoomMenuState
    {
        Scoreboard = 0,
        Options = 1,
        Full = 2,
        Hidde = 3,
        Init = 4,
    }

    private static bl_UIReferences _instance;
    public static bl_UIReferences Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_UIReferences>(); }
            return _instance;
        }
    }
}