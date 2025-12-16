# Unity Editor Background Extension 仕様書

## 概要

VS Codeの「Background」拡張機能のように、Unityエディタ全体に薄く好きな画像を背景として表示するエディタ拡張。

## 目的

- エディタの見た目をカスタマイズしてモチベーション向上
- 作業中に好きなキャラクターやアートワークを表示

---

## 技術仕様

### 対応環境

- Unity 2019.3 以上（UIElements/UIToolkit必須）
- エディタ拡張のため、ビルドには含まれない

### 実装方式

UIToolkit (VisualElement) を使用し、各EditorWindowの`rootVisualElement`に背景用のVisualElementを最背面に挿入する。

---

## 機能要件

### 1. 背景画像表示

- すべての EditorWindow に背景画像を表示
- 画像は最背面に配置し、通常のUI操作を妨げない（`pickingMode = PickingMode.Ignore`）
- 画像のスケールモード: `ScaleAndCrop`（アスペクト比を維持しつつウィンドウを埋める）

### 2. 設定UI（EditorWindow）

メニュー: `Tools > Editor Background > Settings`

設定項目:
| 項目 | 型 | デフォルト値 | 説明 |
|------|-----|-------------|------|
| Enabled | bool | true | 背景表示の有効/無効 |
| Background Image | Texture2D | null | 表示する画像 |
| Opacity | float (0.0-1.0) | 0.08 | 透明度（0=透明、1=不透明） |
| Scale Mode | enum | ScaleAndCrop | ScaleAndCrop / ScaleToFit / StretchToFill |
| Tint Color | Color | White | 画像に乗算する色 |

### 3. 設定の永続化

- `EditorPrefs` を使用して設定を保存
- キープレフィックス: `EditorBackground_`
- 画像パスは `AssetDatabase.GetAssetPath()` で取得し、文字列として保存

### 4. リアルタイムプレビュー

- 設定変更時、即座に全ウィンドウの背景を更新
- 新しく開いたウィンドウにも自動的に背景を適用

---

## クラス設計

```
Assets/
└── Editor/
    └── EditorBackground/
        ├── EditorBackgroundCore.cs      # 背景描画のコアロジック
        ├── EditorBackgroundSettings.cs  # 設定データクラス
        ├── EditorBackgroundWindow.cs    # 設定UI
        └── Resources/
            └── DefaultBackground.png    # (任意) デフォルト画像
```

### EditorBackgroundSettings.cs

```csharp
// 設定データを管理する静的クラス
public static class EditorBackgroundSettings
{
    // プロパティ（EditorPrefsから読み書き）
    public static bool Enabled { get; set; }
    public static string ImagePath { get; set; }
    public static float Opacity { get; set; }
    public static ScaleMode ScaleMode { get; set; }
    public static Color TintColor { get; set; }
    
    // 設定変更時のイベント
    public static event Action OnSettingsChanged;
    
    // EditorPrefsへの保存/読み込み
    public static void Save();
    public static void Load();
}
```

### EditorBackgroundCore.cs

```csharp
[InitializeOnLoad]
public static class EditorBackgroundCore
{
    // 処理済みウィンドウを追跡
    private static HashSet<EditorWindow> processedWindows;
    
    // 背景VisualElementの参照（更新用）
    private static Dictionary<EditorWindow, VisualElement> backgroundElements;
    
    // 初期化（staticコンストラクタ）
    static EditorBackgroundCore()
    {
        EditorBackgroundSettings.Load();
        EditorApplication.update += OnUpdate;
        EditorBackgroundSettings.OnSettingsChanged += RefreshAllBackgrounds;
    }
    
    // 毎フレーム: 新しいウィンドウをチェックして背景を追加
    private static void OnUpdate();
    
    // ウィンドウに背景を追加
    private static void ApplyBackground(EditorWindow window);
    
    // 全ウィンドウの背景を更新
    private static void RefreshAllBackgrounds();
    
    // 全ウィンドウから背景を削除
    private static void RemoveAllBackgrounds();
}
```

