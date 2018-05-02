//////////////////////////////////////////////////////////////////////////////
// bl_PlayerAnimations.cs
//
// - was ordered to encourage TPS player animations using legacy animations,
//  and heat look controller from Unity technologies.
//
//                           Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_PlayerAnimations : bl_MonoBehaviour
{
    [HideInInspector]
    public bool m_Update = true;
    [Header("Animations")]
    public Animator m_animator;
    [HideInInspector]
    public bool grounded = true;
    [HideInInspector]
    public int state = 0;
    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public Vector3 localVelocity = Vector3.zero;
    [HideInInspector]
    public float movementSpeed;
    [HideInInspector]
    public float lastYRotation;
    [HideInInspector] public string UpperState = "Idle";

    private int UpperStateID = 0;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical;
    private float horizontal;
    private Transform PlayerRoot;
    private float turnSpeed;
    private bool parent = false;
    private float TurnLerp = 0;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        PlayerRoot = transform.root;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!m_Update)
            return;

        ControllerInfo();
        Animate();
        UpperControll();
    }
    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        localVelocity = PlayerRoot.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, Time.deltaTime * 10);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, Time.deltaTime * 10);

        parent = !parent;
        if (parent)
        {
            lastYRotation = PlayerRoot.rotation.eulerAngles.y;
        }
        turnSpeed = Mathf.DeltaAngle(lastYRotation, PlayerRoot.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * Time.deltaTime);
        movementSpeed = velocity.magnitude;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private float HorizontalAngle(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat("Vertical", vertical);
        m_animator.SetFloat("Horizontal", horizontal);
        m_animator.SetFloat("Speed", movementSpeed);
        m_animator.SetFloat("Turn", TurnLerp);
        m_animator.SetBool("isGround", grounded);
        bool isCrouch = (state == 3);
        m_animator.SetBool("Crouch", isCrouch);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControll()
    {
        if (UpperState == "Firing")
        {
            UpperStateID = 1;
        }
        else if (UpperState == "Reloading")
        {
            UpperStateID = 2;
        }
        else if (UpperState == "Aimed")
        {
            UpperStateID = 3;
        }
        else if (UpperState == "Running")
        {
            UpperStateID = 4;
        }
        else
        {
            UpperStateID = 0;
        }
        if (m_animator != null)
        {
            m_animator.SetInteger("UpperState", UpperStateID);
        }
    }

    public void HitPlayer()
    {
        if (m_animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            m_animator.SetInteger("HitType", ht);
            m_animator.SetTrigger("Hit");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponType"></param>
    public void SetNetworkWeapon(GunType weaponType)
    {
        if (m_animator != null)
        {
            m_animator.SetInteger("GunType", (int)weaponType);
        }
        cacheWeaponType = weaponType;
    }

    public GunType GetCurretWeaponType() { return cacheWeaponType; }
}