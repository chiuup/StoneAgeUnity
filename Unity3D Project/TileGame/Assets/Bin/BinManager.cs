using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

public class BinManager  {
	private FileStream fsReal;

	//private Dictionary<uint,uint> bitmapTable;
	public uint[] bitmapTable;
	private Dictionary<uint,SpradrnData> spradrnData;

	private Color32[] activePalette;
	
	public List<AdrnData> adrnData;
	public Dictionary<uint,SprData> sprData;
	public LimitedDictionary<int,Texture2D> textureData;

	#region
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct RealHeader
    {
        public ushort ID;
        public byte CompressFlag;
        public int Width;
        public int Height;
        public int Size;
	}
	public struct AdrnData
	{
		public uint BitmapNo;
		public uint Address;
		public uint Size;
		public int X;
		public int Y;
		public uint Width;
		public uint Height;
		public byte Map_X;
		public byte Map_Y;
		public ushort Map_Hit;
		public short Map_Height;
		public short Map_Broken;
		public short Map_Indamage;
		public short Map_Outdamage;
		public short Map_Inpoison;
		public short Map_Innumb;
		public short Map_Inquiet;
		public short Map_Instone;
		public short Map_Indark;
		public short Map_Inconfuse;
		public short Map_Outpoison;
		public short Map_Outnumb;
		public short Map_Outquiet;
		public short Map_Outstone;
		public short Map_Outdark;
		public short Map_Outconfuse;
		public short Map_Effect1;
		public short Map_Effect2;
		public ushort Map_Damy_a;
		public ushort Map_Damy_b;
		public ushort Map_Damy_c;
		public uint Map_BitmapNo;
	};
	public struct SpradrnData
	{
		public uint sprNo;
		public uint offset;
		public ushort animSize;
	};
	public struct SprData
	{
		public ushort animSize;
		public AnimList[] ptAnimList;
	};
	public struct FrameList
	{
		public uint BmpNo;
		public short PosX;
		public short PosY;
		public ushort SoundNo;
	};
	public struct AnimList
	{
		public ushort dir;
		public ushort no;
		public uint dtAnim;
		public uint frameCnt;
		public FrameList[] ptFrameList;
	};
	#endregion
	
	// Get Texture2D by bmpNo
	public Texture2D GetTextureByBmpNo(int bmpno)
	{	
		if( textureData.ContainsKey(bmpno) )
		{
				return textureData[bmpno];
		}
		
		AdrnData adrnBlock = adrnData[bmpno];
		Color32[] colors = GetBitmapByBmpNo(bmpno, adrnBlock);
		if( colors != null )
		{
			Texture2D retTexture = new Texture2D((int)adrnBlock.Width,(int)adrnBlock.Height);
			retTexture.SetPixels32(colors);
			retTexture.filterMode = FilterMode.Trilinear;
			retTexture.alphaIsTransparency = true;
			retTexture.Apply();
			
			textureData.Add(bmpno,retTexture);
			return retTexture;
		}
		else
		{
			return null;	
		}
	}
	
	// Get Texture2D by MapbmpNo
	public Texture2D GetTextureByMapBmpNo(int mapbmpno)
	{
		int index = (int)(bitmapTable[mapbmpno]);

		AdrnData adrnBlock = adrnData[index];
		Texture2D retTexture = GetTextureByBmpNo((int)(adrnBlock.BitmapNo));
		retTexture.filterMode = FilterMode.Point;
		return retTexture;
	}
	
