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
        public static string ClearImage => IsJapanese ? "クリア" : "Clear";
        public static string Opacity => IsJapanese ? "不透明度" : "Opacity";
        public static string OpacityTooltip => IsJapanese
            ? "背景画像の不透明度 (0 = 透明, 1 = 不透明)"
            : "Background image opacity (0 = transparent, 1 = opaque)";
        public static string ScaleMode => IsJapanese ? "スケールモード" : "Scale Mode";
        public static string ScaleModeTooltip => IsJapanese
            ? "ScaleAndCrop: 画面を覆うようにスケール\nScaleToFit: 画面に収まるようにスケール\nStretchToFill: 画面に合わせて引き伸ばし\nTile: タイル状に繰り返し"
            : "ScaleAndCrop: Scale to cover\nScaleToFit: Scale to fit\nStretchToFill: Stretch to fill\nTile: Repeat as tiles";
        public static string TileScale => IsJapanese ? "タイル倍率" : "Tile Scale";
        public static string TileScaleTooltip => IsJapanese
            ? "タイルの大きさ (0.1 = 小さく, 1 = 等倍, 5 = 大きく)"
            : "Tile size (0.1 = smaller, 1 = original, 5 = larger)";
        public static string TintColor => IsJapanese ? "色調" : "Tint Color";
        public static string TintColorTooltip => IsJapanese
            ? "背景画像に適用する色調"
            : "Color tint applied to background image";
        public static string GlobalMode => IsJapanese ? "グローバルモード" : "Global Mode";
        public static string GlobalModeTooltip => IsJapanese
            ? "ON: 全ウィンドウで1枚の背景を共有\nOFF: 各ウィンドウに個別の背景"
            : "ON: Share one background across all windows\nOFF: Individual background per window";

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
    }
}
