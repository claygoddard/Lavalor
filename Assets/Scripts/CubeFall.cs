using UnityEngine;
using System.Collections;

public class CubeFall : MonoBehaviour {
	public float fallSpeed = 5.0f;
	public bool grounded = false;
	
	// Use this for initialization
	void Start () {
		renderer.receiveShadows = true;
		while(Physics.CheckSphere(transform.position, renderer.bounds.size.y / 2.0f)) {
			transform.Translate(Vector3.up * renderer.bounds.size.y, Space.World);
		}
	}
	
	public void SetRotation (bool xRotate, bool yRotate) {
		if (xRotate) {
			transform.Rotate(transform.right, 90, Space.Self);
		}
		if (yRotate) {
			transform.Rotate(transform.up, 90, Space.Self);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!grounded) {
			transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
		}
	}
	
	void OnCollisionEnter(Collision col) {
		foreach (ContactPoint cp in col.contacts) {
			if (Vector3.Dot(cp.normal, Vector3.up) > .9) {
				if ((cp.otherCollider.gameObject.name == "Ground" || cp.otherCollider.gameObject.name == "Crate" || cp.otherCollider.gameObject.name == "Checkpoint")) {
					if (cp.otherCollider.gameObject.name == "Crate") {
						CubeFall theirs = cp.otherCollider.gameObject.GetComponent<CubeFall>();
						if (theirs.grounded) {
							grounded = true;
							GameObject.Find("GameManager").GetComponent<GameManager>().CrateGrounded(transform.position.y);
						} else {
							fallSpeed = theirs.fallSpeed;
						}
					} else {
						grounded = true;
						GameObject.Find("GameManager").GetComponent<GameManager>().CrateGrounded(transform.position.y);
					}
				}
			}
		}
	}
}
