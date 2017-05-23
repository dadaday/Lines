using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFSalgo {
	private int NIL;
	private Dictionary<int, List<int>> graph;
	private int NUM;
	private int DIM;

	private enum STATE {
		NOTVISITED,
		VISITED,
		FINISHED
	};

	public BFSalgo(int num) {
		NIL = -1;
		NUM = num;
		DIM = num * num;
		graph = new Dictionary<int, List<int>> ();

		for (int row = 0; row < NUM; row++) {
			for (int col = 0; col < NUM; col++) {
				List<int> temp = new List<int> ();
				if (row < (NUM-1)) {
					temp.Add ((row+1) * NUM + col);	
				}
				if (row > 0) {
					temp.Add ((row-1) * NUM + col);	
				}
				if (col > 0) {
					temp.Add (row * NUM + (col-1));	
				}
				if (col < (NUM-1)) {
					temp.Add (row * NUM + (col+1));	
				}

				graph.Add ((row * NUM + col), temp);
			}
		}
	}

	public void addTies(int row, int col) {
		int coord = (row * NUM + col);
		List<int> temp = new List<int> ();

		if (row < (NUM-1)) {
			temp.Add ((row+1) * NUM + col);
			graph [(row + 1) * NUM + col].Add (coord);
		}
		if (row > 0) {
			temp.Add ((row-1) * NUM + col);	
			graph [(row - 1) * NUM + col].Add (coord);
		}
		if (col > 0) {
			temp.Add (row * NUM + (col-1));	
			graph [row * NUM + (col-1)].Add (coord);
		}
		if (col < (NUM-1)) {
			temp.Add (row * NUM + (col+1));	
			graph [row * NUM + (col+1)].Add (coord);
		}

		if(temp.Count == 4)
			Debug.Log ((row * NUM + col) + " is connected to " + temp[0] +  " " + temp[1] +  " " + temp[2] +  " " + temp[3]);
		else if(temp.Count == 3)
			Debug.Log ((row * NUM + col) + " is connected to " + temp[0] +  " " + temp[1] +  " " + temp[2]);
		else if(temp.Count == 2)
			Debug.Log ((row * NUM + col) + " is connected to " + temp[0] +  " " + temp[1]);

		graph [row * NUM + col].Clear();
		graph [row * NUM + col] = temp;
	}

	public void cutTies(int row, int col) {
		int coord = (row * NUM + col);
		Debug.Log ("CUTTING TIES FOR " + row + " " + col + " " + coord);

		if (row < (NUM-1)) {
			graph [(row + 1) * NUM + col].Remove(coord);
			Debug.Log ("CANT COME TO " + coord + "FROM " + ((row + 1) * NUM + col));
		}
		if (row > 0) {
			graph [(row - 1) * NUM + col].Remove(coord);
			Debug.Log ("CANT COME TO " + coord + "FROM " + ((row - 1) * NUM + col));
		}
		if (col > 0) {
			graph [row * NUM + (col-1)].Remove(coord);
			Debug.Log ("CANT COME TO " + coord + "FROM " + (row * NUM + (col-1)));
		}
		if (col < (NUM-1)) {
			graph [row * NUM + (col+1)].Remove(coord);
			Debug.Log ("CANT COME TO " + coord + "FROM " + (row * NUM + (col+1)));
		}
	}

	public bool checkAndPutBall(int sourceRow, int sourceCol, int destRow, int destCol) {
		Queue<int> burned = new Queue<int>();
		STATE[] states = new STATE[DIM];
		int[] predec = new int[DIM];

		for (int i = 0; i < DIM; i++) {
			states [i] = STATE.NOTVISITED;
			predec [i] = NIL;
		}

		int sourceCoord = sourceRow * NUM + sourceCol;
		int destCoord = destRow * NUM + destCol;

		Debug.Log ("Looking for path from r: " + sourceRow + " c: " + sourceCol + " to r: " + destRow + " c: " + destCol);
		Debug.Log ("sourceCoord: " + sourceCoord + " destCoord: " + destCoord);

		burned.Enqueue (sourceCoord);
		states [sourceCoord] = STATE.VISITED;

		while (burned.Count > 0) {
			int u = burned.Dequeue ();
			states [u] = STATE.FINISHED;

			foreach (int it in graph[u]) {
				if (states [it] == STATE.NOTVISITED) {
					states[it] = STATE.VISITED;
					predec[it] = u;
					burned.Enqueue(it);
				}
			}
		}

		if (predec [destCoord] != NIL) {
			List<int> path = new List<int> ();
			int dest = destCoord;
			path.Add (dest);

			while (dest != sourceCoord) {
				dest = predec [dest];
				path.Add (dest);
			}

			path.Reverse ();

			Debug.Log ("Path is: ");
			foreach (int p in path) {
				Debug.Log (p);
			}

			cutTies (sourceRow, sourceCol);
			addTies (sourceRow, sourceCol);	
			return true;
		}

		return false;
	}
}
