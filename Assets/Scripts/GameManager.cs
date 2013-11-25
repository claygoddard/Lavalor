using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	public GameObject lava;
	public GameObject player;
	private float lavaSpeed = 1f;
		
	private GameObject crate;
	private GameObject crateLong;
	private float newCrateCounter = 0.0f;
	private float newCrateCounterMax = 1.0f;
	private float highestGroundedCrate = 0.0f;
	
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
	
	private float crateAddSpeed = 1.0f;
	
	private class CrateDesc {
		public float height;
		public bool xRot, yRot, isLong;
		public Vector3 location;
		
		public CrateDesc (float height, bool xRot, bool yRot, bool isLong, Vector3 location) {
			this.height = height;
			this.xRot = xRot;
			this.yRot = yRot;
			this.isLong = isLong;
			this.location = location;
		}
	}
	
	void Start () {
		crate = Resources.Load("Crate") as GameObject;
		crateLong = Resources.Load("CrateLong") as GameObject;
		lastTransparentObjects = new ArrayList();
		normalCrateShader = Shader.Find("Diffuse");
		transparentCrateShader = Shader.Find("Transparent/Diffuse with Shadow");
		Physics.gravity = Vector3.down * 17.5f;
		
		/*
		
		TODO LATER MAYBE
		
		List<CrateDesc> crates = new List<CrateDesc>();
		for (int i = 0; i < 6; i++) {
			float h = 3.1f * i;
			crates.Add(new CrateDesc(h, true, true, true, new Vector3(0.0f, 0.0f, 4.5f)));
			crates.Add(new CrateDesc(h, true, true, true, new Vector3(0.0f, 0.0f, -4.5f)));
			crates.Add(new CrateDesc(h, true, false, true, new Vector3(4.5f, 0.0f, 0.0f)));
			crates.Add(new CrateDesc(h, true, false, true, new Vector3(-4.5f, 0.0f, 0.0f)));
			crates.Add(new CrateDesc(h, true, true, false, new Vector3(4.5f, 0.0f, 4.5f)));
			crates.Add(new CrateDesc(h, true, true, false, new Vector3(-4.5f, 0.0f, -4.5f)));
			crates.Add(new CrateDesc(h, true, false, false, new Vector3(4.5f, 0.0f, -4.5f)));
			crates.Add(new CrateDesc(h, true, false, false, new Vector3(-4.5f, 0.0f, 4.5f)));
			if (i == 5) {
				float off = 15.0f;
				crates.Add(new CrateDesc(h + off, true, false, true, new Vector3(-1.5f, 0.0f, 0.0f)));
				crates.Add(new CrateDesc(h + off, true, false, true, new Vector3(1.5f, 0.0f, 0.0f)));
				crates.Add(new CrateDesc(h + off + 3.0f, true, true, true, new Vector3(0.0f, 0.0f, 4.0f)));
				crates.Add(new CrateDesc(h + off + 3.0f, true, true, true, new Vector3(0.0f, 0.0f, -4.0f)));
				crates.Add(new CrateDesc(h + off + 3.0f, true, false, true, new Vector3(4.0f, 0.0f, 0.0f)));
				crates.Add(new CrateDesc(h + off + 3.0f, true, false, true, new Vector3(-4.0f, 0.0f, 0.0f)));
			}
		}
		AddCrates(crates);*/
	}
	
	void AddCheckpoint(float height) {
		GameObject check = GameObject.Instantiate(Resources.Load("Checkpoint") as GameObject, new Vector3(0.0f, height, 0.0f), Quaternion.identity) as GameObject;
		check.name = "Checkpoint";
		float maxAngle = 45.0f;
		float cos = Mathf.Cos(maxAngle);
        MeshFilter MF = check.GetComponent<MeshFilter>();
        MeshCollider MC = check.GetComponent<MeshCollider>();
        if (MF == null || MC == null || MF.sharedMesh == null)
        {
            Debug.LogError("PlatformCollision needs a MeshFilter and a MeshCollider");
            return;
        }
        Mesh M = new Mesh();
        Vector3[] verts = MF.sharedMesh.vertices;
        List<int> triangles = new List<int>(MF.sharedMesh.triangles);
        for (int i = triangles.Count-1; i >=0 ; i -= 3)
        {
            Vector3 P1 = check.transform.TransformPoint(verts[triangles[i-2]]);
            Vector3 P2 = check.transform.TransformPoint(verts[triangles[i-1]]);
            Vector3 P3 = check.transform.TransformPoint(verts[triangles[i  ]]);
            Vector3 faceNormal = Vector3.Cross(P3-P2,P1-P2).normalized;
            if (Vector3.Dot(faceNormal, Vector3.up) <= cos)
            {
                triangles.RemoveAt(i);
                triangles.RemoveAt(i-1);
                triangles.RemoveAt(i-2);
            }
        }
        M.vertices = verts;
        M.triangles = triangles.ToArray();
        MC.sharedMesh = M;
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
		if(gameStarted)
		{
			newCrateCounter += Time.deltaTime * crateAddSpeed;
			if (newCrateCounter > newCrateCounterMax) {
				newCrateCounter = 0.0f;
				AddCrate(Mathf.Max(player.transform.position.y, highestGroundedCrate) + 10.0f);
			}
			crateAddSpeed += .0001f;
			lava.transform.Translate(Vector3.up * lavaSpeed * Mathf.Sqrt(Mathf.Max(1.0f, player.transform.position.y - lava.transform.position.y) / 8.5f) * Time.deltaTime);
			
			if ((player.gameObject.transform.position.y-0.58) * 10 > gameScore && gameStarted) {
				gameScore = (int)((player.gameObject.transform.position.y-0.58) * 10);
			}
			
			if (player.renderer.bounds.Intersects(lava.renderer.bounds)) {
				CharacterControl characterScript = GameObject.Find("Character").GetComponent<CharacterControl>();
				characterScript.isDying = true;
				characterScript.playerFire.Play();
				endGameText = "DIED BY LAVA";
				endGame = true;
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
		else
		{
			if (Input.GetKey(KeyCode.Space)) {
				gameStarted = true;
				Destroy(GameObject.Find("StartScreen"));
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
	
	void AddCrate (float height, bool isLong, bool xRot, bool yRot, Vector3 location, float fallSpeedDiff) {
		GameObject whichCrate = crate;
		if (isLong) {
			whichCrate = crateLong;
		}
		float x = location.x;
		float z = location.z;
		GameObject clone = GameObject.Instantiate(whichCrate, new Vector3(x, height, z), Quaternion.identity) as GameObject;
		clone.GetComponent<CubeFall>().SetRotation(xRot, yRot);
		clone.GetComponent<CubeFall>().fallSpeed += fallSpeedDiff;
		clone.GetComponent<CubeFall>().fallSpeed *= crateAddSpeed;
		clone.name = crate.name;
	}
	
	void AddCrate (float height) {
		this.AddCrate(height, Random.Range(0.0f, 1.0f) > .6f, Random.Range(0.0f, 1.0f) > .5f, Random.Range(0.0f, 1.0f) > .5f, new Vector3((float)Random.Range(-5, 4) + .5f, 0.0f, (float)Random.Range(-5, 4) + .5f), Random.Range(-1.0f, 2.0f));
	}
	
	void AddCrates (List<CrateDesc> crates) {
		float max = Mathf.Max(player.transform.position.y, highestGroundedCrate) + 10.0f;
		AddCheckpoint(highestGroundedCrate + 1.0f);
		foreach (CrateDesc crate in crates) {
			AddCrate(crate.height + max, crate.isLong, crate.xRot, crate.yRot, crate.location, 0.0f);
		}
	}
	
	public void CrateGrounded(float atHeight) {
		if (atHeight > this.highestGroundedCrate) {
			if (this.highestGroundedCrate % 50 > atHeight % 50) {
				AddCheckpoint(atHeight + 1.0f);
			}
			this.highestGroundedCrate = atHeight;
		}
	}
}
