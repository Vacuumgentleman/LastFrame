using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;

#endif
namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 21: Rig Collider", 21)]
    public class RigCollider : MonoBehaviour
    {
        [field: SerializeField]
        public int Length { get; private set; }

        public void Set_Length(int _length)
        {
            _length = Mathf.Max(0, _length);
            Transform[] _transforms = m_transformsForColliders;
            Collider[] _colliders = Colliders;
            if (_transforms == null || _transforms.Length != _length)
            {
                System.Array.Resize(ref _transforms, _length);
            }

            if (_colliders == null || _colliders.Length != _length)
            {
                System.Array.Resize(ref _colliders, _length);
            }

            m_transformsForColliders = _transforms;
            Colliders = _colliders;
            Length = _length;
        }

        [field: SerializeField]
        public Transform[] m_transformsForColliders { get; private set; }

        public void Set_Transform(int _index, Transform _transformForCollider)
        {
            if (_index < 0 || _index >= Length)
            {
                return;
            }

            m_transformsForColliders[_index] = _transformForCollider;
            Colliders[_index] = _transformForCollider != null ? _transformForCollider.GetComponent<Collider>() : null;
        }

        [field: SerializeField]
        public Collider[] Colliders { get; private set; }

        List<Transform> m_transformForFutureColliders = new List<Transform>();
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;

        [field: SerializeField]
        bool m_helpPropertiesFoldout;
        public void ToggleHelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
        public void AddCollider(int _index)
        {
            if (m_transformsForColliders[_index] == null)
            {
                return;
            }

            if (m_transformsForColliders[_index].TryGetComponent<Collider>(out Collider _collider))
            {
                return;
            }

            if (m_transformsForColliders[_index].childCount == 0)
            {
                return;
            }

            if (m_transformsForColliders[_index].childCount == 1)
            {
                attachCollider(m_transformsForColliders[_index], _index);
            }
            else
            {
                attachColliderToEncapsulateChildren(m_transformsForColliders[_index], _index);
            }
        }

        public void RemoveCollider(int _index)
        {
            if (m_transformsForColliders[_index] == null)
            {
                return;
            }

            if (m_transformsForColliders[_index].TryGetComponent<Collider>(out Collider _collider))
            {
                if (Application.isPlaying)
                {
                    Destroy(_collider);
                }
                else
                {
                    DestroyImmediate(_collider);
                }

                Colliders[_index] = null;
#if (UNITY_EDITOR)
                EditorUtility.SetDirty(this);
#endif
            }
        }

        void attachCollider(Transform _transform, int _index)
        {
            Transform _childTransform = _transform.GetChild(0);
            Vector3 _position = _transform.position;
            Vector3 _childPosition = _childTransform.position;
            _position = _transform.parent.InverseTransformPoint(_position);
            _childPosition = _transform.parent.InverseTransformPoint(_childPosition);
            Vector3 _direction = _childPosition - _position;
            float _magnitude = _direction.magnitude;
            _direction = _direction.normalized;
            // Ensure magnitude is positive
            _magnitude = Mathf.Abs(_magnitude);
            BoxCollider _boxCollider = null;
            if (_transform.TryGetComponent<BoxCollider>(out _boxCollider))
            {
                _boxCollider.size = new Vector3(.001f, _magnitude, .001f);
                _boxCollider.center = new Vector3(0f, _magnitude / 2f, 0f);
            }
            else
            {
                _boxCollider = _transform.gameObject.AddComponent<BoxCollider>();
                _boxCollider.size = new Vector3(.001f, _magnitude, .001f);
                _boxCollider.center = new Vector3(0f, _magnitude / 2f, 0f);
            }

            Colliders[_index] = _boxCollider;
        }

        void attachColliderToEncapsulateChildren(Transform _transform, int _index)
        {
            Transform _currentTransform = _transform;
            Vector3 _accumulatedScale = Vector3.one;
            while (_currentTransform != null)
            {
                _accumulatedScale.x *= _currentTransform.localScale.x;
                _accumulatedScale.y *= _currentTransform.localScale.y;
                _accumulatedScale.z *= _currentTransform.localScale.z;
                _currentTransform = _currentTransform.parent;
            }

            Vector3[] _childPositions = new Vector3[_transform.childCount];
            Vector3 _localCenter = Vector3.zero;
            for (int _i = 0; _i < _transform.childCount; _i++)
            {
                _childPositions[_i] = _transform.GetChild(_i).position;
                _localCenter += _transform.GetChild(_i).localPosition;
            }

            _localCenter /= _transform.childCount - 1;
            Bounds _bounds = new Bounds(_transform.position, Vector3.zero);
            foreach (Vector3 _pos in _childPositions)
            {
                _bounds.Encapsulate(_pos);
            }

            _bounds.size = new Vector3(_bounds.size.x / _accumulatedScale.x, _bounds.size.y / _accumulatedScale.y, _bounds.size.z / _accumulatedScale.z);
            Vector3 _size = _bounds.size;
            BoxCollider _boxCollider = _transform.GetComponent<BoxCollider>();
            if (_boxCollider == null)
            {
                _boxCollider = _transform.gameObject.AddComponent<BoxCollider>();
            }

            _boxCollider.center = _localCenter;
            _boxCollider.size = _size;
            Colliders[_index] = _boxCollider;
        }

        public void RemoveAllColliders()
        {
            for (int _i = 0; _i < m_transformsForColliders.Length; _i++)
            {
                RemoveCollider(_i);
            }
        }

        public void AddAllColliders()
        {
            for (int _i = 0; _i < Colliders.Length; _i++)
            {
                AddCollider(_i);
            }
        }

        public void FindAllCollidersAndGrabReferencs()
        {
            m_transformForFutureColliders.Clear();
            checkChildForCollider(this.transform);
            Set_Length(m_transformForFutureColliders.Count);
            for (int _i = 0; _i < m_transformForFutureColliders.Count; _i++)
            {
                Set_Transform(_i, m_transformForFutureColliders[_i]);
            }
        }

        void checkChildForCollider(Transform parent)
        {
            foreach (Transform _child in parent)
            {
                Collider _collider = _child.GetComponent<Collider>();
                if (_collider != null)
                {
                    m_transformForFutureColliders.Add(_child);
                }

                checkChildForCollider(_child);
            }
        }
    }
}
