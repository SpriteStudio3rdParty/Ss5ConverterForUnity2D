using System;

/// <summary>
/// スプライトスタジオ読み込みクラス
/// </summary>
/// <remarks>
/// .sspjに付随する.ssce.ssaeファイルの読み取りを行う
/// </remarks>
/// <example>
/// 使い方
/// Infomationから各データにアクセス
/// <code>
/// SpriteStudioLoader loader = new SpriteStudioLoader();
/// loader.LoadFromSSPJ("test.sspj");
/// 
/// loader.Infomation
/// </code>
/// </example>
public class SpriteStudioLoader
{
	SSPJLoader _sspjLoader = new SSPJLoader();
	SSCELoader _ssceLoader = new SSCELoader();
	SSAELoader _ssaeLoader = new SSAELoader();

	/// <summary>
	///  .sspj.ssce.ssaeファイルの情報
	/// </summary>
	public SSInfo Infomation;
	/// <summary>
	/// .sspjファイルの読み込み
	/// </summary>
	/// <returns><c>true</c>読み込み成功<c>false</c>読み込み失敗</returns>
	/// <param name="filepath">.sspjファイルのファイルパス</param>
	public bool LoadFromSSPJ(string filepath)
	{
		string dir = filepath.Substring(0,filepath.LastIndexOf("/")+1);
		Infomation = new SSInfo();
		
		//	load sspj
		_sspjLoader.OpenRead(filepath, out Infomation.ProjectInfo);

		//	load ssce
		for(int i = 0;i<Infomation.ProjectInfo.CellmapNames.Count;i++)
		{
			string file = Infomation.ProjectInfo.CellmapNames[i];
			SSInfo.SSCEInfo ssce = null;
			_ssceLoader.OpenRead(dir+file,out ssce);
			Infomation.CellInfoList.Add(ssce);
		}

		//	load ssae
		for(int i = 0;i<Infomation.ProjectInfo.AnimepackNames.Count;i++)
		{
			string file = Infomation.ProjectInfo.AnimepackNames[i];
			SSInfo.SSAEInfo ssae = null;
			_ssaeLoader.OpenRead(dir+file,out ssae);
			Infomation.AnimationInfoList.Add(ssae);
		}

		//	zero base order sort
		int[] minOrderSort = new int[Infomation.AnimationInfoList.Count];
		int ssaeCount = 0;
		foreach(var animInfo in Infomation.AnimationInfoList)
		{
			foreach(var anim in animInfo.Animes)
			{
				foreach(var part in anim.AnimeParts)
				{
					foreach( var keys in part.KeysList)
					{
						if(keys.Type != SSInfo.AnimationKeyType.Priorty)continue;
						foreach(var key in keys.Keys)
						{
							int order = (key as SSInfo.CommonKey<int>).Value;
							if(order < minOrderSort[ssaeCount])minOrderSort[ssaeCount] = order;
						}
					}
				}
			}
			ssaeCount++;
		}
		ssaeCount = 0;
		foreach(var animInfo in Infomation.AnimationInfoList)
		{
			if(minOrderSort[ssaeCount] >=0)continue;
			foreach(var anim in animInfo.Animes)
			{
				foreach(var part in anim.AnimeParts)
				{
					foreach( var keys in part.KeysList)
					{
						if(keys.Type != SSInfo.AnimationKeyType.Priorty)continue;
						foreach(var key in keys.Keys)
						{
							int orderDiff = Math.Abs(minOrderSort[ssaeCount]);
							(key as SSInfo.CommonKey<int>).Value += orderDiff;
						}
					}
				}
			}
			ssaeCount++;
		}
		
		return true;
	}
}