	// Use this for initialization
	public BinManager () {
		// Create default palette
		if( activePalette == null )
		{
			CreatePalette(Consts.DEF_PAL);
		}
		
		// Open real.bin
		fsReal = File.OpenRead(Application.dataPath + Consts.REAL);
		if ( fsReal == null )
		{
			Debug.Log ( "Open real.bin failed.");
			Application.Quit();
		}
		
		// Read adrn.bin and store information
		FileStream fsAdrn = File.OpenRead(Application.dataPath + Consts.ADRN);
		if ( fsAdrn == null )
		{	
			Debug.Log ( "Open adrn.bin failed.");
			Application.Quit();
		}
		int adrnSize = 80;
		int adrnCnt = (int)fsAdrn.Length / adrnSize;
		bitmapTable = new uint[0xffff]; // 0xffff = Max Map_BitmapNo
		adrnData = new List<AdrnData>();
		for ( int i = 0; i < adrnCnt; i ++ )
		{
			byte[] bufferAdrn = new byte[adrnSize];
			fsAdrn.Read(bufferAdrn, 0, adrnSize);
			if( adrnData.Count <= i )
			{
				adrnData.Add (new AdrnData());
			}
			adrnData[i] = ByteToStruct<AdrnData>(bufferAdrn);
			AdrnData adrnBlock = adrnData[i];
			int mapbitmapno = (int)adrnBlock.Map_BitmapNo;
			if ( mapbitmapno != 0 )
			{
				bitmapTable[(int)mapbitmapno] = adrnBlock.BitmapNo;
				// Some hot fixes
				if( (12082 <= mapbitmapno && mapbitmapno <= 12811)
					|| (10132 <= mapbitmapno && mapbitmapno <= 10136) )
				{
					adrnBlock.Map_Hit = (ushort)(300 + adrnBlock.Map_Hit % 100);	
				}
			}
			else
			{
				bitmapTable[(int)mapbitmapno] = 0;
			}
		}
		fsAdrn.Close();
		
		// Read spr.bin and spradrn.bin, and store sprite information
		FileStream fsSpr = File.OpenRead(Application.dataPath + Consts.SPR);
		FileStream fsSpradrn = File.OpenRead(Application.dataPath + Consts.SPRADRN);
		if ( fsSpr == null || fsSpradrn == null )
		{			
			Debug.Log ( "Open spr.bin and spradrn.bin failed.");
			Application.Quit();
		}
		int spradrnSize = 12;
		int index = 0;
		spradrnData = new Dictionary<uint,SpradrnData>();
		while(true)
		{
			byte[] bufferSpradrn = new byte[spradrnSize];
			int bytesread = fsSpradrn.Read(bufferSpradrn, 0, spradrnSize);
			if( bytesread <= 0) break; // End of file
			SpradrnData spradrnBlock = new SpradrnData();
			spradrnBlock = ByteToStruct<SpradrnData>(bufferSpradrn);
			spradrnBlock.sprNo = spradrnBlock.sprNo - Consts.SPRSTART;
			spradrnData.Add( spradrnBlock.sprNo, spradrnBlock );
			index++;
		}
		
		index = 0;
		int keyCnt = 0;
		sprData = new Dictionary<uint, SprData>();
		while(true)
		{
			// If all keys are found, stop loop
			if(keyCnt >= spradrnData.Count) break;
			
			SpradrnData spradrnBlock;
			bool ret = spradrnData.TryGetValue((uint)index, out spradrnBlock);
			
			// If sprNo used
			if( ret == true )
			{
				// Key found counter ++
				keyCnt ++;	
				// New sprData block
				SprData sprBlock = new SprData();
				// Get animList array size
				sprBlock.animSize = spradrnBlock.animSize;
				// New animList array by size
				sprBlock.ptAnimList = new AnimList[(int)sprBlock.animSize];
				// Seek file to offset indicated by sprAdrn info
				fsSpr.Seek((long)spradrnBlock.offset,SeekOrigin.Begin);
				for( int i = 0; i < (int)sprBlock.animSize; i++)
				{
					byte[] buf = new byte[12]; // 12 = Size per AnimList
					fsSpr.Read(buf, 0, 12);
					
					// Can't use ByteToStruct, size not matching
					// Read pre-frame info
					int indexBuf = 0;
					sprBlock.ptAnimList[i].dir = BitConverter.ToUInt16(buf,indexBuf);
					indexBuf += sizeof(ushort);
					sprBlock.ptAnimList[i].no = BitConverter.ToUInt16(buf,indexBuf);
					indexBuf += sizeof(ushort);
					sprBlock.ptAnimList[i].dtAnim = BitConverter.ToUInt32(buf,indexBuf);
					indexBuf += sizeof(uint);
					sprBlock.ptAnimList[i].frameCnt = BitConverter.ToUInt32(buf,indexBuf);
					indexBuf += sizeof(uint);
					
					if(sprBlock.ptAnimList[i].frameCnt != 0)
					{
						sprBlock.ptAnimList[i].dtAnim = sprBlock.ptAnimList[i].dtAnim / (sprBlock.ptAnimList[i].frameCnt << 4);
						if( sprBlock.ptAnimList[i].dtAnim < 1 ) sprBlock.ptAnimList[i].dtAnim = 1;
					}
					
					// Read info per frame
					// Actual size per frame block is 10 = 4 + 2 + 2 + 2
					int size = 5 * (int)sprBlock.ptAnimList[i].frameCnt;
					buf = new byte[size * sizeof(ushort)];
					fsSpr.Read(buf,0,size*sizeof(ushort));
					indexBuf = 0;
					// New frameList by frameCnt
					sprBlock.ptAnimList[i].ptFrameList = new FrameList[(int)sprBlock.ptAnimList[i].frameCnt];
					for( int j = 0; j < (int)sprBlock.ptAnimList[i].frameCnt; j ++)
					{
						sprBlock.ptAnimList[i].ptFrameList[j].BmpNo = BitConverter.ToUInt32(buf,indexBuf);
						indexBuf += sizeof(uint);
						sprBlock.ptAnimList[i].ptFrameList[j].PosX = BitConverter.ToInt16(buf,indexBuf);
						indexBuf += sizeof(short);
						sprBlock.ptAnimList[i].ptFrameList[j].PosY = BitConverter.ToInt16(buf,indexBuf);
						indexBuf += sizeof(short);
						sprBlock.ptAnimList[i].ptFrameList[j].SoundNo = BitConverter.ToUInt16(buf,indexBuf);
						indexBuf += sizeof(ushort);
					}
				}
				// Done reading 1 SprData
				sprData.Add((uint)index,sprBlock);
			}
			index++;
		}
		// Read bin files all done 
		
		// Initialize texture dictionary
		textureData = new LimitedDictionary<int, Texture2D>();
		textureData.MaxItemsToHold = Consts.MAX_TEXTUREDATA;
	}

