using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// .ssaeファイルの読み込みクラス
/// </summary>
public class SSAELoader
{
	/// <summary>
	/// .ssaeファイルの読み込み
	/// </summary>
	/// <returns><c>true</c>読み込み成功<c>false</c>読み込み失敗</returns>
	/// <param name="filepath">.ssaeファイルのファイルパス</param>
	/// <param name="info">読み込みデータの格納先</param>
	public bool OpenRead(string filepath, out SSInfo.SSAEInfo info)
	{
		info = new SSInfo.SSAEInfo();
		XmlDocument DataXML = new XmlDocument();
		DataXML.Load(filepath);
		
		XmlNode NodeRoot = DataXML.FirstChild;
		NodeRoot = NodeRoot.NextSibling;
		
		NameTable NodeNameSpace = new NameTable();
		XmlNamespaceManager ManagerNameSpace = new XmlNamespaceManager(NodeNameSpace);
		XmlNodeList NodeList = null;

		string filename = filepath.Substring(filepath.LastIndexOf('/')+1);
		filename = filename.Substring(0,filename.LastIndexOf('.'));
		info.Name = filename;

		string valueT = "";
		valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "settings/fps", ManagerNameSpace);
		if(!string.IsNullOrEmpty(valueT))
		{
			info.Fps = int.Parse(valueT);
		}

		valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "settings/frameCount", ManagerNameSpace);
		if(!string.IsNullOrEmpty(valueT))
		{
			info.FrameCount = int.Parse(valueT);
		}

		NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "Model/partList/value", ManagerNameSpace);
		if(null == NodeList)
		{
			Debug.LogError("SSAELoader: Error!: PartLit-Node Not-Found");
			return false;
		}

		foreach(XmlNode NodePart in NodeList)
		{
			SSInfo.SSAEInfo.Part part = new SSInfo.SSAEInfo.Part();
			valueT = XMLUtility.TextGetSelectSingleNode(NodePart, "name", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				part.Name = valueT;
			}

			valueT = XMLUtility.TextGetSelectSingleNode(NodePart, "arrayIndex", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				part.Index = int.Parse(valueT);
			}
			valueT = XMLUtility.TextGetSelectSingleNode(NodePart, "parentIndex", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				part.ParentIndex = int.Parse(valueT);
			}
			valueT = XMLUtility.TextGetSelectSingleNode(NodePart, "type", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				part.Type =  (valueT == "normal")?SSInfo.PartsType.Normal:SSInfo.PartsType.Null;
			}
			valueT = XMLUtility.TextGetSelectSingleNode(NodePart, "alphaBlendType", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				switch(valueT)
				{
				case "mix":part.Blend = SSInfo.BlendType.Mix;break;
				case "add":part.Blend = SSInfo.BlendType.Add;break;
				case "sub":part.Blend = SSInfo.BlendType.Sub;break;
				case "mul":part.Blend = SSInfo.BlendType.Mul;break;
				default:part.Blend = SSInfo.BlendType.Mix;break;
				}
			}
			info.Parts.Add(part);
		}

		NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "cellmapNames/value", ManagerNameSpace);
		if(null == NodeList)
		{
			Debug.LogError("SSAELoader: Error!: CellmapNamesList-Node Not-Found");
			return false;
		}
		
		foreach(XmlNode NodeNameCellmapName in NodeList)
		{
			string name = NodeNameCellmapName.InnerText;
			info.CellmapNames.Add(name);
		}

		NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "animeList/anime", ManagerNameSpace);
		if(null == NodeList)
		{
			Debug.LogError("SSAELoader: Error!: CellmapNamesList-Node Not-Found");
			return false;
		}

		foreach(XmlNode NodeAnime in NodeList)
		{
			SSInfo.SSAEInfo.Anime anime = new SSInfo.SSAEInfo.Anime();
			valueT = XMLUtility.TextGetSelectSingleNode(NodeAnime, "name", ManagerNameSpace);
			if(!string.IsNullOrEmpty(valueT))
			{
				anime.Name = valueT;
			}

			XmlNodeList partAnimeNodeList = XMLUtility.XML_SelectNodes(NodeAnime, "partAnimes/partAnime", ManagerNameSpace);
			if(null == NodeList)
			{
				Debug.LogError("SSAELoader: Error!: PartAnimesList-Node Not-Found");
				return false;
			}

			foreach(XmlNode PartAnimeNode in partAnimeNodeList)
			{
				SSInfo.SSAEInfo.AnimePart animePart = new SSInfo.SSAEInfo.AnimePart();
				
				valueT = XMLUtility.TextGetSelectSingleNode(PartAnimeNode, "partName", ManagerNameSpace);
				if(!string.IsNullOrEmpty(valueT))
				{
					animePart.PartName = valueT;
				}

				XmlNodeList attributeNodeList = XMLUtility.XML_SelectNodes(PartAnimeNode, "attributes/attribute", ManagerNameSpace);
				if(null == attributeNodeList)
				{
					Debug.LogError("SSAELoader: Error!: AttributeList-Node Not-Found");
					return false;
				}
				foreach(XmlNode attributeNode in attributeNodeList)
				{
					SSInfo.KeyList keyList = new SSInfo.KeyList();
					keyList.Type = GetKeyAttributeTag(attributeNode.Attributes["tag"].Value);
					if(keyList.Type == SSInfo.AnimationKeyType.UnKnown)continue;


					XmlNodeList keyNodeList = XMLUtility.XML_SelectNodes(attributeNode, "key", ManagerNameSpace);
					if(null == keyNodeList)
					{
						Debug.LogError("SSAELoader: Error!: KeyList-Node Not-Found");
						return false;
					}
					foreach(XmlNode keyNode in keyNodeList)
					{
						var addKey = CreateKey(keyList.Type, keyNode, ManagerNameSpace);
						if(addKey != null)keyList.Keys.Add(addKey);
					}
					animePart.KeysList.Add(keyList);
				}

				anime.AnimeParts.Add(animePart);
			}

			info.Animes.Add(anime);
		}
		return true;
	}

	/// <summary>
	/// Gets the key attribute tag.
	/// </summary>
	/// <returns>The key attribute tag.</returns>
	/// <param name="tag">Tag.</param>
	SSInfo.AnimationKeyType GetKeyAttributeTag(string tag)
	{
		SSInfo.AnimationKeyType type;
		switch(tag)
		{
		case "CELL":type = SSInfo.AnimationKeyType.Cell;break;
		case "POSX":type = SSInfo.AnimationKeyType.PosX;break;
		case "POSY":type = SSInfo.AnimationKeyType.PosY;break;
		case "ROTZ":type = SSInfo.AnimationKeyType.RotZ;break;
		case "SCLX":type = SSInfo.AnimationKeyType.ScaleX;break;
		case "SCLY":type = SSInfo.AnimationKeyType.ScaleY;break;
		case "ALPH":type = SSInfo.AnimationKeyType.Alpha;break;
		case "PRIO":type = SSInfo.AnimationKeyType.Priorty;break;
		case "FLPH":type = SSInfo.AnimationKeyType.FlipH;break;
		case "FLPV":type = SSInfo.AnimationKeyType.FlipV;break;
		case "HIDE":type = SSInfo.AnimationKeyType.Hide;break;
		case "USER":type = SSInfo.AnimationKeyType.User;break;
		
		default:type = SSInfo.AnimationKeyType.UnKnown;break;
		}
		return type;
	}

	/// <summary>
	/// Creates the key.
	/// </summary>
	/// <returns>The key.</returns>
	/// <param name="type">Type.</param>
	/// <param name="node">Node.</param>
	/// <param name="manager">Manager.</param>
	SSInfo.AbstractKey CreateKey(SSInfo.AnimationKeyType type, XmlNode node, XmlNamespaceManager manager)
	{
		SSInfo.AbstractKey key = null;
		string valueT;
		string[] splitItem;
		int value = 0;
		switch(type)
		{
		case SSInfo.AnimationKeyType.Cell:
			key = new SSInfo.CellKey();
			SetKeyBaseInfo(key, node, manager);
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/mapId", manager);
			(key as SSInfo.CellKey).MapId = (int.TryParse(valueT, out value))?value:0;
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/name", manager);
			(key as SSInfo.CellKey).Name = valueT;
			break;

		case SSInfo.AnimationKeyType.PosX:
		case SSInfo.AnimationKeyType.PosY:
		case SSInfo.AnimationKeyType.RotZ:
		case SSInfo.AnimationKeyType.ScaleX:
		case SSInfo.AnimationKeyType.ScaleY:
		case SSInfo.AnimationKeyType.Alpha:
			key = new SSInfo.CommonKey<float>();
			SetKeyBaseInfo(key, node, manager);
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value", manager);
			(key as SSInfo.CommonKey<float>).Value = float.Parse(valueT);
			break;

		case SSInfo.AnimationKeyType.Priorty:
			key = new SSInfo.CommonKey<int>();
			SetKeyBaseInfo(key, node, manager);
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value", manager);
			(key as SSInfo.CommonKey<int>).Value = int.Parse(valueT);
			break;

		case SSInfo.AnimationKeyType.FlipH:
		case SSInfo.AnimationKeyType.FlipV:
		case SSInfo.AnimationKeyType.Hide:
			key = new SSInfo.CommonKey<bool>();
			SetKeyBaseInfo(key, node, manager);
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value", manager);
			(key as SSInfo.CommonKey<bool>).Value = (int.Parse(valueT) == 1);
			break;
			
		case SSInfo.AnimationKeyType.User:
			key = new SSInfo.UserKey();
			SetKeyBaseInfo(key, node, manager);
			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/integer", manager);
			if(!string.IsNullOrEmpty(valueT))
			{
				(key as SSInfo.UserKey).UseFlag |= SSInfo.UserDataFlag.Integer;
				(key as SSInfo.UserKey).Integer = int.Parse(valueT);
			}

			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/rect", manager);
			if(!string.IsNullOrEmpty(valueT))
			{
				(key as SSInfo.UserKey).UseFlag |= SSInfo.UserDataFlag.Rect;
				splitItem = valueT.Split(' ');
				float l = float.Parse(splitItem[0]);
				float t = float.Parse(splitItem[1]);
				float w = float.Parse(splitItem[2])-l;
				float h = float.Parse(splitItem[3])-t;
				(key as SSInfo.UserKey).Rectangle = new Rect(l,t,w,h);
			}

			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/point", manager);
			if(!string.IsNullOrEmpty(valueT))
			{
				(key as SSInfo.UserKey).UseFlag |= SSInfo.UserDataFlag.Point;
				splitItem = valueT.Split(' ');
				(key as SSInfo.UserKey).Point = new Vector2(float.Parse(splitItem[0]),float.Parse(splitItem[1]));
			}

			valueT = XMLUtility.TextGetSelectSingleNode(node, "value/string", manager);
			if(!string.IsNullOrEmpty(valueT))
			{
				(key as SSInfo.UserKey).UseFlag |= SSInfo.UserDataFlag.String;
				(key as SSInfo.UserKey).String = valueT;
			}

			break;
		default:break;
		}

		return key;
	}

	void SetKeyBaseInfo(SSInfo.AbstractKey key, XmlNode node, XmlNamespaceManager manager)
	{
		key.Time = int.Parse(node.Attributes["time"].Value);
		var attr = node.Attributes["ipType"];
		key.Interpolation = SSInfo.InterpolationType.None;
		if(attr != null)
		{
			switch(attr.Value)
			{
			case "linear":key.Interpolation = SSInfo.InterpolationType.Liner;
				break;
			case "bezier":key.Interpolation = SSInfo.InterpolationType.Bezier;
				break;
			case "hermite":key.Interpolation = SSInfo.InterpolationType.Hermite;
				break;
			}
		}
		if(key.Interpolation == SSInfo.InterpolationType.Bezier || key.Interpolation == SSInfo.InterpolationType.Hermite)
		{
			string valueT = XMLUtility.TextGetSelectSingleNode(node, "curve", manager);
			string[] split = valueT.Split(' ');
			if(split != null)
			{
				key.LeftCtrlVec = new Vector2(float.Parse(split[0]),float.Parse(split[1]));
				key.RightCtrlVec = new Vector2(float.Parse(split[2]),float.Parse(split[3]));
			}
		}
	}
}
