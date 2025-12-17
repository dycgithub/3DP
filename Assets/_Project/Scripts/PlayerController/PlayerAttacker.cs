using UnityEngine;
using System;
using ImprovedTimers;

/// <summary>
/// 连招倒计时器
/// 为状态机转变提供信号
/// </summary>
[RequireComponent(typeof(PlayerControllerAdvanced))]
public class PlayerAttacker : MonoBehaviour
{
    private PlayerControllerAdvanced controller;
    private CountdownTimer comboWindowCountDownTimer;
    [Header("Attack Settings")]
    [SerializeField]private float comboWindowTime = 1f;//窗口时间
    [SerializeField] float attackDistance = 1f;
    [SerializeField] int attackDamage = 10;
    
    public bool IsFighting { get; set; } //战斗中
   
    [Header("Combo Settings")]
    [SerializeField] private int maxComboCount = 3; // 最大连招段数
    [SerializeField] private float resetComboTime = 0.5f; // 超过这个时间未攻击，重置连招

    private int currentComboIndex = 0;
    private bool nextAttackBuffered = false; // 是否预输入了下一次攻击
    // 传递 Combo 索引
    public event Action<int> OnAttackIndex = delegate { };//为动画传递索引
    
    private void Awake()
    {
        comboWindowCountDownTimer = new CountdownTimer(comboWindowTime);
        controller=GetComponent<PlayerControllerAdvanced>();
    }
    private void Start()
    {
       controller.input.Fire += OnFight;
       comboWindowCountDownTimer.OnTimerStop += ExitCombo;//一个招式播放完
    }

    void OnFight(bool fire)//按键输入
    {
        if (fire)
        {
            // 如果不在战斗状态，直接开始攻击
            if (!IsFighting)
            {
                StartCombo();
            }
            else
            {
                // 如果已经在战斗（攻击动画中），则缓冲下一次输入
                // 实际项目中通常会结合动画事件（Animation Event）来判断是否允许缓冲
                nextAttackBuffered = true;
                TryContinueCombo();
            }
        }
    }
    
    private void StartCombo()
    { 
        Debug.Log("开始战斗");
        IsFighting = true;
        currentComboIndex = 1;
        Attack();
    }
    
    // 尝试继续连招
    public void TryContinueCombo()
    {
        Debug.Log("尝试继续连招");
        if (nextAttackBuffered && IsFighting)
        {
            currentComboIndex++;
            OnAttackIndex?.Invoke(currentComboIndex);
            Attack();//计算攻击
        }

        if (currentComboIndex > 3)
        {
            nextAttackBuffered = false;
        }
    }
    
    private void ExitCombo() {
        if (!nextAttackBuffered) {
            // 重置所有连招状态
            IsFighting = false;
            currentComboIndex = 0;
            nextAttackBuffered = false;
            OnAttackIndex?.Invoke(0);
            Debug.Log("退出战斗状态");
        }
    }
    
    public void Attack()
    {
        Vector3 attackPos = transform.position + transform.forward;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);
        foreach (var enemy in hitEnemies)
        {
            Debug.Log(enemy.name);
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Health>().TakeDamage(attackDamage);
            }
        }
        OnAttackIndex?.Invoke(currentComboIndex); 
        
        nextAttackBuffered = false; // 消耗掉缓冲
        comboWindowCountDownTimer.Start();// 重置倒计时，等待下一次输入
    }
    
    
    
    private void OnDestroy()
    {
        controller.input.Fire -= OnFight;
    }

}