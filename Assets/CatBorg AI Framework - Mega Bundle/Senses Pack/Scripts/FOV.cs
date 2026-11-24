using UnityEngine;

#if (UNITY_EDITOR)
#endif
namespace Senses2026
{
    [ExecuteAlways]
    public class FOV : MonoBehaviour
    {
        Transform m_originTransform;
        LayerMask m_ignoreMe;
        bool m_current_state_FOV = false;
        MeshRenderer m_meshRenderer;
        MeshFilter m_meshFilter;
        float m_piDividedBy180;
        float m_mainFOV = 60;
        float m_previous_mainFOV;
        float m_viewDistance = 10f;
        float m_peripheralFOV = 10f;
        float m_previous_peripheralFOV;
        float m_peripheralviewDistance = 10f;
        bool m_usePeripheralFOV;
        bool m_previous_usePeripheralFOV;
        [Tooltip("Do not change during runtime")]
        [SerializeField]
        int m_rayCount;
        int[] m_rayCountArray = new int[3];
        float m_angleIncrease;
        float m_angle = 0f;
        Mesh m_mesh;
        Vector3[] m_verticles = new Vector3[3];
        Vector3[] m_initialAngleVectors = new Vector3[3];
        Vector2[] m_uv = new Vector2[3];
        int[] m_traingles = new int[3];
        Vector3 m_transformOfInterest_Position;
        Vector3 m_vertex;
        Vector3 m_angleVector;
        RaycastHit m_hit;
        Quaternion m_quaternion_RequiredRotation;
        Quaternion m_quaternion_TransformRotaion;
        Transform m_transformOfInterest;
        bool m_use_OverrrideRotation_CurrentValue;
        Transform m_previousActiveTransform;
        Transform m_activeTransform;
        bool m_wasEnabled = false;
        [SerializeField, HideInInspector]
        private Senses m_senses;
        private const string m_generatedMeshName = "Senses FOV (Generated)";
        SeeBooster m_seeBooster;
        bool m_useSeeBooster = false;
        int m_vertexIndex = 1;
        int m_triangleIndex = 0;
        public void Enable(Senses _senses)
        {
            if (_senses == null)
            {
                return;
            }

            int _rayCount = Mathf.Max(1, m_rayCount);
            if (m_wasEnabled && m_senses == _senses && m_mesh != null && m_verticles.Length == _rayCount + 2)
            {
                return;
            }

            m_senses = _senses;
            m_originTransform = _senses.transform;
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_meshFilter = GetComponent<MeshFilter>();
            if (m_meshRenderer == null || m_meshFilter == null)
            {
                return;
            }

            m_rayCount = _rayCount;
            m_useSeeBooster = _senses.TryGetComponent(out m_seeBooster);
            enable();
        }

        void enable()
        {
            m_piDividedBy180 = Mathf.PI / 180f;
            if (m_mesh == null)
            {
                string _meshName = m_generatedMeshName + " (" + GetInstanceID() + ")";
                Mesh _existingMesh = m_meshFilter.sharedMesh;
                bool _canReuse = _existingMesh != null && _existingMesh.name == _meshName && (_existingMesh.hideFlags & HideFlags.DontSave) == HideFlags.DontSave;
#if UNITY_EDITOR
                _canReuse = _canReuse && !UnityEditor.EditorUtility.IsPersistent(_existingMesh);
#endif
                if (_canReuse)
                {
                    m_mesh = _existingMesh;
                }
                else
                {
                    m_mesh = new Mesh();
                    m_mesh.name = _meshName;
                    m_mesh.hideFlags = HideFlags.DontSave;
                    m_mesh.MarkDynamic();
                }
            }

            m_meshFilter.sharedMesh = m_mesh;
            int _vertexCount = m_rayCount + 2;
            if (m_verticles.Length != _vertexCount)
            {
                m_mesh.Clear();
                m_verticles = new Vector3[_vertexCount];
                m_initialAngleVectors = new Vector3[_vertexCount];
                m_uv = new Vector2[_vertexCount];
                m_traingles = new int[m_rayCount * 3];
            }

            m_mainFOV = m_senses.Central_VisionAngle;
            m_usePeripheralFOV = m_senses.Use_PeripheralFOV;
            m_peripheralFOV = m_usePeripheralFOV ? m_senses.Peripheral_VisionAngle : 0f;
            m_angleIncrease = (m_mainFOV + m_peripheralFOV) / m_rayCount;
            transform.position = m_senses.Eyes != null ? m_senses.Eyes.position : m_senses.transform.position;
            m_previous_mainFOV = float.NaN;
            m_previous_peripheralFOV = float.NaN;
            m_previous_usePeripheralFOV = !m_usePeripheralFOV;
            calcualte_initialAngleVectors(m_usePeripheralFOV);
            m_verticles[0] = Vector3.zero;
            m_wasEnabled = true;
        }