	// Called when game is terminated
	public void OnApplicationQuit () {
		if( fsReal != null )
			fsReal.Close ();
	}
	
	// Get bitmap data by BmpNo
	private Color32[] GetBitmapByBmpNo(int bmpno, AdrnData adrnBlock)
	{

		if( adrnBlock.BitmapNo != bmpno )
		{
			Debug.Log("Error loading adrnData, could be resulted from errors in bin loading");
			return null;
		}
		if(fsReal == null) fsReal = File.OpenRead (Application.dataPath + Consts.REAL);
		if(fsReal == null) 
		{
			Debug.Log("Error opening real.bin");
			return null;
		}
		fsReal.Seek((long)adrnBlock.Address,SeekOrigin.Begin);
		Stream realOut = new MemoryStream();
		RealHeader rhOut = new RealHeader();
		if( Decode( fsReal, out realOut, out rhOut) == true )
		{
			realOut.Seek(0,SeekOrigin.Begin);
			Color32[] colors = new Color32[realOut.Length];
			for( int i = 0; i < realOut.Length; i ++ )
			{
				int colorindex = realOut.ReadByte();
				if( colorindex < 0 || colorindex > 255 ) return null;
				colors[i].a = activePalette[colorindex].a;
				colors[i].r = activePalette[colorindex].r;
				colors[i].g = activePalette[colorindex].g;
				colors[i].b = activePalette[colorindex].b;
			}
			return colors;
		}
		else
		{
			Debug.Log("Error while decoding from stream");
			return null;			
		}
	}
	
