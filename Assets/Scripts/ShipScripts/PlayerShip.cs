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

		private float barHeight = 80;
		private float barWidth = 10;
		private float barSpace = 14;
		private float pinch = 0;

		public AnimationCurve pitchAssistCurve;
		public AnimationCurve yawAssistCurve;
		public AnimationCurve rollAssistCurve;

		public Texture image;

		void Start()
		{
			shipRigidbody = gameObject.GetComponent<Rigidbody>();

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
		}
		
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
				shipRigidbody.velocity = Vector3.zero;

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
		}

		void OnGUI()
		{
			GUI.Label(new Rect(0, 0, Screen.width, 20), "Glo Vel: " + shipRigidbody.velocity.ToString() + " m/s");
			GUI.Label(new Rect(0, 20, Screen.width, 20), "Glo Spin: " + shipRigidbody.angularVelocity.ToString() + " rad/s");
			GUI.Label(new Rect(0, 40, Screen.width, 20), "Loc Vel: " + localRigidbodyVelocity.ToString() + " m/s");
			GUI.Label(new Rect(0, 60, Screen.width, 20), "Loc AngVel: " + localRigidbodyAngularVelocity.ToString() + " rad/s");
			GUI.Label(new Rect(0, 80, Screen.width, 20), "Loc Acc: " + finalAcceleration.ToString() + " m/s^2");
			GUI.Label(new Rect(0, 100, Screen.width, 20), "Loc AngAcc: " + angularAcceleration.ToString() + " rad/s^2");
			GUI.Label(new Rect(0, 120, Screen.width, 20), "Speed: " + speed.ToString() + " m/s");
			GUI.Label(new Rect(0, 140, Screen.width, 20), "PYR: " + pitchYawRoll.ToString());
			GUI.Label(new Rect(0, 160, Screen.width, 20), "Throttle: " + throttle.ToString());
			GUI.Label(new Rect(0, 180, Screen.width, 20), "PYR Assist: " + pitchYawRollAssist.ToString());

			for (float i=0; i<Speed/25; i++) {
				if (i > 0 && i < 4) { pinch = 30; }
				else if (i > 4) { pinch -= 5; }
				else { pinch = 15; }
				GUI.Box (new Rect(Screen.width - (barHeight - pinch/2), Screen.height - (barSpace*i + barWidth), barHeight - pinch, barWidth), image);
			}
		}

		//----------------------------------------------
		// These are to control the ship with physics.
		//----------------------------------------------

		private const float MAX_PITCH = 3f;
		private const float MAX_YAW = 1f;
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
		private const float VELOCITY_DAMPENING_CONSTANT_FORWARD = -0.1f;

		private const float SPIN_DAMPENING_CONSTANT = -0.75f;//-1.5f;

		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH = 1.8f;//3.6f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW = 1.5f;//3f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL = 1f;//2f;
		private Vector3 SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT;

		private const float MAX_FORWARD_ACCELERATION = 20f;
		private const float INVERSE_MAX_SPEED = 0.005f;
		
		private Vector3 localRigidbodyVelocity;
		private Vector3 localRigidbodyAngularVelocity;
		private Vector3 finalAcceleration;
		private Vector3 angularAcceleration;
		private float speed;

		void FixedUpdate()
		{
			speed = shipRigidbody.velocity.magnitude;
			float speedInterpolation = speed * INVERSE_MAX_SPEED;

			localRigidbodyVelocity = transform.InverseTransformDirection(shipRigidbody.velocity);
			localRigidbodyAngularVelocity = transform.InverseTransformDirection(shipRigidbody.angularVelocity);

			Vector3 forwardAcceleration = Vector3.forward * MAX_FORWARD_ACCELERATION * throttle;

			angularAcceleration = pitchYawRoll + pitchYawRollAssist +
				Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
				                               (SPIN_DAMPENING_CONSTANT * localRigidbodyAngularVelocity));

			Vector3 sudoDragAcceleration = Vector3PointWiseMultiplication(VELOCITY_DAMPENING_CONSTANT, localRigidbodyVelocity);

			finalAcceleration = forwardAcceleration + sudoDragAcceleration;

			shipRigidbody.AddForce(transform.rotation * finalAcceleration, ForceMode.Acceleration);
			shipRigidbody.AddTorque(transform.rotation * angularAcceleration, ForceMode.Acceleration);
		}

		public float Throttle
		{
			set 
			{
				if (value > 4f)
					value = 4f;
				if (value < -1f)
					value = -1f;
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
				if (value < -1f)
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
				if (value < -1f)
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
				if (value < -1f)
					value = -1f;
				pitchYawRoll.z = value * MAX_ROLL;
			}
			get { return pitchYawRoll.z; }
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