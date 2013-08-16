using UnityEngine;
using System.Collections;

public class Consts {

	public const int DEF_PAL = 0;
	public static string[] PAL = {"/Resources/Pal/Palet_1.sap", "/Resources/Pal/Palet_2.sap", "/Resources/Pal/Palet_3.sap",
									"/Resources/Pal/Palet_4.sap", "/Resources/Pal/Palet_5.sap", "/Resources/Pal/Palet_6.sap",
									"/Resources/Pal/Palet_7.sap", "/Resources/Pal/Palet_8.sap", "/Resources/Pal/Palet_9.sap",
									"/Resources/Pal/Palet_10.sap", "/Resources/Pal/Palet_11.sap", "/Resources/Pal/Palet_12.sap",
									"/Resources/Pal/Palet_13.sap", "/Resources/Pal/Palet_14.sap", "/Resources/Pal/Palet_15.sap"};
	public const byte BIT_CMP = 0x80;
	public const byte BIT_ZERO = 0x40;
	public const byte BIT_REP_LARG = 0x10;
	public const byte BIT_REP_LARG2 = 0x20;
	public const string REAL = "/Resources/real.bin";
	public const string ADRN = "/Resources/adrn.bin";
	public const string SPR = "/Resources/spr.bin";
	public const string SPRADRN = "/Resources/spradrn.bin";
	public const int SPRSTART = 100000;
	public enum ANIM_LIST { ANIM_ATTACK, ANIM_DAMAGE, ANIM_DEAD, ANIM_STAND, ANIM_WALK, ANIM_SIT, ANIM_HAND, ANIM_HAPPY, ANIM_ANGRY, ANIM_SAD, ANIM_GUARD, ANIM_NOD, ANIM_THROW };
	public const int MAX_TEXTUREDATA = 5000;
	
	public static Vector2 MOUSEOFFSET = new Vector3(Screen.width/2+64f/2,-Screen.height/2);
	public const int TILE_WIDTH = 64;
	public const int TILE_HEIGHT = 47;
	public const int MAP_RENDERSIZE = 37;
	public const int MAP_DISPLAYSIZE = 18;
	public const int CENTEROFFSET = 5;
}
