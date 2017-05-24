using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainScript : MonoBehaviour {
	BFSalgo bAlgo;

	public int NUM = 9;
	public int BPSTEP = 3;
	public int COMBO = 5;

	private const int COLORSNUMBER = 7;
	private int BOARDDIM;

	public GameObject cellPrefab;
	public GameObject ballPrefab;
	public GameObject pathPrefab;
	
	public Text countText;
	public Text scoreText;
	public Image endImage;

	private Color[,] board;
	private GameObject[] balls;

	private float cellWidth = 1.0f;
	private float ballSize = 0.8f;

	private Color DARKRED;
	private Color[] ballColors;

	private bool finished;
	private int ballsOnTable;
	private int score;

	private Vector3 clickedCoord;
	private bool ballSelected;

	// Use this for initialization
	void Start () {
		DARKRED = new Color (150 / 255.0f, 17 / 255.0f, 17 / 255.0f);
		ballColors = new Color[COLORSNUMBER] {Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, DARKRED};
		BOARDDIM = NUM * NUM;

		board = new Color[NUM, NUM];
		balls = new GameObject[BOARDDIM];

		Reset ();

		drawTable (NUM);
	}
	
	// Update is called once per frame
	void Update () {
		if (!finished) {
			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
				if (hit) {
					GameObject selectedGO = hitInfo.transform.gameObject;
			
					if (selectedGO.tag == "Ball") {
						ballSelected = true;
						clickedCoord = selectedGO.GetComponent<Transform> ().position;

					} else if (ballSelected && selectedGO.tag == "Cell") {
						Vector3 moveTo = selectedGO.GetComponent<Transform> ().position;
						moveBall (clickedCoord, moveTo);
						ballSelected = false;
					}
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
				countText.text = ballsOnTable.ToString ();
				if (ballsOnTable >= BOARDDIM) {
					Debug.Log ("MAX");
					finished = true;
					endImage.gameObject.SetActive (true);
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

			List<int> path = bAlgo.getPath ();
			GameObject[] pathDots = new GameObject[path.Count];

			for(int i = 0; i < path.Count; i++) {
				int row = path [i] / NUM;
				int col = path [i] % NUM;

				pathDots [i] = (GameObject)Instantiate (pathPrefab, new Vector3 (row * cellWidth, 0.1f, col * cellWidth), Quaternion.identity);
				Object.Destroy(pathDots [i], 2.0f);
			}

			board [destRow, destCol] = board [sourceRow, sourceCol];
			board [sourceRow, sourceCol] = Color.black;

			balls [destRow * NUM + destCol] = balls [sourceRow * NUM + sourceCol];
			balls[destRow * NUM + destCol].GetComponent<Transform> ().position = new Vector3(dest.x, source.y, dest.z);

			countSimilar (destRow, destCol, board[destRow, destCol]);
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

	private void countSimilar(int row, int col, Color color) {
		int byRow = 0;
		int byCol = 0;
		int dia1 = 0;
		int dia2 = 0;
		bool vanishedBalls = false;

		int rLow = row;
		for (; rLow >= 0; rLow--) {
			if (board [rLow, col] == color) {
				byRow++;
			} else {
				break;
			}
		}
		rLow++;

		int rHi = row + 1;
		for (; rHi < NUM; rHi++) {
			if (board [rHi, col] == color) {
				byRow++;
			} else {
				break;
			}
		}
		rHi--;

		if (byRow >= COMBO) {
			Debug.Log ("Removing " + byRow + " rows: " + rLow + "-" + rHi + " on column " + col);
			score += byRow;
			removeBallsStraight (rLow, rHi, true, col);
			vanishedBalls = true;
		}

		int cLow = col;
		for (; cLow >= 0; cLow--) {
			if (board [row, cLow] == color) {
				byCol++;
			} else {
				break;
			}
		}
		cLow++;

		int cHi = col + 1;
		for (; cHi < NUM; cHi++) {
			if (board [row, cHi] == color) {
				byCol++;
			} else {
				break;
			}
		}
		cHi--;

		if (byCol >= COMBO) {
			Debug.Log ("Removing "  + byCol + " Cols: " + cLow + "-" + cHi + " on row " + row);
			score += byCol;
			removeBallsStraight (cLow, cHi, false, row);
			vanishedBalls = true;
		}

		int rL = row;
		int cL = col;
		for (; rL >= 0 && cL >= 0; rL--, cL--) {
			if (board [rL, cL] == color) {
				dia1++;
			} else {
				break;
			}
		}
		rL++; cL++;

		int rH = row + 1;
		int cH = col + 1;
		for (; rH < NUM && cH < NUM; rH++, cH++) {
			if (board [rH, cH] == color) {
				dia1++;
			} else {
				break;
			}
		}
		rH--; cH--;

		if (dia1 >= COMBO) {
			Debug.Log ("Removing "  + dia1 + " row: " + rL + " col: " + cL + "-" + rH + " " + cH);
			score += dia1;
			removeBallDiagonal (rL, cL, rH, cH, true);
			vanishedBalls = true;
		}

		int srH = row;
		int scL = col;

		for (; srH < NUM && scL >= 0; srH++, scL--) {
			if (board [srH, scL] == color) {
				dia2++;
			} else {
				break;
			}
		}
		srH--; scL++;

		int srL = row - 1;
		int scH = col + 1;
		for (; srL >= 0 && scH < NUM; srL--, scH++) {
			if (board [srL, scH] == color) {
				dia2++;
			} else {
				break;
			}
		}
		srL++; scH--; 


		if (dia2 >= COMBO) {
			Debug.Log ("Removing "  + dia2 + " row: " + srL + " col: " + scH + "-" + srH + " " + scL);
			score += dia2;
			removeBallDiagonal (srL, scH, srH, scL, false);
			vanishedBalls = true;
		}

		scoreText.text = score.ToString ();
		if (!vanishedBalls) {
			generateBalls (BPSTEP);
		}
	}

	public void removeBallsStraight(int start, int end, bool isRow, int removeAlong) {
		for (int i = start; i <= end; i++) {
			if (isRow) {
				Destroy (balls [i * NUM + removeAlong]);
				board [i, removeAlong] = Color.black;
				bAlgo.addTies (i, removeAlong);
			} else {
				Destroy (balls [removeAlong * NUM + i]);
				board [removeAlong, i] = Color.black;
				bAlgo.addTies (removeAlong, i);
			}
			ballsOnTable--;
			countText.text = ballsOnTable.ToString ();
		}				
	}

	private void removeBallDiagonal(int startRow, int startCol, int endRow, int endCol, bool posSlope) {
		if (posSlope) {
			for (; startRow <= endRow && startCol <= endCol; startRow++, startCol++) {
				Destroy (balls [startRow * NUM + startCol]);
				board [startRow, startCol] = Color.black;
				bAlgo.addTies (startRow, startCol);

				ballsOnTable--;
				countText.text = ballsOnTable.ToString ();
			}	
		} else {
			Debug.Log (startRow + "-" + endRow + "|" + startCol + "-" + endCol);
			for (; startRow <= endRow && startCol >= endCol; startRow++, startCol--) {
				Destroy (balls [startRow * NUM + startCol]);
				board [startRow, startCol] = Color.black;
				bAlgo.addTies (startRow, startCol);

				ballsOnTable--;
				countText.text = ballsOnTable.ToString ();
			}
		}
	}

	public void ExitApplication() {
		Application.Quit ();
	}

	public void Reset() {
		endImage.gameObject.SetActive (false);
		bAlgo = new BFSalgo(NUM);
		
		finished = false;
		ballsOnTable = 0;
		score = 0;
		ballSelected = false;

		scoreText.text = score.ToString ();

		for (int i = 0; i < NUM; i++) {
			for (int j = 0; j < NUM; j++) {
				board [i,j] = Color.black;
				Destroy (balls [i * NUM + j]);
			}
		}
		generateBalls (BPSTEP);
	}
}
