using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class PlayerShip : MonoBehaviour
	{
		private Rigidbody shipRigidbody;

		private float throttle;
		private Vector3 pitchYawRoll;
		private Vector3 pitchYawRollAssist;

		private float shipHealth;
		private float shieldHealth;

		public AnimationCurve pitchAssistCurve;
		public AnimationCurve yawAssistCurve;
		public AnimationCurve rollAssistCurve;

		public float stationaryFieldOfView;
		public float maxSpeedFieldOfView;

		private Camera shipCamera;

		void Start()
		{
			shipRigidbody = gameObject.GetComponent<Rigidbody>();
			shipCamera = gameObject.GetComponentInChildren<Camera>();

			shipRigidbody.angularDrag = 0f;
			shipRigidbody.drag = 0f;

			shipRigidbody.velocity = new Vector3(0, 0, 0);
			shipRigidbody.angularVelocity = new Vector3(0, 0, 0);

			throttle = 0f;

			pitchYawRoll = new Vector3(0f, 0f, 0f);
			pitchYawRollAssist = new Vector3(0f, 0f, 0f);

			shipHealth = 100f;
			shieldHealth = 100f;

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
		}

		private bool afterburner = false;
		private bool afterburnerAvailable = true;
		private float afterburnerFuel = 1f;
		private float directThrottleSet = 0f;
		
		void Update()
		{
			if (Mathf.Abs(pitchYawRoll.x) < PITCH_YAW_ROLL_DEADZONE_PITCH &&
				Mathf.Abs(localRigidbodyAngularVelocity.x) > ANGULAR_VELOCITY_ASSIST_THRESHOLD_PITCH)
				pitchYawRollAssist.x = -Mathf.Sign(localRigidbodyAngularVelocity.x) * MAX_PITCH * pitchAssistCurve.Evaluate(Mathf.Abs(localRigidbodyAngularVelocity.x) * INVERSE_MAX_ANGULAR_VELOCITY.x);
			else
				pitchYawRollAssist.x = 0f;

			if (Mathf.Abs(pitchYawRoll.y) < PITCH_YAW_ROLL_DEADZONE_YAW &&
			    Mathf.Abs(localRigidbodyAngularVelocity.y) > ANGULAR_VELOCITY_ASSIST_THRESHOLD_YAW)
				pitchYawRollAssist.y = -Mathf.Sign(localRigidbodyAngularVelocity.y) * MAX_YAW * yawAssistCurve.Evaluate(Mathf.Abs(localRigidbodyAngularVelocity.y) * INVERSE_MAX_ANGULAR_VELOCITY.y);
			else
				pitchYawRollAssist.y = 0f;

			if (Mathf.Abs(pitchYawRoll.z) < PITCH_YAW_ROLL_DEADZONE_ROLL &&
			    Mathf.Abs(localRigidbodyAngularVelocity.z) > ANGULAR_VELOCITY_ASSIST_THRESHOLD_ROLL)
				pitchYawRollAssist.z = -Mathf.Sign(localRigidbodyAngularVelocity.z) * MAX_ROLL * rollAssistCurve.Evaluate(Mathf.Abs(localRigidbodyAngularVelocity.z) * INVERSE_MAX_ANGULAR_VELOCITY.z);
			else
				pitchYawRollAssist.z = 0f;

			if (afterburner)
				afterburnerFuel -= AFTERBURNER_BURN_RATE * Time.deltaTime;

			if (afterburnerFuel < 0f)
			{
				afterburnerFuel = 0f;
				afterburnerAvailable = false;
				afterburner = false;
			}
		}

		void LateUpdate()
		{
			if (null != shipCamera)
			{
				shipCamera.fieldOfView = stationaryFieldOfView * (1 - speedInterpolation) + maxSpeedFieldOfView * (speedInterpolation);
			}
		}

		void OnGUI()
		{
//			GUI.Label(new Rect(0, 0, Screen.width, 20), "Glo Vel: " + shipRigidbody.velocity.ToString() + " m/s");
//			GUI.Label(new Rect(0, 20, Screen.width, 20), "Glo Spin: " + shipRigidbody.angularVelocity.ToString() + " rad/s");
//			GUI.Label(new Rect(0, 40, Screen.width, 20), "Loc Vel: " + localRigidbodyVelocity.ToString() + " m/s");
//			GUI.Label(new Rect(0, 60, Screen.width, 20), "Loc AngVel: " + localRigidbodyAngularVelocity.ToString() + " rad/s");
//			GUI.Label(new Rect(0, 80, Screen.width, 20), "Loc Acc: " + finalAcceleration.ToString() + " m/s^2");
//			GUI.Label(new Rect(0, 100, Screen.width, 20), "Loc AngAcc: " + angularAcceleration.ToString() + " rad/s^2");
//			GUI.Label(new Rect(0, 120, Screen.width, 20), "Speed: " + speed.ToString() + " m/s");
//			GUI.Label(new Rect(0, 140, Screen.width, 20), "PYR: " + pitchYawRoll.ToString());
//			GUI.Label(new Rect(0, 160, Screen.width, 20), "Throttle: " + throttle.ToString());
//			GUI.Label(new Rect(0, 180, Screen.width, 20), "PYR Assist: " + pitchYawRollAssist.ToString());
//			GUI.Label(new Rect(0, 200, Screen.width, 20), "Afterburner: " + afterburner.ToString());
//			GUI.Label(new Rect(0, 220, Screen.width, 20), "AfterburnerFuel: " + afterburnerFuel.ToString());
//			GUI.Label(new Rect(0, 240, Screen.width, 20), "AfterburnerAvailable: " + afterburnerAvailable.ToString());
		}

		//----------------------------------------------
		// These are to control the ship with physics.
		//----------------------------------------------

		private const float MAX_PITCH = 3f;
		private const float MAX_YAW = 1.5f;
		private const float MAX_ROLL = 6f;

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
		private const float VELOCITY_DAMPENING_CONSTANT_UP = -4f;
		private const float VELOCITY_DAMPENING_CONSTANT_FORWARD = -(90f/700f);//-0.1f;

		private const float SPIN_DAMPENING_CONSTANT = -0.75f;//-1.5f;

		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH = 2.5f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW = 3f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL = 2.5f;
		private Vector3 SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT;

		private const float MAX_FORWARD_ACCELERATION = 90f;//70f;
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

		void FixedUpdate()
		{
			speed = shipRigidbody.velocity.magnitude;
			speedInterpolation = speed * INVERSE_MAX_SPEED;

			localRigidbodyVelocity = transform.InverseTransformDirection(shipRigidbody.velocity);
			localRigidbodyAngularVelocity = transform.InverseTransformDirection(shipRigidbody.angularVelocity);

			Vector3 forwardAcceleration;
			if (afterburner)
			{
				forwardAcceleration = Vector3.forward * MAX_FORWARD_ACCELERATION * AFTERBURNER_SPEED_BOOST * directThrottleSet;
			}
			else
			{
				forwardAcceleration = Vector3.forward * MAX_FORWARD_ACCELERATION * throttle;
			}
			Vector3 brakingAcceleration = -Vector3.forward * 0f;

			angularAcceleration = pitchYawRoll + pitchYawRollAssist +
				Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
				                               (SPIN_DAMPENING_CONSTANT * localRigidbodyAngularVelocity));

			Vector3 sudoDragAcceleration = Vector3PointWiseMultiplication(VELOCITY_DAMPENING_CONSTANT, localRigidbodyVelocity);

			Vector3 localRotationInTimeStep = Vector3PointWiseMultiplication(localRigidbodyAngularVelocity * Time.fixedDeltaTime, ROTATION_VELOCITY_PRESERVATION); // in radians
			Vector3 localRigidbodyVelocityPrime = Quaternion.Euler(localRotationInTimeStep * 57.2957795f) * localRigidbodyVelocity; // in m/s, 57.295... = 180/pi
			Vector3 turningAccelerationChange = (localRigidbodyVelocityPrime - localRigidbodyVelocity) / Time.fixedDeltaTime; // in m/s^2

			finalAcceleration = forwardAcceleration + sudoDragAcceleration + turningAccelerationChange + brakingAcceleration;

			shipRigidbody.AddForce(transform.rotation * finalAcceleration, ForceMode.Acceleration);
			shipRigidbody.AddTorque(transform.rotation * angularAcceleration, ForceMode.Acceleration);
		}

		public float Throttle
		{
			set 
			{
				if (value > 1f)
					value = 1f;
				else if (value < -0.3f)
					value = -0.3f;
				throttle = value;
			}
			get { return throttle; }
		}

		public float Pitch
		{
			set 
			{
				if (value > 1f)
					value = 1f;
				else if (value < -1f)
					value = -1f;
				pitchYawRoll.x = value * MAX_PITCH;
			}
			get { return pitchYawRoll.x; }
		}

		public float Yaw
		{
			set 
			{
				if (value > 1f)
					value = 1f;
				else if (value < -1f)
					value = -1f;
				pitchYawRoll.y = value * MAX_YAW;
			}
			get { return pitchYawRoll.y; }
		}

		public float Roll
		{
			set 
			{
				if (value > 1f)
					value = 1f;
				else if (value < -1f)
					value = -1f;
				pitchYawRoll.z = value * MAX_ROLL;
			}
			get { return pitchYawRoll.z; }
		}

		public bool Afterburner
		{
			set
			{
				if (afterburnerAvailable)
					afterburner = value;
			}
			get { return afterburner; }
		}

		public float AfterburnerFuel
		{
			set
			{
				afterburnerFuel = value;
				afterburnerAvailable = true;
				if (afterburnerFuel > 1f)
				{
					afterburnerFuel = 1f;
				}
				else if (afterburnerFuel < 0f)
				{
					afterburnerFuel = 0f;
					afterburnerAvailable = false;
				}
			}
			get { return afterburnerFuel; }
		}

		public bool AfterburnerAvailable
		{
			get { return afterburnerAvailable; }
		}

		public float DirectThrottleSet
		{
			set
			{
				directThrottleSet = value;
				if (directThrottleSet > 1f)
					directThrottleSet = 1f;
				else if (directThrottleSet < -0.3f)
					directThrottleSet = -0.3f;
			}
			get { return directThrottleSet; }
		}

		public float Speed
		{
			get { return speed; }
		}

		public float ShipHealth
		{
			get { return shipHealth; }
		}

		public float ShieldHealth
		{
			get { return shieldHealth; }
		}

		private Vector3 Vector3PointWiseMultiplication(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
	}
}