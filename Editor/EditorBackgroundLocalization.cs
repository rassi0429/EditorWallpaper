namespace EditorBackground
{
    /// <summary>
    /// ローカライズ文字列を管理するクラス
    /// </summary>
    public static class Localization
    {
        private static bool IsJapanese => EditorBackgroundSettings.CurrentLanguage == EditorBackgroundSettings.Language.Japanese;

        // ウィンドウタイトル
        public static string WindowTitle => IsJapanese ? "エディタ背景設定" : "Editor Background";

        // メインタイトル
        public static string MainTitle => IsJapanese ? "エディタ背景設定" : "Editor Background Settings";

        // 言語
        public static string Language => IsJapanese ? "言語" : "Language";
        public static string LanguageJapanese => "日本語";
        public static string LanguageEnglish => "English";

        // 基本設定
        public static string EnableBackground => IsJapanese ? "背景を有効化" : "Enable Background";

        // 背景画像セクション
        public static string BackgroundImageSection => IsJapanese ? "背景画像" : "Background Image";
        public static string Image => IsJapanese ? "画像" : "Image";
        public static string SelectFile => IsJapanese ? "ファイルを選択" : "Select File";
        public static string SelectImageFile => IsJapanese ? "画像ファイルを選択" : "Select Image File";
        public static string ClearImage => IsJapanese ? "クリア" : "Clear";
        public static string Opacity => IsJapanese ? "不透明度" : "Opacity";
        public static string OpacityTooltip => IsJapanese
            ? "背景画像の不透明度 (0 = 透明, 1 = 不透明)"
            : "Background image opacity (0 = transparent, 1 = opaque)";
        public static string ScaleMode => IsJapanese ? "スケールモード" : "Scale Mode";
        public static string ScaleModeTooltip => IsJapanese
            ? "拡大してクロップ: 画面を覆うようにスケール\n収まるように縮小: 画面に収まるようにスケール\n引き伸ばし: 画面に合わせて引き伸ばし\nタイル: タイル状に繰り返し\n角に配置: 角基準で1枚配置"
            : "Scale and Crop: Scale to cover\nScale to Fit: Scale to fit\nStretch to Fill: Stretch to fill\nTile: Repeat as tiles\nCorner: Single image at corner";
        public static string TileScale => IsJapanese ? "画像倍率" : "Image Scale";
        public static string TileScaleTooltip => IsJapanese
            ? "画像の大きさ (0.01 = 小さく, 1 = 等倍, 5 = 大きく)"
            : "Image size (0.01 = smaller, 1 = original, 5 = larger)";
        public static string CornerPositionLabel => IsJapanese ? "配置位置" : "Position";
        public static string CornerPositionTooltip => IsJapanese
            ? "画像を配置する角の位置"
            : "Corner position for the image";

        // スケールモード選択肢
        public static string[] ScaleModeOptions => IsJapanese
            ? new[] { "拡大してクロップ", "収まるように縮小", "引き伸ばし", "タイル", "角に配置" }
            : new[] { "Scale and Crop", "Scale to Fit", "Stretch to Fill", "Tile", "Corner" };

        // コーナー位置選択肢
        public static string[] CornerPositionOptions => IsJapanese
            ? new[] { "左上", "右上", "左下", "右下" }
            : new[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };

        // オフセット
        public static string OffsetX => IsJapanese ? "X オフセット" : "X Offset";
        public static string OffsetY => IsJapanese ? "Y オフセット" : "Y Offset";
        public static string OffsetTooltip => IsJapanese
            ? "画像の位置を微調整 (-500 〜 500)"
            : "Fine-tune image position (-500 to 500)";

        public static string TintColor => IsJapanese ? "色調" : "Tint Color";
        public static string TintColorTooltip => IsJapanese
            ? "背景画像に適用する色調"
            : "Color tint applied to background image";
        public static string GlobalMode => IsJapanese ? "グローバルモード" : "Global Mode";
        public static string GlobalModeTooltip => IsJapanese
            ? "ON: 全ウィンドウで1枚の背景を共有\nOFF: 各ウィンドウに個別の背景"
            : "ON: Share one background across all windows\nOFF: Individual background per window";
        public static string GlobalModeHint => IsJapanese
            ? "ON: 画面全体で1枚の壁紙を表示\nOFF: 各ウィンドウに個別の壁紙を表示"
            : "ON: Display one wallpaper across the entire screen\nOFF: Display individual wallpaper per window";

        // カラーオーバーレイセクション
        public static string ColorOverlaySection => IsJapanese ? "カラーオーバーレイ" : "Color Overlay";
        public static string EnableOverlay => IsJapanese ? "オーバーレイを有効化" : "Enable Overlay";
        public static string EnableOverlayTooltip => IsJapanese
            ? "ウィンドウ全体に半透明のカラーを重ねる"
            : "Apply semi-transparent color over window";
        public static string OverlayColor => IsJapanese ? "オーバーレイカラー" : "Overlay Color";
        public static string OverlayColorTooltip => IsJapanese
            ? "オーバーレイの色（アルファ値で透明度を調整）"
            : "Overlay color (adjust transparency with alpha)";

        // ボーダーセクション
        public static string BorderSection => IsJapanese ? "ウィンドウボーダー" : "Window Border";
        public static string EnableBorder => IsJapanese ? "ボーダーを有効化" : "Enable Border";
        public static string EnableBorderTooltip => IsJapanese
            ? "ウィンドウの縁にカラーボーダーを追加"
            : "Add color border around window edges";
        public static string BorderColor => IsJapanese ? "ボーダーカラー" : "Border Color";
        public static string BorderColorTooltip => IsJapanese ? "ボーダーの色" : "Border color";
        public static string BorderWidth => IsJapanese ? "ボーダー幅" : "Border Width";
        public static string BorderWidthTooltip => IsJapanese ? "ボーダーの太さ (1-10px)" : "Border width (1-10px)";

        // ボタン
        public static string ResetToDefault => IsJapanese ? "初期設定に戻す" : "Reset to Default";
        public static string ResetConfirmTitle => IsJapanese ? "設定のリセット" : "Reset Settings";
        public static string ResetConfirmMessage => IsJapanese
            ? "すべての設定を初期値に戻しますか？"
            : "Are you sure you want to reset all settings to default?";
        public static string Yes => IsJapanese ? "はい" : "Yes";
        public static string No => IsJapanese ? "いいえ" : "No";

        // 画像情報
        public static string ImageInfo => IsJapanese ? "画像情報" : "Image Info";
        public static string ImageName => IsJapanese ? "ファイル名" : "Name";
        public static string ImageSize => IsJapanese ? "サイズ" : "Size";

        // プレミアム機能
        public static string PremiumSection => IsJapanese ? "サポーター特典" : "Supporter Features";
        public static string PremiumNotAvailable => IsJapanese
            ? "この機能はサポーター特典です。\nSUPPORTERファイルを配置すると有効になります。"
            : "This feature is for supporters.\nPlace SUPPORTER file to enable.";
        public static string ImageSourceMode => IsJapanese ? "画像ソース" : "Image Source";
        public static string ImageSourceModeTooltip => IsJapanese
            ? "単一画像またはフォルダから画像を選択"
            : "Select image from single file or folder";
        public static string[] ImageSourceModeOptions => IsJapanese
            ? new[] { "画像ファイル", "フォルダ" }
            : new[] { "Image File", "Folder" };

        public static string ImageFolder => IsJapanese ? "画像フォルダ" : "Image Folder";
        public static string ImageFolderTooltip => IsJapanese
            ? "スライドショーやランダム表示に使用する画像フォルダ"
            : "Image folder for slideshow and random display";
        public static string SelectFolder => IsJapanese ? "フォルダを選択" : "Select Folder";
        public static string SlideshowEnabled => IsJapanese ? "スライドショー" : "Slideshow";
        public static string RandomizeImage => IsJapanese ? "ランダムに画像を変更" : "Randomize Image";
        public static string RandomizeImageTooltip => IsJapanese
            ? "フォルダからランダムな画像を選択"
            : "Select random image from folder";
        public static string SlideshowEnabledTooltip => IsJapanese
            ? "一定時間ごとに画像を切り替える"
            : "Change image at regular intervals";
        public static string SlideshowInterval => IsJapanese ? "切り替え間隔 (秒)" : "Interval (sec)";
        public static string SlideshowIntervalTooltip => IsJapanese
            ? "画像を切り替える間隔（5〜300秒）"
            : "Interval to change images (5-300 seconds)";
        public static string RandomPerWindow => IsJapanese ? "ウィンドウごとにランダム" : "Random per Window";
        public static string RandomPerWindowTooltip => IsJapanese
            ? "グローバルモードOFF時、各ウィンドウにランダムな画像を表示"
            : "Display random image per window when Global Mode is OFF";
        public static string FolderImageCount => IsJapanese ? "フォルダ内の画像数" : "Images in folder";
    }
}
