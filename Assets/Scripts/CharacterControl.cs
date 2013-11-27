using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour {
	private CharacterController cc;
	
	private float moveSpeed;
	public float turnSpeed = 150.0f;
	public float initialJumpSpeed = 12.0f;
	public float initialMoveSpeed = 2.0f;
	private float jumpSpeed;
	private Vector3 velocity = Vector3.zero;
	private float damping = .8f;
	private Vector3 rightBeforeJump;
	private bool hasSpeedPowerup;
	private bool hasSpeedPowerdown;
	private bool justLanded;
	public bool isDying;
	private float powerupClock = 0;
	private float powerdownClock = 0;
	bool grounded = false;
	bool anyMovementKeysDown = false;
	bool anyDirectionalKeysDown = false;
	private Vector3 lastHitNormal;
	public ParticleSystem playerFire;
	private bool first = true;
	public AudioClip woodJump;
	public AudioClip concreteJump;
	public AudioClip crush;
	
	public Transform playerFollow;
	
	void Start () {
		Time.timeScale = 0;
		cc = gameObject.GetComponent<CharacterController>();
		this.playerFollow.position = this.transform.position + this.transform.up;
		this.playerFollow.rotation = this.transform.rotation;
		hasSpeedPowerup = false;
		hasSpeedPowerdown = false;
		justLanded = true;
		isDying = false;
		playerFire.Stop();
		jumpSpeed = initialJumpSpeed;
		moveSpeed = initialMoveSpeed;
		cc.Move(transform.forward);
	}
	
	/*void OnMouseDown () {
		Screen.lockCursor = true;
	}*/
	
	void Update () {
		float hRotation = 0.0f;
		float hMovement = 0.0f;
		float sMovement = 0.0f;
		if(!isDying && GameManager.gameStarted)
		{
			/*if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
				hRotation = Input.GetAxis("Mouse X") * turnSpeed;
				anyMovementKeysDown = true;
				Screen.lockCursor = true;
			} else {
				Screen.lockCursor = false;
			}*/
			hRotation = Input.GetAxis("Mouse X") * turnSpeed;
			anyMovementKeysDown = true;
			Screen.lockCursor = true;
			hMovement = Input.GetAxis("Vertical") * moveSpeed;
			if (first) {
				// so hacky, eww
				hMovement = moveSpeed * .1f;
			}
			sMovement = Input.GetAxis("Horizontal") * moveSpeed / 3.0f;
			if (hMovement != 0.0f && !first) {
				hMovement /= Mathf.Abs(Input.GetAxis("Vertical"));
			}
			if (sMovement != 0.0f) {
				sMovement /= Mathf.Abs(Input.GetAxis("Horizontal"));
			}
			if (hRotation != 0.0f || hMovement != 0.0f || sMovement != 0.0f) {
				anyMovementKeysDown = true;
			}
			if (hMovement != 0.0f || sMovement != 0.0f) {
				anyDirectionalKeysDown = true;
			}
			first = false;
		}
		if (Input.GetKey(KeyCode.Space)) {
				GameManager.gameStarted = true;
				Destroy(GameObject.Find("StartScreen"));
				Time.timeScale = 1;
		}
		if (cc.isGrounded) {
			if(justLanded)
			{
				Debug.Log("Landed");
				audio.PlayOneShot(woodJump);
				/*if(col.gameObject.name == "Ground")
				{
					Debug.Log("Ground");
					audio.PlayOneShot(concreteJump);
				}
				else if(col.gameObject.name == "Crate")
				{
					Debug.Log("Crate");
					audio.PlayOneShot(woodJump);
				}*/
				justLanded = false;
			}
			this.GroundedUpdate(hRotation, hMovement, sMovement);
		} else {
			this.AirUpdate(hRotation, hMovement, sMovement);
		}
		powerupClock -= .02f;
		powerdownClock -= .02f;
		if(powerupClock >= 0)
		{
			
			Debug.Log(powerupClock);
			if(powerupClock%0.4f < 0.2f)
			{
				renderer.material.color = Color.green;	
			}
			else
			{
				renderer.material.color = Color.yellow;
			}
			
		}
		else if(powerdownClock >= 0)
		{
			if(powerdownClock%0.4f < 0.2f)
			{
				renderer.material.color = Color.gray;	
			}
			else
			{
				renderer.material.color = Color.red;
			}
		}
		else
		{
			renderer.material.color = Color.white;
			hasSpeedPowerdown = false;
			moveSpeed = initialMoveSpeed;
			jumpSpeed = initialJumpSpeed;
		}
		CheckCollisionFlags();
		anyMovementKeysDown = false;
		anyDirectionalKeysDown = false;
	}
	
	void CheckCollisionFlags () {
		if (velocity.y > 0.0f && (cc.collisionFlags & CollisionFlags.Above) != 0) {
			velocity.y = 0.0f;
			foreach (Collider c in Physics.OverlapSphere(transform.position, collider.bounds.size.y * 2.0f)) {
				if (c.name == "Crate") {
					velocity.y = Mathf.Min(0.0f, c.gameObject.GetComponent<CubeFall>().fallSpeed * -1);
				}
			}
		}
	}
	
	void RotateAndMove (float hRotation, float hMovement, float sMovement) {
		cc.transform.Rotate(Vector3.up, hRotation * Time.deltaTime);
		
		Vector3 movement = cc.transform.forward * hMovement + cc.transform.right * sMovement;
		float dampingFactor = 1.0f;
		if (cc.collisionFlags == CollisionFlags.Sides && anyMovementKeysDown) {
			dampingFactor = .75f;
		}
		if (cc.collisionFlags == CollisionFlags.Below && !anyDirectionalKeysDown) {
			dampingFactor = .5f;
		}
		velocity = new Vector3(velocity.x * damping * dampingFactor, velocity.y, velocity.z * damping * dampingFactor);
		velocity = velocity + movement;
		cc.Move(velocity * Time.deltaTime);
	}
	
	void GroundedUpdate (float hRotation, float hMovement, float sMovement) {
		GameManager.gameStarted = true;
		velocity.y = 0.0f;
		if (Input.GetKey(KeyCode.Space) && !isDying) {
			velocity.y = jumpSpeed;
			justLanded = true;
		}
		RotateAndMove(hRotation, hMovement, sMovement);
		this.playerFollow.position = this.transform.position + this.transform.up;
		this.playerFollow.rotation = this.transform.rotation;
	}
	
	void AirUpdate (float hRotation, float hMovement, float sMovement) {
		if (cc.collisionFlags == CollisionFlags.Sides && anyMovementKeysDown) {
			if (velocity.y > 0.0f) {
				velocity.y += Physics.gravity.y / .5f * Time.deltaTime;
			} else {
				velocity.y += Physics.gravity.y * .1f * Time.deltaTime;
			}
			if (Input.GetKeyDown(KeyCode.Space) && Vector3.Dot(Vector3.up, lastHitNormal) < .5f) {
				Vector3 wallJump = (Vector3.up + lastHitNormal * 2.5f).normalized * jumpSpeed * 2.3f;
				audio.PlayOneShot(woodJump);
				velocity.y = 0;
				velocity += wallJump;
				justLanded = true;
			}
		} else {
			velocity.y += Physics.gravity.y * Time.deltaTime;
		}
		RotateAndMove(hRotation, hMovement * 0.5f, sMovement);
		this.playerFollow.position = this.transform.position + this.transform.up;
	}
	
	void TestForCrate (GameObject go, Vector3 normal) {
		if(go.name == "Crate"){
			Debug.Log(cc.collisionFlags);
			if(Vector3.Dot(normal, Vector3.down) > .8f) {
				if((cc.collisionFlags & CollisionFlags.Above) == CollisionFlags.Above && (cc.collisionFlags & CollisionFlags.Below) == CollisionFlags.Below) {
					// Crate above, something else below, died of squishing
					// more reliable than before
					GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
					isDying = true;
					gm.endGameText = "DIED BY SQUISHING!!";
					
					//Squishing death animation
					audio.PlayOneShot(crush);
					Vector3 originalScale = transform.localScale;
					Vector3 crushedScale = transform.localScale;
					crushedScale.y = 0.1f;
					for(float i=0f; i<3f; i+= 0.01f)
					{
						transform.localScale = Vector3.Lerp(originalScale, crushedScale, i/3.0f);
					}
					gm.endGame = true;
				}
			}
		}
	}
	
	void OnCollisionEnter (Collision col) {
		TestForCrate(col.gameObject, col.contacts[0].normal);
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		lastHitNormal = hit.normal;
		if(hit.gameObject.name == "Powerup"){
			GameManager.gameScore += 20;
			Destroy(hit.gameObject);
			hasSpeedPowerup = true;
			powerupClock = 10f;
			powerdownClock = 0f;
			renderer.material.color = Color.green;
			jumpSpeed = 18f;
			velocity.y = 30.0f;
			justLanded = true;
			if (!isDying) {
				velocity.y = jumpSpeed;
				justLanded = true;
			}
			//RotateAndMove(hRotation, hMovement, sMovement);
			this.playerFollow.position = this.transform.position + this.transform.up;
			this.playerFollow.rotation = this.transform.rotation;
		}
		if(hit.gameObject.name == "Powerdown"){
			GameManager.gameScore -= 20;
			Destroy(hit.gameObject);
			hasSpeedPowerdown = true;
			powerdownClock = 10f;
			powerupClock = 0f;
			renderer.material.color = Color.grey;
			moveSpeed = 0.5f;
		}
		TestForCrate(hit.gameObject, hit.normal);
	}
}
