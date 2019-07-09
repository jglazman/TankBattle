/**
 * Procedural world generating algorithms.
 *
 * Copyright (C) 2016 Jeremy Glazman.
 */

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using Glazman.Tank;

public static class WorldGenSeed
{
	public static WorldTileConfig[,] SEED_MAZE1 = new WorldTileConfig[2,2]
	{
		{ new WorldTileConfig( WorldTileType.WallA ), 	new WorldTileConfig( WorldTileType.Room ) },
		{ new WorldTileConfig( WorldTileType.Pillar ),	new WorldTileConfig( WorldTileType.WallB ) }
	};

	public static WorldTileConfig[,] SEED_MAZE2 = new WorldTileConfig[3,3]
	{
		{ new WorldTileConfig( WorldTileType.WallA, 0, -1 ),	new WorldTileConfig( WorldTileType.Pillar ),	new WorldTileConfig( WorldTileType.Pillar ) },
		{ new WorldTileConfig( WorldTileType.WallA ),			new WorldTileConfig( WorldTileType.Pillar ), 	new WorldTileConfig( WorldTileType.Pillar ) },
		{ new WorldTileConfig( WorldTileType.Room ),			new WorldTileConfig( WorldTileType.WallB ),		new WorldTileConfig( WorldTileType.WallB, -1, 0 ) }
	};

	public static WorldTileConfig[,] SEED_CAVERN1 = new WorldTileConfig[3,3]
	{
		{ new WorldTileConfig( WorldTileType.Room, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, -1, 0 ) },
		{ new WorldTileConfig( WorldTileType.WallA, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, -1, 0 ), 	new WorldTileConfig( WorldTileType.WallB, -1, 1 ) },
		{ new WorldTileConfig( WorldTileType.WallA, 0, 1 ),	new WorldTileConfig( WorldTileType.WallB, 0, -1 ),	new WorldTileConfig( WorldTileType.WallA, 1, 0 ) }
	};

	public static WorldTileConfig[,] SEED_CAVERN2 = new WorldTileConfig[5,5]
	{
		{ new WorldTileConfig( WorldTileType.WallA, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, 1, 0 ),	new WorldTileConfig( WorldTileType.WallB, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 1 ),	new WorldTileConfig( WorldTileType.Room, 0, 0 ) },
		{ new WorldTileConfig( WorldTileType.WallA, 0, 1 ),	new WorldTileConfig( WorldTileType.Room, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, 1, 0 ),	new WorldTileConfig( WorldTileType.WallB, 0, 0 ) },
		{ new WorldTileConfig( WorldTileType.WallB, 1, 0 ),	new WorldTileConfig( WorldTileType.WallB, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 1 ),	new WorldTileConfig( WorldTileType.Room, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 0 ) },
		{ new WorldTileConfig( WorldTileType.Room, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, 1, 0 ),	new WorldTileConfig( WorldTileType.WallB, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 1 ) },
		{ new WorldTileConfig( WorldTileType.WallB, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 1 ),	new WorldTileConfig( WorldTileType.Room, 0, 0 ),	new WorldTileConfig( WorldTileType.WallA, 0, 0 ),	new WorldTileConfig( WorldTileType.WallB, 1, 0 ) },
	};

	public static WorldTileConfig[,] SEED_EXPERIMENT = new WorldTileConfig[3,3]
	{
		{ new WorldTileConfig( WorldTileType.Pillar, 0, 0 ),	new WorldTileConfig( WorldTileType.Wall, 0, 0 ),	new WorldTileConfig( WorldTileType.Pillar, 0, 0 ) },
		{ new WorldTileConfig( WorldTileType.Wall, 0, 0 ),		new WorldTileConfig( WorldTileType.Room, 0, 0 ), 	new WorldTileConfig( WorldTileType.Wall, 0, 0 ) },
		{ new WorldTileConfig( WorldTileType.Pillar, 0, 0 ),	new WorldTileConfig( WorldTileType.Wall, 0, 0 ),	new WorldTileConfig( WorldTileType.Pillar, 0, 0 ) }
	};
}


/**
 * Procedural algorithm types.
 */
public enum WorldType
{
	Empty,
	Random,
	Prim,
	SymmetricPrim,
	RadialPrim,
	LinearPrim
}

