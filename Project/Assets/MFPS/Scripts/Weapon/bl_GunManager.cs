/////////////////////////////////////////////////////////////////////////////////
///////////////////////////bl_GunManager.cs//////////////////////////////////////
/////////////Use this to manage all weapons Player///////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_GunManager : MonoBehaviour {

    [Header("Weapons List")]
    /// <summary>
    /// all the Guns of game
    /// </summary>
    public List<bl_Gun> AllGuns = new List<bl_Gun>();
    /// <summary>
    /// weapons that the player take equipped
    /// </summary>
    public List<bl_Gun> PlayerEquip = new List<bl_Gun>();
    [Header("Settings")]
    /// <summary>
    /// ID the weapon to take to start
    /// </summary>
    public int m_Current = 0;
    /// <summary>
    /// time it takes to switch weapons
    /// </summary>
    public float SwichTime = 1;
    public float PickUpTime = 2.5f;

    [HideInInspector] public bl_Gun CurrentGun;
    [HideInInspector] public bool CanSwich;
    [Header("References")]
    public Animator m_HeatAnimator;
    public Transform TrowPoint = null;

    private bl_PickGunManager PUM;
    private int PreviousGun = -1;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        PUM = FindObjectOfType<bl_PickGunManager>();
        //when player instance select player class select in bl_RoomMenu
        GetClass();

        //Desactive all weapons in children and take the first
        foreach (bl_Gun g in PlayerEquip) { g.Setup(true); }
        foreach (bl_Gun guns in AllGuns) { guns.gameObject.SetActive(false); }
        TakeWeapon(PlayerEquip[m_Current].gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetClass()
    {
#if CLASS_CUSTOMIZER
        //Get info for class
           bl_RoomMenu.PlayerClass = bl_ClassManager.m_Class;

            m_AssaultClass.primary = bl_ClassManager.PrimaryAssault;
            m_AssaultClass.secondary = bl_ClassManager.SecundaryAssault;
            m_AssaultClass.Knife = bl_ClassManager.KnifeAssault;
            m_AssaultClass.Special = bl_ClassManager.GrenadeAssault;

            m_EngineerClass.primary = bl_ClassManager.PrimaryEnginner;
            m_EngineerClass.secondary = bl_ClassManager.SecundaryEnginner;
            m_EngineerClass.Knife = bl_ClassManager.KnifeEnginner;
            m_EngineerClass.Special = bl_ClassManager.GrenadeEnginner;

            m_SupportClass.primary = bl_ClassManager.PrimarySupport;
            m_SupportClass.secondary = bl_ClassManager.SecundarySupport;
            m_SupportClass.Knife = bl_ClassManager.KnifeSupport;
            m_SupportClass.Special = bl_ClassManager.GrenadeSupport;

            m_ReconClass.primary = bl_ClassManager.PrimaryRecon;
            m_ReconClass.secondary = bl_ClassManager.SecundaryRecon;
            m_ReconClass.Knife = bl_ClassManager.KnifeRecon;
            m_ReconClass.Special = bl_ClassManager.GrenadeRecon;

            switch (bl_RoomMenu.PlayerClass)
            {
                case PlayerClass.Assault:
                    PlayerEquip[0] = AllGuns[m_AssaultClass.primary];
                    PlayerEquip[1] = AllGuns[m_AssaultClass.secondary];
                    PlayerEquip[2] = AllGuns[m_AssaultClass.Special];
                    PlayerEquip[3] = AllGuns[m_AssaultClass.Knife];
                    break;
                case PlayerClass.Recon:
                    PlayerEquip[0] = AllGuns[m_ReconClass.primary];
                    PlayerEquip[1] = AllGuns[m_ReconClass.secondary];
                    PlayerEquip[2] = AllGuns[m_ReconClass.Special];
                    PlayerEquip[3] = AllGuns[m_ReconClass.Knife];
                    break;
                case PlayerClass.Engineer:
                    PlayerEquip[0] = AllGuns[m_EngineerClass.primary];
                    PlayerEquip[1] = AllGuns[m_EngineerClass.secondary];
                    PlayerEquip[2] = AllGuns[m_EngineerClass.Special];
                    PlayerEquip[3] = AllGuns[m_EngineerClass.Knife];
                    break;
                case PlayerClass.Support:
                    PlayerEquip[0] = AllGuns[m_SupportClass.primary];
                    PlayerEquip[1] = AllGuns[m_SupportClass.secondary];
                    PlayerEquip[2] = AllGuns[m_SupportClass.Special];
                    PlayerEquip[3] = AllGuns[m_SupportClass.Knife];
                    break;
            }
#else
        //when player instance select player class select in bl_RoomMenu
        //when player instance select player class select in bl_RoomMenu
        switch (bl_RoomMenu.PlayerClass)
            {
                case PlayerClass.Assault:
                    PlayerEquip[0] = AllGuns[m_AssaultClass.primary];
                    PlayerEquip[1] = AllGuns[m_AssaultClass.secondary];
                    PlayerEquip[2] = AllGuns[m_AssaultClass.Special];
                    PlayerEquip[3] = AllGuns[m_AssaultClass.Knife];
                    break;
                case PlayerClass.Recon:
                    PlayerEquip[0] = AllGuns[m_ReconClass.primary];
                    PlayerEquip[1] = AllGuns[m_ReconClass.secondary];
                    PlayerEquip[2] = AllGuns[m_ReconClass.Special];
                    PlayerEquip[3] = AllGuns[m_ReconClass.Knife];
                    break;
                case PlayerClass.Engineer:
                    PlayerEquip[0] = AllGuns[m_EngineerClass.primary];
                    PlayerEquip[1] = AllGuns[m_EngineerClass.secondary];
                    PlayerEquip[2] = AllGuns[m_EngineerClass.Special];
                    PlayerEquip[3] = AllGuns[m_EngineerClass.Knife];
                    break;
                case PlayerClass.Support:
                    PlayerEquip[0] = AllGuns[m_SupportClass.primary];
                    PlayerEquip[1] = AllGuns[m_SupportClass.secondary];
                    PlayerEquip[2] = AllGuns[m_SupportClass.Special];
                    PlayerEquip[3] = AllGuns[m_SupportClass.Knife];
                    break;
            }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.OnPickUpGun += this.PickUpGun;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.OnPickUpGun -= this.PickUpGun;
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (!bl_UtilityHelper.GetCursorState)
            return;

        InputControl();
        CurrentGun = PlayerEquip[m_Current];
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
#if !INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Alpha1) && CanSwich && m_Current != 0)
        {

            StartCoroutine(ChangeGun(PlayerEquip[m_Current].gameObject, PlayerEquip[0].gameObject));
            m_Current = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && CanSwich && m_Current != 1)
        {

            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[1].gameObject));
            m_Current = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && CanSwich && m_Current != 2)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[2].gameObject));
            m_Current = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && CanSwich && m_Current != 3)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[3].gameObject));
            m_Current = 3;
        }

        //fast fire knife
        if (Input.GetKeyDown(KeyCode.V) && CanSwich && m_Current != 3)
        {
            PreviousGun = m_Current;
            m_Current = 3; // 3 = knife position in list
            PlayerEquip[PreviousGun].gameObject.SetActive(false);
            PlayerEquip[m_Current].gameObject.SetActive(true);
            PlayerEquip[m_Current].FastKnifeFire(OnReturnWeapon);
            CanSwich = false;
        }

