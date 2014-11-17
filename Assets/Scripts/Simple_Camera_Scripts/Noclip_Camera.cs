using UnityEngine;
using System.Collections;

public class Noclip_Camera : MonoBehaviour
{
	void Start()
	{
		Screen.showCursor = false;
		Screen.lockCursor = true;
	}

	private float pitch = 0;
	private float faster = 1;
	private bool mouseControl = true;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (mouseControl)
			{
				Screen.showCursor = true;
				Screen.lockCursor = false;
				mouseControl = false;
			}
			else
			{
				Screen.showCursor = false;
				Screen.lockCursor = true;
				mouseControl = true;
			}
		}

		faster = 1;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			faster = 12;

		transform.position += transform.rotation * (Input.GetAxis("Vertical") * faster * Vector3.forward);

		transform.position += transform.rotation * (-Input.GetAxis("Horizontal") * faster * Vector3.left);

		if (mouseControl)
		{
			transform.rotation *= Quaternion.Euler(transform.InverseTransformDirection(0, Input.GetAxis("Mouse X"), 0));

			float deltaPitch = -Input.GetAxis("Mouse Y");

			if (pitch + deltaPitch > 90)
				pitch = 90;
			else if (pitch + deltaPitch < -90)
				pitch = -90;
			else
				pitch += deltaPitch;

			transform.rotation = Quaternion.Euler(pitch, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		}
	}
}
