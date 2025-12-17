using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerControllerAdvanced))]
public class AnimationController : MonoBehaviour
{
    PlayerControllerAdvanced controller;
    Animator animator;
    
    protected const float crossFadeDuration = 0.1f;//动画切换

    #region 动画

    protected static readonly int Combo1Hash = Animator.StringToHash("Combo1");
    protected static readonly int Combo2Hash = Animator.StringToHash("Combo2");
    protected static readonly int Combo3Hash = Animator.StringToHash("Combo3");
    protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");

    #endregion

    #region 变量

    readonly int speedHash = Animator.StringToHash("Speed");
    readonly int isJumpingHash = Animator.StringToHash("IsJumping");

    #endregion

    #region 特效

    [SerializeField] private ParticleSystem jumpEffect;

    #endregion
    
    void Start() {
        controller = GetComponent<PlayerControllerAdvanced>();
        animator = GetComponentInChildren<Animator>();
            
        controller.OnJump += HandleJump;
        controller.OnLand += HandleLand;
        
        controller.attacker.OnAttackIndex += HandleComboIndex;
        
    }

    void Update() {
        animator.SetFloat(speedHash, controller.GetMovementVelocity().magnitude);
    }

    void HandleJump(Vector3 momentum)
    {
        animator.SetBool(isJumpingHash, true);
        if (jumpEffect != null)  // 添加空值检查
        {
            jumpEffect.Play();
        }
    }
    void HandleLand(Vector3 momentum) => animator.SetBool(isJumpingHash, false);
    
    void HandleComboIndex(int i)
    {
        switch (i)
        {
            case 1:
                animator.CrossFade(Combo1Hash, crossFadeDuration);
                break;
            case 2:
                animator.CrossFade(Combo2Hash, crossFadeDuration);
                break;
            case 3:
                animator.CrossFade(Combo3Hash, crossFadeDuration);
                break;
            default:
                animator.CrossFade(LocomotionHash, crossFadeDuration);
                break;
        }
    }
    
    void OnDestroy()
    {
        if (controller != null)
        {
            controller.OnJump -= HandleJump;
            controller.OnLand -= HandleLand;
        
            if (controller.attacker != null)
            {
                controller.attacker.OnAttackIndex -= HandleComboIndex;
            }
        }
    }
}