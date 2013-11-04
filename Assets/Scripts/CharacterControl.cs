using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour {
	private CharacterController cc;
	
	private float moveSpeed = 1f;
	private float turnSpeed = 150.0f;
	private float jumpSpeed = 12.0f;
	private Vector3 velocity = Vector3.zero;
	private float damping = .9f;
	private Vector3 rightBeforeJump;
	private bool hasSpeedPowerup;
	private float powerupClock = 0;
	bool grounded = false;
	bool anyMovementKeysDown = false;
	private Vector3 lastHitNormal;
	
	public Transform playerFollow;
	
	void Start () {
		cc = gameObject.GetComponent<CharacterController>();
		this.playerFollow.position = this.transform.position + this.transform.up;
		this.playerFollow.rotation = this.transform.rotation;
	}
	
	void Update () {
		float hRotation = 0.0f;
		float hMovement = 0.0f;
		
		if (Input.GetKey(KeyCode.LeftArrow)) {
			hRotation -= turnSpeed;
			anyMovementKeysDown = true;
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			hRotation += turnSpeed;
			anyMovementKeysDown = true;
		}
		
		if (Input.GetKey(KeyCode.UpArrow)) {
			hMovement += moveSpeed;
			anyMovementKeysDown = true;
		}
		if (Input.GetKey(KeyCode.DownArrow)) {
			hMovement -= moveSpeed;
			anyMovementKeysDown = true;
		}
		
		if (cc.isGrounded) {
			this.GroundedUpdate(hRotation, hMovement);
		} else {
			this.AirUpdate(hRotation, hMovement);
		}
		powerupClock -= .02f;
		if(powerupClock >= 0)
		{
			if(powerupClock%0.4f < 0.2f)
			{
				renderer.material.color = Color.green;	
			}
			else
			{
				renderer.material.color = Color.yellow;
			}
		}
		else
		{
			renderer.material.color = Color.white;
			hasSpeedPowerup = false;
			jumpSpeed = 12f;
			moveSpeed = 1f;
		}
		CheckCollisionFlags();
		anyMovementKeysDown = false;
	}
	
	void CheckCollisionFlags () {
		if (velocity.y > 0.0f && (cc.collisionFlags & CollisionFlags.Above) != 0) {
			velocity.y = 0.0f;
		}
	}
	
	void RotateAndMove (float hRotation, float hMovement) {
		cc.transform.Rotate(Vector3.up, hRotation * Time.deltaTime);
		
		Vector3 movement = cc.transform.forward * hMovement;
		float dampingFactor = 1.0f;
		if (cc.collisionFlags == CollisionFlags.Sides && anyMovementKeysDown) {
			dampingFactor = .75f;
		}
		velocity = new Vector3(velocity.x * damping * dampingFactor, velocity.y, velocity.z * damping * dampingFactor);
		velocity = velocity + movement;
		cc.Move(velocity * Time.deltaTime);
	}
	
	void GroundedUpdate (float hRotation, float hMovement) {
		GameManager.gameStarted = true;
		velocity.y = 0.0f;
		if (Input.GetKey(KeyCode.Space)) {
			velocity.y = jumpSpeed;
		}
		RotateAndMove(hRotation, hMovement);
		this.playerFollow.position = this.transform.position + this.transform.up;
		this.playerFollow.rotation = this.transform.rotation;
	}
	
	void AirUpdate (float hRotation, float hMovement) {
		if (cc.collisionFlags == CollisionFlags.Sides && anyMovementKeysDown) {
			if (velocity.y > 0.0f) {
				velocity.y += Physics.gravity.y / .5f * Time.deltaTime;
			} else {
				velocity.y += Physics.gravity.y * .1f * Time.deltaTime;
			}
			if (Input.GetKeyDown(KeyCode.Space)) {
				Vector3 wallJump = (Vector3.up + lastHitNormal * 2.5f).normalized * jumpSpeed * 2.3f;
				velocity.y = 0;
				velocity += wallJump;
			}
		} else {
			velocity.y += Physics.gravity.y * Time.deltaTime;
		}
		RotateAndMove(hRotation, hMovement);
		this.playerFollow.position = this.transform.position + this.transform.up;
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		lastHitNormal = hit.normal;
		if(hit.gameObject.name == "Powerup"){
			GameManager.gameScore += 20;
			Destroy(hit.gameObject);
			hasSpeedPowerup = true;
			powerupClock = 10f;
			renderer.material.color = Color.green;
			moveSpeed = 1.5f;
			jumpSpeed = 18f;
		}
	}
}
