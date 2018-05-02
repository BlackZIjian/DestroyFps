/////////////////////////////////////////////////////////////////////////////////
////////////////////////////bl_DamageIndicator.cs////////////////////////////////
////////////////////Use this to signal the last attack received///////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Lovatto Studio///////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

public class bl_DamageIndicator : bl_MonoBehaviour
{

    /// <summary>
    /// Attack from direction
    /// </summary>
    [HideInInspector] public Vector3 attackDirection;
    /// <summary>
    /// time reach for fade arrow
    /// </summary>
    [Range(1, 5)] public float FadeTime = 3;
    /// <summary>
    /// the transform root of player 
    /// </summary>
    public Transform target;
    //Private
    private Vector2 pivotPoint;
    private float alpha = 0.0f;
    private float rotationOffset;
    private Transform IndicatorPivot;
    private CanvasGroup IndicatorImage;

    protected override void Awake()
    {
        base.Awake();
        IndicatorImage = bl_UIReferences.Instance.DamageIndicator.GetComponent<CanvasGroup>();
        if (IndicatorImage != null) { IndicatorPivot = IndicatorImage.transform.parent; }
    }


    /// <summary>
    /// Use this to send a new direction of attack
    /// </summary>
    /// <param name="dir">position of attacker</param>
    public void AttackFrom(Vector3 dir)
    {
        this.attackDirection = dir;
        this.alpha = 3f;
    }
    /// <summary>
    /// if this is visible Update position
    /// </summary>
    public override void OnUpdate()
    {
        if (this.alpha > 0)
        {
            this.alpha -= Time.deltaTime;
            this.UpdateDirection();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        if (IndicatorImage != null)
            IndicatorImage.alpha = 0;
    }

    /// <summary>
    /// update direction as the arrow shows
    /// </summary>
    void UpdateDirection()
    {
        Vector3 rhs = this.attackDirection - this.target.position;
        rhs.y = 0;
        rhs.Normalize();
        Vector3 forward;
        if (bl_UtilityHelper.CameraInUse != null)
        {
            forward = bl_UtilityHelper.CameraInUse.transform.forward;
        }
        else
        {
            forward = this.transform.forward;
        }
        float GetPos = Vector3.Dot(forward, rhs);
        if (Vector3.Cross(forward, rhs).y > 0)
        {
            this.rotationOffset = (1f - GetPos) * 90;
        }
        else
        {
            this.rotationOffset = (1f - GetPos) * -90;
        }
        if (IndicatorPivot != null)
        {
            IndicatorImage.alpha = alpha;
            IndicatorPivot.eulerAngles = new Vector3(0, 0, -rotationOffset);
        }
    }
}