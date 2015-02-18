using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ConvertUnity2D;

//Sprite Studio 5 で作成したアニメーションをUnity2Dで再生するためのコンバーターです。
//プロジェクトにあるsspjファイルを右クリックし、sspj Convert for Unity2Dを選択してください。
//Assets/Unity2d/sspj名/のフォルダにコンバートしたアニメーションデータが出力されます。
public sealed class CreateUnity2DSpriteFromSpriteStudio : EditorWindow
{
	public class CellEventTargetItem
	{
		public string name = "";
		public bool childAlso = true;
	}


	static string OutputRootPath = "Assets/Unity2d/";

    static float scale = 1.0f;
    static float defaultFps = 30.0f;
    static float defaultPivot = 0.5f;
    static string fullPath = Application.dataPath + "/../";
    static bool IsRemoveSpriteKeys = false;
    static List<CellEventTargetItem> cellEventTargets = new List<CellEventTargetItem>();

	//コンバートメニュー処理
    [MenuItem("Assets/sspj Convert for Unity2D")]
	static void ConvertUnity2d()
    {
		//リスト初期化
        IsRemoveSpriteKeys = false;
        cellEventTargets.Clear();

		//出力フォルダ名作成
        string sspjPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string assetPath = System.IO.Path.GetDirectoryName(sspjPath) + "/";
        
        SpriteStudioLoader loader = new SpriteStudioLoader();
        loader.LoadFromSSPJ(fullPath + sspjPath);
        
        string pjName = System.IO.Path.GetFileName(sspjPath).Replace(".sspj", "");
        string outputPath = OutputRootPath + pjName + "/";

        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(outputPath);
        if (!di.Exists)
        {
			//フォルダ作成
            System.IO.Directory.CreateDirectory(outputPath);
        }
        
		//テクスチャの情報作成と出力フォルダへコピー
        Dictionary<string, List<Sprite>> sprites = CreateMultiSpriteTexture(loader.Infomation.CellInfoList, assetPath, outputPath);

        foreach (SSInfo.SSAEInfo ssae in loader.Infomation.AnimationInfoList)
        {
            // prefabの作成
            List<Transform> transforms = CreatePrefab(ssae.Parts);

			//Unity2D用アニメファイル作成
            foreach (SSInfo.SSAEInfo.Anime anime in ssae.Animes)
            {
                // create animation
				string AnimationClipName = outputPath + ssae.Name + "_" + anime.Name + ".anim";
				AnimationClip animationClip = CreateAnimationClip(ssae, anime.AnimeParts, transforms, sprites, AnimationClipName);

                // save animation
                animationClip.EnsureQuaternionContinuity();
                EditorUtility.SetDirty(animationClip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            // save prefab
            SavePrefab(ssae.Name, outputPath, transforms);
        }
    }

	//prefabの作成
	public static List<Transform> CreatePrefab(List<SSInfo.SSAEInfo.Part> parts)
    {
        List<Transform> transforms = new List<Transform>();

        foreach (SSInfo.SSAEInfo.Part part in parts)
        {
            GameObject child = new GameObject(part.Name);
			
            child.AddComponent<PartDitector>();
            SpriteRenderer spriteRenderer = child.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = part.Index;
            spriteRenderer.enabled = (part.Type == SSInfo.PartsType.Normal);
            
            transforms.Add(child.transform);
            
            if (part.ParentIndex != -1)
            {
                child.transform.parent = transforms[part.ParentIndex];
            }
        }
        return transforms;
    }

	//アニメデータの初期値を設定
	static void Set_Ainmeclip_Initparam(ref SpriteRenderer renderer, ref PartDitector partDitector)
	{
		renderer.sortingOrder = 20;				//プライオリティの初期化を設定
		partDitector.orderInLayer = 20;
	}
	//アニメデータの作成
	public static AnimationClip CreateAnimationClip(SSInfo.SSAEInfo ssae, List<SSInfo.SSAEInfo.AnimePart> animeParts, List<Transform> transforms, Dictionary<string ,List<Sprite>> sprites, string animPath)
    {  
        AnimationClip animationClip = (AnimationClip) AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip));
        
        if (animationClip == null)
        {
            animationClip = new AnimationClip();
            AssetDatabase.CreateAsset(animationClip, animPath);
        }

        animationClip.ClearCurves();
//        AnimationClip animationClip = new AnimationClip();
        AnimationUtility.SetAnimationType(animationClip, ModelImporterAnimationType.Generic);



        animationClip.frameRate = (ssae.Fps != 0) ? (float) ssae.Fps : defaultFps;
        
		foreach (SSInfo.SSAEInfo.AnimePart part in animeParts)
        {
            Transform transform = transforms.Find((x) => x.name == part.PartName);
            string path = GetPath(transform);
            SpriteRenderer renderer = transform.GetComponent<SpriteRenderer>();
            PartDitector partDitector = transform.GetComponent<PartDitector>();
            SetDummyAnimation(path, animationClip);

			foreach (SSInfo.KeyList keyList in part.KeysList)
            {
                if (keyList.Type == SSInfo.AnimationKeyType.Cell && IsRemoveSpriteKeys)
                {
                    continue;
                }

                switch (keyList.Type)
                {
                    case SSInfo.AnimationKeyType.PosX:
                        SetPositonXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.PosY:
                        SetPositonYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.RotZ:
                        SetRotateAnimation(path, animationClip, keyList.Keys, transform, partDitector);
                        break;
                        
                    case SSInfo.AnimationKeyType.ScaleX:
                        SetScaleXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.ScaleY:
                        SetScaleYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.Alpha:
                        SetAlphaAnimation(path, animationClip, keyList.Keys, renderer);
                        break;
                        
                    case SSInfo.AnimationKeyType.Hide:
                        SetHideAnimation(path, animationClip, keyList.Keys, transform.gameObject);
                        break;
                        
                    case SSInfo.AnimationKeyType.Priorty:
                        SetPriortyAnimation(path, animationClip, keyList.Keys, renderer, partDitector);
                        break;
                        
                    case SSInfo.AnimationKeyType.FlipH:
                        
                        break;
                        
                    case SSInfo.AnimationKeyType.FlipV:
                        
                        break;
                        
                    case SSInfo.AnimationKeyType.Cell:
                        SetCellAnimation(path, animationClip, keyList.Keys, renderer, sprites, ssae);
                        break;
                        
                    case SSInfo.AnimationKeyType.User:
                        SetUserData(animationClip, keyList.Keys);
                        break;
                }
            }
        }

        return animationClip;
    }

