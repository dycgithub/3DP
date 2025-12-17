using UnityEngine;

namespace ImprovedTimers
{
    /// <summary>
    /// 倒计时
    /// </summary>
    public class CountdownTimer: Timer{
        
        
        public CountdownTimer(float value) : base(value)
        {
        }

        public override void Tick()
        {
            if(IsRunning&&CurrentTime>0)
                CurrentTime-=Time.deltaTime;
            if(IsRunning&&CurrentTime<=0)
                Stop();
        }

        public override bool IsFinished => CurrentTime<=0;
    }
}