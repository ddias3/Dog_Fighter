using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class PlayerShip : MonoBehaviour
	{
		private Rigidbody shipRigidbody;

		private float throttle;
		private Vector3 pitchYawRoll;		

		void Start()
		{
			shipRigidbody = gameObject.GetComponent<Rigidbody>();

			shipRigidbody.angularDrag = 0f;
			shipRigidbody.drag = 0f;

			shipRigidbody.velocity = new Vector3(0, 0, 0);
			shipRigidbody.angularVelocity = new Vector3(0, 0, 0);

			throttle = 0f;
			pitchYawRoll = new Vector3(0f, 0f, 0f);

			SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT = new Vector3(SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH,
			                                                         SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW,
			                                                         SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL);

			VELOCITY_DAMPENING_CONSTANT = new Vector3(VELOCITY_DAMPENING_CONSTANT_RIGHT,
			                                          VELOCITY_DAMPENING_CONSTANT_UP,
			                                          VELOCITY_DAMPENING_CONSTANT_FORWARD);
		}
		
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
				shipRigidbody.velocity = Vector3.zero;
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
		}

		private Vector3 VELOCITY_DAMPENING_CONSTANT;
		private const float VELOCITY_DAMPENING_CONSTANT_RIGHT = -2f;
		private const float VELOCITY_DAMPENING_CONSTANT_UP = -4f;
		private const float VELOCITY_DAMPENING_CONSTANT_FORWARD = -0.1f;
		private const float SPIN_DAMPENING_CONSTANT = -1.5f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_PITCH = 3.6f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_YAW = 3f;
		private const float SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT_ROLL = 2f;
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
			angularAcceleration = pitchYawRoll + Vector3PointWiseMultiplication((SPIN_DAMPENING_MOVEMENT_TOP_SPEED_CONSTANT * (speedInterpolation) + Vector3.one * (1 - speedInterpolation)),
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
				if (value > 1f)
					value = 1f;
				if (value < -0.2f)
					value = -0.2f;
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
				pitchYawRoll.x = value * 3f;
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
				pitchYawRoll.y = value * 3f;
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
				pitchYawRoll.z = value * 6f;
			}
			get { return pitchYawRoll.z; }
		}


		private Vector3 Vector3PointWiseMultiplication(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
	}
}