using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFSalgo {
	private int NIL;
	private Dictionary<int, List<int>> graph;
	private int NUM;
	private int DIM;
	private List<int> path;
	private bool[] hasBall;

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
		hasBall = new bool[DIM];

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

				hasBall[row * NUM + col] = false;
				graph.Add ((row * NUM + col), temp);
			}
		}
	}

	public void addTies(int row, int col) {
		int coord = (row * NUM + col);
		hasBall[row * NUM + col] = false;

		List<int> temp = new List<int> ();

		if (row < (NUM-1)) {
			if (!hasBall[(row + 1) * NUM + col]) {
				temp.Add ((row + 1) * NUM + col);
			}
			graph [(row + 1) * NUM + col].Add (coord);
		}
		if (row > 0) {
			if (!hasBall[(row - 1) * NUM + col]) {
				temp.Add ((row - 1) * NUM + col);	
			}
			graph [(row - 1) * NUM + col].Add (coord);
		}
		if (col > 0) {
			if (!hasBall[row * NUM + (col - 1)]) {
				temp.Add (row * NUM + (col - 1));	
			}
			graph [row * NUM + (col - 1)].Add (coord);
		}
		if (col < (NUM-1)) {
			if (!hasBall[row * NUM + (col + 1)]) {
				temp.Add (row * NUM + (col + 1));	
			}
			graph [row * NUM + (col + 1)].Add (coord);
		}

		graph [row * NUM + col].Clear();
		graph [row * NUM + col] = temp;
	}

	public void cutTies(int row, int col) {
		int coord = (row * NUM + col);
		hasBall [row * NUM + col] = true;

		if (row < (NUM-1)) {
			graph [(row + 1) * NUM + col].Remove(coord);
		}
		if (row > 0) {
			graph [(row - 1) * NUM + col].Remove(coord);
		}
		if (col > 0) {
			graph [row * NUM + (col-1)].Remove(coord);
		}
		if (col < (NUM-1)) {
			graph [row * NUM + (col+1)].Remove(coord);
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
			path = new List<int> ();
			int dest = destCoord;
			path.Add (dest);

			while (dest != sourceCoord) {
				dest = predec [dest];
				path.Add (dest);
			}

			path.Reverse ();
				
			cutTies (destRow, destCol);
			addTies (sourceRow, sourceCol);

			return true;
		}

		return false;
	}

	public List<int> getPath() {
		return path;
	}
}
