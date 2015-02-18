using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

/// <summary>
/// スプライトスタジオのファイル情報クラス
/// </summary>
/// <remarks>
/// スプライトスタジオの.sspj.ssce.ssaeファイル情報の格納
/// </remarks>
public partial class SSInfo
{
	/// <summary>
	/// アニメーションキータイプ
	/// </summary>
	/// <remarks>
	/// 未対応のキータイプがあります。未対応のキーを使用した場合
	/// UnKnownタイプのキーとなり登録されません。
	/// </remarks>
	public enum AnimationKeyType
	{
		/// <summary>
		/// 不明
		/// </summary>
		UnKnown,
		/// <summary>
		/// セルマップ参照
		/// </summary>
		Cell,
		/// <summary>
		/// 位置X
		/// </summary>
		PosX,
		/// <summary>
		/// 位置Y
		/// </summary>
		PosY,
		/// <summary>
		/// 回転Z
		/// </summary>
		RotZ,
		/// <summary>
		/// スケールX
		/// </summary>
		ScaleX,
		/// <summary>
		/// スケールY
		/// </summary>
		ScaleY,
		/// <summary>
		/// 透明度
		/// </summary>
		Alpha,
		/// <summary>
		/// 描画優先度
		/// </summary>
		Priorty,
		/// <summary>
		/// 左右反転
		/// </summary>
		FlipH,
		/// <summary>
		/// 上下反転
		/// </summary>
		FlipV,
		/// <summary>
		/// 非表示
		/// </summary>
		Hide,
		/// <summary>
		/// ユーザーデータ
		/// </summary>
		User,
		/// <summary>
		/// カラーブレンド
		/// </summary>
		VColor,
		/*VDeform,
		VPosX,
		VPosY,
		VSizeX,
		VSizeY,
		IFlip*/
	}

	/// <summary>
	/// 補間タイプ
	/// </summary>
	public enum InterpolationType
	{
		/// <summary>
		/// なし
		/// </summary>
		None,
		/// <summary>
		/// 線形
		/// </summary>
		Liner,
		/// <summary>
		/// ベジェ
		/// </summary>
		Bezier,
		/// <summary>
		/// エルミート
		/// </summary>
		Hermite,
	}

	/// <summary>
	/// 頂点カラータイプ
	/// </summary>
	public enum ColorBlendType
	{
		/// <summary>
		/// 単色
		/// </summary>
		Whole,
		/// <summary>
		/// 頂点単位
		/// </summary>
		Vertex,
	}

	/// <summary>
	/// ユーザーデータ使用確認フラグ
	/// </summary>
	[Flags]
	public enum UserDataFlag
	{
		/// <summary>
		/// 整数データの使用
		/// </summary>
		Integer = 0x01,
		/// <summary>
		/// 位置データの使用
		/// </summary>
		Point = 0x02,
		/// <summary>
		/// 矩形データの使用
		/// </summary>
		Rect = 0x04,
		/// <summary>
		/// 文字列データの使用
		/// </summary>
		String = 0x08,
	}
	/// <summary>
	/// アニメーションキー抽象クラス
	/// </summary>
	public abstract class AbstractKey
	{
		/// <summary>
		/// フレーム数
		/// </summary>
		public int Time;
		/// <summary>
		/// 補間タイプ
		/// </summary>
		public InterpolationType Interpolation;
		/// <summary>
		/// 左制御ベクトル
		/// </summary>
		public Vector2 LeftCtrlVec;
		/// <summary>
		/// 右制御ベクトル
		/// </summary>
		public Vector2 RightCtrlVec;
	}
	/// <summary>
	/// 汎用アニメーションキークラス
	/// </summary>
	public class CommonKey<T> : AbstractKey
	{
		/// <summary>
		/// キーフレームに付随する値情報
		/// </summary>
		public T Value;
	}
	/// <summary>
	/// セルマップ切り替えキークラス
	/// </summary>
	public class CellKey : AbstractKey
	{
		/// <summary>
		/// 参照しているセルリスト内ID
		/// </summary>
		public int MapId;
		/// <summary>
		/// 参照しているセルリスト内パーツ名
		/// </summary>
		public string Name;
	}
	/// <summary>
	/// カラー変更キークラス
	/// </summary>
	public class ColorKey : AbstractKey
	{
		/// <summary>
		/// 頂点カラータイプ（塗り方）
		/// </summary>
		public ColorBlendType ColorType;
		/// <summary>
		/// アルファブレンドタイプ
		/// </summary>
		public BlendType BlendType;
		/// <summary>
		/// 色
		/// </summary>
		public Color Color;
	}
	/// <summary>
	/// ユーザーデータキークラス
	/// </summary>
	public class UserKey : AbstractKey
	{
		/// <summary>
		/// データ使用フラグ
		/// </summary>
		public UserDataFlag UseFlag;
		/// <summary>
		/// 整数データ
		/// </summary>
		public int Integer;
		/// <summary>
		/// 位置データ
		/// </summary>
		public Vector2 Point;
		/// <summary>
		/// 矩形データ
		/// </summary>
		public Rect Rectangle;
		/// <summary>
		/// 文字列データ
		/// </summary>
		public string String;
	}
	/// <summary>
	/// 各キータイプごとに格納するためのキーリスト
	/// </summary>
	public class KeyList
	{
		/// <summary>
		/// キーリストに格納するキータイプ
		/// </summary>
		public AnimationKeyType Type;
		/// <summary>
		/// 格納されるキー
		/// </summary>
		public List<AbstractKey> Keys = new List<AbstractKey>();
	}
}