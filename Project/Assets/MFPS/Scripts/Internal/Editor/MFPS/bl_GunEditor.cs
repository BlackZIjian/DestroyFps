using UnityEditor;
using MFPSEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(bl_Gun))]
public class bl_GunEditor : Editor {

    private ReorderableList list;
    private bl_GameData GameData;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(Dependency.GOListPropiertie), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "On No Ammo Desactive"); };
        GameData = bl_GameData.Instance;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bl_Gun script = (bl_Gun)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("","Gun Info", EditorStyles.toolbar);
        script.GunID = EditorGUILayout.Popup("Gun ID ", script.GunID, GameData.AllWeaponStringList());
        script.Info.Type = bl_GameData.Instance.GetWeapon(script.GunID).Type;
        script.CrossHairScale = EditorGUILayout.Slider("CrossHair Scale: ", script.CrossHairScale, 1, 30);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
      
        if (script.Info.Type == GunType.Machinegun || script.Info.Type == GunType.Pistol || script.Info.Type == GunType.Sniper)
        {
            EditorGUILayout.LabelField("", "Gun Settings", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.useSmooth = EditorGUILayout.Toggle("Use Smooth", script.useSmooth);
            script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 0.01f, 15f);
            script.AimSway = EditorGUILayout.Slider("Aim Sway", script.AimSway, 0.0f, 10);
            script.AimSwayAmount = EditorGUILayout.Slider("Aim Sway Amount", script.AimSwayAmount, 0.0f, 0.1f);
            script.AimFog = EditorGUILayout.Slider("Aim Fog", script.AimFog, 0.0f, 179);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            script.bullet = EditorGUILayout.ObjectField("Bullet",script.bullet, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("","Fire Effects", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as UnityEngine.ParticleSystem;
            script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.impactEffect = EditorGUILayout.ObjectField("Impact Effect", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("", "Gun properties", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntSlider("Impact Force", script.impactForce, 0, 30);
            EditorGUILayout.Space();
            script.ShakeIntense = EditorGUILayout.Slider("Shake Intense", script.ShakeIntense, 0.0f, 2.0f);
            script.kickBackAmount = EditorGUILayout.FloatField("Recoil", script.kickBackAmount);
            EditorGUILayout.Space();
            script.AutoReload = EditorGUILayout.Toggle("Auto Reload", script.AutoReload);
            script.bulletsPerClip = EditorGUILayout.IntField("Bullets Per Clips", script.bulletsPerClip);
            script.maxNumberOfClips = EditorGUILayout.IntField("Max Clips", script.maxNumberOfClips);
            script.numberOfClips = EditorGUILayout.IntSlider("Clips",script.numberOfClips, 0, script.maxNumberOfClips);
            EditorGUILayout.Space();
            script.baseSpread = EditorGUILayout.FloatField("Base Spread", script.baseSpread);
            script.maxSpread = EditorGUILayout.FloatField("Max Spread", script.maxSpread);
            script.spreadPerSecond = EditorGUILayout.FloatField("Spread Per Seconds", script.spreadPerSecond);
            script.decreaseSpreadPerSec = EditorGUILayout.FloatField("Decrease Spread Per Sec", script.decreaseSpreadPerSec);
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("", "Audio", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.DryFireSound = EditorGUILayout.ObjectField("DryFire Sound", script.DryFireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            if (script.Info.Type == GunType.Sniper)
            {
                script.delayForSecondFireSound = EditorGUILayout.Slider("Delay Second Fire Sound", script.delayForSecondFireSound, 0.0f, 2.0f);
                script.DelaySource = EditorGUILayout.ObjectField("Second Source", script.DelaySource, typeof(UnityEngine.AudioSource), allowSceneObjects) as UnityEngine.AudioSource;
            }
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;           
            script.SoundReloadByAnim = EditorGUILayout.Toggle("Sounds Reload By Animation", script.SoundReloadByAnim);
            if (!script.SoundReloadByAnim)
            {
                script.ReloadSound = EditorGUILayout.ObjectField("Reload Begin", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                script.ReloadSound3 = EditorGUILayout.ObjectField("Reload End", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            }
            EditorGUILayout.EndVertical();
            
        }
        if (script.Info.Type == GunType.Burst)
        {
            EditorGUILayout.LabelField("", "Gun Settings", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.useSmooth = EditorGUILayout.Toggle("Use Smooth", script.useSmooth);
            script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 1.0f, 15f);
            script.AimSway = EditorGUILayout.Slider("Aim Sway", script.AimSway, 0.0f, 10);
            script.AimSwayAmount = EditorGUILayout.Slider("Aim Sway Amount", script.AimSwayAmount, 0.0f, 0.1f);
            script.AimFog = EditorGUILayout.Slider("Aim Fog", script.AimFog, 0.0f, 179);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            script.bullet = EditorGUILayout.ObjectField("Bullet", script.bullet, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            EditorGUILayout.LabelField("", "Fire Effects", "box");
            script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.impactEffect = EditorGUILayout.ObjectField("Impact Effect", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            EditorGUILayout.LabelField("", "Gun properties", "box");
            script.roundsPerBurst = EditorGUILayout.IntSlider("Rounds Per Burst", script.roundsPerBurst, 1, 10);
            script.lagBetweenShots = EditorGUILayout.Slider("Lag Between Shots", script.lagBetweenShots, 0.01f, 5.0f);
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            EditorGUILayout.Space();
            script.ShakeIntense = EditorGUILayout.Slider("Shake Intense", script.ShakeIntense, 0.0f, 2.0f);
            script.kickBackAmount = EditorGUILayout.FloatField("Kick Back Amount", script.kickBackAmount);
            EditorGUILayout.Space();
            script.AutoReload = EditorGUILayout.Toggle("Auto Reload", script.AutoReload);
            script.bulletsPerClip = EditorGUILayout.IntField("Bullets Per Clips", script.bulletsPerClip);
            script.maxNumberOfClips = EditorGUILayout.IntField("Max Clips", script.maxNumberOfClips);
            script.numberOfClips = EditorGUILayout.IntSlider("Clips", script.numberOfClips, 0, script.maxNumberOfClips);
            EditorGUILayout.Space();
            script.baseSpread = EditorGUILayout.FloatField("Base Spread", script.baseSpread);
            script.maxSpread = EditorGUILayout.FloatField("Max Spread", script.maxSpread);
            script.spreadPerSecond = EditorGUILayout.FloatField("Spread Per Seconds", script.spreadPerSecond);
            script.decreaseSpreadPerSec = EditorGUILayout.FloatField("Decrease Spread Per Sec", script.decreaseSpreadPerSec);
            EditorGUILayout.LabelField("", "Audio", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.DryFireSound = EditorGUILayout.ObjectField("DryFire Sound", script.DryFireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             script.SoundReloadByAnim = EditorGUILayout.Toggle("Sounds Reload By Animation", script.SoundReloadByAnim);
             if (!script.SoundReloadByAnim)
             {
                 script.ReloadSound = EditorGUILayout.ObjectField("Reload Begin", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound3 = EditorGUILayout.ObjectField("Reload End", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             }
                EditorGUILayout.EndVertical();

        }
        if (script.Info.Type == GunType.Shotgun)
        {
            EditorGUILayout.LabelField("", "ShotGun Settings", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.useSmooth = EditorGUILayout.Toggle("Use Smooth", script.useSmooth);
            script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 1.0f, 15f);
            script.AimSway = EditorGUILayout.Slider("Aim Sway", script.AimSway, 0.0f, 10);
            script.AimSwayAmount = EditorGUILayout.Slider("Aim Sway Amount", script.AimSwayAmount, 0.0f, 0.1f);
            script.AimFog = EditorGUILayout.Slider("Aim Fog", script.AimFog, 0.0f, 179);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            script.bullet = EditorGUILayout.ObjectField("Bullet", script.bullet, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            EditorGUILayout.LabelField("", "Fire Effects", "box");
            script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.impactEffect = EditorGUILayout.ObjectField("Impact Effect", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            EditorGUILayout.LabelField("", "Gun properties", EditorStyles.toolbar);

            script.pelletsPerShot = EditorGUILayout.IntSlider("Bullets Per Shots", (int)script.pelletsPerShot, 1, 10);
            script.roundsPerTracer = EditorGUILayout.IntSlider("Rounds Per Tracer", (int)script.roundsPerTracer, 1, 5);
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            EditorGUILayout.Space();
            script.ShakeIntense = EditorGUILayout.Slider("Shake Intense", script.ShakeIntense, 0.0f, 2.0f);
            script.kickBackAmount = EditorGUILayout.FloatField("Kick Back Amount", script.kickBackAmount);
            EditorGUILayout.Space();
            script.AutoReload = EditorGUILayout.Toggle("Auto Reload", script.AutoReload);
            script.bulletsPerClip = EditorGUILayout.IntField("Bullets Per Clips", script.bulletsPerClip);
            script.maxNumberOfClips = EditorGUILayout.IntField("Max Clips", script.maxNumberOfClips);
            script.numberOfClips = EditorGUILayout.IntSlider("Clips", script.numberOfClips, 0, script.maxNumberOfClips);
            EditorGUILayout.Space();
            script.baseSpread = EditorGUILayout.FloatField("Base Spread", script.baseSpread);
            script.maxSpread = EditorGUILayout.FloatField("Max Spread", script.maxSpread);
            script.spreadPerSecond = EditorGUILayout.FloatField("Spread Per Seconds", script.spreadPerSecond);
            script.decreaseSpreadPerSec = EditorGUILayout.FloatField("Decrease Spread Per Sec", script.decreaseSpreadPerSec);
            EditorGUILayout.LabelField("", "Audio", "box");
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.DryFireSound = EditorGUILayout.ObjectField("DryFire Sound", script.DryFireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.delayForSecondFireSound = EditorGUILayout.Slider("Delay Second Fire Sound", script.delayForSecondFireSound, 0.0f, 2.0f);
            script.DelaySource = EditorGUILayout.ObjectField("Second Source", script.DelaySource, typeof(UnityEngine.AudioSource), allowSceneObjects) as UnityEngine.AudioSource;
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             script.SoundReloadByAnim = EditorGUILayout.Toggle("Sounds Reload By Animation", script.SoundReloadByAnim);
             if (!script.SoundReloadByAnim)
             {
                 script.ReloadSound = EditorGUILayout.ObjectField("Reload Begin", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound3 = EditorGUILayout.ObjectField("Reload End", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             }
                EditorGUILayout.EndVertical();

        }
        if (script.Info.Type == GunType.Knife)
        {
            EditorGUILayout.LabelField("", "Knife Settings", "box");
            EditorGUILayout.BeginVertical("box");
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.useSmooth = EditorGUILayout.Toggle("Use Smooth", script.useSmooth);
            script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 0.01f, 15f);
            script.AimSway = EditorGUILayout.Slider("Aim Sway", script.AimSway, 0.0f, 10);
            script.AimSwayAmount = EditorGUILayout.Slider("Aim Sway Amount", script.AimSwayAmount, 0.0f, 0.1f);
            script.AimFog = EditorGUILayout.Slider("Aim Fog", script.AimFog, 0.0f, 179);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            script.bullet = EditorGUILayout.ObjectField("Bullet", script.bullet, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            EditorGUILayout.LabelField("", "Fire Effects", "box");
            script.impactEffect = EditorGUILayout.ObjectField("Impact Effect", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            EditorGUILayout.LabelField("", "Gun properties", "box");
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntSlider("Impact Force", script.impactForce, 0, 30);
            EditorGUILayout.Space();
            script.ShakeIntense = EditorGUILayout.Slider("Shake Intense", script.ShakeIntense, 0.0f, 2.0f);
            script.kickBackAmount = EditorGUILayout.FloatField("Kick Back Amount", script.kickBackAmount);
            EditorGUILayout.LabelField("", "Audio", "box");
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;

            EditorGUILayout.EndVertical();

        }
        if (script.Info.Type == GunType.Grenade)
        {
            EditorGUILayout.LabelField("", "Grenade Settings", EditorStyles.toolbar);
            EditorGUILayout.BeginVertical("box");
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.useSmooth = EditorGUILayout.Toggle("Use Smooth", script.useSmooth);
            script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 1.0f, 15f);
            script.AimSway = EditorGUILayout.Slider("Aim Sway", script.AimSway, 0.0f, 10);
            script.AimSwayAmount = EditorGUILayout.Slider("Aim Sway Amount", script.AimSwayAmount, 0.0f, 0.1f);
            script.AimFog = EditorGUILayout.Slider("Aim Fog", script.AimFog, 0.0f, 179);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            script.grenade = EditorGUILayout.ObjectField("Grenade", script.grenade, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            EditorGUILayout.LabelField("", "Grenade properties", EditorStyles.toolbar);
            script.DelayFire = EditorGUILayout.FloatField("Delay Fire", script.DelayFire);
            script.bulletSpeed = EditorGUILayout.FloatField("Projectile Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            EditorGUILayout.Space();
            script.ShakeIntense = EditorGUILayout.Slider("Shake Intense", script.ShakeIntense, 0.0f, 2.0f);
            script.kickBackAmount = EditorGUILayout.FloatField("Kick Back Amount", script.kickBackAmount);
            EditorGUILayout.Space();
            script.AutoReload = EditorGUILayout.Toggle("Auto Reload", script.AutoReload);
            script.bulletsPerClip = EditorGUILayout.IntField("Bullets Per Clips", script.bulletsPerClip);
            script.maxNumberOfClips = EditorGUILayout.IntField("Max Clips", script.maxNumberOfClips);
            script.numberOfClips = EditorGUILayout.IntSlider("Clips", script.numberOfClips, 0, script.maxNumberOfClips);
            EditorGUILayout.Space();
            script.baseSpread = EditorGUILayout.FloatField("Base Spread", script.baseSpread);
            script.maxSpread = EditorGUILayout.FloatField("Max Spread", script.maxSpread);
            script.spreadPerSecond = EditorGUILayout.FloatField("Spread Per Seconds", script.spreadPerSecond);
            script.decreaseSpreadPerSec = EditorGUILayout.FloatField("Decrease Spread Per Sec", script.decreaseSpreadPerSec);
            EditorGUILayout.Space();

            list.DoLayoutList();

            EditorGUILayout.LabelField("", "Audio", "box");
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             script.SoundReloadByAnim = EditorGUILayout.Toggle("Sounds Reload By Animation", script.SoundReloadByAnim);
             if (!script.SoundReloadByAnim)
             {
                 script.ReloadSound = EditorGUILayout.ObjectField("Reload Begin", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
                 script.ReloadSound3 = EditorGUILayout.ObjectField("Reload End", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
             }
                 EditorGUILayout.EndVertical();

        }
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

}