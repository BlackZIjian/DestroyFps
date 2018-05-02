using UnityEngine;
using System.Collections;

public class bl_GunPickUp : bl_PhotonHelper
{

    [GunID]public int GunID = 0;
    [HideInInspector]
    public bool PickupOnCollide = true;
    [HideInInspector]
    public bool SentPickup = false;
    [Space(5)]
    public bool AutoDestroy = false;
    public float DestroyIn = 15f;
    //
    private bool Into = false;
    private bl_PickGunManager PUM;

    //Cache info
    [System.Serializable]
    public class m_Info
    {
        public int Clips = 0;
        public int Bullets = 0;
    }
    public m_Info Info;
    private bl_UIReferences UIReferences;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (AutoDestroy)
        {
            Destroy(gameObject, DestroyIn);
        }
        PUM = FindObjectOfType<bl_PickGunManager>();
        UIReferences = bl_UIReferences.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        PickupOnCollide = false;
        yield return new WaitForSeconds(3f);
        PickupOnCollide = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        PhotonView v = c.GetComponent<PhotonView>();
        if (PickupOnCollide && v != null && v.isMine && c.tag == bl_PlayerSettings.LocalTag)
        {
            Into = true;
            UIReferences.SetPickUp(true, GunID);
        }

    }
    /// <summary>
    /// 
    /// </summary>
    void OnTriggerExit(Collider c)
    {
        if (c.transform.tag == bl_PlayerSettings.LocalTag)
        {
            Into = false;
            UIReferences.SetPickUp(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (Into)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Pickup()
    {
        if (SentPickup)
            return;

        SentPickup = true;
        PUM.SendPickUp(gameObject.name, GunID, Info);
        SentPickup = false;
        UIReferences.SetPickUp(false);
    }
}