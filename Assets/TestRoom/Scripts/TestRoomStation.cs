using UnityEngine;

namespace SensesTestRoom
{
    /// <summary>
    /// Una "estación" de la test room: agrupa el contenido de una demo del Senses Pack
    /// y le dice al TestRoomManager qué mostrar/activar cuando el jugador está dentro.
    /// </summary>
    public class TestRoomStation : MonoBehaviour
    {
        [Header("Info")]
        public string Title;
        [TextArea(3, 8)] public string Description;
        [TextArea(1, 4)] public string ControlsHint;

        [Header("Zona (en X, coordenadas de mundo)")]
        public float MinX;
        public float MaxX;
        public Transform SpawnPoint;
        [Tooltip("Opcional: la cámara sigue a este punto en vez de al jugador (útil con FreezePlayer).")]
        public Transform CameraFocus;

        [Header("Qué se activa al estar dentro")]
        [Tooltip("Canvas de la demo original: solo se muestra el de la estación actual.")]
        public GameObject[] UiRoots = new GameObject[0];
        [Tooltip("Objetos que arrancan apagados y se encienden la primera vez que entras (y ya no se apagan).")]
        public GameObject[] ActivateOnFirstEnter = new GameObject[0];
        [Tooltip("Componentes que solo funcionan mientras estás en la estación.")]
        public Behaviour[] EnabledOnlyInside = new Behaviour[0];
        [Tooltip("Congela al jugador (por ejemplo cuando WASD controla otros personajes).")]
        public bool FreezePlayer;

        [Header("Iluminación")]
        public bool OverrideLighting;
        public float LightIntensity = 4f;
        public Color LightColor = Color.white;
        public Material Skybox;

        public bool Contains(float _x) => _x >= MinX && _x < MaxX;

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Vector3 _center = new Vector3((MinX + MaxX) * 0.5f, 0.05f, transform.position.z + 5f);
            Gizmos.DrawWireCube(_center, new Vector3(MaxX - MinX, 0.1f, 50f));
        }
    }
}