        private void OnEnable()
        {
            if (m_senses != null)
            {
                Enable(m_senses);
            }
        }

        private void OnDestroy()
        {
            if (m_mesh == null)
            {
                return;
            }

            if (m_meshFilter != null && m_meshFilter.sharedMesh == m_mesh)
            {
                m_meshFilter.sharedMesh = null;
            }

            if (Application.isPlaying)
            {
                Destroy(m_mesh);
            }
            else
            {
                DestroyImmediate(m_mesh);
            }

            m_mesh = null;
        }

        void calcualte_initialAngleVectors(bool _usePeripheralFOV)
        {
            if (!_usePeripheralFOV)
            {
                m_angle = getFloatFromVector(Vector3.forward) - (m_mainFOV / 2f);
                for (int _i = 0; _i < m_initialAngleVectors.Length; _i++)
                {
                    m_initialAngleVectors[_i] = getVectorFromAngle(m_angle);
                    m_angle += m_angleIncrease;
                }
            }
            else
            {
                m_angle = getFloatFromVector(Vector3.forward) - (m_mainFOV / 2f) - (m_peripheralFOV / 2f);
                m_angleIncrease = (m_mainFOV + m_peripheralFOV) / m_rayCount;
                for (int _i = 0; _i < m_initialAngleVectors.Length; _i++)
                {
                    m_initialAngleVectors[_i] = getVectorFromAngle(m_angle);
                    m_angle += m_angleIncrease;
                }
            }
        }

        void LateUpdate()
        {
            if (!m_wasEnabled)
            {
                return;
            }

            if (m_senses == null)
            {
                return;
            }

            if (!m_senses.Use_See || !m_senses.isActiveAndEnabled)
            {
                setFOV(false);
                return;
            }

            m_ignoreMe = m_senses.IgnoreLayers;
            m_usePeripheralFOV = m_senses.Use_PeripheralFOV;
            m_mainFOV = m_senses.Central_VisionAngle;
            m_peripheralFOV = m_usePeripheralFOV ? m_senses.Peripheral_VisionAngle : 0;
            m_viewDistance = m_senses.Central_VisionRadius;
            m_peripheralviewDistance = m_senses.Peripheral_VisionRadius;
            if (m_useSeeBooster)
            {
                if (m_seeBooster.AffectedAspect == AffectedAsspect.cetralVisionRadius)
                {
                    m_viewDistance = m_seeBooster.Apply_BuffTo(m_viewDistance);
                }

                if (m_seeBooster.AffectedAspect == AffectedAsspect.peripheralVisionRadius)
                {
                    m_peripheralviewDistance = m_seeBooster.Apply_BuffTo(m_peripheralviewDistance);
                }
            }

            setFOV(m_senses.Use_DisplayFOV);
            if (!m_senses.Use_DisplayFOV)
            {
                return;
            }

            if (m_previous_mainFOV != m_mainFOV || m_previous_peripheralFOV != m_peripheralFOV)
            {
                m_previous_mainFOV = m_mainFOV;
                m_previous_peripheralFOV = m_peripheralFOV;
                m_angleIncrease = m_usePeripheralFOV ? (m_mainFOV + m_peripheralFOV) / m_rayCount : (m_mainFOV) / m_rayCount;
                calcualte_initialAngleVectors(m_usePeripheralFOV);
                if (m_usePeripheralFOV)
                {
                    m_rayCountArray[0] = (int)(((m_peripheralFOV / 2f) / (m_mainFOV + m_peripheralFOV)) * m_rayCount);
                    m_rayCountArray[1] = m_rayCount - m_rayCountArray[0];
                    m_rayCountArray[2] = m_rayCount;
                }
            }

            if (m_previous_usePeripheralFOV != m_usePeripheralFOV)
            {
                if (m_usePeripheralFOV)
                {
                    m_previous_mainFOV = m_mainFOV;
                    m_previous_peripheralFOV = m_peripheralFOV;
                }
                else
                {
                    m_previous_mainFOV = m_mainFOV;
                }

                m_angleIncrease = m_usePeripheralFOV ? (m_mainFOV + m_peripheralFOV) / m_rayCount : (m_mainFOV) / m_rayCount;
                m_previous_usePeripheralFOV = m_usePeripheralFOV;
                calcualte_initialAngleVectors(m_usePeripheralFOV);
            }

            refresf_FOV_MK3();
            transform.position = new Vector3(m_transformOfInterest_Position.x, m_transformOfInterest_Position.y, m_transformOfInterest_Position.z);
        }

