using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class bl_Bullet : MonoBehaviour
{
    #region Variables
    [HideInInspector]
    public Vector3 DirectionFrom = Vector3.zero;
    [HideInInspector]
    public bool isNetwork = false;
    private int hitCount = 0;         // hit counter for counting bullet impacts for bullet penetration
    private int OwnGunID = 777;  //Information for contain Gun id
    private float damage;             // damage bullet applies to a target
    private float impactForce;        // force applied to a rigid body object
    private float maxInaccuracy;      // maximum amount of inaccuracy
    private float variableInaccuracy; // used in machineguns to decrease accuracy if maintaining fire
    private float speed;              // bullet speed
    private float lifetime = 1.5f;    // time till bullet is destroyed
    [HideInInspector]
    public string GunName = ""; //Weapon name
    private Vector3 velocity = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 oldPos = Vector3.zero;   // bullet's previous location
    private bool hasHit = false;             // has the bullet hit something?
    private Vector3 direction;               // direction bullet is travelling
     [HideInInspector]
    public bool isTracer = false;          // used in raycast bullet system... sets the bullet to just act like a tracer
    [Space(5)]
    public AudioSource AudioReferences;
    [SerializeField]private AudioClip ConcreteSound;
    [SerializeField]private AudioClip MetalSound;
    [SerializeField]private AudioClip GenericSound;
    [SerializeField]private AudioClip WoodSound;
    [SerializeField]private AudioClip WaterSound;
    public Vector2 mPicht = new Vector2(1.0f, 1.5f);

    //impact effects for materials
    public GameObject woodParticle;
	public GameObject metalParticle;
	public GameObject concreteParticle;
	public GameObject sandParticle;
	public GameObject waterParticle;
    public GameObject genericParticle;
    public GameObject bloodParticle;
    [SerializeField] private TrailRenderer Trail;

    #endregion

    #region Bullet Set Up

    public void SetUp(bl_BulletSettings info) // information sent from gun to bullet to change bullet properties
    {
        damage = info.Damage;              // bullet damage
        impactForce = info.ImpactForce;         // force applied to rigid bodies
        maxInaccuracy = info.MaxSpread;       // max inaccuracy of the bullet
        variableInaccuracy = info.Spread;  // current inaccuracy... mostly for machine guns that lose accuracy over time
        speed = info.Speed;               // bullet speed
        DirectionFrom = info.Position;
        GunName = info.WeaponName;
        OwnGunID = info.WeaponID;
        isNetwork = info.isNetwork;
        lifetime = info.Speed;
        // direction bullet is traveling
        direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);

        newPos = transform.position;   // bullet's new position
        oldPos = newPos;               // bullet's old position
        velocity = speed * transform.forward; // bullet's velocity determined by direction and bullet speed
        if(Trail != null) { if (!bl_GameData.Instance.BulletTracer) { Destroy(Trail); } }
        // schedule for destruction if bullet never hits anything
        Destroy(gameObject, lifetime);
    }

    #endregion

   
    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (hasHit)
            return; // if bullet has already hit its max hits... exit

        // assume we move all the way
        newPos += (velocity + direction) * Time.deltaTime;

        // Check if we hit anything on the way
        Vector3 dir = newPos - oldPos;
        float dist = dir.magnitude;

        if (dist > 0)
        {
            // normalize
            dir /= dist;
            RaycastHit hit;
            if (Physics.Raycast(oldPos, dir, out hit, dist))
            {
                newPos = hit.point;
                OnHit(hit);
                hasHit = true;
                Destroy(gameObject);
            }
        }

        oldPos = transform.position;  // set old position to current position
        transform.position = newPos;  // set current position to the new position
    }

    #region Bullet On Hits

    void OnHit(RaycastHit hit)
    {        
        GameObject go = null;
        Ray mRay = new Ray(transform.position, transform.forward);
        if (!isTracer)  // if this is a bullet and not a tracer, then apply damage to the hit object
        {
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic) // if we hit a rigi body... apply a force
            {
                float mAdjust = 1.0f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));
                hit.rigidbody.AddForceAtPosition(((mRay.direction * impactForce) / Time.timeScale) / mAdjust, hit.point);
            }
        }
        switch (hit.transform.tag) // decide what the bullet collided with and what to do with it
        {
            case "Projectile":
                // do nothing if 2 bullets collide
                break;
            case "BodyPart"://Send Damage for other players
                if (hit.transform.GetComponent<bl_BodyPart>() != null && !isNetwork)
                {
                    hit.transform.GetComponent<bl_BodyPart>().GetDamage(damage, PhotonNetwork.player.NickName, GunName, DirectionFrom, OwnGunID);
                }
                go = GameObject.Instantiate(bloodParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                go.transform.parent = hit.transform;
                Destroy(this.gameObject);
                break;
            case "AI"://Send Damage for other players
                if (hit.transform.GetComponent<bl_SimpleAI>() != null && !isNetwork)
                {
                    hit.transform.GetComponent<bl_SimpleAI>().DoDamage((int)damage, bl_GameManager.m_view);
                }
                go = GameObject.Instantiate(bloodParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                go.transform.parent = hit.transform;
                Destroy(gameObject);
                break;
            case "Wood":
                hitCount++; // add another hit to counter
                go = GameObject.Instantiate(woodParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                go.transform.parent = hit.transform;
               // MakeBulletHole(hit, hit.transform.gameObject);
                bl_UtilityHelper.PlayClipAtPoint(WoodSound, transform.position, 1.0f, AudioReferences);
                break;
            case "Concrete":
                hitCount += 2; // add 2 hits to counter... concrete is hard
                go = GameObject.Instantiate(concreteParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
              //  MakeBulletHole(hit, hit.transform.gameObject);
                go.transform.parent = hit.transform;
                bl_UtilityHelper.PlayClipAtPoint(ConcreteSound, transform.position, 1.0f, AudioReferences);
                break;
            case "Metal":
                hitCount += 3; // metal slows bullets alot
                go = GameObject.Instantiate(metalParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
               // MakeBulletHole(hit, hit.transform.gameObject);
                go.transform.parent = hit.transform;
                bl_UtilityHelper.PlayClipAtPoint(MetalSound, transform.position, 1.0f, AudioReferences);
                break;
            case "Dirt":
                hasHit = true; // ground kills bullet
                go = GameObject.Instantiate(sandParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
               // MakeBulletHole(hit, hit.transform.gameObject);
                go.transform.parent = hit.transform;
                bl_UtilityHelper.PlayClipAtPoint(GenericSound, transform.position, 1.0f, AudioReferences);
                break;
            case "Water":
                hasHit = true; // water kills bullet
                go = GameObject.Instantiate(waterParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                go.transform.parent = hit.transform;
                bl_UtilityHelper.PlayClipAtPoint(WaterSound, transform.position, 1.0f, AudioReferences);
                break;
            case "Voxeland" :
                hasHit = true; // water kills bullet
                bl_UtilityHelper.PlayClipAtPoint(WaterSound, transform.position, 1.0f, AudioReferences);
                break;
            default:
                hitCount++; // add a hit
                go = GameObject.Instantiate(genericParticle, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                go.transform.parent = hit.transform;
                bl_UtilityHelper.PlayClipAtPoint(GenericSound, transform.position, 1.0f, AudioReferences);
                break;
        }
       
        
    }
    #endregion

    public void SetTracer()
    {
        isTracer = true; // tell this bullet it is only a tracer... keeps this object from applying damage
    }
}