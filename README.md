Ss5ConverterForUnity2D
================================

##概要
SpriteStudio5のアニメーションデータ(sspj)をUnity2D用のprefab、animeファイルへ変換するコンバータです。
AnimationControllerを使用してモーションの遷移や制御を行う事ができ、モーションブレンドも行う事ができます。

* Unity4.6で動作を確認しています。
* ユニティちゃんのアニメーションを表示するサンプルプロジェクトが含まれています。
![画像](https://raw.githubusercontent.com/SpriteStudio3rdParty/Ss5ConverterForUnity2D/master/image/unityss1.png)

##使用できるアトリビュート
以下のアトリビュート以外は使用できませんのでご注意ください。
* 参照セル
* X座標
* Y座標
* Z軸回転
* Xスケール
* Yスケール
* 不透明度
* 優先度
* 非表示
* ユーザーデータ

##アニメデータ作成時の注意
* パーツの中で使用するアトリビュート（主に非表示）は0フレーム目にキーを設定してください。
* 優先度が同じパーツは描画順が不定になります。
* Ver.4互換には対応していません。互換性設定のチェックはすべて外した状態でアニメを作成してください。

##導入方法
* ダウンロードしたプロジェクトに含まれるAssets\Editor\ConvertUnity2DfromSpriteStudio、Assets\Scripts\ConvertUnity2Dフォルダを使用するゲームのプロジェクトへコピーします。
* UnityメニューのAssetsに「sspj Convert for Unity2D」が追加されます。

##使い方
* 1.アニメーションのコンバート
* Project ViewにSpriteStudioのプロジェクト一式をインポートします。（.sspj .ssce .ssae等）
* .sspjファイルを選択し、右クリックのメニューから「sspj Convert for Unity2D」を実行して変換します。
* Project ViewのAssets\Unity2d\sspj名\フォルダに変換後のデータが書き出されます。

* 2.AnimatorControllerの作成
* 適当なフォルダでAssets > Create > AnimatorContollerからAnimatorControllerを作成します。
* 作成したAnimatorControllerを選択した状態でWindow > Animatorを開き、変換された.animファイルをD&Dします。
* 変換されたPrefabに付いているAnimatorのControllerに先ほど作成したAnimatorControllerをセットします。
* 完成です。PrefabをHierarchyに追加して再生してください。

* 3.イベント
* コンバーターを通した場合、自動的にAnimationEventDitectorというコンポーネントがプレハブに追加されます。
* アニメーションクリップにアニメーションイベントのキーを設定します。
* 設定してもらったキーに関数の選択と値の設定ができるようになっていると思います。関数と値を設定してください。
* 関数と値を設定したら、スクリプトからイベント発生時に呼ばれる関数をAnimationEventDitectorのデリゲートに設定します。
* アニメーションを再生することにより、設定された関数がよばれるので、アニメーションの制御、ＳＥの再生、エフェクトの発生などのトリガーにお使いください。

##ユーザーデータについて
以下のユーザーデータを取得する事ができます。
* 整数
* 文字列

##サンプルプロジェクトに含まれるアニメーションデータについて
* アセットに含まれるユニティちゃんのアニメーションは「ユニティちゃんライセンス」によって配布されます。
![ユニティちゃんライセンス](http://unity-chan.com/images/imageLicenseLogo.png)
* ユニティちゃんのアニメーションをご利用される場合は、『[キャラクター利用のガイドライン](http://unity-chan.com/download/guideline.html)』も併せてご確認ください。