#else
        InputManagerControll();
#endif
        //change gun with Scroll mouse
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[(this.m_Current + 1) % this.PlayerEquip.Count].gameObject));
            m_Current = (this.m_Current + 1) % this.PlayerEquip.Count;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (this.m_Current != 0)
            {
                StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[(this.m_Current - 1) % this.PlayerEquip.Count].gameObject));
                this.m_Current = (this.m_Current - 1) % this.PlayerEquip.Count;
            }
            else
            {
                StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[this.PlayerEquip.Count - 1].gameObject));
                this.m_Current = this.PlayerEquip.Count - 1;
            }

        }

    }

    /// <summary>
    /// 
    /// </summary>
    void OnReturnWeapon()
    {
        PlayerEquip[m_Current].gameObject.SetActive(false);
        m_Current = PreviousGun;
        PlayerEquip[m_Current].gameObject.SetActive(true);
        CanSwich = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_weapon"></param>
    void TakeWeapon(GameObject t_weapon)
    {
        t_weapon.SetActive(true);
        CanSwich = true;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Gun GetCurrentWeapon()
    {
        if (CurrentGun == null)
        {
            return PlayerEquip[m_Current];
        }
        else
        {
            return CurrentGun;
        }
    }
    /// <summary>
    /// Coroutine to Change of Gun
    /// </summary>
    /// <param name="t_current"></param>
    /// <param name="t_next"></param>
    /// <returns></returns>
   public IEnumerator ChangeGun(GameObject t_current,GameObject t_next)
    {
        CanSwich = false;
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", true);
        }
        t_current.GetComponent<bl_Gun>().DisableWeapon();
        yield return new WaitForSeconds(SwichTime);
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(t_next);
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PickUpGun(bl_OnPickUpInfo e)
    {
        if (PUM == null)
        {
            Debug.LogError("Need a 'Pick Up Manager' in scene!");
            return;
        }
        //If not already equip
        if (!PlayerEquip.Exists(x => x.GunID == e.ID))
        {

            int actualID = PlayerEquip[m_Current].GunID;
            int nextID = AllGuns.FindIndex(x => x.GunID == e.ID);
            //Get Info
            int[] info = new int[2];
            info[0] = PlayerEquip[m_Current].numberOfClips;
            info[1] = PlayerEquip[m_Current].bulletsLeft;
            PlayerEquip[m_Current] = AllGuns[nextID];
            //Send Info
            AllGuns[nextID].numberOfClips = e.Clips;
            AllGuns[nextID].bulletsLeft = e.Bullets;
            StartCoroutine(PickUpGun((PlayerEquip[m_Current].gameObject), AllGuns[nextID].gameObject, actualID, info));
        }
        else
        {
            foreach (bl_Gun g in PlayerEquip)
            {
                if (g == AllGuns[e.ID])
                {
                    bl_EventHandler.OnAmmo(3);//Add 3 clips
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator PickUpGun(GameObject t_current, GameObject t_next, int id, int[] info)
    {
        CanSwich = false;
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", true);
        }
        t_current.GetComponent<bl_Gun>().DisableWeapon();
        yield return new WaitForSeconds(PickUpTime);
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(t_next);
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", false);
        }
        PUM.TrownGun(id, TrowPoint.position, info);
    }

    /// <summary>
    /// Throw the current gun
    /// </summary>
    public void ThrwoCurrent()
    {
        int actualID = PlayerEquip[m_Current].GunID;
        int[] info = new int[2];
        info[0] = PlayerEquip[m_Current].numberOfClips;
        info[1] = PlayerEquip[m_Current].bulletsLeft;
        PUM.TrownGun(actualID, TrowPoint.position, info);
    }

#if INPUT_MANAGER
    void InputManagerControll()
    {
        if (bl_Input.GetKeyDown("Weapon1") && CanSwich && m_Current != 0)
        {

            StartCoroutine(ChangeGun(PlayerEquip[m_Current].gameObject, PlayerEquip[0].gameObject));
            m_Current = 0;
        }
        if (bl_Input.GetKeyDown("Weapon2") && CanSwich && m_Current != 1)
        {

            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[1].gameObject));
            m_Current = 1;
        }
        if (bl_Input.GetKeyDown("Weapon3") && CanSwich && m_Current != 2)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[2].gameObject));
            m_Current = 2;
        }
        if (bl_Input.GetKeyDown("Weapon4") && CanSwich && m_Current != 3)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[3].gameObject));
            m_Current = 3;
        }

        //fast fire knife
        if (bl_Input.GetKeyDown("FastKnife") && CanSwich && m_Current != 3)
        {
            PreviousGun = m_Current;
            m_Current = 3; // 3 = knife position in list
            PlayerEquip[PreviousGun].gameObject.SetActive(false);
            PlayerEquip[m_Current].gameObject.SetActive(true);
            PlayerEquip[m_Current].FastKnifeFire(OnReturnWeapon);
            CanSwich = false;
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void heatReloadAnim(int m_state)
   {
       if (m_HeatAnimator == null)
           return;

       switch (m_state)
       {
           case 0:
               m_HeatAnimator.SetInteger("Reload", 0);
               break;
           case 1:
               m_HeatAnimator.SetInteger("Reload", 1);
               break;
           case 2:
               m_HeatAnimator.SetInteger("Reload", 2);
               break;
       }
   }

    
   [System.Serializable]
   public class AssaultClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
    [Header("Player Class")]
    public AssaultClass m_AssaultClass;

   [System.Serializable]
   public class EngineerClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public EngineerClass m_EngineerClass;
    //
   [System.Serializable]
   public class ReconClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public ReconClass m_ReconClass;
    //
   [System.Serializable]
   public class SupportClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public SupportClass m_SupportClass;
}
