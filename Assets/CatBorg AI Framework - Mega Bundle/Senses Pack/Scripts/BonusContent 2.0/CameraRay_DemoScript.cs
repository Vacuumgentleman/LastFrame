using UnityEngine;

namespace Senses2026
{
    public class CameraRay_DemoScript : MonoBehaviour
    {
        void Update()
        {
            if (SensesDemoInput.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.collider.CompareTag("WarningTable"))
                    {
                        if (hit.collider.TryGetComponent<WarningTable_DemoScript>(out WarningTable_DemoScript _warningTable_DemoScript))
                        {
                            _warningTable_DemoScript.DisplayText();
                        }
                    }
                }
            }
        }
    }
}

