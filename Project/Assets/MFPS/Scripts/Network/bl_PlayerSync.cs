////////////////////////////////////////////////////////////////////////////////
//////////////////// bl_PlayerSync.cs///////////////////////////////////////////
////////////////////use this for the synchronizer position , rotation, states,//
///////////////////etc...   via photon//////////////////////////////////////////
////////////////////////////////Lovatto Studio//////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class bl_PlayerSync : bl_MonoBehaviour
{
    /// <summary>
    /// the player's team is not ours
    /// </summary>
    [HideInInspector]
    public string RemoteTeam;
    /// <summary>
    /// the current state of the current weapon
    /// </summary>
    public string WeaponState;
    /// <summary>
    /// the object to which the player looked
    /// </summary>
    public Transform HeatTarget;
    /// <summary>
    /// smooth interpolation amount
    /// </summary>
    public float SmoothingDelay = 8f;
    /// <summary>
    /// list all remote weapons
    /// </summary>
    public List<bl_NetworkGun> NetworkGuns = new List<bl_NetworkGun>();

    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();

    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();

    [SerializeField]
    PhotonTransformViewScaleModel m_ScaleModel = new PhotonTransformViewScaleModel();

    PhotonTransformViewPositionControl m_PositionControl;
    PhotonTransformViewRotationControl m_RotationControl;
    PhotonTransformViewScaleControl m_ScaleControl;

    bool m_ReceivedNetworkUpdate = false;
    [Space(5)]
    //Script Needed
    [Header("Necessary script")]
    public bl_GunManager GManager;
    public bl_PlayerAnimations m_PlayerAnimation;
    //Material for apply when disable a NetGun
    public Material InvicibleMat;
    //private
    private bl_FirstPersonController Controller;
    private GameObject CurrenGun;
    private bl_PlayerSettings Settings;
    private bl_PlayerDamageManager PDM;
    private bl_DrawName DrawName;
    private bl_RoomMenu RoomMenu;
    private bool FrienlyFire = false;
    private bool SendInfo = false;
    private CharacterController m_CController;

#pragma warning disable 0414
    [SerializeField]
    bool ObservedComponentsFoldoutOpen = true;
#pragma warning disable 0414


    protected override void Awake()
    {
        base.Awake();
        if (!PhotonNetwork.connected)
            Destroy(this);

        //FirstUpdate = false;
        if (!this.isMine)
        {
            if (HeatTarget.gameObject.activeSelf == false)
            {
                HeatTarget.gameObject.SetActive(true);
            }
        }

        m_PositionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        m_RotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        m_ScaleControl = new PhotonTransformViewScaleControl(m_ScaleModel);
        Controller = GetComponent<bl_FirstPersonController>();
        Settings = GetComponent<bl_PlayerSettings>();
        PDM = GetComponent<bl_PlayerDamageManager>();
        DrawName = GetComponent<bl_DrawName>();
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        m_CController = GetComponent<CharacterController>();
        FrienlyFire = (bool)PhotonNetwork.room.CustomProperties[PropertiesKeys.RoomFriendlyFire];
    }

    /// <summary>
    /// serialization method of photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        m_PositionControl.OnPhotonSerializeView(transform.localPosition, stream, info);
        m_RotationControl.OnPhotonSerializeView(transform.localRotation, stream, info);
        m_ScaleControl.OnPhotonSerializeView(transform.localScale, stream, info);
        if (isMine == false && m_PositionModel.DrawErrorGizmo == true)
        {
            DoDrawEstimatedPositionError();
        }
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(gameObject.name);
            stream.SendNext(HeatTarget.position);
            stream.SendNext(HeatTarget.rotation);
            stream.SendNext((int)Controller.State);
            stream.SendNext(Controller.isGrounded);
            stream.SendNext(GManager.GetCurrentWeapon().GunID);
            stream.SendNext(Settings.m_Team.ToString());
            stream.SendNext(WeaponState);
            stream.SendNext(Controller.Velocity);
        }
        else
        {
            //Network player, receive data
            RemotePlayerName = (string)stream.ReceiveNext();
            HeadPos = (Vector3)stream.ReceiveNext();
            HeadRot = (Quaternion)stream.ReceiveNext();
            m_state = (int)stream.ReceiveNext();
            m_grounded = (bool)stream.ReceiveNext();
            CurNetGun = (int)stream.ReceiveNext();
            RemoteTeam = (string)stream.ReceiveNext();
            UpperState = (string)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();

            m_ReceivedNetworkUpdate = true;
        }
    }

    private Vector3 HeadPos = Vector3.zero;// Head Look to
    private Quaternion HeadRot = Quaternion.identity;
    private int m_state;
    private bool m_grounded;
    private string RemotePlayerName = string.Empty;
    private int CurNetGun;
    private string UpperState = "";
    private Vector3 velocity;

    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_GameData.Instance.DropGunOnDeath)
        {
            NetGunsRoot.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ///if the player is not ours, then
        if (photonView == null || isMine == true || isConnected == false)
        {
            return;
        }

        UpdatePosition();
        UpdateRotation();
        UpdateScale();
        GetTeamRemote();

        this.HeatTarget.position = Vector3.Lerp(this.HeatTarget.position, HeadPos, Time.deltaTime * this.SmoothingDelay);
        this.HeatTarget.rotation = HeadRot;
        m_PlayerAnimation.state = m_state;//send the state of player local for remote animation
        m_PlayerAnimation.grounded = m_grounded;
        m_PlayerAnimation.velocity = velocity;
        m_PlayerAnimation.UpperState = UpperState;

        gameObject.name = RemotePlayerName;
        if(DrawName != null) { DrawName.m_PlayerName = RemotePlayerName; }
        if (GetGameMode != GameMode.FFA)
        {
            //Determine if remote player is teamMate or enemy
            if (RemoteTeam == (string)PhotonNetwork.player.CustomProperties[PropertiesKeys.TeamKey])
            {
                TeamMate();
            }
            else
            {
                Enemy();
            }
        }
        else if (GetGameMode == GameMode.FFA)
        {
            Enemy();
        }

        CurrentTPVGun();
    }

    void CurrentTPVGun(bool local = false)
    {
        if (GManager == null)
            return;

        //Get the current gun ID local and sync with remote
        bool found = false;
        foreach (bl_NetworkGun guns in NetworkGuns)
        {
            int currentID = (local) ? GManager.GetCurrentWeapon().GunID : CurNetGun;
            if (guns.GetWeaponID == currentID)
            {
                guns.gameObject.SetActive(true);
                if (!local)
                {
                    CurrenGun = guns.gameObject;
                    CurrenGun.GetComponent<bl_NetworkGun>().SetUpType();
                }
                found = true;
            }
            else
            {
                guns.gameObject.SetActive(false);
            }
        }
        if (!found) { Debug.LogWarning("Net gun with id: " + CurNetGun + " is not defined!"); }
    }

    /// <summary>
    /// use this function to set all details for enemy
    /// </summary>
    void Enemy()
    {
        PDM.DamageEnabled = true;
        DrawName.enabled = RoomMenu.SpectatorMode;
    }

    /// <summary>
    /// use this function to set all details for teammate
    /// </summary>
    void TeamMate()
    {
        PDM.DamageEnabled = FrienlyFire;
        DrawName.enabled = true;
        m_CController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            this.GetComponentInChildren<bl_BodyPartManager>().IgnorePlayerCollider();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetNetworkWeapon(GunType weaponType)
    {
        m_PlayerAnimation.SetNetworkWeapon(weaponType);
    }

    /// <summary>
    /// public method to send the RPC shot synchronization
    /// </summary>
    public void IsFire(string m_type, float t_spread, Vector3 pos, Quaternion rot,int damage)
    {
        photonView.RPC("FireSync", PhotonTargets.Others, new object[] { m_type, t_spread, pos, rot });
        VoxelandManager.Instance.DigVoxel(pos,rot * Vector3.forward,damage);
    }

    /// <summary>
    /// public method to send the RPC shot synchronization
    /// </summary>
    public void IsFireGrenade(float t_spread, Vector3 pos, Quaternion rot, Vector3 angular)
    {
        photonView.RPC("FireGrenadeRpc", PhotonTargets.Others, new object[] { t_spread, pos, rot, angular });
    }

    public Transform NetGunsRoot { get { if (!bl_GameData.Instance.DropGunOnDeath) { CurrentTPVGun(true); } return NetworkGuns[0].transform.parent; } }

    /// <summary>
    /// Synchronize the shot with the current remote weapon
    /// send the information necessary so that fire
    /// impact in the same direction as the local
    /// </summary>
    [PunRPC]
    void FireSync(string m_type, float m_spread, Vector3 pos, Quaternion rot)
    {
        if (CurrenGun)
        {
            if (m_type == GunType.Machinegun.ToString())
            {
                CurrenGun.GetComponent<bl_NetworkGun>().Fire(m_spread, pos, rot);
            }
            else if (m_type == GunType.Shotgun.ToString())
            {
                CurrenGun.GetComponent<bl_NetworkGun>().Fire(m_spread, pos, rot);//if you need add your custom fire shotgun in networkgun
            }
            else if (m_type == GunType.Sniper.ToString())
            {
                CurrenGun.GetComponent<bl_NetworkGun>().Fire(m_spread, pos, rot);//if you need add your custom fire sniper in networkgun
            }
            else if (m_type == GunType.Burst.ToString())
            {
                CurrenGun.GetComponent<bl_NetworkGun>().Fire(m_spread, pos, rot);//if you need add your custom fire burst in networkgun
            }
            else if (m_type == GunType.Knife.ToString())
            {
                CurrenGun.GetComponent<bl_NetworkGun>().KnifeFire();//if you need add your custom fire launcher in networkgun
            }
        }
    }

    [PunRPC]
    void FireGrenadeRpc(float m_spread, Vector3 pos, Quaternion rot,Vector3 angular)
    {
        CurrenGun.GetComponent<bl_NetworkGun>().GrenadeFire(m_spread, pos, rot, angular);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveGrenade(bool active)
    {
        photonView.RPC("SyncOffAmmoGrenade", PhotonTargets.Others, active);
    }

    [PunRPC]
    void SyncOffAmmoGrenade(bool active)
    {
        if (CurrenGun == null)
        {
            Debug.LogError("Grenade is not active on TPS Player");
            return;
        }
        CurrenGun.GetComponent<bl_NetworkGun>().DesactiveGrenade(active, InvicibleMat);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetTeamRemote()
    {
        if (RemoteTeam == Team.Recon.ToString())
        {
            Settings.m_Team = Team.Recon;
        }
        else if (RemoteTeam == Team.Delta.ToString())
        {
            Settings.m_Team = Team.Delta;
        }
        else
        {
            Settings.m_Team = Team.All;
        }
    }

#if CUSTOMIZER
    [PunRPC]
    void SyncCustomizer(string info, string p)
    {
        if (CurrenGun)
        {
            if (p == this.gameObject.name)//is mine
            {
                this.CurrenGun.GetComponent<bl_NetworkGun>().ReadCustomizer(info);
            }
        }
        else
        {
            StartCoroutine(WaitForGetGUN(info, p));
        }
    }

    IEnumerator WaitForGetGUN(string info, string p)
    {
        yield return new WaitForSeconds(2);
        this.SyncCustomizer(info, p);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    void UpdatePosition()
    {
        if (m_PositionModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        transform.localPosition = m_PositionControl.UpdatePosition(transform.localPosition);
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateRotation()
    {
        if (m_RotationModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        transform.localRotation = m_RotationControl.GetRotation(transform.localRotation);
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateScale()
    {
        if (m_ScaleModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        transform.localScale = m_ScaleControl.GetScale(transform.localScale);
    }
    /// <summary>
    /// 
    /// </summary>
    void DoDrawEstimatedPositionError()
    {
        Vector3 targetPosition = m_PositionControl.GetNetworkPosition();

        Debug.DrawLine(targetPosition, transform.position, Color.red, 2f);
        Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.green, 2f);
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
    }
    /// <summary>
    /// These values are synchronized to the remote objects if the interpolation mode
    /// or the extrapolation mode SynchronizeValues is used. Your movement script should pass on
    /// the current speed (in units/second) and turning speed (in angles/second) so the remote
    /// object can use them to predict the objects movement.
    /// </summary>
    /// <param name="speed">The current movement vector of the object in units/second.</param>
    /// <param name="turnSpeed">The current turn speed of the object in angles/second.</param>
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
    {
        m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
    }
}