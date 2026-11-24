using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Senses2026
{

    public sealed class NoiseRecivers
    {
        private static volatile NoiseRecivers instance;
        private static object syncRoot = new System.Object();
        public static NoiseRecivers Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NoiseRecivers();
                    }
                }

                return instance;
            }
        }

        List<Senses> m_senses_hear_List = new List<Senses>();
        List<Transform> m_senses_hearTransform_List = new List<Transform>();
        List<float> m_recalcuated_dB_Values = new List<float>();
        float m_distance;
        int m_senses_hearTransform_List_Count;
        float m_recalcuated_dB_Value;

        NavMeshPath m_path;
        bool m_wasEnabled = false;
        void enable()
        {
            m_path = new NavMeshPath();
            m_wasEnabled = true;
        }

        public void RegisterAsNoiseReciver(Senses _senses)
        {
            m_senses_hear_List.Add(_senses);
            m_senses_hearTransform_List.Add(_senses.transform);
            m_recalcuated_dB_Values.Add(0f);
            m_senses_hearTransform_List_Count = m_senses_hearTransform_List.Count;
        }
        public void Remove_NoiseReciver(Senses _senses)
        {
            if (!m_senses_hear_List.Contains(_senses)) return;
            int _index = m_senses_hear_List.IndexOf(_senses);
            m_senses_hear_List.RemoveAt(_index);
            m_senses_hearTransform_List.RemoveAt(_index);
            m_recalcuated_dB_Values.RemoveAt(_index);
            m_senses_hearTransform_List_Count = m_senses_hearTransform_List.Count;
        }

        Vector3 m_position;
        public void SpreadNoise(float _noise_dB, Vector3 _noise_Position, TargetSenses _originTargetSenses, bool _useNaveMesh, bool _spreadAccordingToDistance)
        {

            m_recalcuated_dB_Value = 0f;

            if (!_spreadAccordingToDistance)
            {

                for (int _indexOfReciver = 0; _indexOfReciver < m_senses_hearTransform_List_Count; _indexOfReciver++)
                {
                    m_position = m_senses_hearTransform_List[_indexOfReciver].position;
                    m_recalcuated_dB_Value = recalcuated_dB_Value(_noise_dB, _noise_Position, m_position);

                    if (m_recalcuated_dB_Value > 0)
                    {
                        if (_useNaveMesh)
                        {
                            m_recalcuated_dB_Value = recalcuated_dB_ValueAccordingToNaveMesh(_noise_dB, _noise_Position, m_position);
                        }
                        m_senses_hear_List[_indexOfReciver].Recived_Noise(m_recalcuated_dB_Value, _originTargetSenses);
                    }

                }
            }
            else
            {

                for (int _indexOfReciver = 0; _indexOfReciver < m_senses_hearTransform_List_Count; _indexOfReciver++)
                {
                    m_position = m_senses_hearTransform_List[_indexOfReciver].position;

                    m_recalcuated_dB_Value = recalcuated_dB_Value(_noise_dB, _noise_Position, m_position);
                    if (m_recalcuated_dB_Value > 0)
                    {
                        if (_useNaveMesh)
                        {
                            m_recalcuated_dB_Value = recalcuated_dB_ValueAccordingToNaveMesh(_noise_dB, _noise_Position, m_position);
                        }
                        m_recalcuated_dB_Values[_indexOfReciver] = m_recalcuated_dB_Value;
                    }
                }

                spreadNoiseAccordingTo_Recalcualated_DB_Value(_originTargetSenses);

            }

        }

        void spreadNoiseAccordingTo_Recalcualated_DB_Value(TargetSenses _originTargetSenses)
        {

            bool _someValueIsBiggerThanZero = true;
            float _maxValue = 0f;
            int _indexOfMaxValue = 0;
            float _currentValue = 0f;
            while (_someValueIsBiggerThanZero)
            {
                _maxValue = 0f;
                _indexOfMaxValue = 0;
                for (int _indexOfReciver = 0; _indexOfReciver < m_senses_hearTransform_List_Count; _indexOfReciver++)
                {
                    _currentValue = m_recalcuated_dB_Values[_indexOfReciver];
                    if (_currentValue > _maxValue)
                    {
                        _indexOfMaxValue = _indexOfReciver;
                        _maxValue = _currentValue;
                    }
                }

                if (_maxValue == 0f) _someValueIsBiggerThanZero = false;
                else
                {
                    m_senses_hear_List[_indexOfMaxValue].Recived_Noise(m_recalcuated_dB_Values[_indexOfMaxValue], _originTargetSenses);
                    m_recalcuated_dB_Values[_indexOfMaxValue] = 0;
                }
            }
        }
        float recalcuated_dB_Value(float _noise_dB, Vector3 _noise_Position, Vector3 _reciver_Position)
        {
            float _distance = Vector3.Distance(_noise_Position, _reciver_Position);
            return _noise_dB - 20f * (float)Math.Log10(_distance);
        }

        Vector3[] m_corners = new Vector3[20];
        int m_cornerCount;

        float recalcuated_dB_ValueAccordingToNaveMesh(float _noise_dB, Vector3 _noise_Position, Vector3 _reciver_Position)
        {
            m_distance = 0;

            if (!m_wasEnabled)
                enable();

            NavMesh.CalculatePath(_noise_Position, _reciver_Position, NavMesh.AllAreas, m_path);

            if (m_path != null)
            {
                m_cornerCount = m_path.GetCornersNonAlloc(m_corners); // Get corners without allocating memory

                for (int i = 1; i < m_cornerCount; i++)
                {
                    m_distance += Vector3.Distance(m_corners[i - 1], m_corners[i]);
                }
            }
            else m_distance = Vector3.Distance(_noise_Position, _reciver_Position);


            return _noise_dB - 20f * (float)Math.Log10(m_distance);
        }
    }
}
