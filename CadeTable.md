# 担当ソースコードの詳細  
  
  
## スクリプトファイル
| スクリプトファイル | 概要 |
| --- | --- |
| ▼[Editorフォルダ](https://github.com/shuhei-M/Rear_ProjectFile_2021/tree/main/Rear_MasterVersion/Assets/Editor) |  |
| [FindReferenceAsset.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Editor/FindReferenceAsset.cs) | Editor拡張。オブジェクトの参照を確認する。オブジェクトを右クリックし、そこから「参照を探す」を選択。0個であれば、そのオブジェクトをプロジェクトファイルから削除することを検討する。 |
| ▼[Scriptフォルダ](https://github.com/shuhei-M/Rear_ProjectFile_2021/tree/main/Rear_MasterVersion/Assets/Scripts) |  |
| [AvatarData.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/AvatarData.cs) | 一部を担当<br>DestroySmokeOrbit()関数のみ追加。PlayerControllerクラスから放物線上の投射予測線を消すための関数。 |
| [ButtonCoolTimes.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/ButtonCoolTime.cs) | スキルボタンUIを円形ゲージのように見せるクラス。スキルのクールタイムを視覚的に分かり易くする目的で作成。 |
| [EnemyContlloer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/EnemyController.cs) | 敵一体の挙動を管理するクラス。ヒエラルキーウィンドウから行動パターンを切り替える等、企画担当者がレベルデザインしやすい様に実装した。 |
| [HornAvatorContlloer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/HornAvatarController.cs) | 部分的に担当<br>移動方法・衝突検知方法を変更、死亡時赤いエミッションを放つ機能・ジャンプ機能を追加。 |
| [HornStopper.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/HornStopper.cs) | ツノスライムが壁に当たったかどうか判定するセンサークラス。 |
| [IconContlloer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/IconController.cs) | 敵がプレイヤーを発見した際に、敵の頭上に表示する！アイコンの向きを調整するクラス |
| [ManagerScript.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/ManagerScript.cs) | ESCキーを押すことでゲームを強制終了させるクラス。 |
| [NoteAvatarController.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/NoteAvatarController.cs) | 一部を担当<br>DeadEffect()関数のみ追加。死亡時赤いエミッションを放つ機能。 |
| [PlayerContlloer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/PlayerController.cs) | プレイヤーの挙動を管理するクラス。 |
| [SearchArea.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/SearchArea.cs) | プレイヤー・分身が、視界範囲に入ったか検知を行うクラス。敵の視界を担う。 |
| [SearchLight.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/SearchLight.cs) | 敵の視界範囲を示すライトの挙動を管理するクラス。 |
| [SmokeArea.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/SmokeArea.cs) | 部分的に担当<br>煙幕の有効範囲を示し、視界を遮った敵を記録し、パラメータを更新するクラス。EnemySmokeReset()関数以外の全てを作成。 |
| [SmokeAvatorContlloer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/SmokeAvatarController.cs) | 部分的に担当<br>OnCollisionEnter()関数、煙幕エフェクトのみ追加。 |
| [SmokeScreen.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/SmokeScreen.cs) | 煙幕のエフェクトを発生させるクラス。スケール10になるまで徐々にサイズを大きくし、マテリアルをスクロールさせる。 |
| [StartAndEndGame.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/StartAndEndGame.cs) | 部分的に担当<br>基盤（プレイヤー死亡時処理、敵を全滅させた時の処理）の作成、OnRetry()、OnTitle()関数の作成 |
| [Timer.cs](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/Scripts/Timer.cs) | ゲームの経過時間を記録するクラス。 |
  
  
  
## シェーダファイル
| シェーダファイル | 概要 |
| --- | --- |
| ▼[MyAssets/SoundWaveフォルダ](https://github.com/shuhei-M/Rear_ProjectFile_2021/tree/main/Rear_MasterVersion/Assets/MyAssets/SoundWave) |  |
| [Wave.shader](https://github.com/shuhei-M/Rear_ProjectFile_2021/blob/main/Rear_MasterVersion/Assets/MyAssets/SoundWave/Wave.shader) | オンプスライムの発動と効果範囲を示すための、波紋が広がるようなエフェクト。 |

<!-- 
| [.cs]() |  |
| [ソースファイル名](プロジェクトに保存されているファイル名) | 説明文 |
-->
