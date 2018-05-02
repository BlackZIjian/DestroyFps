using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Voxeland5
{
	public class MoveController : MonoBehaviour 
	{
		public float radius = 0.5f;
		public float height = 2f;
		public float skin = 0.001f;

		public bool cameraSpace = true;
		public float speed = 5f;
		public float shiftSpeed = 9f;
		public float jumpSpeed = 5f; 


		public void Start () 
		{
			CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
			if (capsuleCollider != null)
			{
				radius = capsuleCollider.radius;
				height = capsuleCollider.height;
				capsuleCollider.enabled = false;
			}
		} 

		public void Update () 
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
				lookDir.y = 0;
				transform.rotation = Quaternion.LookRotation(lookDir);
			}

			//reading controls
			float maxSpeed = speed;
			if (Input.GetKey (KeyCode.LeftShift)) maxSpeed = shiftSpeed;
			Vector3 controlsDir = ReadControls(lookDir);
			Vector3 velocity = controlsDir * maxSpeed;

			//moving controller
			Vector3 newPosition = GetMove(transform.position, velocity*deltaTime);
			newPosition = GetFall(newPosition, 5);

			//ensuring move
			if (!AuthorizePos(newPosition)) newPosition = PushOut(newPosition);
			if (!AuthorizePos(newPosition)) newPosition = transform.position;
			if (!AuthorizeMove(transform.position, newPosition)) newPosition = transform.position;

			transform.position = newPosition;

			//track
			#if WDEBUG
			if (writeTrack) WriteTrack(transform.position);
			#endif
		}

		#region Physics

			public RaycastHit CharCast (Vector3 pos, Vector3 direction, float distance)
			{
				Vector3 botSphereCenter = new Vector3(pos.x, pos.y+radius, pos.z);
				Vector3 topSphereCenter = new Vector3(pos.x, pos.y+height-radius, pos.z);

				//if (radius*2 <= height*0.00001f)
				//	Physics.SphereCastNonAlloc(
				//else
				int castsNum = Physics.CapsuleCastNonAlloc(botSphereCenter, topSphereCenter, radius-skin, direction, charCastDatas, distance+skin);

				//no intersections
				if (castsNum == 0)
					return new RaycastHit() { distance=2000000000, point=botSphereCenter };


				//too many intersections
				if (castsNum >= charCastDatas.Length)
				{
					#if WDEBUG
					Debug.Log("Char: Too many intersections");
					#endif
					return new RaycastHit() { distance=0, point=botSphereCenter };
				}


				//finding closest
				RaycastHit hitData = charCastDatas[0];
				float minDist = hitData.distance;
				for (int i=1; i<castsNum; i++)
				{
					float curDist = charCastDatas[i].distance;
					if (curDist < minDist)
					{
						hitData = charCastDatas[i];
						minDist = curDist;
					}
				}


				//setting normal
				//TODO: special case for sphere
				Vector3 hitPos = pos + direction*hitData.distance;
				Vector3 hitPoint = hitData.point;
				float halfHeight = height/2 - radius;

				hitPos.y += radius + halfHeight; //placing pos at the center of the capsule
			
				if (hitPoint.y < botSphereCenter.y)  //finding if bottom or top shere should be used to calculate normal
					hitPos.y -= halfHeight;
			
				else if (hitPoint.y > topSphereCenter.y)
					hitPos.y += halfHeight;

				else //if collided between spere centers - calculating normal in XZ plane
					hitPos.y = hitPoint.y;

				hitData.normal = hitPoint - hitPos;

				#if WDEBUG
				if (hitData.distance <= 0.0000001f) Debug.Log("No move: Cast: " + pos + " " + direction);
				#endif

				//keeping skin offset from hit point (back to school)
				float dot = Vector3.Dot(hitData.normal.normalized, direction);
				float p = (radius-skin)*dot;
				float q = (radius-skin)*(radius-skin) - radius*radius;
				hitData.distance -= Mathf.Sqrt(p*p - q) - p;  //hitData.distance -= (1/dot)*skin;

				return hitData;
			}
			public static RaycastHit[] charCastDatas = new RaycastHit[10];


			public Vector3 GetFall (Vector3 pos, float distance)
			{
				Vector3 sphereCenter = new Vector3(pos.x, pos.y+radius, pos.z); //bottom
				Vector3 castCenter = new Vector3(pos.x, pos.y+radius+skin, pos.z);
				Vector3 castDir = Vector3.down;

				//jump case
				/*if (distance < 0)
				{
					sphereCenter = new Vector3(pos.x, pos.y+height-radius, pos.z); //top
					castCenter = new Vector3(sphereCenter.x, sphereCenter.y-skin, sphereCenter.z);
					castDir = Vector3.up;
					distance = -distance;
				}*/

				if (Physics.CheckSphere(castCenter, radius) ||
					Physics.CheckSphere(sphereCenter, radius) ) return pos; //already intersecting, no fall possible

				int castsNum = Physics.SphereCastNonAlloc(castCenter, radius, castDir, charFallDatas, distance+skin);

				if (castsNum == 0) return pos + castDir*distance; //no intersections

				//too many intersections
				if (castsNum >= charFallDatas.Length)
				{
					#if WDEBUG
					Debug.Log("Fall: Too many intersections");
					#endif
					return pos;
				}

				//finding closest
				float minDist = charFallDatas[0].distance;
				for (int i=1; i<castsNum; i++)
				{
					float curDist = charFallDatas[i].distance;
					if (curDist < minDist) minDist = curDist;
				}

				return pos + castDir*(minDist-skin);
			}
			public static RaycastHit[] charFallDatas = new RaycastHit[10];


			public Vector3 GetSlide (Vector3 pos, Vector3 move, Vector3 refDir=new Vector3(), Vector3 prevPos = new Vector3(), int iterationsLeft=20)
			{
				if (refDir.sqrMagnitude < 0.00001f) refDir = move.normalized;
				if (prevPos.sqrMagnitude<0.00001f) prevPos = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

				if (iterationsLeft <=0) 
				{ 
					#if WDEBUG
					Debug.Log("Left Out Iterations"); 
					#endif
					return pos; 
				}

				Vector3 moveDir = move.normalized;
				float moveLength = move.magnitude;

				//traveled distance
				float distance = moveLength;
				RaycastHit hitData = CharCast(pos, moveDir, moveLength);

				//stepping away from the walls and recasting
				if (distance < 0)
				{
					pos += hitData.normal.normalized*skin;
					hitData = CharCast(pos, moveDir, moveLength);
				}

				distance = Mathf.Min(distance, hitData.distance);

				//next move position
				Vector3 nexMovePos = pos + moveDir*distance;

				//next move length
				float slideFactor = Vector3.Dot(moveDir, refDir);
				if (slideFactor < 0.0001f) slideFactor = 0.0001f; //should not be zero or negative //TODO: return

				float nextMoveLength = moveLength - distance/slideFactor;

				//next move direction
				Vector3 nextMoveDir;

				if (hitData.collider != null)
				{
					//Vector3 hitPos = pos + moveDir*hitData.distance;
					//Vector3 hitNormal = GetHitNormal(hitPos, hitData.point).normalized;
					Vector3 tmpDir = Vector3.Cross(moveDir, hitData.normal).normalized; //note that CharCast assigns char hit normal, not the collider one
					Vector3 slideDir = Vector3.Cross(hitData.normal, tmpDir).normalized;

					/*if (drawGizmos)
					{
						Gizmos.color = Color.blue; 
						Gizmos.DrawSphere(hitData.point, 0.05f);
						Gizmos.DrawRay(hitData.point, slideDir);
						Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y+radius, pos.z) + moveDir*hitData.distance, radius-skin);
					}*/

					nextMoveDir = slideDir;
				}
				else nextMoveDir = refDir;

				//debug
				#if WDEBUG
				if (drawGizmos)
				{
					Gizmos.color = Color.yellow; Gizmos.DrawLine(pos, nexMovePos+Vector3.up*radius);
					if (hitData.collider!=null) Gizmos.color = Color.red;
					else Gizmos.color = Color.green;
					DrawChar(nexMovePos);
					Gizmos.DrawWireSphere(nexMovePos+Vector3.up*radius, radius);
					Gizmos.DrawWireSphere(nexMovePos+Vector3.up*(height-radius), radius);

					//Gizmos.color = Color.blue;
					//if (iterationsLeft==1) Gizmos.DrawRay(pos, nextMoveDir*nextMoveLength);
				}
				#endif

				//returning by no move
				//if (hitData.collider != null && distance < 0.00001f)  
				//	return pos;

				if ((pos-nexMovePos) == Vector3.zero  &&  (pos-prevPos) == Vector3.zero)  //position and direction are the same
				{
					//Debug.Log("No Move: Slide");
					return pos;
				}

				//reached destination
				if (nextMoveLength <= 0.0001f) 
					return pos + move*slideFactor;  

				return GetSlide(nexMovePos, nextMoveDir*nextMoveLength, refDir:refDir, prevPos:pos, iterationsLeft:iterationsLeft-1);
			}


			public Vector3 GetMove (Vector3 pos, Vector3 move, int maxIterations=20)
			{
				Vector3 moveDir = move.normalized;
				float moveLength = move.magnitude;

				float stepDist = radius / 2f;
				int numSteps = (int)(moveLength / stepDist) + 1;
				if (numSteps > maxIterations) numSteps = maxIterations;
				stepDist = moveLength / numSteps;

				for (int i=0; i<numSteps; i++)
				{
					Vector3 newPos = GetSlide(pos, moveDir*stepDist, moveDir.normalized, prevPos:new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity), iterationsLeft:20);
					//if ((pos-newPos).sqrMagnitude < 0.00001f) { Debug.Log("No Move: Move"); return pos; }
					pos = newPos;
				}

				return pos;
			}

			public Vector3 PushOut (Vector3 pos, float range)
			{
				Vector3 botSphereCenter = new Vector3(pos.x, pos.y+radius, pos.z);
				Vector3 topSphereCenter = new Vector3(pos.x, pos.y+height-radius, pos.z);

				float maxDist = 0; //casting from dest to pos, so taking max distance
				Vector3 closestPos = pos;

				foreach (Vector3 dir in RoundDirections(0.5f))
				{
					Vector3 dirBot = botSphereCenter + dir*range;
					Vector3 dirTop = topSphereCenter + dir*range;

					if (Physics.CheckCapsule(dirBot, dirTop, radius)) continue;
					if (Physics.Raycast(botSphereCenter, dir, range)) continue;

					RaycastHit hitData;
					bool hit = Physics.CapsuleCast(dirBot, dirTop, radius-skin, -dir, out hitData, range);
					hitData.distance -= skin*2;
					hitData.distance = Mathf.Max(0, hitData.distance);
					if (!hit) hitData.distance = range;

					if (hitData.distance > maxDist)
					{
						maxDist = hitData.distance;
						closestPos = pos + dir*(range-hitData.distance);
					}
				}

				return closestPos;
			}

			public Vector3 PushOut (Vector3 pos)
			{
				for (int i=1; i<20; i++)
				{
					Vector3 pushOutResult = PushOut(pos, radius*0.25f*i*i);

					if ((pos-pushOutResult) == Vector3.zero) 
					{
						#if WDEBUG
						Debug.Log("Pushing out in " + i + " iterations");
						#endif
						return pushOutResult;
					}
				}
				#if WDEBUG
				Debug.Log("Could not push out");
				#endif
				return pos;
			}

			public IEnumerable<Vector3> RoundDirections (float step)
			{
				for (float x=-1; x<=1.001f; x+=step)
					for (float y=-1; y<1.001f; y+=step)
						for (float z=-1; z<1.001f; z+=step)
				{
					if (x>-0.999f && x<0.999f && y>-0.999f && y<0.999f && z>-0.999f && z<0.999f) continue;
					yield return new Vector3(Mathf.Tan(x), Mathf.Tan(y), Mathf.Tan(z)).normalized;
				}
			}


			public bool AuthorizePos (Vector3 pos)
			{
				Vector3 botSphereCenter = new Vector3(pos.x, pos.y+radius, pos.z);
				Vector3 topSphereCenter = new Vector3(pos.x, pos.y+height-radius, pos.z);

				if (Physics.CheckCapsule(botSphereCenter, topSphereCenter, radius-skin)) 	
				{ 
					#if WDEBUG
					Debug.Log("Illegal Position"); 
					#endif
					return false; 
				}

				if (!Physics.Raycast(botSphereCenter, Vector3.down, (radius+skin)*2)  &&  !Physics.Raycast(botSphereCenter, Vector3.down)) //twice checking raycast (small and infinite) is faster in this case
				{ 
					#if WDEBUG
					Debug.Log("Infinite Fall"); 
					#endif
					return false; 
				}

				else return true;
			}


			public bool AuthorizeMove (Vector3 prevPos, Vector3 newPos)
			{
				Vector3 prevBotCenter = new Vector3(prevPos.x, prevPos.y+radius, prevPos.z);
				Vector3 newBotCenter = new Vector3(newPos.x, newPos.y+radius, newPos.z);
				Vector3 dir = newBotCenter-prevBotCenter;

				if (Physics.Raycast(prevBotCenter, dir, dir.magnitude)) 
				{ 
					#if WDEBUG
					Debug.LogError("Illegal Move"); 
					if (drawGizmos) { Gizmos.color = Color.red; Gizmos.DrawLine(prevBotCenter, newBotCenter); }
					#endif
					return false; 
				}
				else return true;
			}


			public bool IsGrounded (Vector3 pos)
			{
				return Physics.CheckSphere(new Vector3(pos.x, pos.y+radius-skin*2, pos.z), radius);
			}

		#endregion

		#region Control

			public Vector3 ReadControls (Vector3 forward)
			{
				//read controls
				Vector3 direction = new Vector3(0,0,0);
	
				//if (controller.isGrounded)
				{
					Vector3 side= Vector3.Cross(Vector3.up, forward);

					if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {direction += forward; }
					if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {direction -= forward; }
					if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {direction += side; }
					if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {direction -= side; }

					direction = direction.normalized;
				}

				return direction;
			}

			public void Accelerate (ref Vector3 velocity, Vector3 direction, float acceleration)
			{
				#if !WDEBUG
				float deltaTime = Time.deltaTime;
				#endif
				
				//accelerating/stopping
				if (direction.sqrMagnitude > 0.01f) //if moving command
				{
					Vector3 velocityModifier = direction * acceleration * deltaTime;
					velocity += velocityModifier; //clamp velocity later
				}
				else
				{
					Vector3 velocityModifier = velocity.normalized * acceleration * deltaTime;
					if (velocity.sqrMagnitude > velocityModifier.sqrMagnitude) //stopping if still moving
					{
						//if (controller.isGrounded) 
							velocity -= velocityModifier;  
					}
					else velocity = Vector3.zero;  //stand still
				}


				//clamp horizontal speed
				float maxSpeed = speed;
				if (Input.GetKey (KeyCode.LeftShift)) maxSpeed = shiftSpeed;
				velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
			}

			protected static Vector3 AvgPos (List<Vector3> poses, List<float> times, float time, bool clampPosesCount=true)
			{
				Vector3 positionsAvg = new Vector3();
				float timeSum = 0;
				int numEntriesUsed = 0;

				for (int i = poses.Count-1; i>=0; i--)
				{
					//getting points and time
					Vector3 nextPos, prevPos;
					float segmentTime;

					if (i!=0)
					{
						nextPos = poses[i];
						prevPos = poses[i-1];
						segmentTime = times[i];
					}
					else //if last point left
					{
						nextPos = poses[i];
						prevPos = poses[i];
						segmentTime = time;
					}

					//find segment avg pos
					float partialSegmentTime = Mathf.Min(segmentTime, time-timeSum);  //for the furthest segment, that should be taken partly
					float percent = partialSegmentTime / segmentTime;
					percent /= 2;

					Vector3 pos = nextPos*(1-percent) + prevPos*percent;

					//adding segment avg pos with time factor
					timeSum += partialSegmentTime;
					positionsAvg += pos * (partialSegmentTime / time);
					numEntriesUsed ++;

					if (timeSum + 0.00001f > time) break;
				}

				//clearing the list from unused items
				if (clampPosesCount && poses.Count-numEntriesUsed-1 >= 1)
				{
					poses.RemoveRange(0, poses.Count-numEntriesUsed-1);
					times.RemoveRange(0, times.Count-numEntriesUsed-1);
				}

				return positionsAvg;
			}

		#endregion


		#region Debug
		#if WDEBUG

			public bool useLag = false;
			public float lagTime = 0.1f;
			protected float oldTime;
			protected float deltaTime; 

			public bool writeTrack = false;
			private Vector3[] track;
			private float[] timeTrack;
			public bool backTrack = false;
			public bool fwdMove = false;

			#if WDEBUG
			private static bool drawGizmos = false;
			#endif


			public void WriteTrack (Vector3 pos)
			{
				if (track == null || track.Length == 0) track = new Vector3[1000];
				if (timeTrack == null || timeTrack.Length == 0) timeTrack = new float[1000];
				ArrayTools.Add(ref track, pos);
				ArrayTools.Add(ref timeTrack, deltaTime);
				ArrayTools.RemoveAt(ref track, 0);
				ArrayTools.RemoveAt(ref timeTrack, 0);
			}


			public void DrawTrack () 
			{
				for (int i=0; i<track.Length-1; i++)
				{
					//Gizmos.color = new Color(timeTrack[i]/0.02f, 1-timeTrack[i]/0.03f, 0);

					float dist = (track[i] - track[i+1]).magnitude;
					float time = timeTrack[i+1];
					float vel = dist / time;

					Gizmos.color = new Color(1-vel/(speed*2), vel/(speed*2), 0);

					if (i%10==0) Gizmos.DrawCube(track[i], Vector3.one*0.05f);
					else Gizmos.DrawCube(track[i], Vector3.one*0.02f);
					Gizmos.DrawLine(track[i], track[i+1]);

					for (int j=i-10; j<i+10; j++)
					{
						if (j<0 || j>=track.Length-1) continue;
						if ((track[j]-track[i]).sqrMagnitude < 0.001f && j!=i) Gizmos.DrawWireCube(track[i], Vector3.one*0.06f);
					}
				}
			}

			public void BackTrack ()
			{
				transform.position = track[track.Length-1];
				ArrayTools.Insert(ref track, 0, track[0]);
				ArrayTools.Insert(ref timeTrack, 0, timeTrack[0]);
				ArrayTools.RemoveAt(ref track, track.Length-1);
				ArrayTools.RemoveAt(ref timeTrack, timeTrack.Length-1);
			}


			public void OnDrawGizmos () 
			{
				if (writeTrack && track!=null) DrawTrack();
				if (backTrack && track!=null) { backTrack=false; BackTrack(); }
			}


			public static void DrawArc (Vector3 pos, float radius, float startAngle=0, float endAngle=360)
			{
				#if UNITY_EDITOR
				Vector3 camPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
				#else
				Vector3 camPos = Camera.main.transform.position;
				#endif

				Vector3 camDir = (pos - camPos).normalized;
				Vector3 camUp = Camera.main.transform.up;

				Quaternion rot = Quaternion.LookRotation(camDir);
				Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);

				Vector3 start = new Vector3( Mathf.Sin((startAngle)*Mathf.Deg2Rad), Mathf.Cos((startAngle)*Mathf.Deg2Rad), 0 ) * radius;
				start = mat.MultiplyPoint(start);
				start += pos;
				Vector3 prev = start;

				float step = 5;
				for (float angle=startAngle; angle<=endAngle; angle+=step)
				{
					Vector3 next = new Vector3( Mathf.Sin((angle)*Mathf.Deg2Rad), Mathf.Cos((angle)*Mathf.Deg2Rad), 0 ) * radius;
					next = mat.MultiplyPoint(next);
					next += pos;

					Gizmos.DrawLine(prev, next);

					prev = next;
				}
			}


			public void DrawChar (Vector3 pos, bool drawInnerSpheres=false)
			{
				Vector3 botSphereCenter = new Vector3(pos.x, pos.y+radius, pos.z);
				Vector3 topSphereCenter = new Vector3(pos.x, pos.y+height-radius, pos.z);

				//DrawArc(botSphereCenter, radius, 90, 270);
				//DrawArc(topSphereCenter, radius, -90, 90);
				Gizmos.DrawWireSphere(botSphereCenter, radius);
				Gizmos.DrawWireSphere(topSphereCenter, radius);

				if (drawInnerSpheres)
				{
					float oldAlpha = Gizmos.color.a;
					Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.333f);
					DrawArc(botSphereCenter, radius-skin, 90, 270);
					DrawArc(topSphereCenter, radius-skin, -90, 90);
					Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, oldAlpha);
				}
			}

			public Vector3 CheckFall ()
			{
				for (int i=0; i<100000; i++)
				{
					Vector3 rndPos = new Vector3(Random.Range(-29,-1), 0, Random.Range(1,29));
					Vector3 fallPos = GetFall(rndPos, 100);
					Vector3 secondFallPos = GetFall(fallPos, 10);

					if ((fallPos-secondFallPos).sqrMagnitude > 0.00000001f) return fallPos;
				}
				return Vector3.zero;
			}

		#endif
		#endregion

	}
}
