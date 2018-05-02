using UnityEngine;
using System.Collections;

public class bl_Ragdoll : MonoBehaviour
{

    [Header("Settings")]
    public float m_ForceFactor = 1f;
    [Header("References")]
    public bl_KillCam KillCam;
    public Transform RightHand;

    private Rigidbody[] m_Rigidbodies;
    private Vector3 m_velocity = Vector3.zero;
    private bl_GameManager m_manager;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        m_manager = FindObjectOfType<bl_GameManager>();
        this.Init();
    }

    protected void Init()
    {
        m_Rigidbodies = this.transform.GetComponentsInChildren<Rigidbody>();
        ChangeRagdoll(true);
    }

    public void ChangeRagdoll(bool m)
    {
        foreach (Rigidbody rigidbody in this.m_Rigidbodies)
        {
            rigidbody.isKinematic = !m;
            if (m)
            {
                rigidbody.AddForce((Time.deltaTime <= 0f) ? Vector3.zero : (((m_velocity / Time.deltaTime) * this.m_ForceFactor)), ForceMode.Impulse);
            }
        }
    }
    public void RespawnAfter(float t_time, string killer, Transform netRoot)
    {
        if (!bl_GameData.Instance.KillCamStatic)
        {
             KillCam.StaticCamera = false;
             KillCam.enabled = true;
             KillCam.Send_Target(killer);
        }
        else
        {
            KillCam.StaticCamera = true;
            KillCam.enabled = true;
        }
        StartCoroutine(Wait(t_time));

        if (RightHand != null && netRoot != null)
        {
            Vector3 RootPos = netRoot.localPosition;
            netRoot.parent = RightHand;
            netRoot.localPosition = RootPos;
        }
    }

    IEnumerator Wait(float t_time)
    {
        float t = t_time / 3;
        yield return new WaitForSeconds(t * 2);
        StartCoroutine(m_manager.gameObject.GetComponent<bl_RoomMenu>().FadeIn());
        yield return new WaitForSeconds(t);
        if ((string)PhotonNetwork.player.CustomProperties[PropertiesKeys.TeamKey] == Team.Delta.ToString())
        {
            m_manager.SpawnPlayer(Team.Delta);
        }
        else if ((string)PhotonNetwork.player.CustomProperties[PropertiesKeys.TeamKey] == Team.Recon.ToString())
        {
            m_manager.SpawnPlayer(Team.Recon);
        }
        else
        {
            m_manager.SpawnPlayer(Team.All);
        }

        Destroy(KillCam.gameObject);
        Destroy(gameObject);
    }

    public void GetVelocity(Vector3 m_vel)
    {
        m_velocity = m_vel;
    }
}