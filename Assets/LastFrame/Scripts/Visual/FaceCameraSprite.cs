using UnityEngine;

namespace LastFrame.Visual
{
    /// <summary>
    /// Keeps a 2D character sprite facing the gameplay camera (it only pitches, it never yaws), so the
    /// sprite stays upright while its 3D owner turns. Optionally mirrors the sprite toward where the owner faces.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FaceCameraSprite : MonoBehaviour
    {
        [Tooltip("Mirror the sprite so it looks toward where the owner (the parent) faces on screen.")]
        [SerializeField] bool m_flipWithFacing = true;
        [Tooltip("The art is drawn looking to screen-left (otherwise to screen-right).")]
        [SerializeField] bool m_artFacesLeft;

        SpriteRenderer m_renderer;
        Camera m_camera;

        void Awake() => m_renderer = GetComponent<SpriteRenderer>();

        void LateUpdate()
        {
            if (m_camera == null) m_camera = Camera.main;
            if (m_camera == null) return;
            Vector3 _euler = m_camera.transform.eulerAngles;
            transform.rotation = Quaternion.Euler(_euler.x, _euler.y, 0f);
            if (!m_flipWithFacing || transform.parent == null) return;

            float _side = Vector3.Dot(transform.parent.forward, m_camera.transform.right);
            if (Mathf.Abs(_side) < 0.05f) return; // facing the camera or away: keep the last side
            bool _facesLeft = _side < 0f;
            m_renderer.flipX = _facesLeft != m_artFacesLeft;
        }
    }
}
