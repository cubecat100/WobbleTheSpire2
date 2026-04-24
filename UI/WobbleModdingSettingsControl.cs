#nullable enable
using Godot;
using MegaCrit.Sts2.Core.Modding;
using System;

namespace WobbleTheSpire2;

public partial class WobbleModdingSettingsControl : PanelContainer
{
    public const string NodeName = "WobbleModdingSettingsControl";

    private OptionButton _overallWobbleScale = null!;
    private CheckBox _enablePlayerWobble = null!;
    private CheckBox _blockBaseHitAnimation = null!;
    private CheckBox _disableWobbleOnDeath = null!;
    private CheckBox _enableHorizontalWobble = null!;
    private CheckBox _strongerWobble = null!;
    private CheckBox _longerWobble = null!;
    private Label _statusLabel = null!;

    public WobbleModdingSettingsControl()
    {
        Name = NodeName;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        AnchorLeft = 0.0f;
        AnchorTop = 0.0f;
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;
        OffsetLeft = 20.0f;
        OffsetTop = 110.0f;
        OffsetRight = -20.0f;
        OffsetBottom = -20.0f;
        Visible = false;
        AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color("18202b"), new Color("4d657f")));
        BuildUi();
    }

    public void Initialize()
    {
        LoadSettingsIntoInputs();
    }

    public void UpdateSelectedMod(Mod? mod)
    {
        bool isTargetMod = IsWobbleMod(mod);
        Visible = isTargetMod;
        if (isTargetMod == true)
        {
            LoadSettingsIntoInputs();
            _statusLabel.Text = string.Empty;
        }
        else
        {
            _statusLabel.Text = string.Empty;
        }
    }

    private void BuildUi()
    {
        MarginContainer margin = new();
        AddChild(margin);

        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);

        ScrollContainer scroll = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        margin.AddChild(scroll);

        VBoxContainer layout = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        layout.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(layout);

        Label header = new()
        {
            Text = "WobbleTheSpire2 Settings",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        header.AddThemeColorOverride("font_color", new Color("f6fbff"));
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        Label subtitle = new()
        {
            Text = "Creature hit wobble behavior can be adjusted here.",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        subtitle.AddThemeColorOverride("font_color", new Color("b8c7d9"));
        layout.AddChild(subtitle);

        _overallWobbleScale = CreateScaleOptionButton();
        AddScaleItem(_overallWobbleScale, "Very low (70%)", 70);
        AddScaleItem(_overallWobbleScale, "Low (85%)", 85);
        AddScaleItem(_overallWobbleScale, "Normal (100%)", 100);
        AddScaleItem(_overallWobbleScale, "High (115%)", 115);
        AddScaleItem(_overallWobbleScale, "Very high (130%)", 130);
        layout.AddChild(CreateOptionRow(
            "Overall wobble strength",
            _overallWobbleScale,
            "Adjusts the full wobble intensity for all hits. Default is High."));

        _enablePlayerWobble = CreateOptionCheckBox("Enable player wobble");
        layout.AddChild(CreateOptionRow(
            _enablePlayerWobble,
            "Applies the same wobble animation to the player character when hit. Default is on."));

        _blockBaseHitAnimation = CreateOptionCheckBox("Block original hit shake animation");
        layout.AddChild(CreateOptionRow(
            _blockBaseHitAnimation,
            "Keeps only the mod wobble for enemy hits, and also for the player when player wobble is enabled."));

        _disableWobbleOnDeath = CreateOptionCheckBox("Disable wobble on death");
        layout.AddChild(CreateOptionRow(
            _disableWobbleOnDeath,
            "If the target dies from that hit, WobbleTheSpire2 will not play and the base animation can remain."));

        _enableHorizontalWobble = CreateOptionCheckBox("Enable horizontal movement");
        layout.AddChild(CreateOptionRow(
            _enableHorizontalWobble,
            "Moves the body left and right during wobble. Default is off for a rotation-focused wobble."));

        _strongerWobble = CreateOptionCheckBox("Use stronger wobble");
        layout.AddChild(CreateOptionRow(
            _strongerWobble,
            "Increases wobble power for every damage tier, including big hits."));

        _longerWobble = CreateOptionCheckBox("Use longer wobble");
        layout.AddChild(CreateOptionRow(
            _longerWobble,
            "Extends the wobble duration so the motion feels slower and easier to notice."));

        Button applyButton = new()
        {
            Text = "Apply",
            CustomMinimumSize = new Vector2(0.0f, 34.0f),
            FocusMode = FocusModeEnum.None
        };
        applyButton.AddThemeStyleboxOverride("normal", CreatePanelStyle(new Color("2f4359"), new Color("89a9ca")));
        applyButton.AddThemeStyleboxOverride("hover", CreatePanelStyle(new Color("39516c"), new Color("b7d0eb")));
        applyButton.AddThemeStyleboxOverride("pressed", CreatePanelStyle(new Color("25374b"), new Color("89a9ca")));
        applyButton.AddThemeColorOverride("font_color", new Color("f7fbff"));
        applyButton.Pressed += OnApplyPressed;
        layout.AddChild(applyButton);

        _statusLabel = new Label
        {
            Text = string.Empty,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _statusLabel.AddThemeColorOverride("font_color", new Color("d8e6f4"));
        layout.AddChild(_statusLabel);
    }

    private void OnApplyPressed()
    {
        WobbleSettings settings = new()
        {
            OverallWobbleScalePercent = GetSelectedScaleValue(_overallWobbleScale, 115),
            EnablePlayerWobble = _enablePlayerWobble.ButtonPressed,
            BlockBaseHitAnimation = _blockBaseHitAnimation.ButtonPressed,
            DisableWobbleOnDeath = _disableWobbleOnDeath.ButtonPressed,
            EnableHorizontalWobble = _enableHorizontalWobble.ButtonPressed,
            StrongerWobble = _strongerWobble.ButtonPressed,
            LongerWobble = _longerWobble.ButtonPressed
        };

        WobbleSettingsManager.Save(settings);
        _statusLabel.Text = "Saved. Wobble settings updated.";
    }

    private void LoadSettingsIntoInputs()
    {
        WobbleSettings settings = WobbleSettingsManager.Current;
        SelectScaleValue(_overallWobbleScale, settings.OverallWobbleScalePercent);
        _enablePlayerWobble.ButtonPressed = settings.EnablePlayerWobble;
        _blockBaseHitAnimation.ButtonPressed = settings.BlockBaseHitAnimation;
        _disableWobbleOnDeath.ButtonPressed = settings.DisableWobbleOnDeath;
        _enableHorizontalWobble.ButtonPressed = settings.EnableHorizontalWobble;
        _strongerWobble.ButtonPressed = settings.StrongerWobble;
        _longerWobble.ButtonPressed = settings.LongerWobble;
    }

    private static OptionButton CreateScaleOptionButton()
    {
        OptionButton optionButton = new()
        {
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(0.0f, 34.0f),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        optionButton.AddThemeColorOverride("font_color", new Color("f5fbff"));
        optionButton.AddThemeColorOverride("font_hover_color", new Color("ffffff"));
        optionButton.AddThemeColorOverride("font_pressed_color", new Color("ffffff"));
        optionButton.AddThemeFontSizeOverride("font_size", 14);
        optionButton.AddThemeStyleboxOverride("normal", CreatePanelStyle(new Color("293747"), new Color("5f7b97")));
        optionButton.AddThemeStyleboxOverride("hover", CreatePanelStyle(new Color("324456"), new Color("89a9ca")));
        optionButton.AddThemeStyleboxOverride("pressed", CreatePanelStyle(new Color("22313f"), new Color("89a9ca")));
        return optionButton;
    }

    private static CheckBox CreateOptionCheckBox(string text)
    {
        CheckBox checkBox = new()
        {
            Text = text,
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(0.0f, 28.0f),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        checkBox.AddThemeColorOverride("font_color", new Color("f5fbff"));
        checkBox.AddThemeColorOverride("font_hover_color", new Color("ffffff"));
        checkBox.AddThemeColorOverride("font_pressed_color", new Color("ffffff"));
        checkBox.AddThemeFontSizeOverride("font_size", 15);
        return checkBox;
    }

    private static PanelContainer CreateOptionRow(CheckBox checkBox, string description)
    {
        PanelContainer container = new();
        container.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color("223040"), new Color("4f657f")));

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        container.AddChild(margin);

        VBoxContainer layout = new();
        layout.AddThemeConstantOverride("separation", 4);
        margin.AddChild(layout);
        layout.AddChild(checkBox);

        Label note = new()
        {
            Text = description,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        note.AddThemeColorOverride("font_color", new Color("b8c7d8"));
        note.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(note);
        return container;
    }

    private static PanelContainer CreateOptionRow(string title, OptionButton optionButton, string description)
    {
        PanelContainer container = new();
        container.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color("223040"), new Color("4f657f")));

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        container.AddChild(margin);

        VBoxContainer layout = new();
        layout.AddThemeConstantOverride("separation", 6);
        margin.AddChild(layout);

        Label header = new()
        {
            Text = title
        };
        header.AddThemeColorOverride("font_color", new Color("f5fbff"));
        header.AddThemeFontSizeOverride("font_size", 15);
        layout.AddChild(header);
        layout.AddChild(optionButton);

        Label note = new()
        {
            Text = description,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        note.AddThemeColorOverride("font_color", new Color("b8c7d8"));
        note.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(note);
        return container;
    }

    private static void AddScaleItem(OptionButton optionButton, string label, int value)
    {
        optionButton.AddItem(label);
        optionButton.SetItemMetadata(optionButton.ItemCount - 1, value);
    }

    private static int GetSelectedScaleValue(OptionButton optionButton, int fallback)
    {
        int selected = optionButton.Selected;
        if (selected < 0 || selected >= optionButton.ItemCount)
        {
            return fallback;
        }

        Variant metadata = optionButton.GetItemMetadata(selected);
        return metadata.VariantType == Variant.Type.Int
            ? metadata.AsInt32()
            : fallback;
    }

    private static void SelectScaleValue(OptionButton optionButton, int value)
    {
        for (int index = 0; index < optionButton.ItemCount; index++)
        {
            Variant metadata = optionButton.GetItemMetadata(index);
            if (metadata.VariantType == Variant.Type.Int && metadata.AsInt32() == value)
            {
                optionButton.Select(index);
                return;
            }
        }

        if (optionButton.ItemCount > 0)
        {
            optionButton.Select(0);
        }
    }

    private static StyleBoxFlat CreatePanelStyle(Color bgColor, Color borderColor)
    {
        return new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = borderColor,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            ContentMarginBottom = 8,
            ContentMarginLeft = 8,
            ContentMarginRight = 8,
            ContentMarginTop = 8
        };
    }

    private static bool IsWobbleMod(Mod? mod)
    {
        if (mod == null)
        {
            return false;
        }

        return string.Equals(WobbleModReflection.GetModId(mod), "WobbleTheSpire2", StringComparison.OrdinalIgnoreCase);
    }
}
