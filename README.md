# InternetPractice

## このプロジェクトについて
このプロジェクトは通信を学ぶために作成したものです。

クライアント同士をマッチングさせ、座標同期をしてます。
> ゲーム作品ではないです。

## 実行方法
#### 1. VisualStudioでサーバーを起動
#### 2. UnityのBuildを実行しクライアントを起動
```bash
[UnityBuildPath]
Move/Build/Move.exe
```
### ⚠️ 実行時の注意点

> [!IMPORTANT]
> Unity側のClientMatchMakerのURLのIP部分を、サーバーを起動するPCのプライベートIPアドレスに変更してください
> Move/Assets/Script/C#/NetWork/Client/ClientMatchMaker.cs

> [!TIP]
> このゲームのマッチング人数は2人です。
