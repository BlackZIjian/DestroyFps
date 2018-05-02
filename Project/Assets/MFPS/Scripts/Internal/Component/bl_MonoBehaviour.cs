public class bl_MonoBehaviour : bl_PhotonHelper
{
    private bool isRegister = false;

    protected virtual void Awake()
    {
        if (!isRegister)
        {
            bl_UpdateManager.AddItem(this);
           // Debug.Log("Register A: " + this.GetType().Name);
            isRegister = true;
        }
    }

    protected virtual void OnDisable()
    {
        if (isRegister)
        {
            bl_UpdateManager.RemoveSpecificItem(this);
            isRegister = false;
           // Debug.Log("UnRegister O: " + this.GetType().Name);
        }
    }

    protected virtual void OnDestroy()
    {
        if (isRegister)
        {
            bl_UpdateManager.RemoveSpecificItem(this);
            isRegister = false;
          //  Debug.Log("UnRegister D: " + this.GetType().Name);
        }
    }


    protected virtual void OnEnable()
    {
        if (!isRegister)
        {
            bl_UpdateManager.AddItem(this);
            isRegister = true;
          //  Debug.Log("Register E: " + this.GetType().Name);
        }
    }

    public virtual void OnUpdate() { }

    public virtual void OnFixedUpdate() { }

    public virtual void OnLateUpdate() { }
}