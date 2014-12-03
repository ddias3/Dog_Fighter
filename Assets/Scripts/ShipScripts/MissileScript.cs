using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class MissileScript : MonoBehaviour {
		
		private Transform target;
        public int playerNumber;

		void Start() {
			//rigidbody.velocity += transform.rotation * new Vector3(0,0,300);

            previousFrameRotation = currentFrameRotation = transform.rotation;

            SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT = new Vector3(SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH,
                                                                     SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW,
                                                                     SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL);
            
            VELOCITY_DAMPENING_CONSTANT = new Vector3(VELOCITY_DAMPENING_CONSTANT_RIGHT,
                                                      VELOCITY_DAMPENING_CONSTANT_UP,
                                                      VELOCITY_DAMPENING_CONSTANT_FORWARD);
            
            PITCH_YAW_ROLL_DEADZONE = new Vector3(PITCH_YAW_ROLL_DEADZONE_PITCH,
                                                  PITCH_YAW_ROLL_DEADZONE_YAW,
                                                  PITCH_YAW_ROLL_DEADZONE_ROLL);
            
            ANGULAR_VELOCITY_ASSIST_THRESHOLD = new Vector3(ANGULAR_VELOCITY_ASSIST_THRESHOLD_PITCH,
                                                            ANGULAR_VELOCITY_ASSIST_THRESHOLD_YAW,
                                                            ANGULAR_VELOCITY_ASSIST_THRESHOLD_ROLL);
            
            MAX_ANGULAR_VELOCITY_PITCH = -MAX_PITCH / SPIN_DAMPENING_CONSTANT;
            MAX_ANGULAR_VELOCITY_YAW = -MAX_YAW / SPIN_DAMPENING_CONSTANT;
            MAX_ANGULAR_VELOCITY_ROLL = -MAX_ROLL / SPIN_DAMPENING_CONSTANT;
            MAX_ANGULAR_VELOCITY = new Vector3(MAX_ANGULAR_VELOCITY_PITCH,
                                               MAX_ANGULAR_VELOCITY_YAW,
                                               MAX_ANGULAR_VELOCITY_ROLL);
            INVERSE_MAX_ANGULAR_VELOCITY = new Vector3(1f / MAX_ANGULAR_VELOCITY.x,
                                                       1f / MAX_ANGULAR_VELOCITY.y,
                                                       1f / MAX_ANGULAR_VELOCITY.z);
            ROTATION_VELOCITY_PRESERVATION = new Vector3(ROTATION_VELOCITY_PRESERVATION_PITCH,
                                                         ROTATION_VELOCITY_PRESERVATION_YAW,
                                                         ROTATION_VELOCITY_PRESERVATION_ROLL);
            
            MAX_SPEED = -MAX_FORWARD_ACCELERATION / VELOCITY_DAMPENING_CONSTANT_FORWARD;
            INVERSE_MAX_SPEED = 1f / MAX_SPEED;
            timeOut = 5f;
		}

        private const float MAX_PITCH = 3f;
        private const float MAX_YAW = 1.5f;
        private const float MAX_ROLL = 6f;

        private const float MAX_TURN_ANGLE = 3f;
        
        private Vector3 PITCH_YAW_ROLL_DEADZONE;
        private const float PITCH_YAW_ROLL_DEADZONE_PITCH = 0.15f;
        private const float PITCH_YAW_ROLL_DEADZONE_YAW = 0.15f;
        private const float PITCH_YAW_ROLL_DEADZONE_ROLL = 0.05f;
        
        private Vector3 ANGULAR_VELOCITY_ASSIST_THRESHOLD;
        private const float ANGULAR_VELOCITY_ASSIST_THRESHOLD_PITCH = 0.01f;
        private const float ANGULAR_VELOCITY_ASSIST_THRESHOLD_YAW = 0.01f;
        private const float ANGULAR_VELOCITY_ASSIST_THRESHOLD_ROLL = 0.01f;
        
        private Vector3 MAX_ANGULAR_VELOCITY;
        private float MAX_ANGULAR_VELOCITY_PITCH;
        private float MAX_ANGULAR_VELOCITY_YAW;
        private float MAX_ANGULAR_VELOCITY_ROLL;
        private Vector3 INVERSE_MAX_ANGULAR_VELOCITY;
        
        private Vector3 VELOCITY_DAMPENING_CONSTANT;
        private const float VELOCITY_DAMPENING_CONSTANT_RIGHT = -2f;
        private const float VELOCITY_DAMPENING_CONSTANT_UP = -2f;
        private const float VELOCITY_DAMPENING_CONSTANT_FORWARD = -(90f/900f);//-0.1f;
        
        private const float SPIN_DAMPENING_CONSTANT = -0.75f;//-1.5f;
        
        private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH = 2.5f;
        private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW = 3f;
        private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL = 2.5f;
        private Vector3 SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT;
        
        private const float MAX_FORWARD_ACCELERATION = 120f;//70f;
        private float MAX_SPEED;
        private float INVERSE_MAX_SPEED;
        private const float AFTERBURNER_SPEED_BOOST = 2f;
        private const float AFTERBURNER_BURN_RATE = 0.8f;
        
        private Vector3 ROTATION_VELOCITY_PRESERVATION;
        private const float ROTATION_VELOCITY_PRESERVATION_PITCH = 0.9f;
        private const float ROTATION_VELOCITY_PRESERVATION_YAW = 0.9f;
        private const float ROTATION_VELOCITY_PRESERVATION_ROLL = 0.99f;
        
        private Vector3 localRigidbodyVelocity;
        private Vector3 localRigidbodyAngularVelocity;
        private Vector3 finalAcceleration;
        private Vector3 angularAcceleration;
        private float speed;
        private float speedInterpolation;
        
        private const float FLARE_RANGE=1000f;
        
        private float timeOut;
        
		void Update() {
			if(target == null){
				timeOut-= Time.deltaTime;
			}
			if(timeOut <= 0){
				Destroy(this.gameObject);
			}
			GameObject[] flares = GameObject.FindGameObjectsWithTag("Flare");
			if(null != target)
            {
				foreach(GameObject f in flares){
					float distToFlare = Vector3.Distance(transform.position,f.transform.position);
					float flareToTarget = Vector3.Distance(f.transform.position,target.position);
					float distToTarget = Vector3.Distance(transform.position,target.position);
					if(distToFlare<FLARE_RANGE && flareToTarget<distToTarget && distToTarget>distToFlare){
						target = f.transform;
						break;
					}
				}
			}
		}

        private Quaternion previousFrameRotation;
        private Quaternion currentFrameRotation;
        
        void FixedUpdate()
        {
            speed = rigidbody.velocity.magnitude;
            speedInterpolation = speed * INVERSE_MAX_SPEED;
            
            localRigidbodyVelocity = transform.InverseTransformDirection(rigidbody.velocity);
            localRigidbodyAngularVelocity = transform.InverseTransformDirection(rigidbody.angularVelocity);
            
            Vector3 forwardAcceleration = Vector3.forward * MAX_FORWARD_ACCELERATION;
            Vector3 brakingAcceleration = -Vector3.forward * 0f;

            if (null != target)
            {
//                Vector3 directionToTarget = (target.position - transform.position).normalized;
//                float angleToTarget = Vector3.Angle(target.forward, directionToTarget);
//                Vector3 directionToTargetLocal = transform.InverseTransformDirection(directionToTarget);
//                Vector3 pitchYaw = new Vector3(directionToTargetLocal.x, directionToTarget.y, 0f);
//
//                if (angleToTarget < MAX_TURN_ANGLE)
//                    angularAcceleration = angleToTarget * pitchYaw +
//                        Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
//                                                       (SPIN_DAMPENING_CONSTANT * localRigidbodyAngularVelocity));
//                else
//                    angularAcceleration = MAX_TURN_ANGLE * pitchYaw +
//                    Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
//                                                   (SPIN_DAMPENING_CONSTANT * localRigidbodyAngularVelocity));
                transform.LookAt(target);
                currentFrameRotation = transform.rotation;

                RaycastHit hit = new RaycastHit ();
                Vector3 dir = rigidbody.velocity / rigidbody.velocity.magnitude;
                if(Physics.Raycast(transform.position,dir,out hit)){
                    if(Vector3.Distance(hit.point, transform.position)<rigidbody.velocity.magnitude){
                        PlayerShip playerShip = hit.collider.gameObject.GetComponent<PlayerShip>();
                        if (null != playerShip)
                        {
                            if (playerShip.DecrementHealth(60))
                            {
                                SceneManager.SendMessageToAction(null, "DeathMatchAction", "kill " + playerNumber);
                            }
                        }
                        Destroy(this.gameObject);
                    }
                }

                if (Vector3.Distance(transform.position, target.position) < rigidbody.velocity.magnitude)
                {
                    PlayerShip playerShip = target.gameObject.GetComponent<PlayerShip>();
                    if (null != playerShip)
                    {
                        if (playerShip.DecrementHealth(60))
                            SceneManager.SendMessageToAction(null, "DeathMatchAction", "kill " + playerNumber);
                    }
                    Destroy(this.gameObject);
                }
            }

            angularAcceleration = Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
                                                   (SPIN_DAMPENING_CONSTANT * localRigidbodyAngularVelocity));
            
            Vector3 sudoDragAcceleration = Vector3PointWiseMultiplication(VELOCITY_DAMPENING_CONSTANT, localRigidbodyVelocity);

//            Vector3 localRotationInTimeStep = Vector3PointWiseMultiplication(localRigidbodyAngularVelocity * Time.fixedDeltaTime, ROTATION_VELOCITY_PRESERVATION); // in radians
//            Vector3 localRigidbodyVelocityPrime = Quaternion.Euler(localRotationInTimeStep * 57.2957795f) * localRigidbodyVelocity; // in m/s, 57.295... = 180/pi
//            Vector3 turningAccelerationChange = (localRigidbodyVelocityPrime - localRigidbodyVelocity) / Time.fixedDeltaTime; // in m/s^2
            
            finalAcceleration = forwardAcceleration + sudoDragAcceleration;// + turningAccelerationChange + brakingAcceleration;
            
            rigidbody.AddForce(transform.rotation * finalAcceleration, ForceMode.Acceleration);
            rigidbody.AddTorque(transform.rotation * angularAcceleration, ForceMode.Acceleration);

            previousFrameRotation = currentFrameRotation;
        }

        private Vector3 Vector3PointWiseMultiplication(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

//		void FixedUpdate() {
//			if(target != null){
//				Vector3 heading = target.position - transform.position;
//				Vector3 direction = heading / heading.magnitude;
//				Vector3 currDir = transform.TransformDirection(new Vector3(0,0,1));
//				transform.rotation = Quaternion.LookRotation(Vector3.Slerp(currDir,direction,.5f));
//				rigidbody.velocity = transform.rotation * rigidbody.velocity;
//			}
//
//			RaycastHit hit = new RaycastHit ();
//			Vector3 dir = rigidbody.velocity / rigidbody.velocity.magnitude;
//			if(Physics.Raycast(transform.position,dir,out hit)){
//				if(Vector3.Distance(hit.point, transform.position)<rigidbody.velocity.magnitude){
//					//collision
//					Destroy(this.gameObject);
//				}
//			}
//		}
		
		public void SetTarget(Transform newTarget){
			target = newTarget;
		}
	}
}