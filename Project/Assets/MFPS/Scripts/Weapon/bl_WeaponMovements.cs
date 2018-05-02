/////////////////////////////////////////////////////////////////////////////////
///////////////////////bl_WeaponMovements.cs/////////////////////////////////////
/////////////Use this to manage the movement of the gun when running/////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

public class bl_WeaponMovements : MonoBehaviour
{
    private bl_FirstPersonController controller;
    [Space(5)]
    [Header("Weapon On Run Position")]
    [Tooltip("Weapon Position and Position On Run")]
    public Vector3 moveTo;
    [Tooltip("Weapon Rotation and Position On Run")]
    public Vector3 rotateTo;
    [Space(5)]
    [Header("Weapon On Run and Reload Position")]
    [Tooltip("Weapon Position and Position On Run and Reload")]
    public Vector3 moveToReload;
    [Tooltip("Weapon Rotation and Position On Run and Reload")]
    public Vector3 rotateToReload;
    [Space(5)]
    public float swayIn = 2f;
    /// <summary>
    /// Time to return to position origin
    /// </summary>
    public float swayOut = 5f;
    /// <summary>
    /// Speed of Sway movement
    /// </summary>
    public float swaySpeed = 1f;
    //private
    private Transform myTransform;
    private float vel;
    private Quaternion DefaultRot;
    private Vector3 DefaultPos;
    private bl_Gun Gun;

    public void Awake()
    {
        this.myTransform = this.transform;
        DefaultRot = myTransform.localRotation;
        DefaultPos = myTransform.localPosition;
        controller = this.transform.root.GetComponent<bl_FirstPersonController>();
        Gun = transform.parent.GetComponent<bl_Gun>();
    }

    void Update()
    {
        vel = controller.VelocityMagnitude;
        if (((vel > 4f) && this.controller.isGrounded) && this.controller.State == PlayerState.Running && !Gun.isFiring && !Gun.isAmed)
        {
            if (Gun.isReloading)
            {
                Quaternion quaternion2 = Quaternion.Euler(this.rotateToReload);
                myTransform.localRotation = Quaternion.Slerp(this.myTransform.localRotation, quaternion2, Time.deltaTime * this.swayIn);
                myTransform.localPosition = Vector3.Lerp(this.myTransform.localPosition, this.moveToReload, Time.deltaTime * this.swayIn);
            }
            else
            {

                Quaternion quaternion2 = Quaternion.Euler(this.rotateTo);
                myTransform.localRotation = Quaternion.Slerp(this.myTransform.localRotation, quaternion2, Time.deltaTime * this.swayIn);
                myTransform.localPosition = Vector3.Lerp(this.myTransform.localPosition, this.moveTo, Time.deltaTime * this.swayIn);
            }
        }
        else
        {
            myTransform.localRotation = Quaternion.Slerp(this.myTransform.localRotation, DefaultRot, Time.deltaTime * this.swayOut);
            myTransform.localPosition = Vector3.Lerp(this.myTransform.localPosition, DefaultPos, Time.deltaTime * this.swayOut);
        }
    }
}