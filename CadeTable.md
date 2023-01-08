# スクリプト担当箇所
  
# Scriptsフォルダ（[Scripts](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts)）
| スクリプトファイル | 軽い説明 |
| --- | --- |
| ▼[Cameraフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera) |  |
| [MyCinemachineDollyCart.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/MyCinemachineDollyCart.cs) | 鳥をゴールへ向かってレールに沿って動かす際、不自然な傾きにならないよう調節する。 |
| [OccludeeBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/OccludeeBehaviour.cs) | 3Dオブジェクトを（段階的に）（半）透明にする機能を提供する。 |
| [OccluderBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/OccluderBehaviour.cs) | トリガーに接触したオブジェクトが OccludeeController を持っていたら、その機能を呼んで（半）透明にする。単一オブジェクト、複数オブジェクト一括のどちらにも対応。 |
| [PivotBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/PivotBehaviour.cs) | m_start と m_end を繋ぐようなコライダーを作る機能を提供する。 |
| ▼[Characterフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character) |  |
| [AfterimageBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/AfterimageBehaviour.cs) | Recorderクラスに記録されたデータを残像オブジェクトにセットする。 |
| [BirdBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/BirdBehaviour.cs) | ゲーム開始時にゴールへ向かって飛ぶ鳥の挙動を管理するクラス。 |
| [PlayerBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/PlayerBehaviour.cs) | プレイヤーのアップデート処理（パーシャルクラス）。インターフェイスの定義はここで行う。 |
| [Player_Move.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Player_Move.cs) | PlayerBehaviourのパーシャルクラス。プレイヤーの移動処理を管理。 |
| [Player_State.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Player_State.cs) | PlayerBehaviourのパーシャルクラス。プレイヤーの持つステートを設定する。 |
| [Recorder.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Recorder.cs) | プレイヤーの位置、回転、入力、状態の情報を記録する。 |
| [RideSencor.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/RideSencor.cs) | プレイヤーが残像に乗ったか、及び、プレイヤーが乗り状態に移って良いか判定する。 |
| ▼[Controllerフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller) |  |
| [StageController.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller/StageController.cs) | ステージ内で、他のスクリプト等から参照され易いオブジェクトを一纏めにしておく。 |
| [Stage09Controller.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller/Stage09Controller.cs) | ゲームクリア時演出用の特設ステージであるStage09のステートを管理するクラス。プレイヤーのオート移動、カメラワーク、エフェクトの生成などを行う。 |
| ▼[Editorフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Editor) |  |
| [FindReferenceAsset.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Editor/FindReferenceAsset.cs) | Editor拡張。オブジェクトの参照を確認する。オブジェクトを右クリックし、そこから「参照を探す」を選択。0個であれば、そのオブジェクトをプロジェクトファイルから削除することを検討する。 |
| ▼[Effectフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect) |  |
| [EffectManager.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect/EffectManager.cs) | 瞬間移動エフェクトや浮遊エフェクト等、残像周りのエフェクトを生成させる、シングルトンパターンのクラス。 |
| [FloatEffect.cs.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect/FloatEffect.cs) | 残像が乗り状態ではなくなった時に、浮遊エフェクトを終了させるクラス。 |
| ▼[Gimmickフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick) |  |
| [HintGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/HintGimmick.cs) | プレイヤーがヒントエリアに入ったか検知する。ヒントボタンを押した場合、ヒントを表示させる。5秒後に消滅。 |
| [LadderGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/LadderGimmick.cs) | IPlayGimmickインターフェイスの使用方法を説明するため、雛型のみ作成。削除の可能性あり。 |
| [MovableObject.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/MovableObject.cs) | プレイヤーが押し出すことのできるオブジェクトの試作。今後消去する可能性あり。 |
| [SeesawGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/SeesawGimmick.cs) | SeesawSencorの判定を受け、シーソーを傾ける。角度制限を設けた。 |
| [SeesawSencor.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/SeesawSencor.cs) | シーソーの片方にプレイヤー及び残像が乗ったかどうか判定する。 |
| [Wind.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/Wind.cs) | 風によって火が消えてしまうギミックの試作段階。RayCastによって実装。今後削除する可能性大。 |
| ▼[UIフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI) |  |
| [BookUI.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/BookUI.cs) | ステージ攻略開始時とクリア時に使用する、ページをめくるエフェクト。インスペクターウィンドウから、カットインとクリアのモードチェンジをすることが出来る。 |
| [PageSelector.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/PageSelector.cs) | めくるページ一つ一つに付けるスクリプト。 |
| [StageUIScript.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/StageUIScript.cs) | インゲーム時に表示するUIを統括するスクリプト。ゲームの状態によってのパネルの表示・非表示などを行う。 |
| [UITessellator.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UITessellator.cs) | ページ内にある2次元オブジェクト全てにアタッチするスクリプト。 |
| [UI_ActionButton.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_ActionButton.cs) | プレイヤーのとることのできるアクションを示すUI。プレイヤーのいる場所によって異なるアクションがセットされる。 |
| [UI_HintButton.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_HintButton.cs) | ヒント使用可能エリアに入った場合、目が閉じているアイコンから開いているアイコンに差し替える。 |
| [UI_RetryPanel.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_RetryPanel.cs) | ゲームクリア後にステージリトライを選択した場合に、ページめくり演出を開始させ、終了後にシーンを再ロードするクラス。 |
| [UI_WarpEffect.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_WarpEffect.cs) | ゲームクリア時のワープ風の渦巻きの様なエフェクトを再生するクラス。 |
| ▼[Utilityフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility) |  |
| [ASMB_Actions.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/ASMB_Actions.cs) | プレイヤーアニメータコントローラ用のStateMachineBehaviourクラス。<br>各アクションのアニメーションが終了した際に、プレイヤーにアクションステートから遷るよう命令する。 |
| [Interfaces.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/Interfaces.cs) | 各ギミックからプレイヤーにアクセスするためのインターフェイスや、UIボタン用のインターフェイスを集めた.csファイル。 |
| [ManagerScript.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/ManagerScript.cs) | 実行ファイル上でEscapeキーを押すと強制終了できる |
| [SingletonMonoBehaviour.cs.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/SingletonMonoBehaviour.cs) | シングルトンパターンのジェネリッククラス。<br>EffectManagerスラスで使用。 |
| [StateMachine.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/StateMachine.cs) | ステートマシン（有限オートマトン）を作成するジェネリッククラス。<br>PlayerBehaviourクラス（主にPlayer_State.cs）にて使用。 |
  
  
  
## Resourcesフォルダ（[Resources](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources)）
| シェーダファイル | 軽い説明 |
| --- | --- |
| ▼[Effectフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect) |  |
| [NextPage.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/MBL_NextPage.shader) | 見開きの紙を一気にめくるエフェクト |
| [BurningPaper.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/M_BurningPaper.shader) | 紙がじわじわと燃えるエフェクト |
| [PageUI.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/PageUI.shader) | 本のページをめくるエフェクト |
| [AlphaCrunch-AlphaBlended.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/Alpha%20Crunch%20-%20Alpha%20Blended.shader) | 炎がゆらゆらと燃えるエフェクト |

<!-- 
| [.cs]() |  |
| [ソースファイル名](プロジェクトに保存されているファイル名) | 説明文 |
上の文を4行目以降にコピペしてもらって内容書き換えれば表になります
-->
