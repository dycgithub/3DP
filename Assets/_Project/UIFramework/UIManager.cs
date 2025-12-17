using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 单例模式
    public static UIManager Instance { get; private set; }

    // UI 根节点 (Canvas) 下的层级父节点
    private Dictionary<UILayer, Transform> layerParents = new Dictionary<UILayer, Transform>();
    
    // 缓存已实例化的面板 (Key: PanelName)
    private Dictionary<string, BasePanel> panelCache = new Dictionary<string, BasePanel>();
    
    // UI 导航栈 (用于处理 Back 逻辑)
    private Stack<BasePanel> panelStack = new Stack<BasePanel>();

    private void Awake()
    {
        Instance = this;
        InitializeLayers();
        //DontDestroyOnLoad(gameObject);

    }

    // 初始化层级节点 (需要在 Canvas 下预设好或者代码生成)
    private void InitializeLayers()
    {
        Transform canvasTransform = GameObject.Find("Canvas").transform;
        // 假设 Canvas 下有名为 Bottom, Normal, Top, System 的空节点
        layerParents.Add(UILayer.Bottom, canvasTransform.Find("Bottom"));
        layerParents.Add(UILayer.Normal, canvasTransform.Find("Normal"));
        layerParents.Add(UILayer.Top, canvasTransform.Find("Top"));
        layerParents.Add(UILayer.System, canvasTransform.Find("System"));
    }

    // 打开面板 (入栈)
    public void PushPanel(string panelName, UILayer layer = UILayer.Normal)
    {
        // 1. 暂停当前栈顶面板
        if (panelStack.Count > 0)
        {
            BasePanel topPanel = panelStack.Peek();
            topPanel.OnPause();
        }

        // 2. 获取或加载面板
        BasePanel panel = GetPanel(panelName, layer);
        print($"打开面板：{panelName}");
        // 3. 执行面板进入逻辑
        panel.OnEnter();
        
        // 4. 入栈
        panelStack.Push(panel);
    }

    // 关闭面板 (出栈)
    public void PopPanel()
    {
        if (panelStack.Count == 0) return;

        // 1. 移除当前栈顶并退出
        BasePanel topPanel = panelStack.Pop();
        topPanel.OnExit();
        
        // 2. 恢复新的栈顶面板
        if (panelStack.Count > 0)
        {
            BasePanel nextPanel = panelStack.Peek();
            nextPanel.OnResume();
        }
    }

    // 获取面板实例 (对象池/缓存机制)
    private BasePanel GetPanel(string panelName, UILayer layer)
    {
        if (panelCache.TryGetValue(panelName, out BasePanel panel))
        {
            return panel;
        }

        // 加载资源 (此处使用 Resources，生产环境建议替换为 Addressables)
        if (!UIConfig.PanelPaths.TryGetValue(panelName, out string path))
        {
            Debug.LogError($"未配置路径: {panelName}");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null) return null;

        GameObject panelObj = Instantiate(prefab, layerParents[layer]);
        BasePanel panelComp = panelObj.GetComponent<BasePanel>();
        
        panelComp.OnInit(); // 初始化
        panelCache.Add(panelName, panelComp);
        
        return panelComp;
    }
}