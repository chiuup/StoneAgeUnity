using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
	public int direction;
	public Vector2 position;
	public int action;
	public int sprNo;
	private bool walkFlag;
	private Vector2 walkDesPos;
	private ArrayList walkRoute;
	private int walkRouteCnt;
	private float walkNextTime;
	public BinManager binManager;

	
	private int GetDirectionFromPoints( Vector2 fromP, Vector2 toP )
	{
		// 8 directions
		Vector2 delta = toP - fromP;
		int ret = direction;
		if ( delta.y > 0 && delta.x == 0 )
			ret = 5;
		else if ( delta.x > 0 && delta.y > 0 )
			ret = 6;
		else if ( delta.x > 0 && delta.y == 0)
			ret = 7;
		else if ( delta.x > 0 && delta.y < 0 )
			ret = 0;
		else if ( delta.y < 0 && delta.x == 0)
			ret = 1;
		else if ( delta.x < 0 && delta.y < 0 )
			ret = 2;
		else if ( delta.x < 0 && delta.y == 0)
			ret = 3;
		else if ( delta.x < 0 && delta.y > 0 )
			ret = 4;
		return ret;
	}
	
	private void SetPosition(Vector2 sPos)
	{
		if(sPos.x < 0) sPos.x = 0;
		if(sPos.x > Main.mapManager.width) sPos.x = Main.mapManager.width;
		if(sPos.y < 0) sPos.y = 0;
		if(sPos.y > Main.mapManager.height) sPos.y = Main.mapManager.height;
		Main.mapManager.gPosNow = sPos;

	//	this.transform.position = rcPos;
	}
	
	private int GetTileIndexFromPos( Vector2 pos )
	{
		return 0;
	}
	
	private bool GetTileWalkable( Vector2 tPos )
	{
		return true;
		//return Main.mapManager.HitFlags[(int)tPos.x,(int)tPos.y];
	}
	
	private void CalculateWalkRoute()
	{
		
		walkRoute = new ArrayList();
		Vector2 delta = walkDesPos - Main.mapManager.gPosNow;
		int length = Mathf.CeilToInt( Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y) );
		walkRoute.Add(Main.mapManager.gPosNow);
		//Debug.Log (walkRoute[0]);
		for ( int i = 1; i <= length; i ++ )
		{
			Vector2 tempPos = (Vector2)walkRoute[i-1];
			if( walkDesPos.x > tempPos.x ) tempPos.x ++;
			else if( walkDesPos.x < tempPos.x && tempPos.x > 0) tempPos.x --;	
			if(tempPos.x < 0) tempPos.x ++;
			if( GetTileWalkable( tempPos ) == false )
			{
				tempPos.x = ((Vector2)walkRoute[i-1]).x;	
				length ++;
			}
			
			if( walkDesPos.y > tempPos.y ) tempPos.y ++;	
			else if( walkDesPos.y < tempPos.y && tempPos.y > 0) tempPos.y --;
			if(tempPos.y < 0) tempPos.y ++;
			
			if( GetTileWalkable( tempPos ) == false )
			{
				tempPos.y = ((Vector2)walkRoute[i-1]).y;
				length ++;
			}
			if( tempPos == (Vector2)walkRoute[i-1] )
			{
				break;	
			}
			
			walkRoute.Add( tempPos );
			//Debug.Log (walkRoute[i]);
		}
		
		this.walkRouteCnt = 1;
	}
	
	public void Walk ( int direction )
	{
		
	}
	
	public void WalkByRoute ()
	{
		if( walkRouteCnt < walkRoute.Count && walkFlag == true )
		{
			if( Time.time > walkNextTime )
			{
				//Debug.Log ("walk!");
				LookAt(GetDirectionFromPoints(Main.mapManager.gPosNow,(Vector2)walkRoute[walkRouteCnt]));
				SetPosition((Vector2)walkRoute[walkRouteCnt]);
				walkNextTime = Time.time + 0.06f;
				walkRouteCnt++;
				//Debug.Log(string.Format("{0} {1}",walkNextTime,Time.time));
				//Walk (GetDirectionFromPoints(position,(Vector2)walkRoute[walkRouteCnt]));
			}
		}
		else
		{
			this.action = (int)Consts.ANIM_LIST.ANIM_STAND;
			walkFlag = false;
			walkRouteCnt = 0;
			walkRoute.Clear();
		}
	}
	
	public void WalkCheck ()
	{
		if( walkFlag == true )
		{
			WalkByRoute();
		}
		
		if( Input.GetMouseButtonDown(1) )
		{
			Vector2 clickPos = MapManager.Utility.GetAbsGamePosByCamPos(Input.mousePosition + (Vector3)Consts.MOUSEOFFSET,Main.mapManager);
			int clickDir = GetDirectionFromPoints( Main.mapManager.gPosNow, clickPos );
			if( clickDir != direction )
				direction = clickDir;	
			
		}
		if( Input.GetMouseButtonDown(0) )
		{
			Vector2 clickPos = MapManager.Utility.GetAbsGamePosByRelGamePos(MapManager.Utility.GetMarkerGamePosition(),Main.mapManager);
			if( clickPos == Main.mapManager.gPosNow )
			{
				direction = 0;
				//Debug.Log ("Walk Start");
				//walkFlag = true;
				//walkDesPos = clickPos;
				//CalculateWalkRoute();
				//this.action = (int)Consts.ANIM_LIST.ANIM_WALK;
				//Debug.Log (walkRoute.Count);
				//WalkByRoute ();
			}
			else
			{
				
				walkFlag = true;
				walkDesPos = clickPos;
				CalculateWalkRoute();
				this.action = (int)Consts.ANIM_LIST.ANIM_WALK;
				//Debug.Log ("Walk Start to "+clickPos+" by "+walkRoute.Count);
				//Debug.Log (walkRoute.Count);
				WalkByRoute ();
			}
		}
	}
	public void LookAt (int direction)
	{
		this.direction = direction;	
	}

	private int lastframe;
	private void TempSpriteManager ()
	{

		MeshRenderer mesh = this.GetComponent<MeshRenderer>();
		/*Vector2 offset = Vector2.zero;
		int frame = (int)(Time.time * 15);
		frame = frame % 4;
		if(frame > 2) frame = 0;
		offset = new Vector2(frame*0.25f,0.75f - 0.25f*direction);
		mesh.sharedMaterial.mainTextureOffset = offset;*/
		
		BinManager.SprData sprBlock = (BinManager.SprData)(Main.binManager.sprData[(uint)this.sprNo]);
		int i = 0;
		for( i = 0; i < (int)sprBlock.animSize; i ++ )
		{
			if( sprBlock.ptAnimList[i].no == (ushort)this.action && sprBlock.ptAnimList[i].dir == (ushort)direction)
				break;
		}
		if( i >= (int)sprBlock.animSize ) i = 0;
		
		BinManager.AnimList animList = sprBlock.ptAnimList[i];
		BinManager.FrameList[] frameList = animList.ptFrameList;
		
		int frame = (int) (Time.time * animList.dtAnim);
		frame = frame % (int)animList.frameCnt;
		if(frame == lastframe) return;

		Texture2D texture = Main.binManager.GetTextureByBmpNo((int)frameList[frame].BmpNo);
		BinManager.AdrnData adrnBlock = Main.binManager.adrnData[(int)frameList[frame].BmpNo];

		this.transform.localScale = new Vector3( (float)texture.width , (float)texture.height , 1);
		mesh.material.mainTexture = texture;

		Vector3 offset = new Vector3((float)(frameList[frame].PosX+adrnBlock.X) + (float)(texture.width)/2f,
							-(float)(frameList[frame].PosY+adrnBlock.Y) - (float)(texture.height)/2f,
							0);
		this.transform.position = transform.parent.transform.position + offset;
		
	//	if( mesh.material.shader != Shader.Find ("Transparent/Diffuse") )
			//smesh.material.shader = Shader.Find ("Transparent/Diffuse");
	}

	// Use this for initialization
	void Start () {
		lastframe = -1;
		//this.position = Main.mapManager.gPosNow;
		this.direction = 0;
		//SetPosition(this.position);
		//this.position = Vector3.zero;
		this.sprNo = 0;
		this.action = (int)Consts.ANIM_LIST.ANIM_STAND;
		walkFlag = false;
		MeshRenderer mesh = this.GetComponent<MeshRenderer>();
		if( mesh.material.shader != Shader.Find ("Transparent/Cutout/Diffuse") )
			mesh.material.shader = Shader.Find ("Transparent/Cutout/Diffuse");
	//	mesh.material.SetFloat("_Cutoff",0.75f*0.5f);
		//mesh.material.mainTexture.filterMode = FilterMode.Point;
	}
	
	// Update is called once per frame
	void Update () {
		WalkCheck ();
		TempSpriteManager ();
	}
}
