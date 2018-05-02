using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class bl_FirstPersonController : bl_MonoBehaviour
{
    [Header("Settings")]
    public PlayerState State;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_CrouchSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [Header("Mouse Look")]
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private Transform HeatRoot;
    [SerializeField] private Transform CameraRoot;
    [Header("HeadBob")]
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField, Range(1, 15)] private float HeadBobLerp = 7;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private float m_RunStepInterval;
    [Header("FootSounds")]
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip[] WatertepSounds;
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    [HideInInspector] public Vector3 Velocity;
    [HideInInspector] public float VelocityMagnitude;
    private bl_RoomMenu RoomMenu;
    private bl_GunManager GunManager;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;

   /// <summary>
   /// 
   /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_CharacterController = GetComponent<CharacterController>();
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        GunManager = GetComponentInChildren<bl_GunManager>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        defaultCameraRPosition = CameraRoot.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, HeatRoot, RoomMenu, GunManager);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.OnRoundEnd += OnRoundEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.OnRoundEnd -= OnRoundEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        Finish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;
        RotateView();

        if (Finish)
            return;
        if (!bl_UtilityHelper.GetCursorState)
            return;

        // the jump state needs to read here to make sure it is not missed
#if INPUT_MANAGER

        if (!m_Jump && State != PlayerState.Crouching)
        {
            m_Jump = bl_Input.GetKeyDown("Jump");
        }
        if (State != PlayerState.Jumping)
        {
            if (bl_Input.GetKeyDown("Crouch"))
            {
                Crounching = !Crounching;
                if (Crounching) { State = PlayerState.Crouching; } else { State = PlayerState.Idle; }
            }
        }
#else
        if (!m_Jump && State != PlayerState.Crouching)
        {
            m_Jump = Input.GetKeyDown(KeyCode.Space);
        }
        if(State != PlayerState.Jumping)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Crounching = !Crounching;
                if (Crounching) { State = PlayerState.Crouching; } else { State = PlayerState.Idle; }
            }
        }
#endif
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
            State = PlayerState.Idle;
            bl_EventHandler.OnSmallImpactEvent();
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        Crouch();
        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (Finish)
            return;
        if (m_CharacterController == null || !m_CharacterController.enabled)
            return;


        float speed = 0;
        if (bl_UtilityHelper.GetCursorState)
        {
            GetInput(out speed);
        }
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
                State = PlayerState.Jumping;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * ((State == PlayerState.Walking) ? 1f : m_RunstepLenghten))) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        RaycastHit hit;
        string _tag = "none";
        int n = 0;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 10))
        {
            _tag = hit.collider.transform.tag;
        }

        switch (_tag)
        {
            case "Water":
                 n = Random.Range(1, WatertepSounds.Length);
                m_AudioSource.clip = WatertepSounds[n];
                m_AudioSource.PlayOneShot(m_AudioSource.clip);
                // move picked sound to index 0 so it's not picked next time
                WatertepSounds[n] = WatertepSounds[0];
                WatertepSounds[0] = m_AudioSource.clip;
                break;
            default:
                 n = Random.Range(1, m_FootstepSounds.Length);
                m_AudioSource.clip = m_FootstepSounds[n];
                m_AudioSource.PlayOneShot(m_AudioSource.clip);
                // move picked sound to index 0 so it's not picked next time
                m_FootstepSounds[n] = m_FootstepSounds[0];
                m_FootstepSounds[0] = m_AudioSource.clip;
                break;
        }
    }

    /// <summary>
    /// When player is in Crouch
    /// </summary>
    void Crouch()
    {
        if (Crounching)
        {
            if (m_CharacterController.height != 1.4f)
            {
                m_CharacterController.height = 1.4f;
            }
            m_CharacterController.center = new Vector3(0, -0.3f, 0);
            Vector3 ch = CameraRoot.transform.localPosition;

            if (CameraRoot.transform.localPosition.y != 0.2f)
            {
                ch.y = Mathf.Lerp(ch.y, 0.2f, Time.deltaTime * 8);
                CameraRoot.transform.localPosition = ch;
            }
        }
        else
        {
            if (m_CharacterController.height != 2.0f)
            {
                m_CharacterController.height = 2.0f;
            }
            if (m_CharacterController.center != Vector3.zero)
            {
                m_CharacterController.center = Vector3.zero;
            }
            Vector3 ch = CameraRoot.transform.localPosition;
            ch.y = Mathf.Lerp(ch.y, defaultCameraRPosition.y, Time.deltaTime * 8);
            CameraRoot.transform.localPosition = ch;
        }
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
                HeatRoot.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + (speed * ((State == PlayerState.Running) ? m_RunstepLenghten : 1f)));
                newCameraPosition = HeatRoot.localPosition;
                newCameraPosition.y = HeatRoot.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = HeatRoot.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        HeatRoot.localPosition = Vector3.Lerp(HeatRoot.localPosition, newCameraPosition, Time.deltaTime * HeadBobLerp);
    }


    private void GetInput(out float speed)
    {
        // Read input
#if INPUT_MANAGER
        float horizontal = bl_Input.HorizontalAxis;
        float vertical = bl_Input.VerticalAxis;
#else
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
#endif
        m_Input = new Vector2(horizontal, vertical);
        PlayerState waswalking = State;

#if !INPUT_MANAGER
        if (m_Input.magnitude > 0 && m_CharacterController.isGrounded)
        {
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            if (Input.GetKey(KeyCode.LeftShift) && State != PlayerState.Crouching)
            {
                State = PlayerState.Running;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift) && State != PlayerState.Crouching)
            {
                State = PlayerState.Walking;
            }
            else if(State != PlayerState.Crouching)
            {
                State = PlayerState.Walking;
            }
        }
        else if(State != PlayerState.Jumping && State != PlayerState.Crouching)
        {
            State = PlayerState.Idle;
        }
#else
        if (m_Input.magnitude > 0 && m_CharacterController.isGrounded)
        {
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            if (bl_Input.GetKey("Run") && State != PlayerState.Crouching)
            {
                State = PlayerState.Running;
            }
            else if (bl_Input.GetKeyUp("Run") && State != PlayerState.Crouching)
            {
                State = PlayerState.Walking;
            }
            else if (State != PlayerState.Crouching)
            {
                State = PlayerState.Walking;
            }
        }
        else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
        {
            State = PlayerState.Idle;
        }
#endif

        if (Crounching)
        {
            speed = m_CrouchSpeed;
        }
        else
        {
            // set the desired speed to be walking or running
            speed = (State == PlayerState.Running) ? m_RunSpeed : m_WalkSpeed;
        }


        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (State != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0 && State == PlayerState.Running)
        {
            StopAllCoroutines();
            StartCoroutine((State == PlayerState.Running) ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, HeatRoot);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    public void SetKick(float amount) { m_MouseLook.SetRecoil(amount); }
    public bool isGrounded { get { return m_CharacterController.isGrounded; } }
}