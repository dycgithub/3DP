using System;
using UnityEngine;

namespace ImprovedTimers
{
    public class TimerTest:MonoBehaviour
    {
        private CountdownTimer tiemr1;
        [SerializeField]float timer1Duration=5f;

        private void Start()
        {
            tiemr1 = new CountdownTimer(timer1Duration);
            tiemr1.OnTimerStart +=()=>
            {
                Debug.Log("开始倒计时");
            };
            tiemr1.OnTimerStop += () =>
            {
                Debug.Log("倒计时结束");
            };
            tiemr1.Start();
        }
        //TODO:引用UI
        //TODO:在update中向UI中输入当前时间
        private void OnDestroy()
        {
            tiemr1.Dispose();
        }
    }
}