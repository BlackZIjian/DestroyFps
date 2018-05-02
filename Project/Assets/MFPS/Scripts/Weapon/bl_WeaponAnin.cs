using UnityEngine;
using System.Collections;

public class bl_WeaponAnin : MonoBehaviour
{

    public AnimationClip DrawName;
    public AnimationClip TakeOut;
    public AnimationClip FireName;
    public AnimationClip ReloadName;
    [Range(0.1f,5)]public float FireSpeed = 1.0f;
    [Range(0.1f, 5)] public float AimFireSpeed = 1.0f;
    [Range(0.1f, 5)] public float DrawSpeed = 1.0f;
    [Range(0.1f, 5)] public float HideSpeed = 1.0f;
    [Header("ShotGun/Sniper")]
    public AnimationClip StartReloadAnim;
    public AnimationClip InsertAnim;
    public AnimationClip AfterReloadAnim;
    [Header("Audio")]
    public AudioSource m_source;
    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip Reload_3;
    public AudioClip m_Fire;

    //private
    private int m_repeatReload;


    void Awake()
    {
        Anim.wrapMode = WrapMode.Once;
    }
    /// <summary>
    /// 
    /// </summary>
    public void Fire()
    {
        if (FireName == null)
            return;

        Anim.Rewind(FireName.name);
        Anim[FireName.name].speed = FireSpeed;
        Anim.Play(FireName.name);
    }

    /// <summary>
    /// 
    /// </summary>
    public float KnifeFire()
    {
        if (FireName == null)
            return 0;

        Anim.Rewind(FireName.name);
        Anim[FireName.name].speed = FireSpeed;
        Anim.Play(FireName.name);

        return Anim[FireName.name].length;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AimFire()
    {
        if (FireName == null)
            return;

        Anim.Rewind(FireName.name);
        Anim[FireName.name].speed = AimFireSpeed;
        Anim.Play(FireName.name);
    }
    /// <summary>
    /// 
    /// </summary>
    public void DrawWeapon()
    {
        if (DrawName == null)
            return;

        Anim.Rewind(DrawName.name);
        Anim[DrawName.name].speed = DrawSpeed;
        Anim[DrawName.name].time = 0;
        Anim.Play(DrawName.name);
    }
    /// <summary>
    ///  
    /// </summary>
    public void HideWeapon()
    {
        if (TakeOut == null)
            return;

        Anim[TakeOut.name].speed = HideSpeed;
        Anim[TakeOut.name].time = 0;
        Anim[TakeOut.name].wrapMode = WrapMode.Once;
        Anim.Play(TakeOut.name);
    }
    /// <summary>
    /// event called by animation when is a reload state
    /// </summary>
    /// <param name="ReloadTime"></param>
    public void Reload(float ReloadTime)
    {
        if (ReloadName == null)
            return;

        Anim.Stop(ReloadName.name);
        Anim[ReloadName.name].wrapMode = WrapMode.Once;
        Anim[ReloadName.name].speed = (Anim[ReloadName.name].clip.length / ReloadTime);
        Anim.Play(ReloadName.name);
    }
    /// <summary>
    /// event called by animation when is fire
    /// </summary>
    public void FireAudio()
    {
        if (m_source != null && m_Fire != null)
        {
            m_source.clip = m_Fire;
            m_source.pitch = Random.Range(1, 1.5f);
            m_source.Play();
        }
    }
    public void ReloadRepeat(float m_reloadTime, int m_repeat)
    {
        float TotalTime = Anim[StartReloadAnim.name].clip.length + (Anim[InsertAnim.name].clip.length * m_repeat) + Anim[AfterReloadAnim.name].clip.length;

        AnimationState firtsState = Anim.CrossFadeQueued(StartReloadAnim.name);
        firtsState.speed = (TotalTime / m_reloadTime) / 1.4f;

        for (int i = 0; i < m_repeat; i++)
        {
            AnimationState newReload2 = Anim.CrossFadeQueued(InsertAnim.name);
            newReload2.speed = (TotalTime / m_reloadTime) / 1.4f;
        }
        AnimationState newReload3 = Anim.CrossFadeQueued(AfterReloadAnim.name);
        if (m_repeat > 1)
        {
            newReload3.speed = (TotalTime / m_reloadTime) / 1.4f;
        }
        else
        {
            newReload3.speed = (TotalTime / m_reloadTime) / 2;
        }
    }
    /// <summary>
    /// Use this for greater coordination
    /// reload sounds with animation
    /// </summary>
    /// <param name="index">st</param>
    public void ReloadSound(int index)
    {
        if (m_source == null)
            return;

        switch (index)
        {
            case 0:
                m_source.clip = Reload_1;
                m_source.Play();
                if (GManager != null)
                {
                    GManager.heatReloadAnim(1);
                }
                break;
            case 1:
                m_source.clip = Reload_2;
                m_source.Play();
                break;
            case 2:
                if (Reload_3 != null)
                {
                    m_source.clip = Reload_3;
                    m_source.Play();
                }
                if (GManager != null)
                {
                    GManager.heatReloadAnim(2);
                    StartCoroutine(ReturnToIdle());
                }
                break;
        }
    }
    /// <summary>
    /// Heat animation
    /// </summary>
    /// <returns></returns>
    IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(0.6f);
        GManager.heatReloadAnim(0);
    }
    /// <summary>
    /// 
    /// </summary>
    private bl_GunManager GManager
    {
        get
        {
            return this.transform.root.GetComponentInChildren<bl_GunManager>();
        }
    }
    private Animation _Anim;
    private Animation Anim
    {
        get
        {
            if (_Anim == null)
            {
                _Anim = this.GetComponent<Animation>();
            }
            return _Anim;
        }
    }

}