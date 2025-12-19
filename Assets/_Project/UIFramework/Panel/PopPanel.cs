using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 弹窗面板，支持弹出、淡入淡出和打字机效果
/// Pop-up panel with support for pop animation, fade in/out, and typewriter effect
/// </summary>
public class PopPanel : BasePanel
{
    [Header("UI Components")]
    [Tooltip("显示的文本内容组件 / Text component for display")]
    [SerializeField] private Text displayText;

    [Tooltip("TMP文本组件（可选，优先使用此项）/ TMP text component (optional, prioritized)")]
    [SerializeField] private TextMeshProUGUI displayTMP;

    [Header("Animation Settings")]
    [Tooltip("弹出动画时长 / Duration of pop animation")]
    [SerializeField] private float popDuration = 0.5f;

    [Tooltip("淡入淡出动画时长 / Duration of fade animation")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Tooltip("弹出动画缩放曲线 / Scale curve for pop animation")]
    [SerializeField] private Ease popEase = Ease.OutBack;

    [Header("Typewriter Effect Settings")]
    [Tooltip("是否启用打字机效果 / Whether to enable typewriter effect")]
    [SerializeField] private bool useTypewriterEffect = true;

    [Tooltip("打字速度（字符/秒）/ Typing speed (characters per second)")]
    [SerializeField] private float typewriterSpeed = 50f;

    [Tooltip("打字机音效 / Typewriter sound effect (optional)")]
    [SerializeField] private AudioClip typewriterSound;

    // 私有变量 / Private variables
    private AudioSource _audioSource;
    private Tween _currentTween;
    private Coroutine _typewriterCoroutine;

    /// <summary>
    /// 初始化面板 / Initialize panel
    /// </summary>
    public override void OnInit()
    {
        base.OnInit();

        // 获取或添加音频源 / Get or add audio source
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        // 初始化面板状态 / Initialize panel state
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
    }

    /// <summary>
    /// 面板进入时调用 / Called when panel enters
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 重置状态 / Reset state
        StopAllAnimations();

        // 播放进入动画 / Play entrance animation
        PlayEnterAnimation();
    }

    /// <summary>
    /// 面板暂停时调用 / Called when panel is paused
    /// </summary>
    public override void OnPause()
    {
        base.OnPause();

        // 停止打字机效果 / Stop typewriter effect
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
    }

    /// <summary>
    /// 面板恢复时调用 / Called when panel resumes
    /// </summary>
    public override void OnResume()
    {
        base.OnResume();

        // 如果需要，可以在这里恢复动画
    }

    /// <summary>
    /// 面板退出时调用 / Called when panel exits
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();

        // 播放退出动画 / Play exit animation
        PlayExitAnimation();
    }

    /// <summary>
    /// 设置并显示文本内容 / Set and display text content
    /// </summary>
    /// <param name="content">要显示的文本 / Text to display</param>
    public void SetTextContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return;

        // 检查是否有活跃的打字机效果 / Check if there's an active typewriter effect
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        // 如果启用打字机效果 / If typewriter effect is enabled
        if (useTypewriterEffect)
        {
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(content));
        }
        else
        {
            // 直接显示文本 / Display text directly
            SetTextInstantly(content);
        }
    }

    /// <summary>
    /// 立即设置文本 / Set text instantly
    /// </summary>
    private void SetTextInstantly(string content)
    {
        if (displayText != null)
        {
            displayText.text = content;
        }

        if (displayTMP != null)
        {
            displayTMP.text = content;
        }
    }

    /// <summary>
    /// 播放进入动画 / Play entrance animation
    /// </summary>
    private void PlayEnterAnimation()
    {
        // 重置状态 / Reset state
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;

        // 创建动画序列 / Create animation sequence
        Sequence sequence = DOTween.Sequence();

        // 淡入效果 / Fade in effect
        sequence.Append(canvasGroup.DOFade(1f, fadeDuration));

        // 弹出效果 / Pop effect
        sequence.Join(rectTransform.DOScale(1f, popDuration).SetEase(popEase));

        // 保存引用 / Save reference
        _currentTween = sequence;
    }

    /// <summary>
    /// 播放退出动画 / Play exit animation
    /// </summary>
    private void PlayExitAnimation()
    {
        // 停止当前动画 / Stop current animation
        if (_currentTween != null)
        {
            _currentTween.Kill();
        }

        // 创建动画序列 / Create animation sequence
        Sequence sequence = DOTween.Sequence();

        // 缩放和淡出 / Scale down and fade out
        sequence.Append(rectTransform.DOScale(0f, popDuration * 0.5f).SetEase(Ease.InBack));
        sequence.Join(canvasGroup.DOFade(0f, fadeDuration * 0.5f));

        // 动画完成后销毁面板 / Destroy panel after animation completes
        sequence.OnComplete(() =>
        {
            // 由UIManager处理面板的实际销毁
            // UIManager handles actual panel destruction
        });

        // 保存引用 / Save reference
        _currentTween = sequence;
    }

    /// <summary>
    /// 打字机效果协程 / Typewriter effect coroutine
    /// </summary>
    private IEnumerator TypewriterEffect(string content)
    {
        // 清空文本 / Clear text
        SetTextInstantly(string.Empty);

        // 计算字符间隔 / Calculate character interval
        float interval = 1f / typewriterSpeed;

        // 逐字显示 / Display character by character
        for (int i = 0; i <= content.Length; i++)
        {
            string currentText = content.Substring(0, i);

            if (displayText != null)
            {
                displayText.text = currentText;
            }

            if (displayTMP != null)
            {
                displayTMP.text = currentText;
            }

            // 播放音效 / Play sound effect
            if (typewriterSound != null && _audioSource != null && i > 0)
            {
                _audioSource.PlayOneShot(typewriterSound);
            }

            // 等待下一字符 / Wait for next character
            yield return new WaitForSeconds(interval);
        }

        _typewriterCoroutine = null;
    }

    /// <summary>
    /// 停止所有动画 / Stop all animations
    /// </summary>
    private void StopAllAnimations()
    {
        // 停止DOTween动画 / Stop DOTween animations
        if (_currentTween != null)
        {
            _currentTween.Kill();
            _currentTween = null;
        }

        // 停止打字机效果 / Stop typewriter effect
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
    }

    /// <summary>
    /// 设置打字速度 / Set typing speed
    /// </summary>
    /// <param name="speed">字符/秒 / Characters per second</param>
    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = Mathf.Max(1f, speed);
    }

    /// <summary>
    /// 启用或禁用打字机效果 / Enable or disable typewriter effect
    /// </summary>
    /// <param name="enable">是否启用 / Whether to enable</param>
    public void SetTypewriterEffect(bool enable)
    {
        useTypewriterEffect = enable;

        // 如果正在播放打字机效果且被禁用 / If typewriter effect is playing and being disabled
        if (!enable && _typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
    }

    /// <summary>
    /// 立即完成所有动画 / Complete all animations immediately
    /// </summary>
    public void CompleteAnimationsImmediately()
    {
        // 完成DOTween动画 / Complete DOTween animations
        if (_currentTween != null)
        {
            _currentTween.Complete();
        }

        // 如果正在播放打字机效果 / If typewriter effect is playing
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;

            // 显示完整文本 / Show full text
            // 这里需要获取原始文本，可以通过一个变量来保存
            // 或者让调用方传入完整文本
        }
    }

    /// <summary>
    /// 清理资源 / Clean up resources
    /// </summary>
    private void OnDestroy()
    {
        base.OnExit();

        // 停止所有动画 / Stop all animations
        StopAllAnimations();
    }
}