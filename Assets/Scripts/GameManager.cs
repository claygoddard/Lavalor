using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public GameObject lava;
	public GameObject player;
	private float lavaSpeed = 1f;
		
	private GameObject crate;
	private GameObject crateLong;
	private float newCrateCounter = 0.0f;
	private float newCrateCounterMax = 1.0f;
	
	private float groundWidth = 10.0f;
	
	private ArrayList lastTransparentObjects;
	private Shader normalCrateShader;
	private Shader transparentCrateShader;
	
	public bool endGame = false;
	private float endGameCount = 0.0f;
	private float endGameTime = 2.0f;
	public string endGameText;
	
	public static int gameScore = 0;
	public static bool gameStarted = false;
	
	void Start () {
		crate = Resources.Load("Crate") as GameObject;
		crateLong = Resources.Load("CrateLong") as GameObject;
		lastTransparentObjects = new ArrayList();
		normalCrateShader = Shader.Find("Diffuse");
		transparentCrateShader = Shader.Find("Transparent/Diffuse with Shadow");
		Physics.gravity = Vector3.down * 17.5f;
	}
	
	void OnGUI () {
		if (endGame) {
			GUI.Label(new Rect(Screen.width/2.0f,0.0f,Screen.width, Screen.height), endGameText);
		}
		GUI.Label(new Rect(0.0f,0.0f,Screen.width, Screen.height), "SCORE: " + gameScore);
		GUI.Label(new Rect(0.0f,20.0f,Screen.width, Screen.height), "HIGH SCORE: " + PlayerPrefs.GetInt("highScore", 0));
		//GUI.Label(new Rect(0.0f,40.0f,Screen.width, Screen.height), "HEIGHT: " + player.gameObject.transform.position.y);
	}
	
	void FixedUpdate () {
		UpdateTransparencyForCamera();
	}
	
	void Update () {
		newCrateCounter += Time.deltaTime;
		if (newCrateCounter > newCrateCounterMax) {
			newCrateCounter = 0.0f;
			AddCrate(lava.transform.position.y + 20.0f);
		}
		lava.transform.Translate(Vector3.up * lavaSpeed * Time.deltaTime);
		
		if ((player.gameObject.transform.position.y-0.58) * 10 > gameScore && gameStarted) {
			gameScore = (int)((player.gameObject.transform.position.y-0.58) * 10);
		}
		
		if (player.renderer.bounds.Intersects(lava.renderer.bounds)) {
			endGame = true;
			endGameText = "DIED BY LAVA";
		}
		
		if (endGame) {
			endGameCount += Time.deltaTime;
			if (endGameCount > endGameTime) {
				if (gameScore > PlayerPrefs.GetInt("highScore", 0)){
					PlayerPrefs.SetInt("highScore", gameScore);
				}
				gameScore = 0;
				gameStarted = false;
				Application.LoadLevel(0);
			}
		}
	}
	
	void UpdateTransparencyForCamera () {
		foreach (GameObject obj in lastTransparentObjects) {
			obj.renderer.material.shader = normalCrateShader;
			Color old = obj.renderer.material.color;
			obj.renderer.material.color = new Color(old.r, old.g, old.b, 1.0f);
		}
		lastTransparentObjects.Clear();
		
		Camera cam = Camera.allCameras[0];
		if (cam == null) return;
		Vector3 dir = player.transform.position - cam.transform.position;
		Ray ray = new Ray(cam.transform.position, dir.normalized);
		RaycastHit[] hits = Physics.SphereCastAll(ray, .35f, dir.magnitude - .5f);
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.name == "Crate") {
				hit.collider.gameObject.renderer.material.shader = transparentCrateShader;
				Color old = hit.collider.gameObject.renderer.material.color;
				hit.collider.gameObject.renderer.material.color = new Color(old.r, old.g, old.b, .5f);
				lastTransparentObjects.Add(hit.collider.gameObject);
			}
		}
	}
	
	void AddCrate (float height) {
		GameObject whichCrate = crate;
		if (Random.Range(0.0f, 1.0f) < .5f) {
			whichCrate = crateLong;
		}
		float x = (float)Random.Range(-5, 5) + .5f;
		float z = (float)Random.Range(-5, 5) + .5f;
		GameObject clone = GameObject.Instantiate(whichCrate, new Vector3(x, height, z), Quaternion.identity) as GameObject;
		if (Random.Range(0.0f, 1.0f) < .5f) {
			clone.transform.Rotate(Vector3.right, 90.0f);
		}
		if (Random.Range(0.0f, 1.0f) < .5f) {
			clone.transform.Rotate(Vector3.up, 90.0f, Space.World);
		}
		clone.name = crate.name;
	}
}
