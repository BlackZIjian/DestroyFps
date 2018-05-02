using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voxeland5
{

public class HeadBob : MonoBehaviour 
{
	public Transform head;
	public Transform character; //head parent, to determine speed

	public float amplitude = 0.1f;
	public float frequency = 2f;
	public float usualSpeed = 5;

	private float oldSwayPhase; //keep phase because frequency is changing
	private Vector3 originalLocalPos; //to return head to origin no matter of what
	private Vector3 prevCharPos; //to track speed

	static readonly float pi = 3.1415926536f;


	public void OnEnable () 
	{
		if (head==null) head = transform;
		if (character==null) character = head.parent;
		originalLocalPos = head.localPosition;
	}

	public void Update () 
	{
		float speed = Mathf.Sqrt( (prevCharPos.x-character.position.x)*(prevCharPos.x-character.position.x) + (prevCharPos.z-character.position.z)*(prevCharPos.z-character.position.z) ) / Time.deltaTime;
		prevCharPos = character.position;

		head.localPosition = originalLocalPos + HeadBobVector(speed);
	}
 
	public Vector3 HeadBobVector (float curSpeed, float heightBob=1f, float frontBob=0.2f, float sideBob=0.3f)
	{
		float amplitudeSpeedFactor = curSpeed; //when cur speed < norm speed lowering amplitude
		amplitudeSpeedFactor /= usualSpeed;
				
		float frequencySpeedFactor = curSpeed - usualSpeed; //when cur speed > norm speed - changing frequency
		if (frequencySpeedFactor < 0) frequencySpeedFactor = 0;
		frequencySpeedFactor /= usualSpeed*3;
		frequencySpeedFactor += 1; //to make it multiplicative

		float curPhase = oldSwayPhase  +   Time.deltaTime * pi * 2 * frequency * frequencySpeedFactor;
		oldSwayPhase = curPhase;
		if (oldSwayPhase > pi*4) { oldSwayPhase = 0; curPhase = 0; }

		//swaying
		Vector3 swayVector = new Vector3(0,1,0) * amplitude*amplitudeSpeedFactor * Mathf.Sin(curPhase)*heightBob; //height sway
		swayVector += transform.forward * amplitude*amplitudeSpeedFactor * Mathf.Cos(curPhase)*frontBob; //front
		swayVector += transform.right * amplitude*amplitudeSpeedFactor * Mathf.Sin(curPhase/2)*sideBob; //sides

		return swayVector;
	}
}

}
