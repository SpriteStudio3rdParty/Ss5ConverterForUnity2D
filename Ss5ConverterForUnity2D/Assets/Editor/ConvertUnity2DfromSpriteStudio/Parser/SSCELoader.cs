using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// .ssceファイルの読み込みクラス
/// </summary>
public class SSCELoader
{
    /// <summary>
    /// .ssceファイルの読み込み
    /// </summary>
    /// <returns><c>true</c>読み込み成功<c>false</c>読み込み失敗</returns>
    /// <param name="filepath">.ssceファイルのファイルパス</param>
    /// <param name="info">読み込みデータの格納先</param>
    public bool OpenRead(string filepath, out SSInfo.SSCEInfo info)
    {
        info = new SSInfo.SSCEInfo();
        info.FileName = Path.GetFileName(filepath);
        XmlDocument DataXML = new XmlDocument();
        DataXML.Load(filepath);
		
        XmlNode NodeRoot = DataXML.FirstChild;
        NodeRoot = NodeRoot.NextSibling;
		
        NameTable NodeNameSpace = new NameTable();
        XmlNamespaceManager ManagerNameSpace = new XmlNamespaceManager(NodeNameSpace);
        XmlNodeList NodeList = null;
        string valueT = "";
		
        //	
        valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "name", ManagerNameSpace);
        if (!string.IsNullOrEmpty(valueT))
        {
            info.Name = valueT;
        }

        valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "imagePath", ManagerNameSpace);
        if (!string.IsNullOrEmpty(valueT))
        {
            info.ImagePath = valueT;
        }

        valueT = XMLUtility.TextGetSelectSingleNode(NodeRoot, "pixelSize", ManagerNameSpace);
        if (!string.IsNullOrEmpty(valueT))
        {
            string[] pixels = valueT.Split(' ');
            info.PixelSizeW = int.Parse(pixels[0]);
            info.PixelSizeH = int.Parse(pixels[1]);
        }


        NodeList = XMLUtility.XML_SelectNodes(NodeRoot, "cells/cell", ManagerNameSpace);
        if (null == NodeList)
        {
            Debug.LogError("SSCELoader: Error!: CellList-Node Not-Found");
            return false;
        }

        string itemText = null;
        string[] itemTextSprit = null;
        double pivotNormalizeX = 0.0;
        double pivotNormalizeY = 0.0;
        foreach (XmlNode NodeCell in NodeList)
        {
            SSInfo.SSCEInfo.CellData cell = new SSInfo.SSCEInfo.CellData();
            valueT = XMLUtility.TextGetSelectSingleNode(NodeCell, "name", ManagerNameSpace);
            if (!string.IsNullOrEmpty(valueT))
            {
                cell.Name = valueT;
            }

            itemText = XMLUtility.TextGetSelectSingleNode(NodeCell, "pos", ManagerNameSpace);
            itemTextSprit = itemText.Split(' ');
            cell.Pos.x = (float) (XMLUtility.ValueGetInt(itemTextSprit[0]));
            cell.Pos.y = (float) (XMLUtility.ValueGetInt(itemTextSprit[1]));
			
            itemText = XMLUtility.TextGetSelectSingleNode(NodeCell, "size", ManagerNameSpace);
            itemTextSprit = itemText.Split(' ');
            cell.Size.x = (float) (XMLUtility.ValueGetInt(itemTextSprit[0]));
            cell.Size.y = (float) (XMLUtility.ValueGetInt(itemTextSprit[1]));
			
            itemText = XMLUtility.TextGetSelectSingleNode(NodeCell, "pivot", ManagerNameSpace);
            itemTextSprit = itemText.Split(' ');
            pivotNormalizeX = XMLUtility.ValueGetDouble(itemTextSprit[0]);
            pivotNormalizeY = XMLUtility.ValueGetDouble(itemTextSprit[1]);
            cell.Pivot.x = (float) ((double) cell.Size.x * (pivotNormalizeX + 0.5));
            cell.Pivot.y = (float) ((double) cell.Size.y * (-pivotNormalizeY + 0.5));
			
            itemText = XMLUtility.TextGetSelectSingleNode(NodeCell, "rotated", ManagerNameSpace);
            cell.Rotate = XMLUtility.ValueGetInt(itemText);
			
            info.Cells.Add(cell);
        }
		
        return true;
    }
}