/**
 * Possible states of a tile in the map.
 */
public enum WorldTileType
{
	Undefined = 0,
	Room,
	Wall,
	Pillar,
	WallOpen,
	WallA,		// treated as a Wall, but used for debug color output
	WallB		// treated as a Wall, but used for debug color output
}

/**
 * Static data for a seed tile.
 */
public class WorldTileConfig
{
	private WorldTileType _tileType;
	public WorldTileType TileType { get { return _tileType; } }

	private int _xParent;
	public int ParentX { get { return _xParent; } }

	private int _yParent;
	public int ParentY { get { return _yParent; } }

	public WorldTileConfig( WorldTileType tileType )
	{
		_tileType = tileType;
		_xParent = 0;
		_yParent = 0;
	}

	public WorldTileConfig( WorldTileType tileType, int xParent, int yParent )
	{
		_tileType = tileType;
		_xParent = xParent;
		_yParent = yParent;
	}
}

/**
 * A tile instance.
 */
public class WorldTile
{
	private int _col;
	public int Col { get { return _col; } }

	private int _row;
	public int Row { get { return _row; } }

	private WorldTileType _tileType = WorldTileType.Undefined;
	public WorldTileType TileType { get { return _tileType; } }

	private WorldTile _parentTile = null;
	private List<WorldTile> _childTiles = null;


	public WorldTile( int c, int r )
	{
		_col = c;
		_row = r;
	}

	public void SetParentTile( WorldTile parent )
	{
		Assert.IsNull<WorldTile>( _parentTile, "Tried to set another parent: " + _col + "x" + _row);
		Assert.IsTrue( parent != this, "Tried to set parent to ourself: " + _col + "x" + _row);

		_parentTile = parent;
		_parentTile.AddChildTile( this );
	}

	public void AddChildTile( WorldTile child )
	{
		Assert.IsNull<WorldTile>( _parentTile, "Tried to add a child to a tile that is already a child: " + _col + "x" + _row);

		if ( _childTiles == null )
			_childTiles = new List<WorldTile>();

		Assert.IsTrue( !_childTiles.Contains( child ), "Tried to add child tile twice: " + _col + "x" + _row);

		_childTiles.Add( child );
	}

	public List<WorldTile> GetChildrenTiles()
	{
		return _childTiles;
	}

	public WorldTile GetRootTile()
	{
		if ( _parentTile != null )
			return _parentTile;

		return this;
	}

	public List<WorldTile> GetTileGroup()
	{
		List<WorldTile> listTiles = new List<WorldTile>();
		listTiles.Add( this.GetRootTile() );
		if ( _childTiles != null )
			listTiles.AddRange( _childTiles );
		return listTiles;
	}

	public void SetTileType( WorldTileType tileType )
	{
		_tileType = tileType;

//		SetColor( GetTileColor( tileType ) );
	}

	public bool IsOpen()
	{
		if ( _tileType == WorldTileType.Room )
			return true;

		if ( _tileType == WorldTileType.WallOpen )
			return true;

		return false;
	}


#region render colors

	private static Dictionary<WorldTileType,Color> TileColors;

	public static void SetTileColors( Dictionary<WorldTileType,Color> colors )
	{
		TileColors = colors;
	}

	public static Color GetTileColor( WorldTileType tileType )
	{
		Color color;
		if ( !TileColors.TryGetValue( tileType, out color ) )
			color = Color.magenta;

		return color;
	}

//	public void SetColor( Color color )
//	{
//		if ( _gameObject != null )
//			_gameObject.Renderer.material.color = color;
//	}
//
//	public void SetColor( WorldTileType tileType )
//	{
//		if ( _gameObject != null )
//			_gameObject.Renderer.material.color = GetTileColor( tileType );
//	}

#endregion // render colors


#region debug

	public override string ToString()
	{
		string str = "[ " + _col + "x" + _row + " ]( " + _tileType + " )";
		if ( _childTiles != null )
		{
			str += " { ";
			for ( int i = 0; i < _childTiles.Count; i++ )
				str += _childTiles[i] + ",";
			str += " }";
		}
		return str;
	}

#endregion // debug

}


/**
 * State data and methods for generating tilemaps.
 */
public class TerrainGenerator
{
	private bool _isInitializing = false;
	public bool IsInitializing { get { return _isInitializing; } }

