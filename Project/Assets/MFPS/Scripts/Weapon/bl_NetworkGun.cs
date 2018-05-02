using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class bl_NetworkGun : MonoBehaviour
{

    [Header("Settings")]  
    public bl_Gun LocalGun;

    [Header("References")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public GameObject DesactiveOnOffAmmo;

#if CUSTOMIZER
    [Header("CUSTOMIZER")]
     public bool recive = false;
    public ListCustomizer m_Customizer;

    private int BarrelID;
    private int OpticID;
    private int CylinderID;
    private int FeederID;
#endif

    private AudioSource Source;
    private int WeaponID = -1;


    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Source = GetComponent<AudioSource>();
        Source.playOnAwake = false;
    }

    /// <summary>
    /// Update type each is enable 
    /// </summary>
    void OnEnable()
    {
        SetUpType();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpType()
    {
        if (PlayerSync != null)
        {
            PlayerSync.SetNetworkWeapon(Info.Type);
        }
    }

    /// <summary>
    /// Fire Sync in network player
    /// </summary>
    public void Fire(float m_spread, Vector3 pos, Quaternion rot)
    {
        if (LocalGun != null)
        {
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, pos, rot) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            bl_BulletSettings t_info = new bl_BulletSettings();
            t_info.Damage = 0;
            t_info.ImpactForce = 0;
            t_info.MaxSpread = LocalGun.maxSpread;
            t_info.Spread = m_spread;
            t_info.Speed = LocalGun.bulletSpeed;
            t_info.Position = transform.root.position;
            t_info.isNetwork = true;

            newBullet.GetComponent<bl_Bullet>().SetUp(t_info);
            newBullet.GetComponent<bl_Bullet>().isTracer = true;
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
        if (MuzzleFlash)
        {
            MuzzleFlash.Play();
        }
    }

    /// <summary>
    /// if grenade 
    /// </summary>
    /// <param name="s"></param>
    public void GrenadeFire(float s,Vector3 position, Quaternion rotation, Vector3 angular)
    {
        if (LocalGun != null)
        {    
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            bl_BulletSettings t_info = new bl_BulletSettings();    
            t_info.MaxSpread = LocalGun.maxSpread;
            t_info.Spread = s;
            t_info.Speed = LocalGun.bulletSpeed;
            t_info.Position = transform.root.position;
            t_info.isNetwork = true;

            if (newBullet.GetComponent<Rigidbody>() != null)//if grenade have a rigidbody,then apply velocity
            {
                newBullet.GetComponent<Rigidbody>().angularVelocity = angular;
            }
            newBullet.GetComponent<bl_Grenade>().SetUp(t_info);
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// When is knife only reply sounds
    /// </summary>
    public void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void DesactiveGrenade(bool active,Material mat)
    {
        if(Info.Type != GunType.Grenade)
        {
            Debug.LogError("Gun type is not grenade, can't desactive it: " + Info.Type);
            return;
        }
        //when hide network gun / grenade we use method of change material to a invincible
        //due that if desactive the render cause animation  player broken.
        if (DesactiveOnOffAmmo != null)
        {
            DesactiveOnOffAmmo.SetActive(active);
        }
    }

    public int GetWeaponID
    {
        get
        {
            if(WeaponID == -1)
            {
                if (LocalGun != null)
                {
                    WeaponID = LocalGun.GunID;
                }
                Debug.Log("Gun type was not defined: " + gameObject.name);
            }
            return WeaponID;
        }
    }

#if CUSTOMIZER
    public void ReadCustomizer(string info)
    {
        string m_Read = info;
        string[] descompile = m_Read.Split("-"[0]);
        BarrelID = int.Parse(descompile[0]);
        OpticID = int.Parse(descompile[1]);
        CylinderID = int.Parse(descompile[2]);
        FeederID = int.Parse(descompile[3]);
        RendererModule();
        recive = true;
    }


    /// <summary>
    /// 
    /// </summary>
    void RendererModule()
    {
        foreach (infomodule IDBarrel in m_Customizer.Barrel)
        {
            if (IDBarrel.ID == BarrelID)
            {
                IDBarrel.model.SetActive(true);
            }
            else
            {
                IDBarrel.model.SetActive(false);
            }
        }

        foreach (infomodule IDOptics in m_Customizer.Optics)
        {
            if (IDOptics.ID == OpticID)
            {
                IDOptics.model.SetActive(true);
            }
            else
            {
                IDOptics.model.SetActive(false);
            }
        }

        foreach (infomodule IDFeeder in m_Customizer.Feeder)
        {
            if (IDFeeder.ID == FeederID)
            {
                IDFeeder.model.SetActive(true);
            }
            else
            {
                IDFeeder.model.SetActive(false);
            }
        }

        foreach (infomodule IDCylinder in m_Customizer.Cylinder)
        {
            if (IDCylinder.ID == CylinderID)
            {
                IDCylinder.model.SetActive(true);
            }
            else
            {
                IDCylinder.model.SetActive(false);
            }
        }
    }
#endif

    private bl_GunInfo Info
    {
        get
        {
            if (LocalGun != null)
            {
                return bl_GameData.Instance.GetWeapon(GetWeaponID);
            }
            else
            {
                Debug.LogError("This tpv weapon: " + gameObject.name + " has not been defined!");
                return bl_GameData.Instance.GetWeapon(0);
            }
        }
    }

    private bl_PlayerSync m_PS;
    private bl_PlayerSync PlayerSync
    {
        get
        {
            if(m_PS == null) { m_PS = transform.root.GetComponent<bl_PlayerSync>(); }
            return m_PS;
        }
    }
}