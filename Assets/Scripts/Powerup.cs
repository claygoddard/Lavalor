using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision col) {
		foreach (ContactPoint cp in col.contacts) {
			Debug.Log("Powerup: " + cp.otherCollider.gameObject.name);
			if (cp.otherCollider.gameObject.name == "Character")// && cp.otherCollider.gameObject.GetComponent<CharacterController>().isGrounded) 
			{
				GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
				gm.endGame = true;
				gm.endGameText = "DIED BY POWERUP!!";
			}
		}
	}
}
