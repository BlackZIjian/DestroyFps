using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Voxeland5
{
	public class FlyController : MoveController 
	{
		public float smothness = 0.2f; //time in seconds to find avg position

		private Vector3 controllerPos;
		private List<Vector3> poses = new List<Vector3>();
		private List<float> times = new List<float>();


		public void OnEnable () 
		{
			controllerPos = transform.position;
		} 

		public new void Update () 
		{
			//emulating lag
			#if WDEBUG
			if (useLag)
			{
				if (Time.time-oldTime < lagTime) return;
				deltaTime = Time.time-oldTime;
				oldTime = Time.time;
			}
			else deltaTime = Time.deltaTime;
			#else
			float deltaTime = Time.deltaTime;
			#endif

			//rotate tfm towards look direction
			Vector3 lookDir = transform.forward;
			if (cameraSpace) 
			{
				lookDir = Camera.main.transform.forward;
				transform.rotation = Quaternion.LookRotation(lookDir);
			}

			//reading controls
			float maxSpeed = speed;
			if (Input.GetKey (KeyCode.LeftShift)) maxSpeed = shiftSpeed;
			Vector3 controlsDir = ReadControls(lookDir);
			Vector3 velocity = controlsDir * maxSpeed;

			//moving controller
			Vector3 newControllerPos = GetMove(controllerPos, velocity*deltaTime);

			//ensuring move
			if (!AuthorizePos(newControllerPos)) newControllerPos = PushOut(newControllerPos);
			if (!AuthorizePos(newControllerPos)) newControllerPos = transform.position;
			if (!AuthorizeMove(transform.position, newControllerPos)) newControllerPos = transform.position;

			controllerPos = newControllerPos;

			//smoothing
			poses.Add(controllerPos); 
			times.Add(deltaTime);
			Vector3 smoothedPos = AvgPos(poses, times, smothness);
			transform.position = smoothedPos;

			//track
			#if WDEBUG
			if (writeTrack) WriteTrack(transform.position);
			#endif
		}

	}
}