	// Decode input to bitmap data
	private bool Decode(Stream input, out Stream output, out RealHeader header)
	{
		output = new MemoryStream();
		byte[] bufferHeader = new byte[16];
		
		input.Read(bufferHeader,0,bufferHeader.Length);
		header = new RealHeader();
		header = ByteToStruct<RealHeader>(bufferHeader);
		
		if(!string.Equals("RD",System.Text.Encoding.Default.GetString(System.BitConverter.GetBytes(header.ID))))
        {
            output = null;
            return false;
        }
		
		byte[] bufferInput = new byte[header.Size - bufferHeader.Length];
		int bytesread = input.Read(bufferInput,0,header.Size - bufferHeader.Length);
		
		// 未壓縮圖像數據大小尚未明朗
		if( header.CompressFlag == 0 )
		{
			Debug.Log("Warning: Uncompressed image data detected!");
			output.Seek(0,SeekOrigin.Begin);
			output.Write(bufferInput,0,bytesread);
			return true;
		}
		
		int indexInput = 0;
		byte idx;
		byte repData;
		int cnt;
		
		output.Seek(0,SeekOrigin.Begin);
		
		while( indexInput < bufferInput.Length )
		{
			idx = bufferInput[indexInput++];
			if ((idx & Consts. BIT_CMP ) != 0)
			{
				if (( idx & Consts.BIT_ZERO) != 0)
				{
					repData = 0;	
				}
				else
				{
					repData = bufferInput[indexInput++];
				}
				if (( idx & Consts.BIT_REP_LARG2) != 0)
				{
					cnt = ((idx & 0x0f) << 16);
					cnt |= ((bufferInput[indexInput]) << 8);
					indexInput ++;
					cnt |= bufferInput[indexInput++];
				}
				else if ((idx & Consts.BIT_REP_LARG) != 0)
				{
					cnt = ((idx & 0x0f)<<8);
					cnt |= bufferInput[indexInput++];
				}
				else
				{
					cnt = (idx & 0x0f);
				}
				for( int i = 0; i < cnt && output.CanWrite; i ++ )
				{
					output.WriteByte(repData);	
				}
			}
			else
			{
				if ((idx & Consts.BIT_REP_LARG) != 0)
				{
					cnt = ((idx & 0x0f)<<8);
					cnt |= bufferInput[indexInput++];
				}
				else
				{
					cnt = (idx & 0x0f);	
				}
				if ( cnt >= 0xfffff )
				{
					return false;	
				}
				for (int i = 0; i < cnt && output.CanWrite; i++) // TODO 長度限制待測試
					output.WriteByte(bufferInput[indexInput++]);
			}
		}
		return true;
	}

	// Convert byte array to T type struct
    private T ByteToStruct<T>(byte[] bytes) where T: struct
	{
		GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
		handle.Free();
		return stuff;
	}
	
	// Create Palette
	private bool CreatePalette( int palIndex )
	{	
		activePalette = new Color32[256];
		// Open palette from file
		FileStream fsPal = File.Open(Application.dataPath+Consts.PAL[palIndex], FileMode.Open, FileAccess.Read);
		if ( fsPal == null ) return false;
		
		// Additional palette information
		AdditionalPalette();
		
		// Read palette from file
		for( int i = 16; i < 240; i ++ )
		{
			activePalette[i].a = 0xff;
			activePalette[i].b = (byte)fsPal.ReadByte();
			activePalette[i].g = (byte)fsPal.ReadByte();
			activePalette[i].r = (byte)fsPal.ReadByte();
		}
		fsPal.Close();
		
		// Set index = 168 as Color.Black
		activePalette[168].a = 0xff;
		activePalette[168].r = 0x00;
		activePalette[168].g = 0x00;
		activePalette[168].b = 0x00;
		
		return true;	
	}
	
