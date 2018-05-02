using UnityEngine;
using System.Collections;

public class bl_GunBob : bl_MonoBehaviour {

    public float walkBobbingSpeed = 0.21f;
    public float runBobbingSpeed = 0.35f;
    public float idleBobbingSpeed = 0.1f;
    public float bobbingAmount = 0.1f;
    public float RunbobbingAmount = 0.1f;
    public float smooth = 1;
    public float RunSmooth = 5;

    Vector3 midpoint;
    GameObject player;
    float timer = 0.0f;
    float bobbingSpeed;
    bl_FirstPersonController motor;
    float BobbingAmount;
    float Dsmooth;
    float tempWalkSpeed = 0;
    float tempRunSpeed = 0;
    float tempIdleSpeed = 0;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        //Find player and FPScontroller script
        player = transform.root.gameObject;
        motor = player.GetComponent<bl_FirstPersonController>();
        midpoint = transform.localPosition;
        Dsmooth = smooth;

    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        //Walk/Run sway speed
        if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Running)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = bobbingAmount;
            smooth = Dsmooth;
        }
        if (motor.State == PlayerState.Running)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = RunbobbingAmount;
            smooth = RunSmooth;
        }

        if (motor.State != PlayerState.Running && motor.VelocityMagnitude < 0.1f || !bl_UtilityHelper.GetCursorState)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = bobbingAmount * 0.2f;
            smooth = Dsmooth;

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        float waveslice = 0.0f;
        float waveslice2 = 0.0f;
        Vector3 currentPosition = Vector3.zero;

        //This variables is used for slow motion effect (0.02 should be default fixed time value)
         tempWalkSpeed = 0;
         tempRunSpeed = 0;
         tempIdleSpeed = 0;

        if (Time.timeScale == 1)
        {
            if (tempWalkSpeed != walkBobbingSpeed || tempRunSpeed != runBobbingSpeed || tempIdleSpeed != idleBobbingSpeed)
            {
                tempWalkSpeed = walkBobbingSpeed;
                tempRunSpeed = runBobbingSpeed;
                tempIdleSpeed = idleBobbingSpeed;
            }
        }
        else
        {
            tempWalkSpeed = walkBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
            tempRunSpeed = runBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
            tempIdleSpeed = idleBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }
        //} 
        if (waveslice != 0)
        {
            var TranslateChange = waveslice * BobbingAmount;
            var TranslateChange2 = waveslice2 * BobbingAmount;
            var TotalAxes = Mathf.Clamp(1.0f, 0.0f, 1.0f);
            var translateChange = TotalAxes * TranslateChange;
            var translateChange2 = TotalAxes * TranslateChange2;

            if (motor.isGrounded)
            {
                //Player walk
                currentPosition = new Vector3(midpoint.x + translateChange2, midpoint.y + translateChange, currentPosition.z);
            }

        }
        else
        {
            //Player not move
            currentPosition = midpoint;
        }
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, currentPosition, Time.deltaTime * smooth);

    }
}
