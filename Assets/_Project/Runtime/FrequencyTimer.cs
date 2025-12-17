using System;
using UnityEngine;

namespace ImprovedTimers
{
    /// <summary>
    /// 频率计时器
    /// </summary>
    public class FrequencyTimer : Timer
    {
        public int TickPerSecond{get;private set;}
        public Action OnTick=delegate{};
        private float timeThreshold;
        public FrequencyTimer(int ticksPerSecond) : base(0)
        {
            CalculateTimeThreshold(ticksPerSecond);
        }

        public override void Tick()
        {
            if (IsRunning && CurrentTime > timeThreshold)
            {
                CurrentTime-=timeThreshold;
                OnTick.Invoke();
            }
            if (IsRunning && CurrentTime <= timeThreshold)
            {
                CurrentTime += Time.deltaTime;
            }
        }
        
        public override bool IsFinished => !IsRunning;

        public override void Reset() {
            CurrentTime = 0;
        }
        
        public void Reset(int newTicksPerSecond) {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }
        void CalculateTimeThreshold(int ticksPerSecond)
        {
            TickPerSecond = ticksPerSecond;
            timeThreshold = 1f/ticksPerSecond;
        }
    }
}