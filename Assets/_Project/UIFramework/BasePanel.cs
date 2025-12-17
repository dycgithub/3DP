using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BasePanel : MonoBehaviour
{
    // 自身对应的 CanvasGroup，用于控制交互和透明度
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    
    public bool IsActive { get; private set; } = false;

    // 初始化：只执行一次
    public virtual void OnInit() 
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform= GetComponent<RectTransform>();
    }

    // 进栈/显示时调用
    public virtual void OnEnter() 
    { 
        IsActive = true;
        //gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true; // 开启交互
    }

    // 暂停：当有新界面压在上面时调用
    public virtual void OnPause() 
    { 
        canvasGroup.blocksRaycasts = false; // 禁用交互，但保持显示
    }

    // 恢复：当上层界面移除，重新成为栈顶时调用
    public virtual void OnResume() 
    { 
        canvasGroup.blocksRaycasts = true; // 恢复交互
    }

    // 出栈/关闭时调用
    public virtual void OnExit() 
    { 
        IsActive = false;
        //gameObject.SetActive(false);
    }
}