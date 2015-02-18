using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// .sspjファイルの読み込みクラス
/// </summary>
public class SSPJLoader
{
	/// <summary>
	/// .sspjファイルの読み込み
	/// </summary>
	/// <returns><c>true</c>読み込み成功<c>false</c>読み込み失敗</returns>
	/// <param name="filepath">.sspjファイルのファイルパス</param>
	/// <param name="info">読み込みデータの格納先</param>
	public bool OpenRead(string filepath, out SSInfo.SSPJInfo info)
	{
		info = new SSInfo.SSPJInfo();
		XmlDocument DataXML = new XmlDocument();
		DataXML.Load(filepath);
		
		//info = null;
		XmlNode NodeRoot = DataXML.FirstChild;
		NodeRoot = NodeRoot.NextSibling;

		NameTable NodeNameSpace = new NameTable();
		XmlNamespaceManager ManagerNameSpace = new XmlNamespaceManager(NodeNameSpace);
		XmlNodeList NodeList = null;
		string valueT = "";

		//	
		valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "name", ManagerNameSpace);
		if(!string.IsNullOrEmpty(valueT))
		{
			info.Name = valueT;
		}

		NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "cellmapNames/value", ManagerNameSpace);
		if(null == NodeList)
		{
			Debug.LogError("SSPJLoader: Error!: CellMapNameList-Node Not-Found");
			return false;
		}

		foreach(XmlNode NodeNameCellMap in NodeList)
		{
			string fileName = NodeNameCellMap.InnerText;
			info.CellmapNames.Add(fileName);
		}
		
		NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "animepackNames/value", ManagerNameSpace);
		if(null == NodeList)
		{
			Debug.LogError("SSPJLoader: Error!: AnimepackNameList-Node Not-Found");
			return false;
		}

		foreach(XmlNode NodeNameAnimepack in NodeList)
		{
			string fileName = NodeNameAnimepack.InnerText;
			info.AnimepackNames.Add(fileName);
		}
		
		return true;
	}
}

