using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class StorePanel : BasePanel
{
    public List<Image> Images;
    public Button backButton;
    private float fadeTime = 1f;

    private void Awake()
    {
        base.OnInit();
        
        backButton.onClick.AddListener(() => {
            // 关闭自己，返回上一级
            UIManager.Instance.PopPanel();
        });
    }

    public override void OnEnter()
    {
        base.OnEnter();
        PanelFadeIn();
        StartCoroutine(ImageLoadAnimation(Images));
    }

    public override void OnExit()
    {
        PanelFadeOut();
        base.OnExit();
    }
    
    IEnumerator ImageLoadAnimation(List<Image>  images)
    {
        foreach (var image in images)
        {
            image.transform.localScale = Vector3.zero;
        }
        foreach (var image in images)
        {
            
            image.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void PanelFadeIn()
    {
        canvasGroup.alpha = 0f;
        rectTransform. transform. localPosition = new Vector3(0f, -1000f, 6f);
        rectTransform.DOAnchorPos(new Vector2(6f, 6f), fadeTime, false).SetEase(Ease.OutElastic);
        canvasGroup.DOFade(1, fadeTime);
    }
    public void PanelFadeOut()
    {
        canvasGroup. alpha = 1;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos (new Vector2(0f, -1600f), fadeTime, false).SetEase(Ease.OutElastic);
        canvasGroup.DOFade(1, fadeTime);
    }
}