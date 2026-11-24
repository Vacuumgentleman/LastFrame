using UnityEngine;

namespace Senses2026BonusContent
{
    public class RuntimeStats : MonoBehaviour
    {
        public int FontSize = 14;
        public Color TextColor = Color.white;
        public Vector2 ScreenOffset = new Vector2(10f, 10f);

        const int FPS_BUFFER = 120;  // last 120 frames (~2 seconds at 60fps)
        float[] fpsBuffer = new float[FPS_BUFFER];
        int fpsIndex = 0;
        bool bufferFilled = false;

        GUIStyle style;
        float smoothDelta;

        void Start()
        {
            style = new GUIStyle();
            style.fontSize = FontSize;
            style.normal.textColor = TextColor;
        }

        void Update()
        {
            float rawDelta = Time.unscaledDeltaTime;
            float rawFps = 1f / rawDelta;

            // Smooth only for displayed FPS
            smoothDelta += (rawDelta - smoothDelta) * 0.1f;

            // store in rolling buffer
            fpsBuffer[fpsIndex] = rawFps;
            fpsIndex++;

            if (fpsIndex >= FPS_BUFFER)
            {
                fpsIndex = 0;
                bufferFilled = true;
            }
        }

        (float min, float max, float avg) ComputeFpsStats()
        {
            int count = bufferFilled ? FPS_BUFFER : fpsIndex;

            float minFps = float.MaxValue;
            float maxFps = 0f;
            float sum = 0f;

            for (int i = 0; i < count; i++)
            {
                float f = fpsBuffer[i];
                sum += f;
                if (f < minFps) minFps = f;
                if (f > maxFps) maxFps = f;
            }

            return (minFps, maxFps, sum / count);
        }

        void OnGUI()
        {
            var (minFps, maxFps, avgFps) = ComputeFpsStats();
            float displayFps = 1f / smoothDelta;

            long mem = System.GC.GetTotalMemory(false) / (1024 * 1024);

            GUI.Label(
                new Rect(ScreenOffset.x,
                         Screen.height - 100 - ScreenOffset.y,
                         300, 100),
                $"FPS: {displayFps:0.0}\n" +
                $"Avg (Rolling): {avgFps:0.0}\n" +
                //$"Min/Max (Rolling): {minFps:0.0} / {maxFps:0.0}\n" +
                $"Memory: {mem} MB",
                style);
        }
    }
}
