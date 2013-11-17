using UnityEngine;
using System.Collections;
 
[AddComponentMenu("Camera/3RDPerson Camera")]
public class CameraFollow : MonoBehaviour
{
 
	public Transform target;
	// The distance in the x-z plane to the target
	private float distance = 10.0f;
	// the height we want the camera to be above the target
	private float height = 4.5f;
 
	// Update is called once per frame
	void Update ()
	{
		if (target) {
			// Calculate the current rotation angles
			float wantedRotationAngle = target.eulerAngles.y;
			float wantedHeight = target.position.y + height;
 
			// Convert the angle into a rotation
			Quaternion currentRotation = Quaternion.Euler (0, wantedRotationAngle, 0);
 
			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
 
			Vector3 pos = target.position;
			pos -= currentRotation * Vector3.forward * distance;
			pos.y = wantedHeight;
			transform.position = pos;
 
 
			// Always look at the target
			transform.LookAt (target);
		}
	}
}