	// Addtional palette information
	private void AdditionalPalette()
	{
		// 0:黑
		activePalette[0].a = 0x00;
		activePalette[0].r = 0x00;
		activePalette[0].g = 0x00;
		activePalette[0].b = 0x00;
		// 0:黑
		activePalette[1].a = 0xff;
		activePalette[1].r = 0x80;
		activePalette[1].g = 0x00;
		activePalette[1].b = 0x00;
		// 0:黑
		activePalette[2].a = 0xff;
		activePalette[2].r = 0x00;
		activePalette[2].g = 0x80;
		activePalette[2].b = 0x00;
		// 0:黑
		activePalette[3].a = 0xff;
		activePalette[3].r = 0x80;
		activePalette[3].g = 0x80;
		activePalette[3].b = 0x00;
		// 0:黑
		activePalette[4].a = 0xff;
		activePalette[4].r = 0x00;
		activePalette[4].g = 0x00;
		activePalette[4].b = 0x80;
		// 0:黑
		activePalette[5].a = 0xff;
		activePalette[5].r = 0x80;
		activePalette[5].g = 0x00;
		activePalette[5].b = 0x80;
		// 0:黑
		activePalette[6].a = 0xff;
		activePalette[6].r = 0x00;
		activePalette[6].g = 0x80;
		activePalette[6].b = 0x80;
		// 0:黑
		activePalette[7].a = 0xff;
		activePalette[7].r = 0xc0;
		activePalette[7].g = 0xc0;
		activePalette[7].b = 0xc0;
		// 0:黑
		activePalette[8].a = 0xff;
		activePalette[8].r = 0xc0;
		activePalette[8].g = 0xdc;
		activePalette[8].b = 0xc0;
		// 0:黑
		activePalette[9].a = 0xff;
		activePalette[9].r = 0xa6;
		activePalette[9].g = 0xca;
		activePalette[9].b = 0xf0;
		// 0:黑
		activePalette[10].a = 0xff;
		activePalette[10].r = 0xde;
		activePalette[10].g = 0x00;
		activePalette[10].b = 0x00;
		// 0:黑
		activePalette[11].a = 0xff;
		activePalette[11].r = 0xff;
		activePalette[11].g = 0x5f;
		activePalette[11].b = 0x00;
		// 0:黑
		activePalette[12].a = 0xff;
		activePalette[12].r = 0xff;
		activePalette[12].g = 0xff;
		activePalette[12].b = 0xa0;
		// 0:黑
		activePalette[13].a = 0xff;
		activePalette[13].r = 0x00;
		activePalette[13].g = 0x5f;
		activePalette[13].b = 0xd2;
		// 0:黑
		activePalette[14].a = 0xff;
		activePalette[14].r = 0x50;
		activePalette[14].g = 0xd2;
		activePalette[14].b = 0xff;
		// 0:黑
		activePalette[15].a = 0xff;
		activePalette[15].r = 0x28;
		activePalette[15].g = 0xe1;
		activePalette[15].b = 0x28;
		// 0:黑
		activePalette[240].a = 0xff;
		activePalette[240].r = 0xf5;
		activePalette[240].g = 0xc3;
		activePalette[240].b = 0x96;
		// 0:黑
		activePalette[241].a = 0xff;
		activePalette[241].r = 0xe1;
		activePalette[241].g = 0xa0;
		activePalette[241].b = 0x5f;
		// 0:黑
		activePalette[242].a = 0xff;
		activePalette[242].r = 0xc3;
		activePalette[242].g = 0x7d;
		activePalette[242].b = 0x46;
		// 0:黑
		activePalette[243].a = 0xff;
		activePalette[243].r = 0x9b;
		activePalette[243].g = 0x55;
		activePalette[243].b = 0x1e;
		// 0:黑
		activePalette[244].a = 0xff;
		activePalette[244].r = 0x46;
		activePalette[244].g = 0x41;
		activePalette[244].b = 0x37;
		// 0:黑
		activePalette[245].a = 0xff;
		activePalette[245].r = 0x28;
		activePalette[245].g = 0x23;
		activePalette[245].b = 0x1e;
		// 0:黑
		activePalette[246].a = 0xff;
		activePalette[246].r = 0xff;
		activePalette[246].g = 0xfb;
		activePalette[246].b = 0xf0;
		// 0:黑
		activePalette[247].a = 0xff;
		activePalette[247].r = 0xa0;
		activePalette[247].g = 0xa0;
		activePalette[247].b = 0xa4;
		// 0:黑
		activePalette[248].a = 0xff;
		activePalette[248].r = 0x80;
		activePalette[248].g = 0x80;
		activePalette[248].b = 0x80;
		// 0:黑
		activePalette[249].a = 0xff;
		activePalette[249].r = 0xff;
		activePalette[249].g = 0x00;
		activePalette[249].b = 0x00;
		// 0:黑
		activePalette[250].a = 0xff;
		activePalette[250].r = 0x00;
		activePalette[250].g = 0xff;
		activePalette[250].b = 0x00;
		// 0:黑
		activePalette[251].a = 0xff;
		activePalette[251].r = 0xff;
		activePalette[251].g = 0xff;
		activePalette[251].b = 0x00;
		// 0:黑
		activePalette[252].a = 0xff;
		activePalette[252].r = 0x00;
		activePalette[252].g = 0x00;
		activePalette[252].b = 0xff;
		// 0:黑
		activePalette[253].a = 0xff;
		activePalette[253].r = 0xff;
		activePalette[253].g = 0x00;
		activePalette[253].b = 0xff;		
		// 0:黑
		activePalette[254].a = 0xff;
		activePalette[254].r = 0x00;
		activePalette[254].g = 0xff;
		activePalette[254].b = 0xff;
		// 0:黑
		activePalette[255].a = 0xff;
		activePalette[255].r = 0xff;
		activePalette[255].g = 0xff;
		activePalette[255].b = 0xff;
	}
}
