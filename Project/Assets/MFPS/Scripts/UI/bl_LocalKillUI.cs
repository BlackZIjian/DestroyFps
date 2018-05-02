using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_LocalKillUI : MonoBehaviour
{
    [SerializeField]private Text NameText;
    [SerializeField]private Text ValueText;

    private CanvasGroup Alpha;

    public void Init(string namet,string valuet)
    {
        NameText.text = namet;
        ValueText.text = valuet;
        Alpha = GetComponent<CanvasGroup>();
        StartCoroutine(Hide());
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(7);
        while(Alpha.alpha > 0)
        {
            Alpha.alpha -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

}