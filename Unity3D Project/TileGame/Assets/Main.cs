using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	public static BinManager binManager;
	public static MapManager mapManager;
	
	// Use this for initialization
	void Start () {
		// Initiate a BIN manger
		binManager = new BinManager();
		// Initiate a map manger
		mapManager = new MapManager();
		
	}
	
	// Update is called once per frame
	void Update () {
		// Draw map and marker
		mapManager.Update ();
		
		// Debug
		if( Input.GetMouseButtonDown(1) )
		{
			Vector2 gPosMarker = MapManager.Utility.GetAbsGamePosByRelGamePos(MapManager.Utility.GetMarkerGamePosition(),mapManager);
			Debug.Log(mapManager.HitFlags[(int)gPosMarker.x,(int)gPosMarker.y]);
		}
	}
	
	void OnGUI () {
		mapManager.OnGUI ();	
	}
}
