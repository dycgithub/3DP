using System.Collections.Generic;

// UI 层级定义
public enum UILayer
{
    Bottom,     // 底层 (如主界面背景)
    Normal,     // 普通层 (如背包、商店)
    Top,        // 顶层 (如弹窗、提示框)
    System      // 系统层 (如Loading、断线重连)
}

public static class UIConfig
{
    // Key: 面板名称, Value: Resources 路径
    public static readonly Dictionary<string, string> PanelPaths = new Dictionary<string, string>
    {
        { "MainMenuPanel", "Prefabs/UI/MainMenuPanel" },
        { "StorePanel",     "Prefabs/UI/StorePanel" },
        { "TopicPanel",     "Prefabs/UI/TopicPanel" },
        { "SettingsPanel", "Prefabs/UI/SettingsPanel" },
        { "PopupPanel",    "Prefabs/UI/PopupPanel" }
    };
}