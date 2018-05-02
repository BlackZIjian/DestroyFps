using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class bl_SniperScope : bl_MonoBehaviour {

    public Sprite Scope;
    /// <summary>
    /// Object to deactivate when is aimed
    /// </summary>
    public List<GameObject> OnScopeDisable = new List<GameObject>();
    public bool m_show_distance = true;
    /// <summary>
    /// maximum distance raycast
    /// </summary>
    public float Max_Distance = 1000;

    public float m_SmoothAppear = 12;
    //private
    private bl_Gun m_gun;
    private float m_alpha = 0;
    private Vector3 m_point = Vector3.zero;
    private float m_dist = 0.0f;
    private Image ScopeImage;
    private CanvasGroup Alpha;
    private Text DistanceText;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_gun = GetComponent<bl_Gun>();
        ScopeImage = bl_UIReferences.Instance.SniperScope;
       
        if (ScopeImage)
        {
            ScopeImage.sprite = Scope;
            Alpha = ScopeImage.gameObject.GetComponent<CanvasGroup>();
            DistanceText = ScopeImage.gameObject.GetComponentInChildren<Text>();
        }
    }

    public override void OnUpdate()
    {
        if (Scope == null || ScopeImage == null)
            return;

        if (m_gun.isAmed)
        {
            if (m_show_distance)
            {
                GetDistance();
            }
            //add a little fade in to avoid the impact of appearing once
            m_alpha = Mathf.Lerp(m_alpha, 1.0f, Time.deltaTime * m_SmoothAppear);
            foreach (GameObject go in OnScopeDisable)
            {
                go.SetActive(false);
            }
        }
        else
        {
            m_alpha = Mathf.Lerp(m_alpha, 0.0f, Time.deltaTime * m_SmoothAppear);
            foreach (GameObject go in OnScopeDisable)
            {
                go.SetActive(true);
            }
        }
        if(m_show_distance && DistanceText)
        {
            DistanceText.text = m_dist.ToString("00") + "<size=10>m</size>";
        }
        Alpha.alpha = m_alpha;
    }

    /// <summary>
    /// calculate the distance to the first object that raycast hits
    /// </summary>
    void GetDistance()
    {
        RaycastHit m_ray;
        Vector3 fwd = Camera.main.transform.forward;
        if (Physics.Raycast(Camera.main.transform.position, fwd, out m_ray, Max_Distance))
        {
            m_point = m_ray.point;
            m_dist = bl_UtilityHelper.GetDistance(m_point, Camera.main.transform.position);
        }
        else
        {
            m_dist = 0.0f;
        }
    }

}