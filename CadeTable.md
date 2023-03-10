# スクリプト担当箇所
  
# Scriptsフォルダ（[Scripts](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts)）
| スクリプトファイル | 概要 | 備考 |
| --- | --- | --- |
| ▼[Cameraフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera) |  |  |
| [MyCinemachineDollyCart.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/MyCinemachineDollyCart.cs) | 鳥をゴールへ向かってレールに沿って動かす際、不自然な傾きにならないよう調節する。 |  |
| [OccludeeBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/OccludeeBehaviour.cs) | 3Dオブジェクトを（段階的に）（半）透明にする機能を提供する。 |  |
| [OccluderBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/OccluderBehaviour.cs) | トリガーに接触したオブジェクトが OccludeeController を持っていたら、その機能を呼んで（半）透明にする。 | 【工夫】単一オブジェクト、複数オブジェクト一括のどちらにも対応させた。 |
| [PivotBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Camera/PivotBehaviour.cs) | A地点と B地点 を繋ぐようなコライダーを作る機能を提供する。<br>プレイヤーとカメラ間に障害物があるかどうか判定するための当たり判定。 |  |
| ▼[Characterフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character) |  |  |
| [AfterimageBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/AfterimageBehaviour.cs) | 弟の幽霊（残像）の挙動を管理する。Recorderクラスに記録されたプレイヤーのデータを5秒遅れで自身にセットする。 | 【工夫】シーソー上部にいる際、自動的にシーソーに接地するようにした。 |
| [BirdBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/BirdBehaviour.cs) | ゲーム開始時にゴールへ向かって飛ぶ鳥の挙動を管理するクラス。 |  |
| [PlayerBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/PlayerBehaviour.cs) | プレイヤーの挙動を管理するパーシャルクラス。<br>Update関数はここで使用する。 | 【工夫】IPlayGimmickインターフェイスを継承。ギミックとの情報のやり取りをインターフェイスで行う。<br>【担当箇所】BarrierPlayer()関数以外。 |
| [Player_Move.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Player_Move.cs) | PlayerBehaviourのパーシャルクラス。<br>プレイヤーの移動処理を担う関数を定義。 |  |
| [Player_State.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Player_State.cs) | PlayerBehaviourのパーシャルクラス。<br>プレイヤーの持つステートを設定する。 |【工夫】ステートマシン(有限オートマトン)のStateMachineジェネリッククラスを使用。 |
| [Recorder.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/Recorder.cs) | プレイヤーの位置、回転、入力、ステートの情報を記録する。 |  |
| [RideSencor.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Character/RideSencor.cs) | プレイヤーが幽霊の弟（残像）に乗ったか、及び、プレイヤーが乗り状態に移って良いか判定する。 |  |
| ▼[Controllerフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller) |  |  |
| [StageController.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller/StageController.cs) | ステージ内で、他のスクリプト等から参照され易いオブジェクトを一纏めにしておく。 | 【担当箇所】[担当箇所] 43 ～ 64行目（BGMの実装）以外。 |
| [Stage09Controller.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Controller/Stage09Controller.cs) | ゲームクリア時演出用の特設ステージであるStage09のステートを管理するクラス。<br>プレイヤーのオート移動、カメラワーク、エフェクトの生成などを行う。 | 【工夫】列挙体でステートを設定し、ステートごとに挙動を管理した。 |
| ▼[Editorフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Editor) |  |  |
| [FindReferenceAsset.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Editor/FindReferenceAsset.cs) | Editor拡張。オブジェクトの参照を確認する。オブジェクトを右クリックし、そこから「参照を探す」を選択。0個であれば、そのオブジェクトをプロジェクトファイルから削除することを検討する。 |  |
| ▼[Effectフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect) |  |  |
| [EffectManager.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect/EffectManager.cs) | 瞬間移動エフェクト・浮遊エフェクト等の弟の幽霊（残像）周りのエフェクトや、ゲームクリア演出用のエフェクトを生成させるクラス。<br>AfterimageBehaviourクラス、Stage09Controllerクラスで使用する。 | 【工夫】シングルトンパターンのジェネリッククラス（SingletonMonoBehaviour）を継承し、どのクラスからもPlay関数で、エフェクトを生成できる。 |
| [FloatEffect.cs.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Effect/FloatEffect.cs) | 残像が乗り状態ではなくなった時に、浮遊エフェクトを終了させるクラス。 |  |
| ▼[Gimmickフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick) |  |  |
| [HintGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/HintGimmick.cs) | プレイヤーがヒントエリアに入ったか検知する。ヒントボタン(A)を押した場合、ヒントを表示させる。5秒後に消滅。 |  |
| [LadderGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/LadderGimmick.cs) | IPlayGimmickインターフェイスの使用方法を説明するため、雛型のみ作成。 |  |
| [MovableObject.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/MovableObject.cs) | インターフェイスの使用方法をギミック担当者に説明するためのもの。<br>プレイヤーが押し出すことのできるオブジェクトの試作。 |  |
| [SeesawGimmick.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/SeesawGimmick.cs) | SeesawSencorの判定を受け、シーソーを傾ける。角度制限を設けた。 | 【工夫】シーソーの傾き具合を列挙体でステートとして設定し、ステートごとに挙動を管理した。 |
| [SeesawSencor.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/SeesawSencor.cs) | シーソーの片方にプレイヤー及び幽霊の弟（残像）が乗ったかどうか判定する。 |  |
| [Wind.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Gimmick/Wind.cs) | 風によって火が消えてしまうギミックの試作段階。RayCastによって実装。 |  |
| ▼[UIフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI) |  |  |
| [BookUI.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/BookUI.cs) | ページをめくるエフェクト。<br>ステージ開始・クリア時、ステージセレクト画面にて使用する。 | 【工夫】列挙体を使用し、インスペクタウィンドウから、用途に合わせてモードチェンジできるようにした。<br>企画者でも容易に変更できるよう意識した。 |
| [PageSelector.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/PageSelector.cs) | めくるページ一つ一つに付けるスクリプト。BookUIクラスと連動。 |  |
| [StageUIScript.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/StageUIScript.cs) | インゲーム時に表示するUIを統括するスクリプト。<br>ゲームの状態によってのパネルの表示・非表示などを行う。 | 【工夫】Game_State.csのInGame列挙体でのステートの設計を担当者に提案。このクラスのステート毎に、UI全体の挙動を管理した。<br>【担当箇所】ポーズ画面、BGM、SEの部分以外 |
| [UITessellator.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UITessellator.cs) | ページの折り目（中央線）近くにあるオブジェクトにアタッチする。<br>ページめくりの際に折り目部分に見た目を対応させる。 |  |
| [UI_ActionButton.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_ActionButton.cs) | プレイヤーのとることのできるアクションを示すUI。<br>プレイヤーのいる場所によって異なるアクションが表示される。 | 【工夫】アクション内容によっては、アクション(B)ボタンの長押しに、アニメーションを対応させた。 |
| [UI_HintButton.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_HintButton.cs) | ヒント使用可能かどうかを表示する。<br>使用可能になった・ボタンが押された場合に、アニメーションを再生する。 |  |
| [UI_RetryPanel.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_RetryPanel.cs) | ゲームクリア後にステージリトライを選択した場合に、ページめくり演出を開始させ、終了後にシーンを再ロードするクラス。 |  |
| [UI_WarpEffect.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/UI/UI_WarpEffect.cs) | ゲームクリア時のワープ風の渦巻きの様なエフェクトを再生するクラス。 |  |
| ▼[Utilityフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility) |  |  |
| [ASMB_Actions.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/ASMB_Actions.cs) | プレイヤーアニメータコントローラ用のStateMachineBehaviourクラス。<br>各アクションのアニメーションが終了した際に、プレイヤーにアクションステートから遷るよう命令する。 |  |
| [Interfaces.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/Interfaces.cs) | 各ギミックからプレイヤーにアクセスするためのインターフェイスや、UIボタン用のインターフェイスを集めた.csファイル。 |  |
| [ManagerScript.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/ManagerScript.cs) | 実行ファイル上でEscapeキーを押すと強制終了できる |  |
| [SingletonMonoBehaviour.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/SingletonMonoBehaviour.cs) | シングルトンパターンのジェネリッククラス。<br>EffectManagerスラスで使用。 |  |
| [StateMachine.cs](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Scripts/Utility/StateMachine.cs) | ステートマシン（有限オートマトン）を作成するジェネリッククラス。<br>PlayerBehaviourクラス（主にPlayer_State.cs）にて使用。 |  |
  
  
  
# Resourcesフォルダ（[Resources](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources)）
| シェーダファイル | 概要 | 備考 |
| --- | --- | --- |
| ▼[Effectフォルダ](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect) |  |  |
| [NextPage.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/NextPage.shader) | 見開きの紙を一気にめくるエフェクト |  |
| [BurningPaper.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/BurningPaper.shader) | 紙がじわじわと燃えるエフェクト |  |
| [PageUI.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/PageUI.shader) | 本のページをめくるエフェクト |  |
| [AlphaCrunch-AlphaBlended.shader](https://github.com/shuhei-M/With-you-after-5-seconds/tree/main/With_you_after_5_seconds/Assets/Resources/Effect/AlphaCrunch-AlphaBlended.shader) | 炎がゆらゆらと燃えるエフェクト |  |

<!-- 
| [.cs]() |  |
| [ソースファイル名](プロジェクトに保存されているファイル名) | 説明文 |
上の文を4行目以降にコピペしてもらって内容書き換えれば表になります
-->
