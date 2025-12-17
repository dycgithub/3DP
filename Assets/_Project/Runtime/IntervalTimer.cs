using System;
using UnityEngine;

namespace ImprovedTimers
{
    /// <summary>
    /// 循环计时器
    /// </summary>
    public class IntervalTimer : Timer {
        readonly float interval;
        float nextInterval;

        public Action OnInterval = delegate { };

        public IntervalTimer(float totalTime, float intervalSeconds) : base(totalTime) {
            interval = intervalSeconds;
            nextInterval = totalTime - interval;
        }

        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;

                // 当阈值被跨越时，持续触发间隔事件
                while (CurrentTime <= nextInterval && nextInterval >= 0) {
                    OnInterval.Invoke();
                    nextInterval -= interval;
                }
            }

            if (IsRunning && CurrentTime <= 0) {
                CurrentTime = 0;
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;

        public override void Reset() {
            base.Reset();
            nextInterval = initialTime - interval;
        }

        public override void Reset(float newTime) {
            base.Reset(newTime);
            nextInterval = initialTime - interval;
        }
    }
}