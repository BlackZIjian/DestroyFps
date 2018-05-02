using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voxeland5
{
	public class WalkController : MoveController 
	{
		public Vector3 velocity;  //could not be changed in air, y stores fall
		public bool gravity = true;

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
			bool wasGrounded =  IsGrounded(controllerPos);

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
				lookDir.y = 0;
				transform.rotation = Quaternion.LookRotation(lookDir);
			}

			//reading controls
			float maxSpeed = speed;
			if (Input.GetKey (KeyCode.LeftShift)) maxSpeed = shiftSpeed;
			Vector3 controlsDir = ReadControls(lookDir);
			if (IsGrounded(controllerPos)) velocity = controlsDir * maxSpeed;

			//jump
			if (Input.GetKeyDown (KeyCode.Space) && wasGrounded)
				velocity.y = jumpSpeed;

			//moving controller
			Vector3 controlledVelocity = new Vector3(velocity.x, 0, velocity.z);
			if (velocity.y > 0) controlledVelocity.y = velocity.y; //adding jump to move
			Vector3 newControllerPos = GetMove(controllerPos, controlledVelocity*deltaTime);

			//gravity
			if (gravity)
			{
				velocity.y -= deltaTime*9.8f;
				if (velocity.y < 0)
				{
					newControllerPos = GetFall(newControllerPos, -velocity.y*deltaTime);
					velocity.y = (newControllerPos.y - controllerPos.y)/deltaTime;
				}
				if (wasGrounded && velocity.y<0) velocity.y = -maxSpeed/2;  //preventing micro-jumps when travelling down the slope
			}

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




		#region Test


			public int iterations;
			public Vector3 EmulateMove (Vector3 initialPos, Vector3 velocity, float deltaTime)
			{
				//moving controller
				Vector3 controlledVelocity = new Vector3(velocity.x, 0, velocity.z);
				Vector3 newControllerPos = GetMove(controllerPos, controlledVelocity*deltaTime);

				//gravity
				velocity.y -= deltaTime*9.8f;
				if (velocity.y < 0)
				{
					newControllerPos = GetFall(newControllerPos, -velocity.y*deltaTime);
					velocity.y = (newControllerPos.y - controllerPos.y)/deltaTime;
				}

				//float deltaHeight = newControllerPos.y - controllerPos.y;
				//verticalSpeed = (newControllerPos.y - controllerPos.y)/deltaTime;

				//ensuring move
				if (!AuthorizePos(newControllerPos)) newControllerPos = PushOut(newControllerPos);
				if (!AuthorizePos(newControllerPos)) newControllerPos = transform.position;
				if (!AuthorizeMove(transform.position, newControllerPos)) newControllerPos = transform.position;

				controllerPos = newControllerPos;

				return newControllerPos;
			}

			#if WDEBUG
			public void OnDrawGizmosSelected () 
			{
				Vector3 pos = transform.position;
				for (int i=0; i<iterations; i++)
				{
					pos = EmulateMove(pos, transform.forward*speed, 0.025f);
					DrawChar(pos);
					if (fwdMove) { fwdMove=false; transform.position=pos; }
				}
			}
			#endif

		#endregion

	}
}
