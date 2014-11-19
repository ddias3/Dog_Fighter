using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class AsteroidHardBound : MonoBehaviour
	{
        public Vector3 adjustVector;

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Asteroid")
            {
                Rigidbody otherRigidbody = other.rigidbody;
                otherRigidbody.velocity = new Vector3(adjustVector.x * otherRigidbody.velocity.x,
                                                      adjustVector.y * otherRigidbody.velocity.y,
                                                      adjustVector.z * otherRigidbody.velocity.z);
            }
        }
	}
}