using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class bl_ChatRoom : bl_PhotonHelper
{

    public KeyCode AllKey = KeyCode.T;
    public KeyCode TeamKey = KeyCode.Y;
    public bool IsVisible = true;
    public int MaxMsn = 7;

    [Header("References")]
    public AudioClip MsnSound;
    public Text ChatText;
    public GUISkin m_Skin;

    public static readonly string ChatRPC = "";
    [SerializeField] float m_alpha = 0f;
    private bool isChat = false;
    private List<string> messages = new List<string>();
    private string inputLine = "";
    private MessageTarget messageTarget = MessageTarget.All;
    private bool isTypedChar = false;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Refresh();
    }

    public void OnGUI()
    {
        if (ChatText == null)
            return;

        GUI.skin = m_Skin;
        GUI.color = new Color(1, 1, 1, m_alpha);

        if (Application.isPlaying)
        {
            if (m_alpha > 0.0f && !isChat)
            {
                m_alpha -= Time.deltaTime / 2;
            }
            else if (isChat)
            {
                m_alpha = 10;
            }

            Color t_color = ChatText.color;
            t_color.a = m_alpha;
            ChatText.color = t_color;

            if (!this.IsVisible || PhotonNetwork.connectionStateDetailed != ClientState.Joined)
            {
                return;
            }

            if (string.IsNullOrEmpty(inputLine) && !isChat && Event.current.type == EventType.KeyUp && Event.current.keyCode == AllKey)
            {
                messageTarget = MessageTarget.All;
                GUI.FocusControl("MyChatInput");
                isChat = true;
                isTypedChar = true;
                this.inputLine = "";
            }
            else if (string.IsNullOrEmpty(inputLine) && !isChat && Event.current.type == EventType.KeyUp && Event.current.keyCode == TeamKey)
            {
                messageTarget = MessageTarget.Team;
                GUI.FocusControl("MyChatInput");
                isChat = true;
                isTypedChar = true;
                this.inputLine = "";
            }

            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
            {
                if (!string.IsNullOrEmpty(this.inputLine) && isChat && bl_UtilityHelper.GetCursorState)
                {
                    this.photonView.RPC("Chat", PhotonTargets.All, this.inputLine, messageTarget);
                    this.inputLine = "";
                    GUI.FocusControl("");
                    isChat = false;
                    return; // printing the now modified list would result in an error. to avoid this, we just skip this single frame
                }
                else if (!isChat && bl_UtilityHelper.GetCursorState)
                {

                }
                else
                {
                    if (isChat)
                    {
                        Closet();
                    }
                }
            }

            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t'))
            {
                Event.current.Use();
            }
            GUI.SetNextControlName("");
        }
        else
        {
            inputLine = "Chat Test";
        }

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height - 35, 300, 50));
        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("MyChatInput");
        GUILayout.Box(messageTarget.ToString().ToUpper(), m_Skin.customStyles[0], GUILayout.Width(30));
        inputLine = GUILayout.TextField(inputLine, m_Skin.customStyles[0]);
        GUI.SetNextControlName("None");
        if (GUILayout.Button("Send", m_Skin.customStyles[1], GUILayout.ExpandWidth(false)))
        {
            this.photonView.RPC("Chat", PhotonTargets.All, this.inputLine, messageTarget);
            this.inputLine = "";
            GUI.FocusControl("");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        if (isTypedChar) { inputLine = string.Empty; isTypedChar = false; }
    }

    /// <summary>
    /// 
    /// </summary>
    void Closet()
    {
        isChat = false;
        GUI.FocusControl("");
    }
    /// <summary>
    /// Sync Method
    /// </summary>
    /// <param name="newLine"></param>
    /// <param name="mi"></param>
    [PunRPC]
    public void Chat(string newLine, MessageTarget mt, PhotonMessageInfo mi)
    {
        if (mt == MessageTarget.Team && mi.sender.GetPlayerTeam() != PhotonNetwork.player.GetPlayerTeam())//check if team message and only team receive it
            return;

        m_alpha = 7;
        string senderName = "anonymous";

        if (mi.sender != null)
        {
            if (!string.IsNullOrEmpty(mi.sender.NickName))
            {
                senderName = mi.sender.NickName;
            }
            else
            {
                senderName = "Player " + mi.sender.ID;
            }
        }

        string txt = string.Format("[{0}] [{1}]:{2}", mt.ToString().ToUpper(), senderName, newLine);
        this.messages.Add(txt);
        if (MsnSound != null)
        {
            GetComponent<AudioSource>().PlayOneShot(MsnSound);
        }
        if (messages.Count > MaxMsn)
        {
            messages.RemoveAt(0);
        }

        ChatText.text = "";
        foreach (string m in messages)
            ChatText.text += m + "\n";
    }

    /// <summary>
    /// Local Method
    /// </summary>
    /// <param name="newLine"></param>
    public void AddLine(string newLine)
    {
        m_alpha = 7;
        this.messages.Add(newLine);
        if (messages.Count > MaxMsn)
        {
            messages.RemoveAt(0);
        }
    }

    public void Refresh()
    {
        ChatText.text = "";
        foreach (string m in messages)
            ChatText.text += m + "\n";
    }

    public enum MessageTarget
    {
        All,
        Team,
    }
}