	private bool _isInitialized = false;
	public bool IsInitialized { get { return _isInitialized; } }

	private bool _isGenerating = false;
	public bool IsGenerating { get { return _isGenerating; } }

	private bool _isGenerated = false;
	public bool IsGenerated { get { return _isGenerated; } }

	private System.Action _onGeneratedCallback = null;
	public void SetOnGeneratedCallback( System.Action callback ) { _onGeneratedCallback = callback; }

	public enum InputMode
	{
		Manual,
		Config
	}
	private InputMode _inputMode = InputMode.Manual;
	public void SetInputMode( InputMode mode ) { _inputMode = mode; }
	public InputMode GetInputMode() { return _inputMode; }

	private WorldType _worldType;				//!< the procedural algorithm currently being used
	private int _numCols;						//!< width of the tilemap
	private int _numRows;						//!< height of the tilemap
	private WorldTile[,] _tiles;				//!< the tilemap

	private Vector3 _startPos;					//!< worldspace position of tile 0,0
	public Vector3 StartPos { get { return _startPos; } }

	private Vector3 _blockScale;				//!< worldspace scale of a tile
	public Vector3 BlockScale { get { return _blockScale; } }

	private Vector3 _halfBlock;					//!< half of _blockScale
	public Vector3 HalfBlock { get { return _halfBlock; } }

	private float _blockScaleAdjust = 1f;		//!< scale each tile mesh by this amount
	public float BlockScaleAdjust { get { return _blockScaleAdjust; } }

	private int _generationRate = 2;			//!< rate of tile generation per frame
	private TileSeed _tileSeed;					//!<
	private List<WorldTile> _listWalls = new List<WorldTile>();
	private List<WorldTile> _listSymmetricWalls = new List<WorldTile>();
	private List<WorldTile> _listRooms = new List<WorldTile>();
//	private List<EntityGameObject> _listProps = new List<EntityGameObject>();
//	private EntityGameObject _playfield = null;

	public int NumCols { get { return _numCols; } }
	public int NumRows { get { return _numRows; } }
	public WorldTile[,] Tiles { get { return _tiles; } }
	public List<WorldTile> Rooms { get { return _listRooms; } }

	public class WorldGenConfig
	{
		public int randomSeed;
		public WorldType worldType;
		public int numCols;
		public int numRows;
		public Vector2 size;
		public WorldTileConfig[,] tileSeed;

		public WorldGenConfig( int rs, WorldType w, int c, int r, Vector2 s, WorldTileConfig[,] seed )
		{
			randomSeed = rs;
			worldType = w;
			numCols = c;
			numRows = r;
			size = s;
			tileSeed = seed;
		}
	}

	private WorldGenConfig _config = null;
	public WorldGenConfig Config { get { return _config; } }
	public bool HasConfig() { return ( _config != null ); }
	public void SetConfig( WorldGenConfig config ) { _config = config; }


#region public interface

	public void SwitchWorldType( WorldType newWorldType )
	{
		if ( _isGenerating )
		{
			_worldType = newWorldType;
//			Debug.LogWarning("Switch to world type: " + newWorldType );
		}
	}

	public void SetRate( int rate )
	{
		_generationRate = rate;
	}

	public void InitWorldFromConfig( WorldGenConfig config )
	{
		if ( config.randomSeed != 0 )
		{
			Debug.LogWarning(" ## set world gen random seed to " + config.randomSeed );
			UnityEngine.Random.InitState( config.randomSeed );
		}

		InitWorld( config.worldType, config.numCols, config.numRows, config.size, config.tileSeed );
	}

	public void InitWorld( WorldType worldType, int numCols, int numRows, Vector2 size, WorldTileConfig[,] tileSeed )
	{
		_isInitializing = false;
		_isInitialized = false;
		_isGenerating = false;
		_isGenerated = false;
		_worldType = worldType;
		_numCols = numCols;
		_numRows = numRows;
		_tiles = new WorldTile[_numCols,_numRows];
		_tileSeed = new TileSeed( tileSeed );
		_startPos = new Vector3( size.x * -0.5f, 1f, size.y * -0.5f );
		_blockScale = new Vector3( size.x / numCols, size.y / numRows, 1f );
		_halfBlock = new Vector3( _blockScale.x * 0.5f, 0f, _blockScale.y * 0.5f );
		_listWalls.Clear();
		_listRooms.Clear();

		UnityWrapper.Instance.StartCoroutine( InitWorldTask() );
	}