    public static AnimationClip CreateAnimationClip(SSInfo.SSAEInfo ssae, List<SSInfo.SSAEInfo.AnimePart> animeParts, List<Transform> transforms, List<Sprite> sprites, string animPath)
    {
        AnimationClip animationClip = (AnimationClip) AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip));
        
        if (animationClip == null)
        {
            animationClip = new AnimationClip();
            AssetDatabase.CreateAsset(animationClip, animPath);
        }
        
        animationClip.ClearCurves();
        AnimationUtility.SetAnimationType(animationClip, ModelImporterAnimationType.Generic);
        animationClip.frameRate = (ssae.Fps != 0) ? (float) ssae.Fps : defaultFps;
        
		foreach (SSInfo.SSAEInfo.AnimePart part in animeParts)
        {
            Transform transform = transforms.Find((x) => x.name == part.PartName);
            string path = GetPath(transform);
            SpriteRenderer renderer = transform.GetComponent<SpriteRenderer>();
            PartDitector partDitector = transform.GetComponent<PartDitector>();
            
            SetDummyAnimation(path, animationClip);

			//初期値を設定
			Set_Ainmeclip_Initparam(ref renderer, ref partDitector);

            foreach (SSInfo.KeyList keyList in part.KeysList)
            {
                if (keyList.Type == SSInfo.AnimationKeyType.Cell && IsRemoveSpriteKeys)
                {
                    continue;
                }

                switch (keyList.Type)
                {
                    case SSInfo.AnimationKeyType.PosX:
                        SetPositonXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.PosY:
                        SetPositonYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.RotZ:
                        SetRotateAnimation(path, animationClip, keyList.Keys, transform, partDitector);
                        break;
                        
                    case SSInfo.AnimationKeyType.ScaleX:
                        SetScaleXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.ScaleY:
                        SetScaleYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
                        
                    case SSInfo.AnimationKeyType.Alpha:
                        SetAlphaAnimation(path, animationClip, keyList.Keys, renderer);
                        break;
                        
                    case SSInfo.AnimationKeyType.Hide:
                        SetHideAnimation(path, animationClip, keyList.Keys, transform.gameObject);
                        break;
                        
                    case SSInfo.AnimationKeyType.Priorty:
                        SetPriortyAnimation(path, animationClip, keyList.Keys, renderer, partDitector);
                        break;
                        
                    case SSInfo.AnimationKeyType.FlipH:
                        
                        break;
                        
                    case SSInfo.AnimationKeyType.FlipV:
                        
                        break;
                        
                    case SSInfo.AnimationKeyType.Cell:
                        SetCellAnimation(path, animationClip, keyList.Keys, renderer, sprites);
                        break;
                        
                    case SSInfo.AnimationKeyType.User:
                        break;
                }
            }
        }
        
        return animationClip;
    }

    public static AnimationClip CreateAnimationClip(SSInfo.SSAEInfo ssae, List<SSInfo.SSAEInfo.AnimePart> animeParts, List<Transform> transforms, Dictionary<string ,List<Sprite>> sprites, List<string> filterlist, string animPath)
    {
        AnimationClip animationClip = (AnimationClip) AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip));
        
        if (animationClip == null)
        {
            animationClip = new AnimationClip();
            AssetDatabase.CreateAsset(animationClip, animPath);
        }
        
        animationClip.ClearCurves();
        AnimationUtility.SetAnimationType(animationClip, ModelImporterAnimationType.Generic);
        animationClip.frameRate = (ssae.Fps != 0) ? (float) ssae.Fps : defaultFps;
        List<AnimationEvent> animationEvents = new List<AnimationEvent>();

        foreach (SSInfo.SSAEInfo.AnimePart part in animeParts)
        {
            Transform transform = transforms.Find((x) => x.name == part.PartName);
            string path = GetPath(transform);
            SpriteRenderer renderer = transform.GetComponent<SpriteRenderer>();
            PartDitector partDitector = transform.GetComponent<PartDitector>();
            SetDummyAnimation(path, animationClip);

			//初期値を設定
			Set_Ainmeclip_Initparam(ref renderer, ref partDitector);

			foreach (SSInfo.KeyList keyList in part.KeysList)
            {
                if (keyList.Type == SSInfo.AnimationKeyType.Cell)
                {
                    string find = filterlist.Find((x) => x == transform.name);
                    if (!string.IsNullOrEmpty(find))
                    {
                        SetSpriteChangeEvent(animationClip, keyList.Keys, animationEvents, find);
                        continue;
                    }
                }
                if (keyList.Type == SSInfo.AnimationKeyType.Cell && IsRemoveSpriteKeys)
                {
                    continue;
                }
				
                switch (keyList.Type)
                {
                    case SSInfo.AnimationKeyType.PosX:
                        SetPositonXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
					
                    case SSInfo.AnimationKeyType.PosY:
                        SetPositonYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
					
                    case SSInfo.AnimationKeyType.RotZ:
                        SetRotateAnimation(path, animationClip, keyList.Keys, transform, partDitector);
                        break;
					
                    case SSInfo.AnimationKeyType.ScaleX:
                        SetScaleXAnimation(path, animationClip, keyList.Keys, transform);
                        break;
					
                    case SSInfo.AnimationKeyType.ScaleY:
                        SetScaleYAnimation(path, animationClip, keyList.Keys, transform);
                        break;
					
                    case SSInfo.AnimationKeyType.Alpha:
                        SetAlphaAnimation(path, animationClip, keyList.Keys, renderer);
                        break;
					
                    case SSInfo.AnimationKeyType.Hide:
                        SetHideAnimation(path, animationClip, keyList.Keys, transform.gameObject);
                        break;
					
                    case SSInfo.AnimationKeyType.Priorty:
                        SetPriortyAnimation(path, animationClip, keyList.Keys, renderer, partDitector);
                        break;
					
                    case SSInfo.AnimationKeyType.FlipH:
					
                        break;
					
                    case SSInfo.AnimationKeyType.FlipV:
					
                        break;
					
                    case SSInfo.AnimationKeyType.Cell:
                        SetCellAnimation(path, animationClip, keyList.Keys, renderer, sprites, ssae);
                        break;
					
                    case SSInfo.AnimationKeyType.User:
                        SetUserData(animationClip, keyList.Keys, animationEvents);
                        break;
                }
            }
        }
        AnimationUtility.SetAnimationEvents(animationClip, animationEvents.ToArray());
        return animationClip;
    }
    
    //
    static void SetDummyAnimation(string path, AnimationClip clip)
    {
        AnimationCurve dummyScaleCurve = new AnimationCurve();
        dummyScaleCurve.AddKey(0f, 1f);
        clip.SetCurve(path, typeof(Transform), "localScale.x", dummyScaleCurve);
        clip.SetCurve(path, typeof(Transform), "localScale.y", dummyScaleCurve);
    }

    //Ｘ座標出力
    static void SetPositonXAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, Transform transform)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Vector3 pos = transform.localPosition;
                pos.x = (commonKey.Value * scale);
                transform.localPosition = pos;
            }
        }
        AnimationCurve animationCurve = GetPositionAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(Transform), "localPosition.x", animationCurve);
    }

	//Ｙ座標出力
    static void SetPositonYAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, Transform transform)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Vector3 pos = transform.localPosition;
                pos.y = (commonKey.Value * scale);
                transform.localPosition = pos;
            }
        }
        AnimationCurve animationCurve = GetPositionAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", animationCurve);
    }

    // 
    static void SetRotateAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, Transform transform, PartDitector partDitector)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Vector3 eulerAngles = transform.localEulerAngles;
                eulerAngles.z = commonKey.Value;
                transform.localEulerAngles = eulerAngles;
                partDitector.localEulerAngles = eulerAngles;
            }
        }
        AnimationCurve animationCurve = GetAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(PartDitector), "localEulerAngles.z", animationCurve);
    }
    
    //
    static void SetScaleXAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, Transform transform)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Vector3 scale = transform.localScale;
                scale.x = (commonKey.Value);
                transform.localScale = scale;
            }
        }
        AnimationCurve animationCurve = GetAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(Transform), "localScale.x", animationCurve);
    }
    
    //
    static void SetScaleYAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, Transform transform)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Vector3 scale = transform.localScale;
                scale.y = (commonKey.Value);
                transform.localScale = scale;
            }
        }
        AnimationCurve animationCurve = GetAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(Transform), "localScale.y", animationCurve);
    }
    
    //
    static void SetAlphaAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, SpriteRenderer renderer)
    {
		foreach (SSInfo.CommonKey<float> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				Color color = renderer.color;
                color.a = commonKey.Value;
                renderer.color = color;
            }
        }
        AnimationCurve animationCurve = GetAnimationCurve(clip, keys);
        clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", animationCurve);
    }
    
    //
    static void SetHideAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, GameObject gameObject)
    {
		AnimationCurve animationCurve = new AnimationCurve();
        foreach (SSInfo.CommonKey<bool> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
				gameObject.SetActive(!commonKey.Value);
            }

            Keyframe keyframe = new Keyframe((float) commonKey.Time / clip.frameRate, commonKey.Value ? 0f : 1f);
            keyframe.tangentMode = 31;
            keyframe.inTangent = float.PositiveInfinity;
            keyframe.outTangent = float.PositiveInfinity;
            animationCurve.AddKey(keyframe);

        }
        clip.SetCurve(path, typeof(GameObject), "m_IsActive", animationCurve);
    }

    //
    static void SetPriortyAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, SpriteRenderer renderer, PartDitector partDitector)
    {
		AnimationCurve animationCurve = new AnimationCurve();
        foreach (SSInfo.CommonKey<int> commonKey in keys)
        {
            if (commonKey.Time == 0)
			{
                renderer.sortingOrder = commonKey.Value;
                partDitector.orderInLayer = commonKey.Value;
            }

            animationCurve.AddKey((float) commonKey.Time / clip.frameRate, (float) commonKey.Value);
        }
        clip.SetCurve(path, typeof(PartDitector), "orderInLayer", animationCurve);
    }
    
    //
    static void SetCellAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, SpriteRenderer renderer, Dictionary<string,List<Sprite>> sprites, SSInfo.SSAEInfo ssae)
    {
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = path;
        curveBinding.propertyName = "m_Sprite";
        List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
		foreach (SSInfo.CellKey cellKeys in keys)
        {
            if (cellKeys.Time == 0)
			{
				renderer.sprite = sprites[ssae.CellmapNames[cellKeys.MapId]].Find((x) => x.name == cellKeys.Name);
            }
            ObjectReferenceKeyframe keyframe = new ObjectReferenceKeyframe();
            keyframe.time = cellKeys.Time / clip.frameRate;
            keyframe.value = sprites[ssae.CellmapNames[cellKeys.MapId]].Find((x) => x.name == cellKeys.Name);
            keyframes.Add(keyframe);
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes.ToArray());
    }
    
    //
    static void SetCellAnimation(string path, AnimationClip clip, List<SSInfo.AbstractKey> keys, SpriteRenderer renderer, List<Sprite> sprites)
    {
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = path;
        curveBinding.propertyName = "m_Sprite";
        List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
		foreach (SSInfo.CellKey cellKeys in keys)
        {
            if (cellKeys.Time == 0)
			{
				renderer.sprite = sprites.Find((x) => x.name == cellKeys.Name + cellKeys.MapId.ToString());
            }
            ObjectReferenceKeyframe keyframe = new ObjectReferenceKeyframe();
            keyframe.time = cellKeys.Time / clip.frameRate;
            keyframe.value = sprites.Find((x) => x.name == cellKeys.Name + cellKeys.MapId.ToString());
            keyframes.Add(keyframe);
        }
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes.ToArray());
    }

    static void SetUserData(AnimationClip clip, List<SSInfo.AbstractKey> keys)
    {
        List<AnimationEvent> animEvents = new List<AnimationEvent>();
        foreach (var key in keys)
        {
            AnimationEvent animEvent = new AnimationEvent();
            animEvent.time = key.Time / clip.frameRate;

            SSInfo.UserKey ukey = key as SSInfo.UserKey;
            if ((ukey.UseFlag & SSInfo.UserDataFlag.Integer) == SSInfo.UserDataFlag.Integer)
            {
                animEvent.intParameter = ukey.Integer;
                animEvent.functionName = "IntEvent";
            }
            if ((ukey.UseFlag & SSInfo.UserDataFlag.String) == SSInfo.UserDataFlag.String)
            {
                animEvent.stringParameter = ukey.String;
                animEvent.functionName = "StringEvent";
            }
            animEvents.Add(animEvent);
        }
        AnimationUtility.SetAnimationEvents(clip, animEvents.ToArray());
    }

    static void SetUserData(AnimationClip clip, List<SSInfo.AbstractKey> keys, List<AnimationEvent> animEvents)
    {
        foreach (var key in keys)
        {
            AnimationEvent animEvent = new AnimationEvent();
            animEvent.time = key.Time / clip.frameRate;
			
            SSInfo.UserKey ukey = key as SSInfo.UserKey;
            if ((ukey.UseFlag & SSInfo.UserDataFlag.Integer) == SSInfo.UserDataFlag.Integer)
            {
                animEvent.intParameter = ukey.Integer;
                animEvent.functionName = "IntEvent";
            }
            if ((ukey.UseFlag & SSInfo.UserDataFlag.String) == SSInfo.UserDataFlag.String)
            {
                animEvent.stringParameter = ukey.String;
                animEvent.functionName = "StringEvent";
            }
            animEvents.Add(animEvent);
        }
    }

    static void SetSpriteChangeEvent(AnimationClip clip, List<SSInfo.AbstractKey> keys, List<AnimationEvent> animEvents, string objectName)
    {
        foreach (SSInfo.CellKey key in keys)
        {
            AnimationEvent animEvent = new AnimationEvent();
            animEvent.time = key.Time / clip.frameRate;
            animEvent.stringParameter = string.Format("avatar({0},{1})", objectName, key.Name);
            animEvent.functionName = "StringEvent";
            animEvents.Add(animEvent);
        }
    }

    static AnimationCurve GetPositionAnimationCurve(AnimationClip clip, List<SSInfo.AbstractKey> keys)
    {
        Vector2 point1;
        Vector2 point2;
        Vector2 controller1;
        Vector2 controller2;
//        Vector2 deltapoint;
        
        List<Keyframe> keyframes = new List<Keyframe>();
        for (int i=0; i<keys.Count; i++)
        {
            SSInfo.CommonKey<float> commonKey = keys[i] as SSInfo.CommonKey<float>;
            SSInfo.CommonKey<float> prevCommonKey = i > 0 ? keys[i - 1] as SSInfo.CommonKey<float> : keys[i] as SSInfo.CommonKey<float>;
            SSInfo.CommonKey<float> nextCommonKey = i < keys.Count - 1 ? keys[i + 1] as SSInfo.CommonKey<float> : keys[i] as SSInfo.CommonKey<float>;
            Keyframe keyframe = new Keyframe((float) commonKey.Time / clip.frameRate, commonKey.Value * scale);

            if (i > 0)
            {
                keyframe.inTangent = float.PositiveInfinity;
                
                if (commonKey.Time - prevCommonKey.Time == 1)
                {
                    keyframe.inTangent = float.PositiveInfinity;
                }
            }
            
            if (i < keys.Count - 1)
            {
                switch (commonKey.Interpolation)
                {
                    case SSInfo.InterpolationType.None:
                        keyframe.outTangent = float.PositiveInfinity;
                        break;
                        
                    case SSInfo.InterpolationType.Bezier:

                        point1 = new Vector2(commonKey.Time / clip.frameRate, commonKey.Value * scale);
                        point2 = new Vector2(nextCommonKey.Time / clip.frameRate, nextCommonKey.Value * scale);
                        controller1 = new Vector2(commonKey.LeftCtrlVec.x / clip.frameRate, commonKey.LeftCtrlVec.y * scale);
                        controller2 = new Vector2(commonKey.RightCtrlVec.x / clip.frameRate, commonKey.RightCtrlVec.y * scale);

                        float t = nextCommonKey.Time - commonKey.Time;
                        float tmp = 100f;
                        for (int j=1, jt = 1; j<1000 && jt <t; j++)
                        {
                            Vector2 p = Bezier2Poly.Bezier3(j / 1000f, point1, controller1, point2, controller2);
                            float tmpt = Mathf.Abs(p.x - (commonKey.Time + jt) / clip.frameRate);
                            if (tmp > tmpt)
                            {
                                tmp = tmpt;
                                continue;
                            }
                            tmp = 100f;
                            
                            Keyframe kf = new Keyframe((float) (commonKey.Time + jt++) / clip.frameRate, p.y);
                            kf.outTangent = float.PositiveInfinity;
                            keyframes.Add(kf);
                        }

                        keyframe.outTangent = float.PositiveInfinity;
                        break;
                        
                    default:

                        point1 = new Vector2(commonKey.Time / clip.frameRate, commonKey.Value);
                        point2 = new Vector2(nextCommonKey.Time / clip.frameRate, nextCommonKey.Value);
                        int time = nextCommonKey.Time - commonKey.Time;
                        
                        for (int j = 1; j <time; j++)
                        {
                            Keyframe kf = new Keyframe((float) (commonKey.Time + j) / clip.frameRate, point1.y + (point2.y - point1.y) / (float) time * j);
                            kf.outTangent = float.PositiveInfinity;
                            keyframes.Add(kf);
                        }
                        
                        keyframe.outTangent = float.PositiveInfinity;
                        break;
                }
                
                if (nextCommonKey.Time - commonKey.Time == 1)
                {
                    keyframe.outTangent = float.PositiveInfinity;
                }
            }
            keyframes.Add(keyframe);
        }
        return new AnimationCurve(keyframes.ToArray());
    }

    static AnimationCurve GetAnimationCurve(AnimationClip clip, List<SSInfo.AbstractKey> keys)
    {
        Vector2 point1;
        Vector2 point2;
        Vector2 controller1;
        Vector2 controller2;
//        Vector2 deltapoint;
        
        List<Keyframe> keyframes = new List<Keyframe>();
        for (int i=0; i<keys.Count; i++)
        {
            SSInfo.CommonKey<float> commonKey = keys[i] as SSInfo.CommonKey<float>;
            SSInfo.CommonKey<float> prevCommonKey = i > 0 ? keys[i - 1] as SSInfo.CommonKey<float> : keys[i] as SSInfo.CommonKey<float>;
            SSInfo.CommonKey<float> nextCommonKey = i < keys.Count - 1 ? keys[i + 1] as SSInfo.CommonKey<float> : keys[i] as SSInfo.CommonKey<float>;
            Keyframe keyframe = new Keyframe((float) commonKey.Time / clip.frameRate, commonKey.Value);

            if (i > 0)
            {
                keyframe.inTangent = float.PositiveInfinity;

                if (commonKey.Time - prevCommonKey.Time == 1)
                {
                    keyframe.inTangent = float.PositiveInfinity;
                }
            }

            if (i < keys.Count - 1)
            {
                switch (commonKey.Interpolation)
                {
                    case SSInfo.InterpolationType.None:
                        keyframe.outTangent = float.PositiveInfinity;
                        break;

                    case SSInfo.InterpolationType.Bezier:
                        point1 = new Vector2(commonKey.Time / clip.frameRate, commonKey.Value);
                        point2 = new Vector2(nextCommonKey.Time / clip.frameRate, nextCommonKey.Value);
                        controller1 = new Vector2(commonKey.LeftCtrlVec.x / clip.frameRate, commonKey.LeftCtrlVec.y);
                        controller2 = new Vector2(commonKey.RightCtrlVec.x / clip.frameRate, commonKey.RightCtrlVec.y);

                        float t = nextCommonKey.Time - commonKey.Time;
                        float tmp = 100f;
                        for (int j=1, jt = 1; j<1000 && jt <t; j++)
                        {
                            Vector2 p = Bezier2Poly.Bezier3(j / 1000f, point1, controller1, point2, controller2);
                            float tmpt = Mathf.Abs(p.x - (commonKey.Time + jt) / clip.frameRate);
                            if (tmp > tmpt)
                            {
                                tmp = tmpt;
                                continue;
                            }
                            tmp = 100f;

                            Keyframe kf = new Keyframe((float) (commonKey.Time + jt++) / clip.frameRate, p.y);
                            kf.outTangent = float.PositiveInfinity;
                            keyframes.Add(kf);
                        }

                        keyframe.outTangent = float.PositiveInfinity;
                        break;

                    default:
                    
                        point1 = new Vector2(commonKey.Time / clip.frameRate, commonKey.Value);
                        point2 = new Vector2(nextCommonKey.Time / clip.frameRate, nextCommonKey.Value);
                        int time = nextCommonKey.Time - commonKey.Time;
                    
                        for (int j = 1; j <time; j++)
                        {
                            Keyframe kf = new Keyframe((float) (commonKey.Time + j) / clip.frameRate, point1.y + (point2.y - point1.y) / (float) time * j);
                            kf.outTangent = float.PositiveInfinity;
                            keyframes.Add(kf);
                        }
                    
                        keyframe.outTangent = float.PositiveInfinity;
                        break;
                }

                if (nextCommonKey.Time - commonKey.Time == 1)
                {
                    keyframe.outTangent = float.PositiveInfinity;
                }
            }

            keyframes.Add(keyframe);
        }
        
        return new AnimationCurve(keyframes.ToArray());
    }

    //
    public static Dictionary<string, List<Sprite>> CreateMultiSpriteTexture(List<SSInfo.SSCEInfo> cellInfoList, string inputPath, string outputPath)
    {
        Dictionary<string, List<Sprite>> sprites = new Dictionary<string, List<Sprite>>();

        foreach (SSInfo.SSCEInfo ssceInfo in cellInfoList)
        {
            // import sprite texture
            TextureImporter importer = AssetImporter.GetAtPath(inputPath + ssceInfo.ImagePath) as TextureImporter;
            importer.textureType = TextureImporterType.Advanced;
            importer.isReadable = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureFormat = TextureImporterFormat.ARGB32;
            AssetDatabase.ImportAsset(inputPath + ssceInfo.ImagePath);

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            foreach (SSInfo.SSCEInfo.CellData data in ssceInfo.Cells)
            {
                SpriteMetaData meta = new SpriteMetaData();
                meta.name = data.Name;
                meta.rect = new Rect(data.Pos.x, ssceInfo.PixelSizeH - data.Pos.y - data.Size.y, data.Size.x, data.Size.y);
                Vector2 pivot;
                if (data.Size.x != .0f && data.Size.y != .0f)
                {
                    pivot = new Vector2(data.Pivot.x / data.Size.x, 1 - (data.Pivot.y / data.Size.y));
                }
                else
                {
                    pivot = new Vector2(defaultPivot, defaultPivot);
                }
                meta.alignment = 9;
                meta.pivot = pivot;
                metas.Add(meta);
            }
            
            //  texture copy
            AssetDatabase.CopyAsset(inputPath + ssceInfo.ImagePath, outputPath + ssceInfo.ImagePath);
            
            // import sprite texture
            TextureImporter textureImporter = AssetImporter.GetAtPath(outputPath + ssceInfo.ImagePath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritesheet = metas.ToArray();
            textureImporter.spritePixelsToUnits = 1.0f;
            AssetDatabase.ImportAsset(outputPath + ssceInfo.ImagePath);

            sprites.Add(ssceInfo.FileName, GetSprites(outputPath + ssceInfo.ImagePath));
        }

        return sprites;
    }

    //
    static List<Sprite> CreateSingleSpriteTexture(List<SSInfo.SSCEInfo> cellInfoList, string inputPath, string outputPath, string pjName)
    {
        List<Texture2D> spriteTextures = new List<Texture2D>();
        string dummyTexturePath = "";
        
        cellInfoList.ForEach((SSInfo.SSCEInfo info) => {
            dummyTexturePath = info.ImagePath;

            // import sprite texture
            TextureImporter importer = AssetImporter.GetAtPath(inputPath + info.ImagePath) as TextureImporter;
            importer.textureType = TextureImporterType.Advanced;
            importer.isReadable = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureFormat = TextureImporterFormat.ARGB32;
            AssetDatabase.ImportAsset(inputPath + info.ImagePath);

            spriteTextures.Add((Texture2D) AssetDatabase.LoadAssetAtPath(inputPath + info.ImagePath, typeof(Texture2D)));
        });

        string newTextureExtension = System.IO.Path.GetExtension(dummyTexturePath);
        
        Texture2D outputTexture = new Texture2D(1, 1);
        List<Rect> textureRects = new List<Rect>(outputTexture.PackTextures(spriteTextures.ToArray(), 1));
        
        AssetDatabase.CopyAsset(inputPath + dummyTexturePath, outputPath + pjName + newTextureExtension);
        File.WriteAllBytes(fullPath + outputPath + pjName + newTextureExtension, outputTexture.EncodeToPNG());

        List<SpriteMetaData> metas = new List<SpriteMetaData>();
        for (int i=0; i<cellInfoList.Count; i++)
        {
            SSInfo.SSCEInfo ssceInfo = cellInfoList[i];
            
            foreach (SSInfo.SSCEInfo.CellData data in ssceInfo.Cells)
            {
                SpriteMetaData meta = new SpriteMetaData();
                meta.name = data.Name + i.ToString();
                meta.rect = new Rect(data.Pos.x + outputTexture.width * textureRects[i].x, outputTexture.height * textureRects[i].y + (ssceInfo.PixelSizeH - data.Pos.y - data.Size.y), data.Size.x, data.Size.y);
                Vector2 pivot;
                if (data.Size.x != .0f && data.Size.y != .0f)
                {
                    pivot = new Vector2(data.Pivot.x / data.Size.x, 1 - (data.Pivot.y / data.Size.y));
                }
                else
                {
                    pivot = new Vector2(defaultPivot, defaultPivot);
                }
                meta.alignment = 9;
                meta.pivot = pivot;
                metas.Add(meta);
            }
        }
        
        // import sprite texture
        TextureImporter textureImporter = AssetImporter.GetAtPath(outputPath + pjName + newTextureExtension) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.maxTextureSize = 4096;
        textureImporter.spritesheet = metas.ToArray();
        textureImporter.spritePixelsToUnits = 1.0f;
        AssetDatabase.ImportAsset(outputPath + pjName + newTextureExtension);

        return GetSprites(outputPath + pjName + newTextureExtension);
    }

    public static void SavePrefab(string name, string outputPath, List<Transform> transforms)
    {
        // 
        GameObject root = new GameObject(name);
        root.AddComponent<UnityEngine.Animator>();
        root.AddComponent<AnimationEventDitector>();
        
        List<Transform> rootTransforms = transforms.FindAll((x) => x.parent == null);

        foreach (Transform rootTransform in rootTransforms)
        {
            rootTransform.parent = root.transform;
        }
        
        PrefabUtility.CreatePrefab(outputPath + name + ".prefab", root);

        Object.DestroyImmediate(root);
    }
    
    static string GetPath(Transform transform)
    {
        string path = "";
        
        if (transform == null)
        {
            return path;
        }
        
        path = transform.name;
        transform = transform.parent;
        
        while (transform != null)
        {
            path = transform.name + "/" + path;
            transform = transform.parent;
        }
        
        return path;
    }
    
    static List<Sprite> GetSprites(string path)
    {
        List<Sprite> sprites = new List<Sprite>();
        Object[] textureObjects = AssetDatabase.LoadAllAssetsAtPath(path);
        
        foreach (Object textureObject in textureObjects)
        {
            if (textureObject.GetType().Equals(typeof(UnityEngine.Sprite)))
            {
                sprites.Add((Sprite) textureObject);
            }
        }
        
        return sprites;
    }
    
}
