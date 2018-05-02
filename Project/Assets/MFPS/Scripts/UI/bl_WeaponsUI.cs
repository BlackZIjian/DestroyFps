using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class bl_WeaponsUI : bl_MonoBehaviour
{
    public Color AmmoNormal = new Color(1, 1, 1, 1);
    public Color AmmoLow = new Color(0.8f, 0, 0, 1);
    public int isLowBullet = 5;
    public int isLowBulletSniper = 1;
    //Private 
    private Text AmmoTextUI = null;
    private Text ClipText = null;
    private bl_Gun CurrentGun;
    private bl_GunManager GManager;
    private int BulletLeft;
    private int Clips;
    private Color m_color = new Color(0, 0, 0, 0);


    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        GManager = this.GetComponent<bl_GunManager>();
        AmmoTextUI = bl_UIReferences.Instance.AmmoText;
        ClipText = bl_UIReferences.Instance.ClipText;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        CurrentGun = GManager.GetCurrentWeapon();
        BulletLeft = CurrentGun.bulletsLeft;
        Clips = CurrentGun.numberOfClips;

        if (BulletLeft <= isLowBullet && CurrentGun.Info.Type == GunType.Machinegun || BulletLeft <= isLowBullet && CurrentGun.Info.Type == GunType.Burst || BulletLeft <= isLowBullet && CurrentGun.Info.Type == GunType.Pistol)
        {
            m_color = Color.Lerp(m_color, AmmoLow, Time.deltaTime * 8);
        }
        else if (CurrentGun.Info.Type == GunType.Shotgun && BulletLeft <= isLowBulletSniper || CurrentGun.Info.Type == GunType.Sniper && BulletLeft <= isLowBulletSniper)
        {
            m_color = Color.Lerp(m_color, AmmoLow, Time.deltaTime * 8);
        }
        else
        {
            m_color = Color.Lerp(m_color, AmmoNormal, Time.deltaTime * 8);
        }

        if (AmmoTextUI != null)
        {
            if (CurrentGun.Info.Type != GunType.Knife)
            {
                AmmoTextUI.text = BulletLeft.ToString("F0");
            }
            else
            {
                AmmoTextUI.text = "--";
            }
            AmmoTextUI.color = m_color;
        }
        if(ClipText != null)
        {
            if (CurrentGun.Info.Type != GunType.Knife)
            {
                ClipText.text = string.Format("/ {0}", Clips.ToString("F0"));
            }
            else
            {
                ClipText.text = "/ --";
            }
            ClipText.color = m_color;
        }
    }

}