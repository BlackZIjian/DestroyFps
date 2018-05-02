/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////bl_RoomMenu.cs///////////////////////////////////
/////////////////place this in a scene for handling menus of room////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class bl_RoomMenu : bl_MonoBehaviour
{
    [HideInInspector]
    public bool isPlaying = false;
    [HideInInspector]
    public float m_sensitive = 2.0f,SensitivityAim;
    [HideInInspector]public int WeaponCameraFog = 60;
    [HideInInspector]
    public bool ShowWarningPing = false;
    [HideInInspector]
    public List<PhotonPlayer> FFAPlayerSort = new List<PhotonPlayer>();
    [HideInInspector]
    public string PlayerStar = "";
    [HideInInspector]
    public bool showMenu = true;
    [HideInInspector]
    public bool isFinish = false;
    [HideInInspector]
    public bool SpectatorMode, WaitForSpectator = false;
    /// <summary>
    /// Reference of player class select
    /// </summary>
    public static PlayerClass PlayerClass = PlayerClass.Assault;

    [HideInInspector] public bool AutoTeamSelection = false;
    [Header("Global")]
    public string LeftRoomReturnScene = "MainMenu";
    [Header("Inputs")]
    public KeyCode ScoreboardKey = KeyCode.N;
    public KeyCode PauseMenuKey = KeyCode.Escape;
    public KeyCode ChangeClassKey = KeyCode.M;
    [Header("Ping Settings")]
    /// <summary>
    /// When ping is > at this, them show a message
    /// </summary>
    public int MaxPing = 500;
    [Header("Map Camera")]
    /// <summary>
    /// Rotate room camera?
    /// </summary>
    public bool RotateCamera = true;
    /// <summary>
    /// Rotation Camera Speed
    /// </summary>
    public float RotSpeed = 5;
    [Range(0.0f,1.0f)]
    public float VigAlpha = 0.8f;
    public bool Use_Vignette = true;
    public static float m_alphafade = 3;
    [Header("LeftRoom")]
    [Range(0.0f,5)]
    public float DelayLeave = 1.5f;
    [Header("Others")]
    public Image VignetteImage = null;
    public GameObject ButtonsClassPlay = null;
    public Canvas m_CanvasRoot = null;

    private bl_GameManager GM;  
    private bool CanSpawn = false;
    private bool AlredyAuto = false;
    private bl_UIReferences UIReferences;
    private bool m_showbuttons;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
        GM = FindObjectOfType<bl_GameManager>();
        UIReferences = FindObjectOfType<bl_UIReferences>();
        this.GetComponent<bl_ChatRoom>().AddLine("Play " + GetGameMode.ToString() + " Mode");
        ShowWarningPing = false;
        showMenu = true;
        if (AutoTeamSelection)
        {
            if (GetGameMode != GameMode.FFA)
            {
                StartCoroutine(CanSpawnIE());
            }
            else
            {
                CanSpawn = true;
            }
        }
        if (VignetteImage && Use_Vignette)
        {
            VignetteImage.color = new Color(VignetteImage.color.r, VignetteImage.color.b, VignetteImage.color.g, VigAlpha);
        }
        StartCoroutine(FadeOut(1.5f));
        GetPrefabs();
        m_CanvasRoot.enabled = false;
    }

    protected override void OnEnable()
    {
        bl_EventHandler.OnLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.OnLocalPlayerDeath += OnPlayerLocalDeath;
    }

    protected override void OnDisable()
    {
        bl_EventHandler.OnLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.OnLocalPlayerDeath -= OnPlayerLocalDeath;
    }

    void OnPlayerSpawn()
    {
        m_CanvasRoot.enabled = true;
    }

    void OnPlayerLocalDeath()
    {
        m_CanvasRoot.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        PauseControll();
        ScoreboardControll();

        if (RotateCamera &&  !isPlaying && !SpectatorMode)
        {
            this.transform.Rotate(Vector3.up * Time.deltaTime * RotSpeed);
        }

        if (AutoTeamSelection && !AlredyAuto)
        {
            AutoTeam();
        }

        if (isPlaying && Input.GetKeyDown(ChangeClassKey) && ButtonsClassPlay != null)
        {
            m_showbuttons = !m_showbuttons;
            if (m_showbuttons)
            {
                if (!ButtonsClassPlay.activeSelf)
                {
                    ButtonsClassPlay.SetActive(true);
                    bl_UtilityHelper.LockCursor(false);
                }
            }
            else
            {
                if (ButtonsClassPlay.activeSelf)
                {
                    ButtonsClassPlay.SetActive(false);
                    bl_UtilityHelper.LockCursor(true);
                }
            }
        }

        if (SpectatorMode && Input.GetKeyUp(KeyCode.Escape)) { bl_UtilityHelper.LockCursor(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    void PauseControll()
    {
        if (Input.GetKeyDown(PauseMenuKey) && GM.isEnterinGamePlay && !isFinish && !SpectatorMode)
        {
            bool asb = UIReferences.isMenuActive;
            asb = !asb;
            UIReferences.ShowMenu(asb);
            //new bl_GlobalEvent.OnPause(asb);
            bl_UtilityHelper.LockCursor(!asb);
            bl_UCrosshair.Instance.Show(!asb);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ScoreboardControll()
    {
        if (!UIReferences.isOnlyMenuActive && !isFinish)
        {
            if (Input.GetKeyDown(ScoreboardKey))
            {
                bool asb = UIReferences.isScoreboardActive;
                asb = !asb;
                UIReferences.ShowScoreboard(asb);
            }
            if (Input.GetKeyUp(ScoreboardKey))
            {
                bool asb = UIReferences.isScoreboardActive;
                asb = !asb;
                UIReferences.ShowScoreboard(asb);
            }
        }
    }

    public void OnSpectator(bool active)
    {
        SpectatorMode = active;
        bl_UtilityHelper.LockCursor(active);
        if (active)
        {
            this.GetComponentInChildren<Camera>().transform.rotation = Quaternion.identity;
        }
        GetComponentInChildren<bl_SpectatorCamera>().enabled = active;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (GetGameMode == GameMode.FFA)
        {
            FFAPlayerSort.Clear();
            FFAPlayerSort = GetPlayerList;
            if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
            {
                FFAPlayerSort.Sort(GetSortPlayerByKills);
                PlayerStar = FFAPlayerSort[0].NickName;
            }
        }
    }
    /// <summary>
    /// Use for change player class for next Re spawn
    /// </summary>
    /// <param name="m_class"></param>
    public void ChangeClass(int m_class)
    {
        switch (m_class)
        {
            case 0:
                PlayerClass = PlayerClass.Assault;
                break;
            case 1:
                PlayerClass = PlayerClass.Engineer;
                break;
            case 2:
                PlayerClass = PlayerClass.Recon;
                break;
            case 3:
                PlayerClass = PlayerClass.Support;
                break;
        }

        PlayerClass.SavePlayerClass();
        UIReferences.OnChangeClass(PlayerClass);

#if CLASS_CUSTOMIZER
        if (FindObjectOfType<bl_ClassManager>() != null)
        {
            bl_ClassManager.m_Class = PlayerClass;
        }
#endif

        if (isPlaying && GM.isEnterinGamePlay)
        {
            ButtonsClassPlay.SetActive(false);
            bl_UtilityHelper.LockCursor(true);
        }
        m_showbuttons = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void AutoTeam()
    {
        if (CanSpawn && !isPlaying && !AlredyAuto)
        {
            AlredyAuto = true;
            if (GetGameMode != GameMode.FFA)
            {
                if (GetPlayerInDeltaCount > GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Recon);
                    bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", bl_GameTexts.JoinIn + Team.Recon.GetTeamName(), Team.Recon.ToString(), 777, 30);
                    isPlaying = true;
                }
                else if (GetPlayerInDeltaCount < GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Delta);
                    bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", bl_GameTexts.JoinIn + Team.Delta.GetTeamName(), Team.Delta.ToString(), 777, 30);
                    isPlaying = true;
                }
                else if (GetPlayerInDeltaCount == GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Delta);
                    bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", bl_GameTexts.JoinIn + Team.Delta.GetTeamName(), Team.Delta.ToString(), 777, 30);
                    isPlaying = true;
                }
            }
            else
            {
                bl_UtilityHelper.LockCursor(true);
                showMenu = false;
                GM.SpawnPlayer(Team.All);
                bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", bl_GameTexts.JoinedInMatch, Team.Delta.ToString(), 777, 30);
                isPlaying = true;
            }
            UIReferences.AutoTeam(false);
        }
        else
        {
            UIReferences.AutoTeam(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
        if (id == 0)
        {
            showMenu = false;
            GM.SpawnPlayer(Team.All);
            bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", bl_GameTexts.JoinedInMatch, Team.All.ToString(), 777, 30);
#if !PSELECTOR
            bl_UtilityHelper.LockCursor(true);
            isPlaying = true;
#endif
        }
        else if (id == 1)
        {
            showMenu = false;
            GM.SpawnPlayer(Team.Delta);
            string jt = string.Format("{0} {1}", bl_GameTexts.JoinIn, bl_GameData.Instance.Team1Name);
            bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", jt, Team.Delta.ToString(), 777, 30);
#if !PSELECTOR
            bl_UtilityHelper.LockCursor(true);
            isPlaying = true;
#endif
        }
        else if (id == 2)
        {
            showMenu = false;
            GM.SpawnPlayer(Team.Recon);
            string jt = string.Format("{0} {1}", bl_GameTexts.JoinIn, bl_GameData.Instance.Team2Name);
            bl_EventHandler.KillEvent(PhotonNetwork.player.NickName, "", jt, Team.Recon.ToString(), 777, 30);
#if !PSELECTOR
            bl_UtilityHelper.LockCursor(true);
            isPlaying = true;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftOfRoom()
    {
        //Good place to save info before reset statistics
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_UtilityHelper.LoadLevel(LeftRoomReturnScene);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        PhotonView view = PhotonView.Find(bl_GameManager.m_view);
        if (view != null)
        {

            bl_PlayerDamageManager pdm = view.GetComponent<bl_PlayerDamageManager>();
            pdm.Suicide();
            bl_UtilityHelper.LockCursor(true);
            showMenu = false;
            if (view.isMine)
            {
                bl_GameManager.SuicideCount++;
                //Debug.Log("Suicide " + bl_GameManager.SuicideCount + " times");
                //if player is a joker o abuse of suicide, them kick of room
                if (bl_GameManager.SuicideCount >= 3)//Max number of suicides  = 3, you can change
                {
                    isPlaying = false;
                    bl_GameManager.isAlive = false;
                    bl_UtilityHelper.LockCursor(false);
                    LeftOfRoom();
                }
            }
        }
        else
        {
            Debug.LogError("This view " + bl_GameManager.m_view + " is not found");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPrefabs()
    {
        PlayerClass = PlayerClass.GetSavePlayerClass();
#if CLASS_CUSTOMIZER
        if (FindObjectOfType<bl_ClassManager>() != null)
        {
            PlayerClass = bl_ClassManager.m_Class;
        }
#endif
        UIReferences.OnChangeClass(PlayerClass);
    }

    /// <summary>
    /// 
    /// </summary>
    public int GetStartPlayerScore
    {
        get
        {
            if(FFAPlayerSort.Count > 0)
            {
                return FFAPlayerSort[0].GetPlayerScore();
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Get All Player in Room List
    /// </summary>
    public List<PhotonPlayer> GetPlayerList
    {
        get
        {
            List<PhotonPlayer> list = new List<PhotonPlayer>();
            foreach (PhotonPlayer players in PhotonNetwork.playerList)
            {
                list.Add(players);
            }
            return list;
        }
    }
    /// <summary>
    /// Get the total players in team Delta
    /// </summary>
    public int GetPlayerInDeltaCount
    {
        get
        {
            int count = 0;
            foreach (PhotonPlayer players in PhotonNetwork.playerList)
            {
                if ((string)players.CustomProperties[PropertiesKeys.TeamKey] == Team.Delta.ToString())
                {
                    count++;
                }
            }
            return count;
        }
    }
    /// <summary>
    /// Get the total players in team Recon
    /// </summary>
    public int GetPlayerInReconCount
    {
        get
        {
            int count = 0;
            foreach (PhotonPlayer players in PhotonNetwork.playerList)
            {
                if ((string)players.CustomProperties[PropertiesKeys.TeamKey] == Team.Recon.ToString())
                {
                    count++;
                }
            }
            return count;
        }
    }
    /// <summary>
    /// Sort Player by Kills,for more info watch this: http://answers.unity3d.com/questions/233917/custom-sorting-function-need-help.html
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    /// <returns></returns>
    private static int GetSortPlayerByKills(PhotonPlayer player1, PhotonPlayer player2)
    {
        if (player1.CustomProperties[PropertiesKeys.KillsKey] != null && player2.CustomProperties[PropertiesKeys.KillsKey] != null)
        {
            return (int)player2.CustomProperties[PropertiesKeys.KillsKey] - (int)player1.CustomProperties[PropertiesKeys.KillsKey];
        }
        else
        {
            return 0;
        }
    }

    IEnumerator CanSpawnIE()
    {
        yield return new WaitForSeconds(3);
        CanSpawn = true;
    }

    public IEnumerator FadeIn(float delay = 0.0f, bool load = false)
    {

        m_alphafade = 0;
        while (m_alphafade < 2.0f)
        {
            m_alphafade += Time.deltaTime;
            yield return 0;
        }
        yield return new WaitForSeconds(delay);
        if (load)
        {
            // back to main menu        
            bl_UtilityHelper.LoadLevel(LeftRoomReturnScene);
        }
    }

   public static IEnumerator FadeOut(float t_time)
    {
        m_alphafade = t_time;
        while (m_alphafade > 0.0f)
        {
            m_alphafade -= Time.deltaTime;
            yield return 0;
        }
    }

    public bool isMenuOpen
    {
        get
        {
            return UIReferences.State != bl_UIReferences.RoomMenuState.Hidde;
        }
    }

   public override void OnLeftRoom()
   {
       Debug.Log("OnLeftRoom (local)");      
       this.GetComponent<bl_RoundTime>().enabled = false;
       StartCoroutine(FadeIn(DelayLeave, true));
   }
}