	private IEnumerator InitWorldTask()
	{
		_isInitializing = true;
		_isInitialized = false;

		switch ( _worldType )
		{
			case WorldType.Empty:
				yield return UnityWrapper.Instance.StartCoroutine( InitWorld_Empty() );
			break;

			case WorldType.Random:
				yield return UnityWrapper.Instance.StartCoroutine( InitWorld_Random() );
			break;

			case WorldType.Prim:
			case WorldType.SymmetricPrim:
			case WorldType.RadialPrim:
			case WorldType.LinearPrim:
				yield return UnityWrapper.Instance.StartCoroutine( InitWorld_Prim() );
			break;
		}

		_isInitializing = false;
		_isInitialized = true;

		Debug.Log(" -= Initialized =- ");
	}

	public void GenerateWorld( int randomSeed=0 )
	{
		UnityWrapper.Instance.StartCoroutine( GenerateWorldTask(randomSeed) );
	}

	private IEnumerator GenerateWorldTask( int randomSeed=0 )
	{
		_isGenerating = true;
		_isGenerated = false;

		switch ( _worldType )
		{
			case WorldType.Random:
				yield return UnityWrapper.Instance.StartCoroutine( GenerateWorld_Random() );
			break;

			case WorldType.Prim:
			case WorldType.SymmetricPrim:
			case WorldType.RadialPrim:
			case WorldType.LinearPrim:
				yield return UnityWrapper.Instance.StartCoroutine( GenerateWorld_Prim( randomSeed ) );
			break;
		}

		_isGenerating = false;
		_isGenerated = true;

		if ( _onGeneratedCallback != null )
			_onGeneratedCallback();

		Debug.Log(" -= Generated =- ");
	}

#endregion // public interface


#region WorldType.Empty

