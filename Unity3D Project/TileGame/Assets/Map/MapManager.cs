using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class MapManager {
	// Test use
	public const string DefaultMap = "/Resources/Map/1000.dat";
	
	public int width;
	public int height;
	public ushort[,] TileData;
	public ushort[,] ObjectData;
	public ushort[,] FlagData;
	public GameObject TileLayer;
	public GameObject[,] Tiles;
	public MeshRenderer[,] MeshTiles;
	public GameObject ObjectLayer;
	public GameObject[,] Objects;
	public MeshRenderer[,] MeshObjects;
	public GameObject FlagLayer;
	public GameObject Marker;
	public bool[,] HitFlags;
	public Vector2 gPosNow;
	public Vector2 gPosOld;
	
	public class Utility{
		public static Vector2 ConvertGamePosToCamPos(Vector2 gPos)
		{
			Vector2 cPos = new Vector2(0,0);

			float x,y;
			float tx = Consts.TILE_WIDTH/2;
			float ty = Consts.TILE_HEIGHT/2;

			x = gPos.x * tx + gPos.y * tx;
			y = - gPos.x * ty + gPos.y * ty;
			
			cPos.x = x - Screen.width/2;
			cPos.y = y - 0*Screen.height/2;
			return cPos;
		}
		public static Vector2 ConvertGamePosToCamPosWithSize(Vector2 gPos, Vector2 size)
		{
			Vector2 cPos = new Vector2(0,0);

			float x,y;
			//float tx = size.x/2;
			//float ty = size.y/2;
			float tx = Consts.TILE_WIDTH/2;
			float ty = Consts.TILE_HEIGHT/2;
			//gPos.x +=1;
			//gPos.y -=1;
			float ttx = ((size.x/Consts.TILE_WIDTH) - 2);
			float tty = ((size.y/Consts.TILE_HEIGHT) - 2);
			//Debug.Log (ttx+" "+tty);
			gPos.x = gPos.x - ttx;
			gPos.y = gPos.y + tty;
			x = gPos.x * tx + gPos.y * tx;
			y = - gPos.x * ty + gPos.y * ty;
			
			cPos.x = x - Screen.width/2 ;
			cPos.y = y - 0*Screen.height/2 ;
			return cPos;
		}
		public static Vector2 ConvertCamPosToGamePos(Vector2 cPos)
		{
			Vector2 gPos = new Vector2(0,0);
			float x0,y0;
			float x,y;
			float tx = Consts.TILE_WIDTH;
			float ty = Consts.TILE_HEIGHT;
			x0 = cPos.x - Screen.width/2;
			y0 = cPos.y - 0*Screen.height/2;
			x = x0 / tx - y0 / ty;
			y = x0 / tx + y0 / ty;

			gPos.x = Mathf.Floor(x);
			gPos.y = Mathf.Floor(y);
			return gPos;
		}
		public static Vector2 GetAbsGamePosByCamPos(Vector2 cPos,MapManager m)
		{
			Vector2 gPos;
			gPos = ConvertCamPosToGamePos(cPos);
			return GetAbsGamePosByRelGamePos(gPos,m);
		}
		public static Vector2 GetAbsGamePosByRelGamePos(Vector2 rgPos, MapManager m)
		{
			Vector2 agPos = m.gPosNow- Vector2.one*5;;
			return agPos+rgPos;
			// agPos = Main.mapManager.gPosNow - vector2.one *5 + rgPos
		}
		public static Vector3 GetMarkerCamPosition()
		{
			
			Vector3 mPos = new Vector3(0,0,0);
			Vector2 gPos = GetMarkerGamePosition();
			Vector2 cPos = ConvertGamePosToCamPos(gPos);
			mPos.x = cPos.x;
			mPos.y = cPos.y;
			mPos.z = (gPos.y - gPos.x)/Consts.MAP_RENDERSIZE;
			return mPos;
		}
		public static Vector2 GetRelGamePosByAbsGamePos(Vector2 agPos)
		{
			return (agPos + Vector2.one * 5 -Main.mapManager.gPosNow);
		}
		public static Vector3 GetMarkerGamePosition()
		{
			
			Vector2 cPos = Input.mousePosition + (Vector3)Consts.MOUSEOFFSET;
			
			Vector2 gPos = ConvertCamPosToGamePos(cPos);
			
			return gPos;
		}
		
		public static int GetArrayIndexByAbsGamePos(Vector2 gPos,MapManager m)
		{
			return (int) ( gPos.x * m.width + gPos.y);
		}
	}
	
	// Use this for initialization
	public MapManager () {
		gPosNow = new Vector2(76,77);
		
		ReadAutoMap();
		
		TileLayer = new GameObject("Tile Layer");
		//TileLayer.transform.parent = this.transform;
		
		ObjectLayer = new GameObject("Object Layer");
		//ObjectLayer.transform.parent = this.transform;
		
		FlagLayer = new GameObject("Flag Layer");
		//FlagLayer.transform.parent = this.transform;
		
		Tiles = new GameObject[Consts.MAP_RENDERSIZE, Consts.MAP_RENDERSIZE];
		MeshTiles = new MeshRenderer[Consts.MAP_RENDERSIZE, Consts.MAP_RENDERSIZE];
		Objects = new GameObject[Consts.MAP_RENDERSIZE, Consts.MAP_RENDERSIZE];
		MeshObjects = new MeshRenderer[Consts.MAP_RENDERSIZE, Consts.MAP_RENDERSIZE];

		for(int i = 0; i < Consts.MAP_RENDERSIZE && i < width; i++)
		{
			for(int j = 0; j < Consts.MAP_RENDERSIZE && j < height; j++)
			{
				Vector2 gPos = new Vector2(i - Consts.MAP_DISPLAYSIZE + Consts.CENTEROFFSET, j - Consts.MAP_DISPLAYSIZE + Consts.CENTEROFFSET);
				Vector2 cPos = Utility.ConvertGamePosToCamPos(gPos);
				Vector3 setPos = new Vector3(cPos.x, cPos.y, 2);
				
				Tiles[i,j] = GameObject.CreatePrimitive(PrimitiveType.Quad);
				Tiles[i,j].transform.localScale = new Vector3(Consts.TILE_WIDTH, Consts.TILE_HEIGHT, 1);
				Tiles[i,j].name = string.Format("Tile_{0}_{1}",i,j);
				Tiles[i,j].transform.parent = TileLayer.transform;
				Tiles[i,j].transform.localPosition = setPos;
				MeshTiles[i,j] = Tiles[i,j].GetComponent<MeshRenderer>();
				MeshTiles[i,j].material.shader = Shader.Find("Transparent/Cutout/Diffuse");

				
				Objects[i,j] = GameObject.CreatePrimitive(PrimitiveType.Quad);
				Objects[i,j].transform.localScale = new Vector3(Consts.TILE_WIDTH, Consts.TILE_HEIGHT, 1);
				Objects[i,j].name = string.Format("Objects_{0}_{1}",i,j);
				Objects[i,j].transform.parent = ObjectLayer.transform;
				Objects[i,j].transform.localPosition = setPos + Vector3.forward;
				MeshObjects[i,j] = Objects[i,j].GetComponent<MeshRenderer>();
				MeshObjects[i,j].material.shader = Shader.Find("Transparent/Cutout/Diffuse");

			}
		}

		Marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
		Marker.transform.position = Input.mousePosition;
		Marker.name = "Marker";
		MeshRenderer meshMarker = Marker.GetComponent<MeshRenderer>();
		meshMarker.material.shader = Shader.Find("Transparent/Cutout/Diffuse");
		meshMarker.material.mainTexture = Main.binManager.GetTextureByMapBmpNo(25001);
		Marker.transform.localScale = new Vector3(meshMarker.material.mainTexture.width,meshMarker.material.mainTexture.height,1);
		gPosOld = Vector2.one * -1;
	}

	// Read auto map file
	// Supposed to take parameter int floor, but for test, load default map
	internal void ReadAutoMap() {
		FileStream fsMap = File.OpenRead(Application.dataPath+DefaultMap);
		byte[] bufMap = new byte[fsMap.Length];
		int indexBuf = 0;
		fsMap.Read(bufMap,0,(int)fsMap.Length);
		
		width = (int)BitConverter.ToUInt32(bufMap,indexBuf);
		indexBuf += sizeof(uint);
		height = (int)BitConverter.ToUInt32(bufMap,indexBuf);
		indexBuf += sizeof(uint);

		if( (int)fsMap.Length != width*height*sizeof(ushort)*3+sizeof(uint)*2)
		{
			Debug.Log( "Map file format is not corrected");	
			return;
		}
		TileData = new ushort[width,height];
		ObjectData = new ushort[width,height];
		FlagData = new ushort[width,height];
		HitFlags = new bool[width,height];
		for(int i = 0; i < width; i ++)
		{
			for(int j = 0; j < height; j ++)
			{
				TileData[i,j] = BitConverter.ToUInt16(bufMap,indexBuf);
				indexBuf += sizeof(ushort);
			}			
		}
		for(int i = 0; i < width; i ++)
		{
			for(int j = 0; j < height; j ++)
			{
				ObjectData[i,j] = BitConverter.ToUInt16(bufMap,indexBuf);
				indexBuf += sizeof(ushort);
			}			
		}
		for(int i = 0; i < width; i ++)
		{
			for(int j = 0; j < height; j ++)
			{
				FlagData[i,j] = BitConverter.ToUInt16(bufMap,indexBuf);
				indexBuf += sizeof(ushort);
			}			
		}
		// To be done
		// Get hit map
		fsMap.Close();
	}
	
	internal void DrawMap()
	{
		if(gPosNow == gPosOld) return;
		for(int i = 0; i < Consts.MAP_RENDERSIZE && i < width; i++)
		{
			for(int j = 0; j < Consts.MAP_RENDERSIZE && j < height; j++)
			{
				MeshRenderer mesh = MeshTiles[i,j];
				GameObject go = Tiles[i,j];

				if(go == null || mesh == null) 
				{
					Debug.Log ("Error finding tile");	
					return;
				}
				
				int x = (int)(gPosNow.x - Consts.MAP_DISPLAYSIZE + i);
				int y = (int)(gPosNow.y - Consts.MAP_DISPLAYSIZE + j);
				if( x < 0 || y < 0 || x >= width || y >= height || TileData[x,y] <= 99 )
				{
					mesh.material.mainTexture = Main.binManager.GetTextureByMapBmpNo(0);
					continue;
				}
				int mapbmpno = (int)TileData[x,y]; 
				mesh.material.mainTexture = Main.binManager.GetTextureByMapBmpNo(mapbmpno);
			}
		}
		for(int i = 0; i < Consts.MAP_RENDERSIZE && i < width; i++)
		{
			for(int j = 0; j < Consts.MAP_RENDERSIZE && j < height; j++)
			{
				MeshRenderer mesh = MeshObjects[i,j];
				GameObject go = Objects[i,j];
				
				if(go == null || mesh == null) 
				{
					Debug.Log ("Error finding object");	
					return;
				}
				
				int x = (int)(gPosNow.x - Consts.MAP_DISPLAYSIZE + i);
				int y = (int)(gPosNow.y - Consts.MAP_DISPLAYSIZE + j);
				if( x < 0 || y < 0 || x >= width || y >= height || ObjectData[x,y] <= 99 )
				{
					mesh.material.mainTexture = Main.binManager.GetTextureByMapBmpNo(0);
					continue;
				}
				int mapbmpno = (int)ObjectData[x,y]; 
				mesh.enabled = true;
				mesh.material.mainTexture = Main.binManager.GetTextureByMapBmpNo(mapbmpno);
				go.transform.localScale = new Vector3(mesh.material.mainTexture.width,mesh.material.mainTexture.height,1);
				BinManager.AdrnData adrnBlock = Main.binManager.adrnData[(int)Main.binManager.bitmapTable[mapbmpno]];
				
				// Split non-unit object into unit size
				if(adrnBlock.Map_X == 1 && adrnBlock.Map_Y == 1)
				{
					Vector2 gPos = new Vector2(i - Consts.MAP_DISPLAYSIZE + Consts.CENTEROFFSET, j - Consts.MAP_DISPLAYSIZE + Consts.CENTEROFFSET);
					Vector2 cPos = Utility.ConvertGamePosToCamPos(gPos);
					cPos.x = cPos.x + adrnBlock.X;
					cPos.y = cPos.y - adrnBlock.Y;
					Vector3 setPos = new Vector3(cPos.x,cPos.y,(gPos.y-gPos.x)/(Consts.MAP_RENDERSIZE+1)+1);
					go.transform.localPosition = setPos;
				}
				else
				{
					// Split here
					int mapx,mapy;
					mapx = Mathf.CeilToInt((float)adrnBlock.Width/Consts.TILE_WIDTH);
					mapy = Mathf.CeilToInt((float)adrnBlock.Height/Consts.TILE_HEIGHT);
					
					if(go.transform.childCount > 0)
					{
						foreach(Transform child in go.transform)
						{
							GameObject.Destroy(child.gameObject);	
						}
					}
					
					for( int k = 0; k < mapx; k ++ )
					{
						for( int l = 0; l < mapy; l ++)
						{
							// Create game objects here
							// Get mesh renderer
							// TBD
						}
					}
					
					// Disable parent renderer
					mesh.enabled = false;
				}
			}			
		}
		gPosOld = gPosNow;
	}
	
	internal void DrawMarker()
	{
		Marker.transform.position = Utility.GetMarkerCamPosition();
	}
	
	// Update is called once per frame
	public void Update () {
		DrawMap();
		DrawMarker();
	}
	
	public void OnGUI() {

		Vector2 mousePos = Utility.GetAbsGamePosByCamPos(Input.mousePosition + (Vector3)Consts.MOUSEOFFSET,Main.mapManager);
		Vector2 mouserPos = Utility.GetMarkerGamePosition();

		GUI.TextArea(new Rect(10,10,110,130),string.Format("Debug INFO\nnowX = {0}\nnowY = {1}\nMouseX = {2}\nMouseY = {3}\nMouseRX = {4}\nMouseRY = {5}",Main.mapManager.gPosNow.x,Main.mapManager.gPosNow.y,mousePos.x,mousePos.y
			,mouserPos.x,mouserPos.y));
	}
}
