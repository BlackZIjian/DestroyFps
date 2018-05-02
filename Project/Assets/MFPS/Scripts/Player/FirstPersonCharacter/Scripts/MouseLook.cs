using System;
using UnityEngine;

[Serializable]
public class MouseLook
{
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;


    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    [HideInInspector] public float YOffset = 0;
    private bl_RoomMenu RoomMenu;
    private bl_GunManager GunManager;
    private Transform m_Camera;

    public void Init(Transform character, Transform camera, bl_RoomMenu r, bl_GunManager gm)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
        m_Camera = camera;
        RoomMenu = r;
        GunManager = gm;
        if (RoomMenu != null)
        {
            XSensitivity = RoomMenu.m_sensitive;
            YSensitivity = RoomMenu.m_sensitive;
        }
    }


    public void LookRotation(Transform character, Transform camera)
    {

        if (bl_UtilityHelper.GetCursorState)
        {
            CalculateSensitivity();

            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                if (character != null) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime); }
                if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime); }
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
            YOffset = Mathf.Lerp(YOffset, 0, Time.deltaTime * 8);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateSensitivity()
    {
        if (GunManager != null)
        {
            if (!GunManager.GetCurrentWeapon().isAmed)
            {
                XSensitivity = RoomMenu.m_sensitive;
                YSensitivity = RoomMenu.m_sensitive;
            }
            else
            {
                XSensitivity = RoomMenu.SensitivityAim;
                YSensitivity = RoomMenu.SensitivityAim;
            }
        }
    }

    public void SetRecoil(float r)
    {
        YOffset += r;
        YOffset = Mathf.Clamp(YOffset, -10, 10);
       /* if (m_Camera != null)
        {
            Vector3 lr = m_Camera.localEulerAngles;
            lr.x -= r;
            m_Camera.localEulerAngles = lr;
        }
        else
        {
            Debug.Log("Can't recoil");
        }*/
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}