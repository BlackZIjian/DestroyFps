using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerReplaceEditor : EditorWindow
{

    private bl_PlayerSync PlayerPrefab;
    private Transform PlayerModel;
    private Transform RightHand;

    private GameObject TempPlayerPrefab;
    private GameObject TempPlayerModel;
    private bool done = false;
    private string ErrorLine;

    void OnGUI()
    {
        if (!done)
        {
            if (string.IsNullOrEmpty(ErrorLine))
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("CHANGE PLAYER WIZARD", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("This tool will make it easier to change the model of the character,\n before doing this step make sure that your new model of the player \n has a rigid 'Humanoid' and you have a prefab rag-dolled of this.", GUILayout.MaxHeight(100));
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(ErrorLine, MessageType.Info);
                if (GUILayout.Button("Retry", GUILayout.Height(25)))
                {
                    ErrorLine = "";
                    done = false;
                    PlayerPrefab = null;
                    PlayerModel = null;
                }
            }
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            PlayerPrefab = EditorGUILayout.ObjectField("Player Prefab", PlayerPrefab, typeof(bl_PlayerSync), false) as bl_PlayerSync;
            PlayerModel = EditorGUILayout.ObjectField("New Player Model", PlayerModel, typeof(Transform), true) as Transform;
            EditorGUILayout.EndVertical();

            bool enb = (PlayerPrefab != null && PlayerModel != null && /*RightHand != null &&*/ !done);
            GUI.enabled = enb;
            if (GUILayout.Button("Replace", GUILayout.Height(40)))
            {
                Replace();
            }
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Done!", GUILayout.Height(25)))
            {
                Close();
            }
        }
    }

    void Replace()
    {
        //instantiate prefabs
        TempPlayerPrefab = PrefabUtility.InstantiatePrefab(PlayerPrefab.gameObject) as GameObject;
        TempPlayerModel = PrefabUtility.InstantiatePrefab(PlayerModel.gameObject) as GameObject;

        //change name of prefabs to identify
        TempPlayerPrefab.gameObject.name += " [NEW]";
        TempPlayerModel.name += " [NEW]";
         
        // get the current player model
        GameObject RemoteChildPlayer = TempPlayerPrefab.GetComponentInChildren<bl_PlayerAnimations>().gameObject;
        GameObject ActualModel = TempPlayerPrefab.GetComponentInChildren<bl_HeatLookMecanim>().gameObject;
        Transform NetGunns = TempPlayerPrefab.GetComponent<bl_PlayerSync>().NetworkGuns[0].transform.parent;

        //set the new model to the same position as the current model
        TempPlayerModel.transform.parent = RemoteChildPlayer.transform;
        TempPlayerModel.transform.localPosition = ActualModel.transform.localPosition;
        TempPlayerModel.transform.localRotation = ActualModel.transform.localRotation;

        //add and copy components of actual player model
        bl_HeatLookMecanim ahl = ActualModel.GetComponent<bl_HeatLookMecanim>();
        if (TempPlayerModel.GetComponent<Animator>() == null) { TempPlayerModel.AddComponent<Animator>(); }
        Animator NewAnimator = TempPlayerModel.GetComponent<Animator>();

          RightHand = NewAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (RightHand == null)
        {
            ErrorLine = "Can't get right hand from new model, are u sure that is a humanoid rig?";
            return;
        }

        if (ahl != null)
        {

            bl_HeatLookMecanim newht = TempPlayerModel.AddComponent<bl_HeatLookMecanim>();
            newht.Target = ahl.Target;
            newht.Body = ahl.Body;
            newht.Weight = ahl.Weight;
            newht.Head = ahl.Head;
            newht.Lerp = ahl.Lerp;
            newht.Eyes = ahl.Eyes;
            newht.Clamp = ahl.Clamp;
          //  newht.Offset = ahl.Offset;

            Animator oldAnimator = ActualModel.GetComponent<Animator>();
            NewAnimator.runtimeAnimatorController = oldAnimator.runtimeAnimatorController;
            NewAnimator.applyRootMotion = oldAnimator.hasRootMotion;
        }

        bl_PlayerAnimations pa = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerAnimations>();
        bl_BodyPartManager bdm = TempPlayerPrefab.transform.GetComponentInChildren<bl_BodyPartManager>();
        pa.m_animator = NewAnimator;
        bdm.m_Animator = NewAnimator;

        bdm.mRigidBody.Clear();
        bdm.HitBoxs.Clear();
        bdm.mRigidBody.AddRange(GetRigidBodys(TempPlayerModel.transform));
        Collider[] allColliders = GetCollider(TempPlayerModel.transform);
        if(allColliders == null || allColliders.Length <= 0)
        {
            ErrorLine = "New player model prefab is not rag-dolled, to continue please create a rag-doll of it.";
            return;
        }
        foreach (Collider c in allColliders)
        {
            if (c.gameObject.tag != bl_BodyPartManager.HitBoxTag)
            {
                c.gameObject.tag = bl_BodyPartManager.HitBoxTag;
            }
            bl_BodyPartManager.Part p = new bl_BodyPartManager.Part();
            p.m_Collider = c;
            p.name = c.name;
            if (c.gameObject.name.ToLower() == "head")
            {
                p.m_HeatShot = true;
                p.m_Multipler = 50;
            }
            bdm.HitBoxs.Add(p);
        }
        bdm.AddScript();

        NetGunns.parent = RightHand;
        NetGunns.localPosition = Vector3.zero;
        NetGunns.rotation = RightHand.rotation;

        ActualModel.name += " (DELETE)";
        ActualModel.SetActive(false);

        var view = (SceneView)SceneView.sceneViews[0];
        view.AlignViewToObject(TempPlayerPrefab.transform);
        view.ShowNotification(new GUIContent("Player Model Replace!"));

        done = true;
        maxSize = new Vector2(400, 50);
    }

    private Rigidbody[] GetRigidBodys(Transform t)
    {
        Rigidbody[] R = t.GetComponentsInChildren<Rigidbody>();
        return R;
    }

    private Collider[] GetCollider(Transform t)
    {
        Collider[] R = t.GetComponentsInChildren<Collider>();
        return R;
    }

    [MenuItem("MFPS/Tools/Replace Player")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindowWithRect(typeof(bl_PlayerReplaceEditor), new Rect(400, 300, 400, 200));
    }
}