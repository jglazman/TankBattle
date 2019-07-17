using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	/// <summary>
	/// Dynamic path generation across the current state of ground tiles.
	/// </summary>
	public class PathfindingModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Pathfinding; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Transform }; } }

		private TransformModule _transform;
		private bool _isDebug;
		
		
		public PathfindingModule(bool debug=false)
		{
			_tileRay = new Ray(Vector3.zero, Vector3.down);
			_isDebug = debug;
		}

		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Transform))
			{
				_transform = dependency as TransformModule;
			}
		}
		
		protected override void DestroyInternal()
		{
			_transform = null;
		}

		
		/// <summary>
		/// Find a path from our current position to the given tile coordinates, or return null if a path cannot be found.
		/// </summary>
		public List<Vector3> GetPathToTile(int xEnd, int yEnd)
		{
			// TODO: public static ;(
			Assert.IsTrue(Game.TerrainGen != null, "Pathfinding requires a TerrainGenerator!");
			
			// find the coordinates of the tile under our feet
			int xStart, yStart;
			if (!GetTileCoordinates(_transform.transform.position, out xStart, out yStart))
				return null; // we are off the grid, there is no valid path

			// prepare for A*
			Node[,] nodes = new Node[Game.TerrainGen.NumCols,Game.TerrainGen.NumRows];
			var openNodes = new List<Node>(Game.TerrainGen.NumCols*Game.TerrainGen.NumRows);

			// find the first node
			var startNode = GetOrCreateNode(ref nodes, 0, xStart, yStart, xStart, yStart, xEnd, yEnd);
			if (startNode == null || startNode.isBlocked)
				return null; // no path

			startNode.isOpen = true;
			openNodes.Add(startNode);
			Node endNode = null;
			
			do
			{
				// find the open node with the cheapest cost
				openNodes.Sort((n1, n2) => n2.f.CompareTo(n1.f));	// sort descending...
				
				// close that node
				var currentNode = openNodes[openNodes.Count - 1];	// ...and take from the end...
				currentNode.isClosed = true;
				openNodes.RemoveAt(openNodes.Count - 1);			// ...cuz this is cheaper: o(1) vs o(n)

				// is it the end?
				if (currentNode.x == xEnd && currentNode.y == yEnd)
				{
					endNode = currentNode;
					break;
				}

				// find adjacent nodes
				Node[] adjacentNodes = new Node[4];
				adjacentNodes[0] = GetOrCreateNode(ref nodes, currentNode.g, currentNode.x, currentNode.y+1, xStart, yStart, xEnd, yEnd);
				adjacentNodes[1] = GetOrCreateNode(ref nodes, currentNode.g, currentNode.x, currentNode.y-1, xStart, yStart, xEnd, yEnd);
				adjacentNodes[2] = GetOrCreateNode(ref nodes, currentNode.g, currentNode.x+1, currentNode.y, xStart, yStart, xEnd, yEnd);
				adjacentNodes[3] = GetOrCreateNode(ref nodes, currentNode.g, currentNode.x-1, currentNode.y, xStart, yStart, xEnd, yEnd);

				// open any available adjacent nodes
				for (int i = 0; i < 4; i++)
				{
					var node = adjacentNodes[i];
					if (node == null || node.isClosed || node.isBlocked)
						continue;	// ignore
					
					if (node.isOpen)
					{
						// check if we found a shorter path
						if (node.g < currentNode.g)
						{
							node.parentNode = currentNode;
							node.g = currentNode.g + 1;
						}
					}
					else
					{
						node.isOpen = true;
						node.parentNode = currentNode;
						openNodes.Add(node);
					}
				}
				
				// did we fail?
				if (openNodes.Count == 0)
					break;
			} while (openNodes.Count > 0);

			if (endNode == null)
				return null; // no path found

			// construct the final path from our linked list
			var path = new List<Vector3>();
			var pathNode = endNode;
			do
			{
				path.Add(pathNode.tile.transform.position);
				pathNode = pathNode.parentNode;
			} while (pathNode != null);
			path.Reverse();
			
			return path;
		}

		private static Node GetOrCreateNode(ref Node[,] nodes, int g, int x, int y, int xStart, int xEnd, int yStart, int yEnd)
		{
			if (x < 0 || y < 0 || x >= Game.TerrainGen.NumCols || y >= Game.TerrainGen.NumRows)
				return null;
			
			var node = nodes[x,y];
			if (node == null)
			{
				var index = Game.TerrainGen.GetLinearIndex(x, y);
				var tile = Game.TerrainEntities[index]?.GetModule<TerrainModule>(ModuleType.Terrain);

				node = new Node() {
					x = x,
					y = y,
					g = g + 1,
					h = Mathf.Abs(x - xEnd) + Mathf.Abs(y - yEnd),	// manhattan distance -- our tanks only have a 4-way d-pad
					isOpen = false,
					isClosed = false,
					isBlocked = tile?.IsOpen() == false,
					parentNode = null,
					tile = tile
				};

				nodes[x,y] = node;
			}

			return node;
		}

		private class Node
		{
			public int x;
			public int y;
			public int g;
			public int h;
			public int f => g + h;
			public bool isOpen;
			public bool isClosed;
			public bool isBlocked;
			public Node parentNode;
			public TerrainModule tile;
		}

		
		
		private static Ray _tileRay;
		private static RaycastHit _hitInfo;
		private static int _layerMask = 1 << LayerMask.NameToLayer("Terrain");

		private static bool GetTileCoordinates(Vector3 worldPos, out int x, out int y)
		{
			_tileRay.origin = worldPos + Vector3.up;
			
			if (Physics.Raycast(_tileRay, out _hitInfo, 10f, _layerMask))
			{
				var tile = _hitInfo.collider.gameObject.GetComponent<TerrainBehaviour>();
				if (tile != null)
				{
					x = tile.X;
					y = tile.Y;
					return true;
				}
			}

			x = -1;
			y = -1;
			return false;
		}
	
		
		
		
		private List<Vector3> _debugPath = new List<Vector3>();
		private Vector3 _debugGoal;
		private Color _debugColor;
		private bool _debugNext;
		private bool _isDebugActive;
		
		public override void Update(float deltaTime)
		{
			if (!_isDebug || _transform == null)
				return;

			if (_isDebugActive)
			{
				if (_debugPath.Count > 0)
				{
					Debug.DrawLine(_transform.transform.position + Vector3.up, _debugGoal + Vector3.up, Color.yellow);
					Debug.DrawLine(_transform.transform.position + Vector3.up, _debugPath[0] + Vector3.up, _debugColor);

					if (_debugPath.Count > 1)
					{
						for (int i = 0; i < _debugPath.Count - 1; i++)
							Debug.DrawLine(_debugPath[i] + Vector3.up, _debugPath[i + 1] + Vector3.up, _debugColor);
					}
				}
				else
				{
					Debug.DrawLine(_transform.transform.position + Vector3.up, _debugGoal + Vector3.up, Color.red);
				}

				if (Input.GetKeyDown(KeyCode.N))
				{
					_debugPath.Clear();
					_debugNext = true;
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.P))
					UnityWrapper.Instance.StartCoroutine(Debug_FindAllPaths());
			}
		}

		private IEnumerator Debug_FindAllPaths()
		{
			_isDebugActive = true;
			
			for (int row = 0; row < Game.TerrainGen.NumRows; row++)
			{
				for (int col = 0; col < Game.TerrainGen.NumCols; col++)
				{
					var path = GetPathToTile(col, row);
					_debugGoal = Game.GetTileWorldPosition(col, row);

					yield return null;

					if (path == null)
					{
						Debug.Log($" ==> find path to {col} x {row}: not found!");
						_debugColor = Color.red;
					}
					else
					{
						Debug.Log($" ==> find path to {col} x {row}: length={path.Count}");
						_debugPath = path;
						_debugColor = Color.blue;
					}
					
					_debugNext = false;
					
					while (!_debugNext)
						yield return null;
				}
			}

			_isDebugActive = false;
		}
	}
}