### EditorBackgroundWindow.cs

```csharp
public class EditorBackgroundWindow : EditorWindow
{
    [MenuItem("Tools/Editor Background/Settings")]
    public static void ShowWindow();
    
    private void OnGUI()
    {
        // 有効/無効トグル
        // 画像選択フィールド（ObjectField）
        // 透明度スライダー
        // スケールモード選択
        // ティントカラー選択
        // 適用ボタン / リセットボタン
    }
}
```

---

## 実装上の注意点

### 1. ウィンドウのライフサイクル管理

```csharp
// nullチェック必須（ウィンドウが閉じられた場合）
processedWindows.RemoveWhere(w => w == null);
backgroundElements = backgroundElements
    .Where(kvp => kvp.Key != null)
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
```

### 2. 背景VisualElementの構成

```csharp
var bg = new VisualElement();
bg.name = "editor-background-image";
bg.style.position = Position.Absolute;
bg.style.left = 0;
bg.style.top = 0;
bg.style.right = 0;
bg.style.bottom = 0;
bg.style.backgroundImage = texture;
bg.style.unityBackgroundScaleMode = scaleMode;
bg.style.opacity = opacity;
bg.style.unityBackgroundImageTintColor = tintColor;
bg.pickingMode = PickingMode.Ignore;

// 最背面に挿入
window.rootVisualElement.Insert(0, bg);
```

### 3. 画像の読み込み

```csharp
// パスからTexture2Dを読み込み
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
if (texture == null)
{
    Debug.LogWarning($"[EditorBackground] Image not found: {imagePath}");
}
```

### 4. EditorPrefsのキー

```csharp
private const string KEY_ENABLED = "EditorBackground_Enabled";
private const string KEY_IMAGE_PATH = "EditorBackground_ImagePath";
private const string KEY_OPACITY = "EditorBackground_Opacity";
private const string KEY_SCALE_MODE = "EditorBackground_ScaleMode";
private const string KEY_TINT_COLOR = "EditorBackground_TintColor"; // ColorUtility使用
```

---

## 設定UIのレイアウト案

```
┌─────────────────────────────────────────────┐
│  Editor Background Settings                 │
├─────────────────────────────────────────────┤
│                                             │
│  [✓] Enable Background                      │
│                                             │
│  Image:  [    (None)    ▼] [Select...]     │
│                                             │
│  Opacity:  ════════●══════  0.08           │
│                                             │
│  Scale Mode:  [ScaleAndCrop ▼]             │
│                                             │
│  Tint Color:  [■ White     ]               │
│                                             │
│  ─────────────────────────────────────────  │
│                                             │
│  [ Apply ]  [ Reset to Default ]           │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 追加機能（オプション・優先度低）

将来的に追加可能な機能:

1. **複数画像のランダム/ローテーション表示**
2. **ウィンドウごとの個別設定**（SceneViewだけ違う画像など）
3. **画像のオフセット/位置調整**
4. **ブラー効果**（シェーダー使用）
5. **アニメーション対応**（スプライトシート）

---

## テスト項目

- [ ] 設定ウィンドウが開く
- [ ] 画像を設定すると全EditorWindowに表示される
- [ ] 透明度スライダーが機能する
- [ ] 有効/無効の切り替えが即座に反映される
- [ ] Unityを再起動しても設定が保持される
- [ ] 新しく開いたウィンドウにも背景が適用される
- [ ] ウィンドウを閉じてもエラーが出ない
- [ ] 通常のUI操作（ボタンクリック等）が背景に邪魔されない
- [ ] 画像が存在しないパスを設定してもクラッシュしない

---

## 参考

- Unity UIElements Documentation: https://docs.unity3d.com/Manual/UIElements.html
- EditorWindow.rootVisualElement: https://docs.unity3d.com/ScriptReference/EditorWindow-rootVisualElement.html
- VS Code Background Extension: https://marketplace.visualstudio.com/items?itemName=shalldie.background