using System;
using System.Collections;
using UnityEngine;


namespace Senses2026
{
    public class ScentVisualisation : MonoBehaviour
    {
        [SerializeField] Transform m_transformToFollow;
        [SerializeField][Range(0f, 1f)] float m_speed = .5f;
        [SerializeField] TrailRenderer m_trailRenderer;
        [SerializeField] ParticleSystem m_particleSystem;
        [SerializeField] ScentType m_type;
        Transform m_transform;
        float m_offsetY = 0f;
        Vector3 m_desiredPosition;
        bool m_state = true;
        float m_scentStrength;

        public void Setup(Transform _transform, float _scentStrength, Gradient _scentColorGradient, float _offsetY)
        {
            m_transformToFollow = _transform;
            this.transform.position = _transform.position;
            m_scentStrength = _scentStrength;
            if (m_type == ScentType.TrailRenderer)
            {
                m_trailRenderer.time = _scentStrength;
                m_trailRenderer.colorGradient = _scentColorGradient;
            }
            if (m_type == ScentType.ParticleEffect)
            {
                m_particleSystem.Stop();
                var _main = m_particleSystem.main;
                _main.duration = _scentStrength;
                _main.startLifetime = _scentStrength;

                ParticleSystem.ColorOverLifetimeModule col = m_particleSystem.colorOverLifetime;
                col.color = _scentColorGradient;

                m_particleSystem.Play();
            }
            m_offsetY = _offsetY;

            reEnable();
        }
        public void Set_OffsetY(float _offsetY) => m_offsetY = _offsetY;

        internal void ClearTransform()
        {
            m_transformToFollow = null;
            StartCoroutine(ReturnToQueneAfterDisapear());
        }

        internal void SetEmmision(bool _state)      //nned to sync
        {
            if (_state != m_state)
            {
                m_state = _state;

                switch (m_type)
                {
                    case ScentType.TrailRenderer:
                        {
                            m_trailRenderer.emitting = _state;
                        }
                        break;
                    case ScentType.ParticleEffect:
                        {
                            var _emissionModule = m_particleSystem.emission;
                            _emissionModule.enabled = _state;
                        }
                        break;
                }
            }
        }

        internal void SetGradient(Gradient _scentColorGradient)
        {
            switch (m_type)
            {
                case ScentType.TrailRenderer:
                    {
                        m_trailRenderer.colorGradient = _scentColorGradient;
                    }
                    break;
                case ScentType.ParticleEffect:
                    {
                        ParticleSystem.ColorOverLifetimeModule _col = m_particleSystem.colorOverLifetime;
                        _col.color = _scentColorGradient;
                    }
                    break;
            }
        }

        internal void SetScentStrength(float _scentStrength)
        {
            switch (m_type)
            {
                case ScentType.TrailRenderer:
                    {
                        m_trailRenderer.time = _scentStrength;
                        m_trailRenderer.Clear();
                    }
                    break;
                case ScentType.ParticleEffect:
                    {
                        m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                        var _main = m_particleSystem.main;
                        _main.duration = _scentStrength;
                        _main.startLifetime = _scentStrength;

                        m_particleSystem.Play();
                    }
                    break;
            }
        }

        void Awake()
        {
            m_transform = this.transform;
        }
        void FixedUpdate()
        {
            if (m_transformToFollow == null) return;
            m_desiredPosition = Vector3.Slerp(m_transform.position, m_transformToFollow.position, m_speed);
            m_desiredPosition = new Vector3(m_desiredPosition.x, m_desiredPosition.y + m_offsetY, m_desiredPosition.z);

            m_transform.position = m_desiredPosition;
        }

        void reEnable()
        {
            switch (m_type)
            {
                case ScentType.TrailRenderer:
                    {
                        m_trailRenderer.Clear();
                        m_trailRenderer.emitting = true;
                    }
                    break;
                case ScentType.ParticleEffect:
                    {
                        var _emissionModule = m_particleSystem.emission;
                        _emissionModule.enabled = true;
                        m_particleSystem.Play();
                    }
                    break;
            }
        }


        IEnumerator ReturnToQueneAfterDisapear()
        {
            yield return new WaitForSecondsRealtime(m_scentStrength);
            reurnToQuene();
        }
        void reurnToQuene()
        {
            StopAllCoroutines();
            switch (m_type)
            {
                case ScentType.TrailRenderer:
                    {
                        m_trailRenderer.emitting = false;
                        m_trailRenderer.Clear();
                    }
                    break;
                case ScentType.ParticleEffect:
                    {
                        m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        var _emissionModule = m_particleSystem.emission;
                        _emissionModule.enabled = false;
                    }
                    break;
            }
            ScenVisualisatorsPoll.Instance.ReturnToPool(this);
        }
    }

}
