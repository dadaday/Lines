using UnityEngine;
using System.Collections;

public class MainScript : MonoBehaviour {
	BFSalgo bAlgo;

	public static int NUM = 9;
	public static int BPSTEP = 3;

	private const int COLORSNUMBER = 7;
	private int BOARDDIM;

	public GameObject cellPrefab;
	public GameObject ballPrefab;

	private Color[,] board;
	private GameObject[] balls;

	private float cellWidth = 1.0f;
	private float ballSize = 0.8f;
	private Color DARKRED;
	private Color[] ballColors;

	private bool finished;
	private int ballsOnTable;

	private Vector3 clickedCoord;
	private bool ballSelected;

	// Use this for initialization
	void Start () {
		BOARDDIM = NUM * NUM;
		bAlgo = new BFSalgo(NUM);

		board = new Color[NUM, NUM];
		balls = new GameObject[BOARDDIM];

		DARKRED = new Color (150 / 255.0f, 17 / 255.0f, 17 / 255.0f);
		ballColors = new Color[COLORSNUMBER] {Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, DARKRED};

		finished = false;
		ballsOnTable = 0;
		clickedCoord = new Vector3 (-1, -1, -1);
		ballSelected = false;

		drawTable (NUM);	

		for (int i = 0; i < NUM; i++) {
			for (int j = 0; j < NUM; j++) {
				board [i,j] = Color.black;
			}
		}

		generateBalls (BPSTEP);
	}
	
	// Update is called once per frame
	void Update () {
		if (!finished) {
			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
				if (hit) 
				{
					GameObject selectedGO = hitInfo.transform.gameObject;
			
					if (selectedGO.tag == "Ball")
					{
						ballSelected = true;
						clickedCoord = selectedGO.GetComponent<Transform> ().position;
						int clickedRow = (int) clickedCoord.x;
						int clickedCol = (int) clickedCoord.z;
		
						Debug.Log ("Ball at row: " + clickedRow + " col: " + clickedCol + " is clicked: " + (clickedRow * NUM + clickedCol));

					} else if (ballSelected && selectedGO.tag == "Cell") {
						int clickedRow = (int) clickedCoord.z;
						int clickedCol = (int) clickedCoord.x;

						Vector3 moveTo = selectedGO.GetComponent<Transform> ().position;
						int moveToRow = (int) moveTo.x;
						int moveToCol = (int) moveTo.z;

						Debug.Log ("Wanna move the ball at row: " + clickedRow + " col: " + clickedCol + " to cell row: " + moveToRow + " col: " + moveToCol);
						moveBall (clickedCoord, moveTo);

						ballSelected = false;
					}
				} else {
					Debug.Log("No hit");
				}
			} 

			if (Input.GetKeyDown ("space")) {
				generateBalls (BPSTEP);
			}
		}
	}

	private void generateBalls(int numBalls) {
		int added = 0;
		while (added < numBalls && !finished) {
			int row = Random.Range (0, NUM);
			int col = Random.Range (0, NUM);
			int color = Random.Range (0, COLORSNUMBER);

			if (putBall(row, col, ballColors[color])) {
				added++;
				ballsOnTable++;
				if (ballsOnTable >= BOARDDIM) {
					Debug.Log ("MAX");
					finished = true;
				}
			}
		}
	}

	// put the ball of given color at given row and column
	private bool putBall(int row, int col, Color color) {
		if (board [row, col] == Color.black) {
			bAlgo.cutTies (row, col);

			board [row, col] = color;
			balls [row * NUM + col] = (GameObject)Instantiate (ballPrefab, new Vector3 (row * cellWidth, ballSize / 2.0f, col * cellWidth), Quaternion.identity);
			balls [row * NUM + col].GetComponent<MeshRenderer> ().material.SetColor ("_Color", color);
			Debug.Log ("putBall at: " + (row * NUM + col));
			return true;
		}
		return false;
	}

	private void moveBall(Vector3 source, Vector3 dest) {
		int sourceCol = (int) source.z;
		int sourceRow = (int) source.x;

		int destCol = (int) dest.z;
		int destRow = (int) dest.x;

		if (board [destRow, destCol] == Color.black
			&& bAlgo.checkAndPutBall(sourceRow, sourceCol, destRow, destCol)) {

			board [destRow, destCol] = board [sourceRow, sourceCol];
			board [sourceRow, sourceCol] = Color.black;

			balls [destRow * NUM + destCol] = balls [sourceRow * NUM + sourceCol];
			balls[destRow * NUM + destCol].GetComponent<Transform> ().position = new Vector3(dest.x, source.y, dest.z);
		} else {
			Debug.Log ("CANT PUT AT ROW: " + destRow + " COL: " + destCol);
		}
	}

	private void putBalls(Color color) {
		for (int i = 0; i < NUM; i++) {
			for (int j = 0; j < NUM; j++) {
					putBall(i, j, color);
			}
		}
	}

	private void drawTable(int num) {

		cellPrefab.transform.localScale = new Vector3(cellWidth, 0.1f, cellWidth);

		for (int row = 0; row < num; ++row) {
			for (int col = 0; col < num; ++col) {
				Instantiate (cellPrefab, new Vector3 (row * cellWidth, 0, col * cellWidth), Quaternion.identity);
			}
		}
	}

	public void ExitApplication() {
		Application.Quit ();
	}
}
