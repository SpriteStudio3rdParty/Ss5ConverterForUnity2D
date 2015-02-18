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
    /// 合成タイプ
    /// </summary>
    public enum BlendType
    {
        /// <summary>
        /// 通常合成
        /// </summary>
        Mix,
        /// <summary>
        /// 加算合成
        /// </summary>
        Add,
        /// <summary>
        /// 減算合成
        /// </summary>
        Sub,
        /// <summary>
        /// 乗算合成
        /// </summary>
        Mul,
    }
    /// <summary>
    /// パーツタイプ
    /// </summary>
    public enum PartsType
    {
        /// <summary>
        /// NULLパーツ
        /// </summary>
        Null = 0,
        /// <summary>
        /// 通常パーツ
        /// </summary>
        Normal,
    }

    /// <summary>
    /// SSPJファイル情報
    /// </summary>
    public class SSPJInfo
    {
        /// <summary>
        /// .sspjファイル名
        /// </summary>
        public string Name;
        /// <summary>
        /// セルマップ名リスト
        /// </summary>
        public List<string> CellmapNames = new List<string>();
        /// <summary>
        /// アニメーション名リスト
        /// </summary>
        public List<string> AnimepackNames = new List<string>();
    }

    /// <summary>
    /// SSCEファイル情報
    /// </summary>
    public class SSCEInfo
    {
        /// <summary>
        /// セルマップパーツデータ
        /// </summary>
        public struct CellData
        {
            /// <summary>
            /// パーツ名
            /// </summary>
            public string Name;
            /// <summary>
            /// 切り取り位置
            /// </summary>
            public Vector2 Pos;
            /// <summary>
            /// 切り取りサイズ
            /// </summary>
            public Vector2 Size;
            /// <summary>
            /// 回転位置
            /// </summary>
            public Vector2 Pivot;
            /// <summary>
            /// 回転量
            /// </summary>
            public float Rotate;
        }
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName;
        /// <summary>
        /// セルマップ名
        /// </summary>
        public string Name;
        /// <summary>
        /// テクスチャーファイルパス
        /// </summary>
        public string ImagePath;
        /// <summary>
        /// テクスチャーサイズ幅
        /// </summary>
        public int PixelSizeW;
        /// <summary>
        /// テクスチャーサイズ高さ
        /// </summary>
        public int PixelSizeH;
        /// <summary>
        /// セルマップパーツリスト
        /// </summary>
        public List<CellData> Cells = new List<CellData>();
    }

    /// <summary>
    /// SSAEファイル情報
    /// </summary>
    public class SSAEInfo
    {
        /// <summary>
        /// パーツ
        /// </summary>
        public struct Part
        {
            /// <summary>
            /// パーツ名
            /// </summary>
            public string Name;
            /// <summary>
            /// パーツタイプ
            /// </summary>
            public PartsType Type;
            /// <summary>
            /// パーツインデックス
            /// </summary>
            public int Index;
            /// <summary>
            /// 親パーツインデックス
            /// </summary>
            public int ParentIndex;
            /// <summary>
            /// 合成タイプ
            /// </summary>
            public BlendType Blend;
        }
        /// <summary>
        /// アニメーションパーツ
        /// </summary>
        public class AnimePart
        {
            /// <summary>
            /// 参照しているパーツ名
            /// </summary>
            public string PartName;
            /// <summary>
            /// アニメーションキーリスト
            /// </summary>
            public List<KeyList> KeysList = new List<KeyList>();
        }
        /// <summary>
        /// アニメーション
        /// </summary>
        public class Anime
        {
            /// <summary>
            /// アニメーション名
            /// </summary>
            public string Name;
            /// <summary>
            /// アニメーションパーツリスト
            /// </summary>
            public List<AnimePart> AnimeParts = new List<AnimePart>();
        }
        /// <summary>
        /// SSAE名
        /// </summary>
        public string Name;
        /// <summary>
        /// FPS
        /// </summary>
        public int Fps;
        /// <summary>
        /// 最大フレーム数
        /// </summary>
        public int FrameCount;
        /// <summary>
        /// 参照しているセルマップネームリスト
        /// </summary>
        /// <remarks>
        /// バージョン４まではセルマップは１つしか指定できない
        /// バージョン５以降の場合はセルマップを複数指定できる
        /// </remarks>
        public List<string> CellmapNames = new List<string>();
        /// <summary>
        /// アニメーション対象パーツリスト
        /// </summary>
        public List<Part> Parts = new List<Part>();
        /// <summary>
        /// アニメーションリスト
        /// </summary>
        public List<Anime> Animes = new List<Anime>();
    }
    /// <summary>
    /// プロジェクト情報
    /// </summary>
    public SSPJInfo ProjectInfo = new SSPJInfo();
    /// <summary>
    /// セルマップファイル情報リスト
    /// </summary>
    public List<SSCEInfo> CellInfoList = new List<SSCEInfo>();
    /// <summary>
    /// アニメーションファイル情報リスト
    /// </summary>
    public List<SSAEInfo> AnimationInfoList = new List<SSAEInfo>();
}