        void draw_FOV(int i, float _distance)
        {
            /*
            m_angleVector = m_initialAngleVectors[i];
            if (m_use_OverrrideRotation_CurrentValue) m_angleVector = m_quaternion_RequiredRotation * m_angleVector;
            if (m_senses.Use_IgnoreRotationX) m_angleVector.y = 0f;
            m_angleVector = m_quaternion_TransformRotaion * m_angleVector;
            */
            m_angleVector = m_initialAngleVectors[i];
            if (m_use_OverrrideRotation_CurrentValue)
            {
                m_angleVector = m_quaternion_RequiredRotation * m_angleVector;
            }

            m_angleVector = m_quaternion_TransformRotaion * m_angleVector;
            if (m_senses.Use_IgnoreRotationX)
            {
                m_angleVector.y = 0f;
                m_angleVector.Normalize();
            }

            Physics.Raycast(m_transformOfInterest_Position, m_angleVector, out m_hit, _distance, ~m_ignoreMe);
            m_vertex = m_hit.collider == null ? (m_transformOfInterest_Position + (m_angleVector * _distance)) : m_hit.point;
            m_vertex -= m_transformOfInterest_Position;
            m_verticles[m_vertexIndex] = m_vertex;
            if (i > 0)
            {
                m_traingles[m_triangleIndex + 0] = m_vertexIndex;
                m_traingles[m_triangleIndex + 1] = m_vertexIndex - 1;
                m_traingles[m_triangleIndex + 2] = 0;
                m_triangleIndex += 3;
            }

            m_vertexIndex++;
        }

        void refresf_FOV_MK3()
        {
            if (!m_originTransform)
            {
                setFOV(false);
                return;
            }

            m_transformOfInterest = m_senses.Eyes != null ? m_senses.Eyes : m_senses.transform;
            m_transformOfInterest_Position = m_transformOfInterest.position;
            m_quaternion_TransformRotaion = m_transformOfInterest.rotation;
            if (m_senses.Use_OffsetPosition)
            {
                m_transformOfInterest_Position += m_senses.Offset_Position;
            }

            m_use_OverrrideRotation_CurrentValue = m_senses.Use_OffsetRotation;
            if (m_use_OverrrideRotation_CurrentValue)
            {
                m_quaternion_RequiredRotation = Quaternion.Euler(m_senses.Offset_Rotation);
            }

            m_vertexIndex = 1;
            m_triangleIndex = 0;
            if (m_usePeripheralFOV)
            {
                for (int _i = 0; _i <= m_rayCountArray[0]; _i++)
                {
                    draw_FOV(_i, m_peripheralviewDistance);
                }

                for (int _i = m_rayCountArray[0]; _i < m_rayCountArray[1]; _i++)
                {
                    draw_FOV(_i, m_viewDistance);
                }

                for (int _i = m_rayCountArray[1]; _i < m_rayCountArray[2]; _i++)
                {
                    draw_FOV(_i, m_peripheralviewDistance);
                }
            }
            else
            {
                for (int _i = 0; _i <= m_rayCount; _i++)
                {
                    draw_FOV(_i, m_viewDistance);
                }
            }

            m_mesh.vertices = m_verticles;
            m_mesh.uv = m_uv;
            m_mesh.triangles = m_traingles;
        }

        Vector3 getVectorFromAngle(float _angle)
        {
            float _angleRad = _angle * m_piDividedBy180;
            return new Vector3(Mathf.Cos(_angleRad), 0f, Mathf.Sin(_angleRad));
        }

        float getFloatFromVector(Vector3 _dir)
        {
            _dir = _dir.normalized;
            float _n = Mathf.Atan2(_dir.z, _dir.x) * Mathf.Rad2Deg;
            if (_n < 0)
            {
                _n += 360;
            }

            return _n;
        }

        public void SetFov_State(Senses _senses, bool _state)
        {
            if (_senses == null)
            {
                return;
            }

            Enable(_senses);
            m_activeTransform = _senses.transform;
            m_previousActiveTransform = m_activeTransform;
            setFOV(_state && _senses.Use_See && _senses.isActiveAndEnabled);
        }

        void setFOV(bool _state)
        {
            if (m_current_state_FOV == _state && m_meshRenderer != null && m_meshRenderer.enabled == _state)
            {
                return;
            }

            m_current_state_FOV = _state;
            if (m_meshRenderer != null)
            {
                m_meshRenderer.enabled = _state;
            }
        }

        public void StopDisplayingThisSense(Senses _senses)
        {
            if (m_senses == _senses)
            {
                setFOV(false);
                m_previousActiveTransform = null;
            }
        }
    }
}