	private IEnumerator InitWorld_Empty()
	{
		WorldTile.SetTileColors(
			new Dictionary<WorldTileType,Color>() {
				{ WorldTileType.Undefined, new Color( 0f, 0f, 0f ) },
				{ WorldTileType.Room, new Color( 0.1f, 0.1f, 0.1f ) },
				{ WorldTileType.Wall, new Color( 0.5f, 0.5f, 0.5f ) },
				{ WorldTileType.Pillar, new Color( 1.0f, 1.0f, 1.0f ) }
			}
		);

		for ( int row = 0; row < _numRows; row++ )
		{
			for ( int col = 0; col < _numCols; col++ )
			{
				_tiles[col,row] = CreateTile( col, row, WorldTileType.Room );
			}

			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator GenerateWorld_Empty()
	{
		// nothing to do

		yield return new WaitForEndOfFrame();
	}

#endregion // WorldType.Empty


#region WorldType.Random

	private IEnumerator InitWorld_Random()
	{
		WorldTile.SetTileColors(
			new Dictionary<WorldTileType,Color>() {
				{ WorldTileType.Undefined, new Color( 0f, 0f, 0f ) },
				{ WorldTileType.Room, new Color( 0.1f, 0.1f, 0.1f ) },
				{ WorldTileType.Wall, new Color( 0.5f, 0.5f, 0.5f ) },
				{ WorldTileType.Pillar, new Color( 1.0f, 1.0f, 1.0f ) }
			}
		);

		for ( int row = 0; row < _numRows; row++ )
		{
			for ( int col = 0; col < _numCols; col++ )
			{
				//WorldTileConfig tileConfig = _tileSeed.GetTileConfig( col, row );

				WorldTileType tileType = WorldTileType.Undefined;
				if ( col == 0 || col == _numCols - 1 )
					tileType = WorldTileType.Pillar;
				if ( row == 0 || row == _numRows - 1 )
					tileType = WorldTileType.Pillar;

				WorldTile tile = CreateTile( col, row, tileType );
				_tiles[col,row] = tile;
			}

			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator GenerateWorld_Random()
	{
		int numTypes = System.Enum.GetValues( typeof(WorldTileType) ).Length;

		for ( int row = 0; row < _numRows; row++ )
		{
			for ( int col = 0; col < _numCols; col++ )
			{
				WorldTileType tileType = (WorldTileType)( Random.Range( 0, numTypes ) );
				if ( col == 0 || col == _numCols - 1 )
					tileType = WorldTileType.Pillar;
				if ( row == 0 || row == _numRows - 1 )
					tileType = WorldTileType.Pillar;

				WorldTile tile = GetRootTile( col, row );
				tile.SetTileType( tileType );
			}

			yield return new WaitForEndOfFrame();
		}
	}

#endregion // WorldType.Random


#region WorldType.Prim

	/**
	 * Internal representation of the seed used to generate the map.
	 */
	private class TileSeed
	{
		private int _numCols;
		private int _numRows;
		private WorldTileConfig[,] _seed;

		public TileSeed( WorldTileConfig[,] pattern )
		{
			if ( pattern != null )
			{
				_numRows = pattern.GetLength( 0 );
				_numCols = pattern.GetLength( 1 );
				_seed = pattern;
			}
		}

		public WorldTileConfig GetTileConfig( int col, int row )
		{
			// do a transformation on the data so that it can be laid out visually matching what is seen in game (with origin in lower left corner)
			//return _seed[ col % _numCols, row % _numRows ];	// this is what you would expect this code to look like
			return _seed[ (_numRows - 1) - (row % _numRows), col % _numCols ];	// but actually it's rotated and upside down!
		}
	}

	private IEnumerator InitWorld_Prim()
	{
		WorldTile.SetTileColors(
			new Dictionary<WorldTileType,Color>() {
				{ WorldTileType.Undefined, new Color( 0f, 0f, 0f ) },
				{ WorldTileType.Room, new Color( 0.8f, 0.9f, 0.8f ) },
				{ WorldTileType.Wall, new Color( 0.2f, 0.2f, 0.2f ) },
				{ WorldTileType.Pillar, new Color( 0.1f, 0.1f, 0.1f ) },
				{ WorldTileType.WallOpen, new Color( 0.8f, 0.9f, 0.8f ) },

				{ WorldTileType.WallA, new Color( 0.2f, 0.1f, 0.1f ) },
				{ WorldTileType.WallB, new Color( 0.1f, 0.2f, 0.1f ) },
			}
		);

		// initialize the board with tiles
		for ( int row = 0; row < _numRows; row++ )
		{
			for ( int col = 0; col < _numCols; col++ )
			{
				WorldTileConfig tileConfig = _tileSeed.GetTileConfig( col, row );
				WorldTileType tileType = tileConfig.TileType;
				if ( _numCols > 5 )
				{
					if ( tileType == WorldTileType.WallA )
						tileType = WorldTileType.Wall;
					else if ( tileType == WorldTileType.WallB )
						tileType = WorldTileType.Wall;
				}

				// world edges are always pillars
				if ( _numCols > 10 && _numRows > 10 )
				{
					if ( col == 0 || col == _numCols - 1 )
						tileType = WorldTileType.Pillar;
					else if ( row == 0 || row == _numRows - 1 )
						tileType = WorldTileType.Pillar;
				}

				WorldTile tile = CreateTile( col, row, tileType );
				_tiles[col,row] = tile;

				// remember where we put the rooms so we can easily choose a random room to start our generation
				if ( tile.TileType == WorldTileType.Room )
					_listRooms.Add( tile );
			}

			// frame limiting
			if ( row % _generationRate == 0 )
				yield return new WaitForEndOfFrame();
		}

		//Debug.Log("connecting adjoined tiles...");

		yield return new WaitForEndOfFrame();

		// connect adjoined tiles
		for ( int row = 0; row < _numRows; row++ )
		{
			for ( int col = 0; col < _numCols; col++ )
			{
				WorldTileConfig tileConfig = _tileSeed.GetTileConfig( col, row );
				if ( tileConfig.ParentX != 0 || tileConfig.ParentY != 0 )
				{
//					Debug.Log(" tile " + col + "x" + row + " is a child of offset (" + tileConfig.ParentX + "," + tileConfig.ParentY + ")");

					WorldTile parentTile = GetTile( col + tileConfig.ParentX, row + tileConfig.ParentY );
					if ( parentTile != null )
					{
						WorldTile childTile = GetTile( col, row );

//						Debug.Log("  >> setting tile " + childTile + " as child of " + parentTile + " ==> (" + tileConfig.ParentX + "," + tileConfig.ParentY + ")");

						childTile.SetParentTile( parentTile );
					}
				}
			}

//			yield return new WaitForEndOfFrame();
		}
	}

	private WorldTile GetPrimStartRoom()
	{
		switch ( _worldType )
		{
			case WorldType.Prim:
			case WorldType.LinearPrim:
			{
				// start from a random room
				return _listRooms[ Random.Range( 0, _listRooms.Count ) ];
			}

			case WorldType.SymmetricPrim:
			case WorldType.RadialPrim:
			{
				// start from a room somewhere near the center of the map (it looks nicer)
				int bias = Mathf.FloorToInt( (float)_numCols * 0.45f );
				WorldTile startRoom;
				int numTries = 100;
				do
				{
					startRoom = _listRooms[ Random.Range( 0, _listRooms.Count ) ];
					if ( ( startRoom.Col < bias ) || ( startRoom.Col > _numCols - bias ) ||
						 ( startRoom.Row < bias ) || ( startRoom.Row > _numRows - bias ) )
						startRoom = null;
				}
				while ( startRoom == null && numTries > 0 );

				// if we failed then just pick one at random
				if ( startRoom == null )
					startRoom = _listRooms[ Random.Range( 0, _listRooms.Count ) ];

				return startRoom;
			}
		}

		Assert.IsTrue( false, "Unsupported generation type: " + _worldType );
		return null;
	}

	private WorldTile GetPrimNextWall()
	{
		switch ( _worldType )
		{
			case WorldType.Prim:
			{
				// get a random unchecked wall
				return _listWalls[ Random.Range( 0, _listWalls.Count ) ];
			}

			case WorldType.SymmetricPrim:
			case WorldType.RadialPrim:
			{
				WorldTile nextWall;
				if ( _listSymmetricWalls.Count > 0 )
				{
					// process the next symmetric wall
					nextWall = _listSymmetricWalls[0];
					_listSymmetricWalls.RemoveAt( 0 );
				}
				else
				{
					// get a random unchecked wall
					nextWall = _listWalls[ Random.Range( 0, _listWalls.Count ) ];

					// add symmetric walls to the queue
					WorldTile tile90 = TryGetRootTile( ( _numCols - 1 ) - nextWall.Col, nextWall.Row, WorldTileType.Wall );
					if ( tile90 != null )
						_listSymmetricWalls.Add( tile90 );

					// add radial walls to the queue
					if ( _worldType == WorldType.RadialPrim )
					{
						WorldTile tile180 = TryGetRootTile( ( _numCols - 1 ) - nextWall.Col, ( _numRows - 1 ) - nextWall.Row, WorldTileType.Wall );
						if ( tile180 != null )
							_listSymmetricWalls.Add( tile180 );

						WorldTile tile270 = TryGetRootTile( nextWall.Col, ( _numRows - 1 ) - nextWall.Row, WorldTileType.Wall );
						if ( tile270 != null )
							_listSymmetricWalls.Add( tile270 );
					}
				}

				return nextWall;
			}

			case WorldType.LinearPrim:
			{
				// iterate linearly
				if ( _listWalls.Count >= 2 )
				{
					if ( _listWalls.Count % 2 == 0 )
						return _listWalls[0];
					else
						return _listWalls[ _listWalls.Count - 1 ];
				}
				else
					return _listWalls[0];
			}
		}

		Assert.IsTrue( false, "Unsupported generation type: " + _worldType );
		return null;
	}

	private IEnumerator GenerateWorld_Prim( int randomSeed )
	{
		if ( randomSeed != 0 )
			UnityEngine.Random.InitState( randomSeed );

		// decide which room to start in
		WorldTile startRoom = GetPrimStartRoom();
//		startRoom.SetColor( Color.green );
		_listRooms.Clear();
		_listRooms.Add( startRoom );

		// start with all walls adjacent to the random room
		List<WorldTile> adjacentRootWalls = GetAdjacentRootTiles( startRoom.Col, startRoom.Row, WorldTileType.Wall );
		_listWalls.AddRange( adjacentRootWalls );

		// color the initial set just for debug fun
//		for ( int i = 0; i < adjacentRootWalls.Count; i++ )
//		{
//			adjacentRootWalls[i].SetColor( Color.yellow );
//
//			List<WorldTile> adjacentChildWalls = adjacentRootWalls[i].GetChildrenTiles();
//			if ( adjacentChildWalls != null )
//			{
//				for ( int n = 0; n < adjacentChildWalls.Count; n++ )
//					adjacentChildWalls[n].SetColor( new Color( 1f, 1f, 0.5f ) );
//			}
//		}

		// keep searching as long as we have walls in the queue
		int step = 0;
		while ( _listWalls.Count > 0 )
		{
			// decide which wall to process next
			WorldTile nextWall = GetPrimNextWall();
			List<WorldTile> randomWallGroup = nextWall.GetTileGroup();

			// find all rooms next to the wall
			List<WorldTile> adjacentRooms = GetAdjacentRootTiles( nextWall.Col, nextWall.Row, WorldTileType.Room );

			// check if we found more than one room
			if ( adjacentRooms.Count >= 2 )
			{
				// check if any of those rooms aren't yet part of the main path
				List<WorldTile> unvisitedRooms = new List<WorldTile>();
				for ( int i = 0; i < adjacentRooms.Count; i++ )
				{
					if ( !_listRooms.Contains( adjacentRooms[i] ) )
						unvisitedRooms.Add( adjacentRooms[i] );
				}

				// check if we have exactly one adjacent room that isn't part of the main path
				if ( unvisitedRooms.Count == 1 )
				{
					// open the wall
					for ( int i = 0; i < randomWallGroup.Count; i++ )
						randomWallGroup[i].SetTileType( WorldTileType.WallOpen );

					// add the room to the main path
					_listRooms.Add( unvisitedRooms[0] );

					// color the processed room for debugging
//					List<Tile> unvisitedGroup = unvisitedRooms[0].GetTileGroup();
//					for ( int n = 0; n < unvisitedGroup.Count; n++ )
//					{
//						float color = 1f - ( (float)step * 0.001f );
//						unvisitedGroup[n].SetColor( new Color( 0f, color, 0f ) );
//					}

					// and any newly discovered walls to the queue
					adjacentRootWalls.Clear();
					adjacentRootWalls = GetAdjacentRootTiles( unvisitedRooms[0].Col, unvisitedRooms[0].Row, WorldTileType.Wall );
					for ( int i = 0; i < adjacentRootWalls.Count; i++ )
					{
						if ( !_listWalls.Contains( adjacentRootWalls[i] ) )
							_listWalls.Add( adjacentRootWalls[i] );
					}
				}
			}

//			if ( nextWall.TileType == WorldTileType.Wall )
//			{
//				for ( int i = 0; i < randomWallGroup.Count; i++ )
//					randomWallGroup[i].SetColor( WorldTile.GetTileColor( WorldTileType.Pillar ) );
//			}

			// we are done processing this wall, remove it from the queue
			_listWalls.Remove( nextWall );

			// color the walls we removed
//			for ( int i = 0; i < randomWallGroup.Count; i++ )
//			{
//				if ( randomWallGroup[i].TileType == WorldTileType.Wall )
//					randomWallGroup[i].SetColor( WorldTile.GetTileColor(WorldTileType.Pillar));
//			}

			// frame limiting
			step++;
			int stepRate = Mathf.FloorToInt( _generationRate * ( 2 + ((float)_numCols / 16 )));
			if ( step % stepRate == 0 )
				yield return new WaitForEndOfFrame();
		}
	}

	private WorldTile TryGetRootTile( int col, int row, WorldTileType tileType )
	{
		WorldTile maybeRootTile = GetRootTile( col, row );

		if ( maybeRootTile == null )
		{
//			Debug.Log(" adjacent tile is null~");
			return null;
		}

		if ( maybeRootTile.TileType != tileType )
		{
//			Debug.Log(" adjacent tile is wrong type: " + adjacentTile.TileType );
			return null;
		}

		return maybeRootTile;
	}

	private List<WorldTile> GetAdjacentRootTiles( int col, int row, WorldTileType tileType )
	{
		WorldTile rootTile = GetRootTile( col, row );
		List<WorldTile> rootTiles = rootTile.GetTileGroup();

//		Debug.Log(" Looking for " + tileType + " adjacent to " + rootTile + " : ");
//		for ( int i = 0; i < rootTiles.Count; i++ )
//			Debug.Log("   --> " + rootTiles[i]);

		List<WorldTile> adjacentRootTiles = new List<WorldTile>();

		for ( int i = 0; i < rootTiles.Count; i++ )
		{
			WorldTile tile1 = TryGetRootTile( rootTiles[i].Col + 1, rootTiles[i].Row, tileType );
			if ( tile1 != null && !rootTiles.Contains( tile1 ) && !adjacentRootTiles.Contains( tile1 ) )
				adjacentRootTiles.Add( tile1 );
//			else
//				Debug.Log(" --- right tile is not added as adjacent");

			WorldTile tile2 = TryGetRootTile( rootTiles[i].Col - 1, rootTiles[i].Row, tileType );
			if ( tile2 != null && !rootTiles.Contains( tile2 ) && !adjacentRootTiles.Contains( tile2 ) )
				adjacentRootTiles.Add( tile2 );
//			else
//				Debug.Log(" --- left tile is not added as adjacent : tile2=" + ( tile2 != null ).ToString() + ", rootTiles.Contains( tile2 )=" + rootTiles.Contains( tile2 ).ToString() + ", adjacentTiles.Contains( tile2 )=" + adjacentTiles.Contains( tile2 ) );

			WorldTile tile3 = TryGetRootTile( rootTiles[i].Col, rootTiles[i].Row + 1, tileType );
			if ( tile3 != null && !rootTiles.Contains( tile3 ) && !adjacentRootTiles.Contains( tile3 ) )
				adjacentRootTiles.Add( tile3 );
//			else
//				Debug.Log(" --- up tile is not added as adjacent");

			WorldTile tile4 = TryGetRootTile( rootTiles[i].Col, rootTiles[i].Row - 1, tileType );
			if ( tile4 != null && !rootTiles.Contains( tile4 ) && !adjacentRootTiles.Contains( tile4 ) )
				adjacentRootTiles.Add( tile4 );
//			else
//				Debug.Log(" --- down tile is not added as adjacent");
		}

		return adjacentRootTiles;
	}


	private int GetLinearIndex( int col, int row )
	{
		return ( col * _numCols ) + row;
	}

	private Vector3 GetTilePos( int col, int row )
	{
		return new Vector3( col * _blockScale.x, 0f, row * _blockScale.y );
	}

	private WorldTile GetRootTile( int col, int row )
	{
		WorldTile tile = GetTile( col, row );
		if ( tile != null )
			return tile.GetRootTile();

		return null;
	}

	public WorldTile GetTile( int col, int row )
	{
		if ( col < 0 || col >= _numCols )
			return null;

		if ( row < 0 || row >= _numRows )
			return null;

		return _tiles[col,row];
	}

	private WorldTile CreateTile( int col, int row, WorldTileType tileType )
	{
		WorldTile tile = new WorldTile( col, row );
		tile.SetTileType( tileType );

		return tile;
	}

#endregion // WorldType.Prim


//#region debug
//
//	public bool IsDebugEnabled = false;
//
//	public void DrawDebugConnections()
//	{
//		if ( !IsDebugEnabled )
//			return;
//
//		for ( int row = 0; row < _numRows; row++ )
//		{
//			for ( int col = 0; col < _numCols; col++ )
//			{
//				WorldTile tile = GetTile( col, row );
//				List<WorldTile> children = tile.GetChildrenTiles();
//				if ( children != null )
//				{
//					Vector3 pos = tile.WorldPosition;
//					Vector3 topPos = pos + new Vector3( 0f, 7f, 0f );
//					Debug.DrawLine( pos, topPos, Color.green );
//
//					for ( int i = 0; i < children.Count; i++ )
//					{
//						Vector3 childPos = children[i].WorldPosition;
//						Vector3 childTopPos = childPos + new Vector3( 0f, 3f, 0f );
//						Debug.DrawLine( childPos, childTopPos, Color.cyan );
//						Debug.DrawLine( topPos, childTopPos, Color.yellow );
//					}
//				}
//			}
//		}
//	}
//
//#endregion // debug

}
