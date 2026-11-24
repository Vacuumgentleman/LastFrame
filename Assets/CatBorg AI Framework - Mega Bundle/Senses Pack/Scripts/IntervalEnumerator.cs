namespace Senses2026
{
    public struct IntervalEnumerator
    {
        private float interval;
        private readonly System.Action action;
        private float timer;

        public IntervalEnumerator(float interval, System.Action action)
        {
            this.interval = interval;
            this.action = action;
            this.timer = 0f;
        }

        public void MoveNext(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= interval)
            {
                action();
                timer = 0f;
            }
        }

        public void ChangeInterval(float newInterval)
        {
            interval = newInterval;
        }
    }
}

