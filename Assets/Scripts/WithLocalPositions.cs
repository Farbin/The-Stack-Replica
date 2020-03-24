using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WithLocalPositions : MonoBehaviour {

	public GameObject[] theStack;
	public GameObject stackBase;
	public float moveSpeed = 2.5f;
	public Camera cam;
	public Text scoreText;
	public Text highestScoreText;
	public GameObject endCanvas;


	public Material mat1;
	public Material mat2;

	private int scoreCount=0;
	private int stackIndex;
	private int combo =0;

	private bool isMovingOnX = true;
	private bool gameOver = false;

	private const float BOUNDS = 1.25f;
	private const float Y_SCALE = 0.25f;
	private const float STACK_DOWN_MOVE_SPEED = 5f;
	private const float ERROR_LIMIT = .075f;
	private const float BOUNDS_GAIN = 0.25f;
	private const int COMBO_COUNT = 4;


	private float tileTransition = 0f;
	private float secondaryPosition = 0f;

	private Vector2 stackBounds = new Vector2 (BOUNDS, BOUNDS);
	private Vector3 desiredStackPosition ;
	private Vector3 lastTilePosition;
	private Vector3 lastTileSize; 



	// Use this for initialization
	void Start () {

		highestScoreText.text = "Highest Score : " + PlayerPrefs.GetInt ("HighestScore").ToString();

		endCanvas.SetActive (false);

		theStack = new GameObject [transform.childCount];
		stackIndex = transform.childCount - 1;
        

		for (int i = 0; i < transform.childCount; i++) {
			theStack [i] = transform.GetChild (i).gameObject;
		}

		lastTileSize = new Vector3 (BOUNDS, Y_SCALE, BOUNDS);
		lastTilePosition = theStack [0].transform.localPosition;
	}



	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			if (PlaceTile ()) {
				scoreCount++;
				scoreText.text = scoreCount.ToString ();
				SpawnTile ();

			} else {
				EndGame ();
			}
		}
		moveTile ();
	//	transform.position = Vector3.Lerp (transform.position, desiredStackPosition, STACK_DOWN_MOVE_SPEED*Time.deltaTime);
	}



	private void moveTile (){
		if (gameOver) return ;

		tileTransition += Time.deltaTime * moveSpeed;
		if (isMovingOnX) {
			theStack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin (tileTransition) * BOUNDS, scoreCount*.25f, secondaryPosition);
		}
		else {
			theStack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition, scoreCount*.25f, Mathf.Sin (tileTransition) * BOUNDS);
		}
			
	}



	private void SpawnTile (){
		lastTilePosition = theStack [stackIndex].transform.localPosition;
		lastTileSize = theStack [stackIndex].transform.localScale;

		stackIndex--;
		if (stackIndex < 0) {
			stackIndex = transform.childCount - 1;
		}

		theStack[stackIndex].transform.localScale = new Vector3 (stackBounds.x, Y_SCALE, stackBounds.y);
		theStack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount*0.25f, 0f);
	}



	private bool PlaceTile ()
	{	
		Transform t = theStack [stackIndex].transform;


		if (isMovingOnX) {
			float delX = Mathf.Abs (t.localPosition.x - lastTilePosition.x);

			if (delX > ERROR_LIMIT) {  //cut the tile
				stackBounds.x -= delX;
				combo = 0;

				if (stackBounds.x <= 0)
					return false;

				t.localScale = new Vector3 (stackBounds.x, Y_SCALE, stackBounds.y);

				if (t.localPosition.x > lastTilePosition.x) {
					t.localPosition = new Vector3 (lastTilePosition.x + lastTileSize.x / 2 - stackBounds.x / 2, scoreCount * Y_SCALE, t.localPosition.z);
					CreateRubble (new Vector3 (lastTilePosition.x + lastTileSize.x / 2 + delX/2, t.position.y, t.position.z),
									new Vector3 (delX, Y_SCALE, stackBounds.y), mat1);
				} else {
					t.localPosition = new Vector3 (lastTilePosition.x - lastTileSize.x / 2 + stackBounds.x / 2, scoreCount * Y_SCALE, t.localPosition.z);
					CreateRubble (new Vector3 (lastTilePosition.x - lastTileSize.x / 2 - delX/2, t.position.y, t.position.z),
						new Vector3 (delX, Y_SCALE, stackBounds.y), mat1);
				
				}


			} else { //player managed to place accurately 

				if (combo >= COMBO_COUNT && stackBounds.x < BOUNDS) {
					stackBounds.x += BOUNDS_GAIN;
					combo = 0;
					t.localScale = new Vector3 (stackBounds.x, Y_SCALE, stackBounds.y);

					if (t.localPosition.x > lastTilePosition.x) {
						t.localPosition = new Vector3 (lastTilePosition.x + lastTileSize.x / 2 - stackBounds.x / 2, scoreCount * Y_SCALE, t.localPosition.z);
					} else {
						t.localPosition = new Vector3 (lastTilePosition.x - lastTileSize.x / 2 + stackBounds.x / 2, scoreCount * Y_SCALE, t.localPosition.z);
					}
				
				} else {
					combo++;
					t.position = lastTilePosition + Vector3.up * Y_SCALE;
				}
			}
			///////////////////end of isMovingOnX/////////////////////////////////////
		}
		else /////now moving on z///////////////////////
		{

			float delZ = Mathf.Abs (t.localPosition.z - lastTilePosition.z);
			if (delZ > ERROR_LIMIT) {  //cut the tile	
				stackBounds.y -= delZ;
				combo = 0;

				if (stackBounds.y <= 0)
					return false;


				t.localScale = new Vector3 (stackBounds.x, Y_SCALE, stackBounds.y);

				if (t.localPosition.z >  lastTilePosition.z) {
					t.localPosition = new Vector3 (t.localPosition.x, scoreCount * Y_SCALE, lastTilePosition.z + lastTileSize.z / 2 - stackBounds.y / 2);
					CreateRubble (new Vector3 (t.position.x, t.position.y, lastTilePosition.z + lastTileSize.z / 2 + delZ/2),
						new Vector3 (stackBounds.x, Y_SCALE, delZ), mat2);
				
				} else {
					t.localPosition = new Vector3 (t.localPosition.x, scoreCount * Y_SCALE, lastTilePosition.z - lastTileSize.z / 2 + stackBounds.y / 2);
					CreateRubble (new Vector3 (t.position.x, t.position.y, lastTilePosition.z - lastTileSize.z / 2 - delZ/2),
						new Vector3 (stackBounds.x, Y_SCALE, delZ), mat2);
				}
			
			
			
			} else { //player managed to place successfully 

				if (combo >= COMBO_COUNT && stackBounds.y < BOUNDS) {
					stackBounds.y += BOUNDS_GAIN;
					combo = 0;
					t.localScale = new Vector3 (stackBounds.x, Y_SCALE, stackBounds.y);
				
					if (t.localPosition.z >  lastTilePosition.z) {
						t.localPosition = new Vector3 (t.localPosition.x, scoreCount * Y_SCALE, lastTilePosition.z + lastTileSize.z / 2 - stackBounds.y / 2);
					} else {
						t.localPosition = new Vector3 (t.localPosition.x, scoreCount * Y_SCALE, lastTilePosition.z - lastTileSize.z / 2 + stackBounds.y / 2);
					}
				
				} else {
					combo++;
					t.position = lastTilePosition + Vector3.up * Y_SCALE;
				}
			}


		}


		secondaryPosition = isMovingOnX ? t.transform.localPosition.x : t.transform.localPosition.z;
		cam.transform.position = new Vector3 (cam.transform.position.x, cam.transform.position.y + .24f, cam.transform.position.z);
		isMovingOnX = !isMovingOnX;
		stackBase.transform.position = stackBase.transform.position + Vector3.up * Y_SCALE;
		return true;	
	}



	private void CreateRubble (Vector3 pos, Vector3 size, Material mat){
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.transform.localScale = size;
		go.transform.localPosition = pos;
		go.AddComponent<Renderer>();

		Renderer r = go.GetComponent <Renderer> ();
		r.material = mat;
		go.AddComponent<Rigidbody> ();

	}


	private void EndGame (){
		Debug.Log ("Lose");
		theStack [stackIndex].AddComponent <Rigidbody> ();
		gameOver = true;
	

		if (PlayerPrefs.GetInt ("HighestScore") < scoreCount) {
			PlayerPrefs.SetInt ("HighestScore", scoreCount);
			highestScoreText.text = "Highest Score : " + PlayerPrefs.GetInt ("HighestScore").ToString();
		}
		endCanvas.SetActive (true);
	}


	public void ButtonClick (string sceneName){
		SceneManager.LoadScene (sceneName);
	}
}
