////////////////////////////////////////////////////////////////////////////////
//////////////////// bl_InstanceEventKit.cs                                  ///
////////////////////Use this to instantiate a prefabs (Kit) called.          ///
////////////////////////////////Lovatto Studio////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_InstanceEventKit : bl_MonoBehaviour
{
    /// <summary>
    /// key to instantiate MedKit
    /// </summary>
    public KeyCode MedKitKey = KeyCode.H;
    /// <summary>
    /// key to instantiate AmmoKit
    /// </summary>
    public KeyCode AmmoKey = KeyCode.J;
    /// <summary>
    /// Medkit Prefabs for instantiate
    /// </summary>
    public GameObject Kit;
    /// <summary>
    /// Reference position where the kit will be instantiated
    /// </summary>
    public Transform InstancePoint;
    public AudioClip SpawnSound;
    /// <summary>
    /// number of kits available to instantiate.
    /// </summary>
    public int MedkitAmount = 3;
    public int AmmoKitAmount = 3;
    /// <summary>
    /// force when it is instantiated prefabs
    /// </summary>
    public float ForceImpulse = 500;
    [Space(5)]
    public GUISkin Skin;
    public string MedKitMsn = "Medkit";
    public string AmmoKitMsn = "AmmoKit";

    private PlayerClass m_class = PlayerClass.Assault;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        m_class = bl_RoomMenu.PlayerClass;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
#if !INPUT_MANAGER
        if (Input.GetKeyDown(MedKitKey) && Kit != null && MedkitAmount > 0 && m_class == PlayerClass.Engineer)
        {
            MedkitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Medit;
            kit.GetComponent<bl_CallEventKit>().m_text = MedKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }

        }
        if (Input.GetKeyDown(MedKitKey) && Kit != null && MedkitAmount > 0 && m_class == PlayerClass.Support)
        {
            MedkitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Medit;
            kit.GetComponent<bl_CallEventKit>().m_text = MedKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }

        if (Input.GetKeyDown(AmmoKey) && Kit != null && AmmoKitAmount > 0 && m_class == PlayerClass.Assault)
        {
            AmmoKitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Ammo;
            kit.GetComponent<bl_CallEventKit>().m_text = AmmoKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }
        if (Input.GetKeyDown(AmmoKey) && Kit != null && AmmoKitAmount > 0 && m_class == PlayerClass.Recon)
        {
            AmmoKitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Ammo;
            kit.GetComponent<bl_CallEventKit>().m_text = AmmoKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }
#else
        if (bl_Input.GetKeyDown("Medkit") && Kit != null && MedkitAmount > 0 && m_class == PlayerClass.Engineer)
        {
            MedkitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Medit;
            kit.GetComponent<bl_CallEventKit>().m_text = MedKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }

        }
        if (bl_Input.GetKeyDown("Medkit") && Kit != null && MedkitAmount > 0 && m_class == PlayerClass.Support)
        {
            MedkitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Medit;
            kit.GetComponent<bl_CallEventKit>().m_text = MedKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }

        if (bl_Input.GetKeyDown("AmmoKit") && Kit != null && AmmoKitAmount > 0 && m_class == PlayerClass.Assault)
        {
            AmmoKitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Ammo;
            kit.GetComponent<bl_CallEventKit>().m_text = AmmoKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }
        if (bl_Input.GetKeyDown("AmmoKit") && Kit != null && AmmoKitAmount > 0 && m_class == PlayerClass.Recon)
        {
            AmmoKitAmount--;
            GameObject kit = Instantiate(Kit, InstancePoint.position, Quaternion.identity) as GameObject;
            kit.GetComponent<bl_CallEventKit>().m_type = bl_CallEventKit.KitType.Ammo;
            kit.GetComponent<bl_CallEventKit>().m_text = AmmoKitMsn;
            kit.GetComponent<Rigidbody>().AddForce(transform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }
#endif

    }
}