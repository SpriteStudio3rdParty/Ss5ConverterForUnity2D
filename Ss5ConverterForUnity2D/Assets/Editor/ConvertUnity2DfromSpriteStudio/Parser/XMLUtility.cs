using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public static class XMLUtility
{
	public static bool ValueGetBool<_Type>(_Type Source)
	{
		return((0 != ValueGetInt(Source)) ? true : false);
	}
	
	public static byte ValueGetByte<_Type>(_Type Source)
	{
		return(System.Convert.ToByte(Source));
	}
	
	public static int ValueGetInt<_Type>(_Type Source)
	{
		return(System.Convert.ToInt32(Source));
	}
	
	public static uint ValueGetUInt<_Type>(_Type Source)
	{
		return(System.Convert.ToUInt32(Source));
	}
	
	public static float ValueGetFloat<_Type>(_Type Source)
	{
		return(System.Convert.ToSingle(Source));
	}
	
	public static double ValueGetDouble<_Type>(_Type Source)
	{
		return(System.Convert.ToDouble(Source));
	}
	
	public static bool TextToBool(string Text)
	{
		bool ret = false;
		try {
			ret = System.Convert.ToBoolean(Text);
		}catch(System.FormatException){
			int i = System.Convert.ToInt32(Text);
			ret = ((0 == i) ? (false) : (true));
		}
		return(ret);
	}
	
	public static int TextHexToInt(string Text)
	{
		return(System.Convert.ToInt32(Text, 16));
	}
	
	public static uint TextHexToUInt(string Text)
	{
		return(System.Convert.ToUInt32(Text, 16));
	}
	
	public static XmlNode XML_SelectSingleNode(XmlNode Node, string NamePath, XmlNamespaceManager Manager)
	{
		return(Node.SelectSingleNode(NamePath, Manager));
	}
	
	public static string TextGetSelectSingleNode(XmlNode Node, string NamePath, XmlNamespaceManager Manager)
	{
		XmlNode NodeNow = XML_SelectSingleNode(Node, NamePath, Manager);
		return((null != NodeNow) ? NodeNow.InnerText : null);
	}
	
	public static XmlNodeList XML_SelectNodes(XmlNode Node, string NamePath, XmlNamespaceManager Manager)
	{
		return(Node.SelectNodes(NamePath, Manager));
	}
	
	public static int VersionGetHexCode(string Text)
	{
		string[] Item = Text.Split('.');
		if (3 != Item.Length)
		{
			return(-1);
		}
		
		int VersionMajor = TextHexToInt(Item[0]);
		int VersionMinor = TextHexToInt(Item[1]);
		int Revision = TextHexToInt(Item[2]);
		return((VersionMajor << 16) | (VersionMinor << 8) | Revision);
	}
	
	public static string VersionGetString(int VersionCode)
	{
		int VersionMajor = (VersionCode >> 16) & 0xff;
		if (0 == VersionMajor)
		{
			return(null);
		}
		int VersionMinor = (VersionCode >> 8) & 0xff;
		int Revision = (VersionCode & 0xff);
		return(System.String.Format("{0:X}.{1:X2}.{2:X2}", VersionMajor, VersionMinor, Revision));
	}
}

