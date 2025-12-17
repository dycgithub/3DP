using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel
{
    public Button storeButton;
    public Button startButton;
    public Button exitButton;
    
    private void Start()
    {
        base.OnInit();
        // 绑定事件
        storeButton.onClick.AddListener(() => {
            // 打开设置面板，设置面板会覆盖在当前面板之上
            UIManager.Instance.PushPanel("StorePanel", UILayer.Normal);
        });
    }
    
    public override void OnPause()
    {
        base.OnPause();
        Debug.Log("主菜单被暂停（失去交互）");
    }

    public override void OnResume()
    {
        base.OnResume();
        Debug.Log("主菜单恢复");
    }

    public void ShakeStoreB()
    {
        storeButton.transform.DOShakePosition(0.5f, new Vector3(0, 5, 0), 1, 90, false, true);
    }
    public void ShakeStartB()
    {
        startButton.transform.DOShakePosition(0.5f, new Vector3(0, 5, 0), 1, 90, false, true);
    }
    public void ShakeExitB()
    {
        exitButton.transform.DOShakePosition(0.5f, new Vector3(0, 5, 0), 1, 90, false, true);
    }
    
    public override void OnEnter()
    { 
        base.OnEnter();
        Vector3 targetPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = targetPosition - Vector3.up * 1000f;
        rectTransform.DOAnchorPos(targetPosition, 0.5f).SetEase(Ease.OutBack);
        
        RectTransform startRect = startButton.GetComponent<RectTransform>();
        RectTransform storeRect = storeButton.GetComponent<RectTransform>();
        RectTransform exitRect = exitButton.GetComponent<RectTransform>();
    
        // 保存原始位置
        Vector2 startPos = startRect.anchoredPosition;
        Vector2 storePos = storeRect.anchoredPosition;
        Vector2 exitPos = exitRect.anchoredPosition;
    
        // 设置起始位置在左侧屏幕外
        startRect.anchoredPosition = new Vector2(startPos.x + 500f, startPos.y);
        storeRect.anchoredPosition = new Vector2(storePos.x + 500f, storePos.y);
        exitRect.anchoredPosition = new Vector2(exitPos.x + 500f, exitPos.y);
    
        // 滑入动画
        startRect.DOAnchorPos(startPos, 0.4f).SetEase(Ease.OutBack);
        storeRect.DOAnchorPos(storePos, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);
        exitRect.DOAnchorPos(exitPos, 0.4f).SetEase(Ease.OutBack).SetDelay(0.2f);
    }

    public override void OnExit()
    {
        base.OnExit();
        rectTransform.DOAnchorPos(rectTransform.anchoredPosition - Vector2.up * 1000f, 0.5f)
            .SetEase(Ease.InQuart)
            .OnComplete(() => {
                canvasGroup.alpha = 0f;
            });
    }

}