using UnityEngine;

public class bl_HeatLookMecanim : MonoBehaviour {

	public Transform Target;
    [Range(0,1)]public float Weight;
    [Range(0,1)]public float Body;
    [Range(0,1)]public float Head;
    [Range(0,1)]public float Eyes;
    [Range(0,1)]public float Clamp;
    [Range(1,20)]public float Lerp = 8;

    private Animator animator;
    private Vector3 target;
    private Vector3 StartSpine;

    void OnAnimatorIK(int layer)
    {
        if (Target == null)
            return;

        animator.SetLookAtWeight(Weight, Body, Head, Eyes, Clamp);
        target = Vector3.Slerp(target, Target.position, Time.deltaTime * 8);
        animator.SetLookAtPosition(target);
        Vector3 sr = animator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
        sr.x = StartSpine.x;
        sr.y = StartSpine.y;
        animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(sr));
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        StartSpine = animator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
    }
}