using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(bl_NetworkGun))]
public class bl_NetworkGunEditor : Editor
{

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bl_NetworkGun script = (bl_NetworkGun)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        if(script.LocalGun != null)
        {
            script.gameObject.name = bl_GameData.Instance.GetWeapon(script.LocalGun.GunID).Name;
        }

        EditorGUILayout.BeginVertical("box");
        script.LocalGun = EditorGUILayout.ObjectField("Local Weapon", script.LocalGun, typeof(bl_Gun), allowSceneObjects) as bl_Gun;
        EditorGUILayout.EndVertical();

        if (script.LocalGun != null)
        {
            EditorGUILayout.BeginVertical("box");
            if (script.LocalGun.Info.Type != GunType.Knife)
            {
                script.Bullet = EditorGUILayout.ObjectField("Bullet", script.Bullet, typeof(GameObject), allowSceneObjects) as GameObject;

                if(script.LocalGun.Info.Type != GunType.Grenade)
                {
                    script.MuzzleFlash = EditorGUILayout.ObjectField("MuzzleFlash", script.MuzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                }
            }
            if (script.LocalGun.Info.Type == GunType.Grenade)
            {
                script.DesactiveOnOffAmmo = EditorGUILayout.ObjectField("Desactive On No Ammo", script.DesactiveOnOffAmmo, typeof(GameObject), allowSceneObjects) as GameObject;
            }
                EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}