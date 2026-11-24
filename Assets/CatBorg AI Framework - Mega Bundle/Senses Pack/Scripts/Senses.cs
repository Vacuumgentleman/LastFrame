using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 10: Senses", 10)]
    [ExecuteAlways]
    public class Senses : MonoBehaviour, IUpdate
    {
        public void PerformUpdate(float _deltaTime)
        {
            if (Application.isPlaying)
            {
                if (!m_wasEnabled)
                {
                    enable();
                }

                if (m_use_See || m_use_Hear || m_use_Smell)
                {
                    reset_WereHandledFlags_Universal();
                }

                if (m_use_See || m_use_Smell)
                {
                    grab_frameVaraibles();
                }

                if (m_use_See)
                {
                    reset_WereHandledFlags_See();
                    see_ForceUpdate();
                    //wait for flag... enable only if ever casted... for !NeedToBeInRange...
                    if (m_needToHandleTargets_OutOfRange)
                    {
                        //check if we had any with -1
                        //calc.... deistance and sets as - value?
                        //angle... calc distance
                        calcualte_DistanceForRemeberedTargetSensesOutOfRange();
                    }
                }

                if (m_use_Smell)
                {
                    reset_WereHandledFlags_Smell();
                    smell_ForceUpdate();
                }

                if (m_use_Forgeting)
                {
                    if (m_use_See)
                    {
                        decrease_SeeAwarness_IfWasNotDetected_DuringLastSeeForceUpdate();
                    }

                    if (m_use_Hear)
                    {
                        decrease_HearAwarness();
                    }

                    if (m_use_Smell)
                    {
                        decrease_SmellAwarness_IfWasNotDetected_DuringLastSeeForceUpdate();
                    }

                    arrays_CleanUp_MK6();
                }
                else
                {
                // we still need to make arraysCleaning in case of lost gameobject
                }
            }
        }

#region Variables
        Transform m_transform;
        public Transform Eyes => m_eyes;

        [field: SerializeField]
        Transform m_eyes;
        public void Set_Eyes(Transform _transform)
        {
            if (m_eyes == _transform)
            {
                m_transform = m_eyes != null ? m_eyes : transform;
                return;
            }

            m_eyes = _transform;
            m_transform = m_eyes != null ? m_eyes : transform;
            if (m_fov != null)
            {
                m_fov.Enable(this);
            }
        }

        public bool Use_See => m_use_See;

        [field: SerializeField]
        bool m_use_See;
        public void Set_UseSee(bool _state)
        {
            if (m_use_See == _state)
            {
                return;
            }

            m_use_See = _state;
            if (m_fov != null)
            {
                m_fov.SetFov_State(this, _state && m_use_DisplayFOV);
            }
            else if (_state && m_use_DisplayFOV)
            {
                Set_UseDisplayFOV(true);
            }
        }

        [field: SerializeField]
        FOV m_fov;
        public bool Use_Hear => m_use_Hear;

        [field: SerializeField]
        bool m_use_Hear;
        public void Set_UseHear(bool _state)
        {
            m_use_Hear = _state;
            if (m_use_Hear)
            {
                NoiseRecivers.Instance.RegisterAsNoiseReciver(this);
            }
            else
            {
                NoiseRecivers.Instance.Remove_NoiseReciver(this);
            }
        }

        public bool Use_Smell => m_use_Smell;

        [field: SerializeField]
        bool m_use_Smell;
        public void Set_UseSmell(bool _state)
        {
            m_use_Smell = _state;
        }

        [field: SerializeField]
        public LayerMask IgnoreLayers;

        [SerializeField]
        SeeLayersSolution m_seeLayersSolution = SeeLayersSolution.Legacy;
        public SeeLayersSolution LayersSolution => m_seeLayersSolution;

        public void Set_SeeLayersSolution(SeeLayersSolution _solution)
        {
            m_seeLayersSolution = _solution;
        }

        [SerializeField]
        LayerMask m_seeTargetLayers = ~0;
        public LayerMask SeeTargetLayers => m_seeTargetLayers;

        [SerializeField]
        LayerMask m_seeBlockingLayers = ~0;
        public LayerMask SeeBlockingLayers => m_seeBlockingLayers;

        public void Set_SeeLayerMasks(LayerMask _targetLayers, LayerMask _blockingLayers)
        {
            m_seeTargetLayers = _targetLayers;
            m_seeBlockingLayers = _blockingLayers;
        }

        [SerializeField]
        [Min(1)]
        int m_initialColliderBufferSize = 25;
        public int InitialColliderBufferSize => m_initialColliderBufferSize;

        public bool Use_Forgeting => m_use_Forgeting;

        [field: SerializeField]
        bool m_use_Forgeting;
        public void Set_UseForgeting(bool _ableToReset_Awarness_See) => m_use_Forgeting = _ableToReset_Awarness_See;
        [field: SerializeField]
        public float Forgeting_Rate { get; private set; }

        public void Set_ForgetingRate(float _forgetingRate) => Forgeting_Rate = _forgetingRate;
        public void OnValidate()
        {
            m_initialColliderBufferSize = Mathf.Max(1, m_initialColliderBufferSize);
            if (m_peripheral_VisionAngle + m_central_VisionAngle >= 360)
            {
                m_peripheral_VisionAngle = 360 - m_central_VisionAngle;
            }
        }

        public bool Use_IgnoreOwnFaction => m_use_ignoreOwnFaction;

        [field: SerializeField]
        bool m_use_ignoreOwnFaction;
        public void Set_UseIgnoreOwnFaction(bool _use_ignoreOwnFaction) => m_use_ignoreOwnFaction = _use_ignoreOwnFaction;
        public bool Use_PerfectLineOfViewCheck => m_use_PerfectLineOfViewCheck;

        [field: SerializeField]
        bool m_use_PerfectLineOfViewCheck;
        public void Set_UsePerfectLineOfViewCheck(bool _use_PerfectLineOfViewCheck) => m_use_PerfectLineOfViewCheck = _use_PerfectLineOfViewCheck;
        FactionNames m_ownFaction;
        int m_ownFactionInt;
        public bool Debug_Detected => m_use_debug_Detected;

        [field: SerializeField]
        bool m_use_debug_Detected;
        public void Set_UseDebug_Detected(bool _use_debug_Detected) => m_use_debug_Detected = _use_debug_Detected;
        public float RequiredAwarness => m_requiredAwarness;

        [field: SerializeField]
        float m_requiredAwarness = 1f;
        public void Set_RequiredAwarness(float _requiredAwarness) => m_requiredAwarness = _requiredAwarness;
        [field: SerializeField]
        public int RequiredFactions;
        [field: SerializeField]
        public List<int> SelectedFactionNameIndices { get; private set; } = new List<int>();

        public void Set_SelectedFactionNames(List<int> _selectedFactionNameIndices)
        {
            SelectedFactionNameIndices = _selectedFactionNameIndices;
            FactionNames[] _factionNamesArray = new FactionNames[SelectedFactionNameIndices.Count];
            // Use the selected faction name index
            for (int _i = 0; _i < SelectedFactionNameIndices.Count; _i++)
            {
                var _factionNameValue = Enum.GetValues(typeof(FactionNames)).GetValue(SelectedFactionNameIndices[_i]);
                _factionNamesArray.SetValue(_factionNameValue, _i);
            }

            m_requiredFactions = _factionNamesArray;
        }

        [field: SerializeField]
        FactionNames[] m_requiredFactions;
        //
        public bool SeePropertiesFoldout => m_seePropertiesFoldout;

        [field: SerializeField]
        bool m_seePropertiesFoldout;
        public void Toggle_SeePropertiesFoldout() => m_seePropertiesFoldout = !m_seePropertiesFoldout;
        //
        public bool HearPropertiesFoldout => m_hearPropertiesFoldout;

        [field: SerializeField]
        bool m_hearPropertiesFoldout;
        public void Toggle_HearPropertiesFoldou() => m_hearPropertiesFoldout = !m_hearPropertiesFoldout;
        //
        public bool SmellPropertiesFoldout => m_smellPropertiesFoldout;

        [field: SerializeField]
        bool m_smellPropertiesFoldout;
        public void Toggle_SmellPropertiesFoldout() => m_smellPropertiesFoldout = !m_smellPropertiesFoldout;
        //
        public bool BrainPropertiesFoldout => m_brainPropertiesFoldout;

        [field: SerializeField]
        bool m_brainPropertiesFoldout;
        public void Toggle_BrainPropertiesFoldout() => m_brainPropertiesFoldout = !m_brainPropertiesFoldout;
        //
        public bool SettingsPropertiesFoldout => m_settingsPropertiesFoldout;

        [field: SerializeField]
        bool m_settingsPropertiesFoldout;
        public void Toggle_SettingsPropertiesFoldout() => m_settingsPropertiesFoldout = !m_settingsPropertiesFoldout;
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;

        [field: SerializeField]
        bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
#endregion
#region Awarness Handling
        Dictionary<TargetSenses, int>[] m_array_TargetSenses_Index;
        GameObject[] Array_gameObjects; //added IOT check for nott null fast way
        float[] Array_awarness_See;
        float[] Array_awarness_Hear;
        float[] Array_awarness_Smell;
        bool[] Array_awarness_See_WasIncreased;
        //bool[] Array_awarness_Hear_WasIncreased;
        //bool[] Array_awarness_Smell_WasIncreased;
        bool[] Array_targetSensesWasAlreadyHandledInThisFrame;
        float[] Array_distanceSQR;
        float[] Array_angle;
        Vector3[] Array_hearLastPosition;
        Vector3[] Array_scentLeadingPosition;
        float[] Array_scentStrength;
        int[] Array_scentLeadingNode;
        ushort[] m_array_currentNumberOfTargets;
        int[] m_1DBeginingOfFactionIndex;
        ushort[] m_1DAmountOfSlots;
        int m_amountOfFactions;
        ushort n_initialNumberOfTrackedTargets = 20;
        bool m_needToHandleTargets_OutOfRange = false; //Introduce in 2.03.01
        void reset_WereHandledFlags_Universal()
        {
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    Array_awarness_See_WasIncreased[_targetIndex] = false;
                }
            }
        }

        void reset_WereHandledFlags_See()
        {
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    Array_targetSensesWasAlreadyHandledInThisFrame[_targetIndex] = false;
                    Array_distanceSQR[_targetIndex] = -1f;
                    Array_angle[_targetIndex] = -1f;
                }
            }
        }

        void reset_WereHandledFlags_Smell()
        {
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    Array_scentLeadingPosition[_targetIndex] = Vector3.zero;
                }
            }
        }

        //Introduced in 2.03.01
        //Need to be optimezed!
        void calcualte_DistanceForRemeberedTargetSensesOutOfRange()
        {
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    //only i was not handled....
                    if (!Array_targetSensesWasAlreadyHandledInThisFrame[_targetIndex])
                    {
                        if (Array_distanceSQR[_targetIndex] == -1f)
                        {
                            //Lets caclc Distance
                            TargetSenses _targetSenses = get_ByFactionAndValue(_i, _targetIndex - _begningOfArray);
                            if (_targetSenses != null)
                            {
                                Transform _targetSensesTransform = _targetSenses.transform;
                                float _distanceSQR = currentDistanceSQR_XZ(_targetSensesTransform.position);
                                Array_distanceSQR[_targetIndex] = _distanceSQR;
                                m_target_Direction = _targetSensesTransform.position - m_transform_Position;
                                m_target_Direction = m_target_Direction.normalized;
                                //float _angle = anle_XZ(m_target_Direction);
                                //mathf_StupidButSimple_ABS();
                                //Array_angle[_targetIndex] = _angle;
                                m_currentAngle = anle_XZ(m_target_Direction);
                                mathf_StupidButSimple_ABS();
                                Array_angle[_targetIndex] = m_currentAngle;
                            }
                        }
                    }
                }
            }
        }

        void decrease_SeeAwarness_IfWasNotDetected_DuringLastSeeForceUpdate()
        {
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    if (!Array_awarness_See_WasIncreased[_targetIndex]) // Only decrease if it was NOT increased
                    {
                        float _seeAwareness = Array_awarness_See[_targetIndex];
                        Array_awarness_See[_targetIndex] = Math.Clamp(_seeAwareness - Forgeting_Rate, -100f, 100f);
                    }
                }
            }
        }

        void decrease_HearAwarness()
        {
            float _hear_Awarness;
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    _hear_Awarness = Array_awarness_Hear[_targetIndex];
                    Array_awarness_Hear[_targetIndex] = Mathf.Clamp(_hear_Awarness - Forgeting_Rate, -100, 100);
                //we not clear last Hear position
                }
            }
        }

        void decrease_SmellAwarness_IfWasNotDetected_DuringLastSeeForceUpdate()
        {
            float _smell_Awarness;
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                int _amountOfAcctualTargets = m_array_currentNumberOfTargets[_i];
                int _begningOfArray = m_1DBeginingOfFactionIndex[_i];
                for (int _targetIndex = _begningOfArray; _targetIndex < _begningOfArray + _amountOfAcctualTargets; _targetIndex++)
                {
                    if (!Array_awarness_See_WasIncreased[_targetIndex])
                    {
                        _smell_Awarness = Array_awarness_Smell[_targetIndex];
                        Array_awarness_Smell[_targetIndex] = Mathf.Clamp(_smell_Awarness - Forgeting_Rate, -100, 100);
                    }
                }
            }
        }

        int m_lastIndex;
        int m_length;
        void arrays_CleanUp_MK4()
        {
            TargetSenses _toRemove;
            TargetSenses _toChange;
            bool _needToRemove = false;
            for (int _factionNumber = 0; _factionNumber < m_amountOfFactions; _factionNumber++)
            {
                bool _needToRestart;
                if (m_array_currentNumberOfTargets[_factionNumber] > 0)
                {
                    int _beginingOfFactionIndex = m_1DBeginingOfFactionIndex[_factionNumber];
                    m_lastIndex = _beginingOfFactionIndex + m_array_currentNumberOfTargets[_factionNumber] - 1;
                    m_length = _beginingOfFactionIndex + m_array_currentNumberOfTargets[_factionNumber];
                    do
                    {
                        _needToRestart = false;
                        for (int _i = _beginingOfFactionIndex; _i < m_length; _i++)
                        {
                            _needToRemove = false;
                            if ((m_use_See && Array_awarness_See[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (!_needToRemove && (m_use_Hear && Array_awarness_Hear[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (!_needToRemove && (m_use_Smell && Array_awarness_Smell[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (_needToRemove)
                            {
                                if (_i == m_length - 1)
                                {
                                    removeAtIndex(_factionNumber, _i, _beginingOfFactionIndex); //OK?
                                }
                                else
                                {
                                    setLastTargetValue(_factionNumber, m_lastIndex, _i);
                                    m_array_currentNumberOfTargets[_factionNumber]--;
                                    _toChange = get_ByFactionAndValue(_factionNumber, m_lastIndex - _beginingOfFactionIndex); //Ok?
                                    _toRemove = get_ByFactionAndValue(_factionNumber, _i - _beginingOfFactionIndex);
                                    m_array_TargetSenses_Index[_factionNumber][_toChange] = _i - _beginingOfFactionIndex;
                                    m_array_TargetSenses_Index[_factionNumber].Remove(_toRemove);
                                    m_length--;
                                    m_lastIndex--;
                                    _needToRestart = true;
                                    break;
                                }
                            }
                        }
                    }
                    while (_needToRestart);
                }
            }
        }

        void arrays_CleanUp_MK5()
        {
            TargetSenses _toRemove;
            TargetSenses _toChange;
            bool _needToRemove = false;
            for (int _factionNumber = 0; _factionNumber < m_amountOfFactions; _factionNumber++)
            {
                bool _needToRestart;
                if (m_array_currentNumberOfTargets[_factionNumber] > 0)
                {
                    int _beginingOfFactionIndex = m_1DBeginingOfFactionIndex[_factionNumber];
                    m_lastIndex = _beginingOfFactionIndex + m_array_currentNumberOfTargets[_factionNumber] - 1;
                    m_length = _beginingOfFactionIndex + m_array_currentNumberOfTargets[_factionNumber];
                    do
                    {
                        _needToRestart = false;
                        for (int _i = _beginingOfFactionIndex; _i < m_length; _i++)
                        {
                            _needToRemove = false;
                            if (!Array_gameObjects[_i])
                            {
                                _needToRemove = true; //Added in 3.0
                                Debug.Log("GameObject Equal Null");
                            }

                            if ((m_use_See && Array_awarness_See[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (!_needToRemove && (m_use_Hear && Array_awarness_Hear[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (!_needToRemove && (m_use_Smell && Array_awarness_Smell[_i] <= -10))
                            {
                                _needToRemove = true;
                            }

                            if (_needToRemove)
                            {
                                if (_i == m_length - 1)
                                {
                                    removeAtIndex(_factionNumber, _i, _beginingOfFactionIndex); //OK?
                                }
                                else
                                {
                                    setLastTargetValue(_factionNumber, m_lastIndex, _i);
                                    m_array_currentNumberOfTargets[_factionNumber]--;
                                    _toChange = get_ByFactionAndValue(_factionNumber, m_lastIndex - _beginingOfFactionIndex); //Ok?
                                    _toRemove = get_ByFactionAndValue(_factionNumber, _i - _beginingOfFactionIndex);
                                    m_array_TargetSenses_Index[_factionNumber][_toChange] = _i - _beginingOfFactionIndex;
                                    m_array_TargetSenses_Index[_factionNumber].Remove(_toRemove);
                                    Array_gameObjects[m_lastIndex - _beginingOfFactionIndex] = null; //3.0
                                    m_length--;
                                    m_lastIndex--;
                                    _needToRestart = true;
                                    break;
                                }
                            }
                        }
                    }
                    while (_needToRestart);
                }
            }
        }

        void arrays_CleanUp_MK6()
        {
            for (int _factionNumber = 0; _factionNumber < m_amountOfFactions; _factionNumber++)
            {
                if (m_array_currentNumberOfTargets[_factionNumber] == 0)
                {
                    continue;
                }

                int _beginingOfFactionIndex = m_1DBeginingOfFactionIndex[_factionNumber];
                m_length = _beginingOfFactionIndex + m_array_currentNumberOfTargets[_factionNumber];
                m_lastIndex = m_length - 1;
                var _dictionary = m_array_TargetSenses_Index[_factionNumber];
                int _i = _beginingOfFactionIndex;
                while (_i < m_length)
                {
                    bool _needToRemove = !Array_gameObjects[_i] || (m_use_See && Array_awarness_See[_i] <= -10f) || (m_use_Hear && Array_awarness_Hear[_i] <= -10f) || (m_use_Smell && Array_awarness_Smell[_i] <= -10f);
                    if (!_needToRemove)
                    {
                        _i++;
                        continue;
                    }

                    if (_i == m_lastIndex)
                    {
                        // Funkcja zeruje slot, usuwa wpis ze słownika
                        // i zmniejsza liczbę celów, m_length oraz m_lastIndex.
                        removeAtIndex(_factionNumber, _i, _beginingOfFactionIndex);
                    }
                    else
                    {
                        int _localIndex = _i - _beginingOfFactionIndex;
                        int _lastLocalIndex = m_lastIndex - _beginingOfFactionIndex;
                        TargetSenses _toRemove = get_ByFactionAndValue(_factionNumber, _localIndex);
                        TargetSenses _toMove = get_ByFactionAndValue(_factionNumber, _lastLocalIndex);
                        // Przenosimy wszystkie dane ostatniego celu.
                        setLastTargetValue(_factionNumber, m_lastIndex, _i);
                        // Słownik przechowuje indeksy lokalne.
                        _dictionary[_toMove] = _localIndex;
                        _dictionary.Remove(_toRemove);
                        // Tablica wymaga indeksu globalnego.
                        Array_gameObjects[m_lastIndex] = null;
                        m_array_currentNumberOfTargets[_factionNumber]--;
                        m_length--;
                        m_lastIndex--;
                    }
                // Nie zwiększamy i.
                // Po podmianie sprawdzamy przeniesiony cel.
                // Po usunięciu ostatniego elementu pętla się zakończy.
                }
            }
        }

        void removeAtIndex(int _factionNumber, int _1D_Index, int _beginingOf_1D_Index)
        {
            setDeffaultValueToArrays(_1D_Index);
            m_array_currentNumberOfTargets[_factionNumber]--;
            TargetSenses _toRemove = get_ByFactionAndValue(_factionNumber, _1D_Index - _beginingOf_1D_Index);
            m_array_TargetSenses_Index[_factionNumber].Remove(_toRemove);
            m_length--;
            m_lastIndex--;
        }

        /*
        int get1DIndex(int _factionNumber, int _index)
        {
            return default;
        }

        int get1DBeginingOfFactionIndex(int _factionNumber)
        {
            int _index = 0;

            for (int i = 0; i < _factionNumber; i++)
            {
                _index += n_initialNumberOfTrackedTargets;
            }

            return _index;
        }

        int get1DBeginingOfFactionIndex(int _factionNumber, int _initialNumberOfTrackedTargets)
        {
            int _index = 0;

            for (int i = 0; i < _factionNumber; i++)
            {
                _index += _initialNumberOfTrackedTargets;
            }

            return _index;
        }
        */
        void setLastTargetValue(int _factionNumber, int _indexFromCoppy, int _indexToRepace)
        {
            if (m_use_See)
            {
                /*
                m_array_awarness_See[_factionNumber][i] = m_array_awarness_See[_factionNumber][lastIndex];
                m_array_awarness_See_WasIncreased[_factionNumber][i] = m_array_awarness_See_WasIncreased[_factionNumber][lastIndex];
                m_array_targetSensesWereHandledInFrame[_factionNumber][i] = m_array_targetSensesWereHandledInFrame[_factionNumber][lastIndex];
                m_array_distanceSQR[_factionNumber][i] = m_array_distanceSQR[_factionNumber][lastIndex];   //+ 1.06
                m_array_angle[_factionNumber][i] = m_array_angle[_factionNumber][lastIndex];         //+ 1.0
                */
                Array_awarness_See[_indexToRepace] = Array_awarness_See[_indexFromCoppy];
                Array_targetSensesWasAlreadyHandledInThisFrame[_indexToRepace] = Array_targetSensesWasAlreadyHandledInThisFrame[_indexFromCoppy];
            }

            if (m_use_Hear)
            {
                /*
                m_array_awarness_Hear[_factionNumber][i] = m_array_awarness_Hear[_factionNumber][lastIndex];
                m_array_awarness_Hear_WasIncreased[_factionNumber][i] = m_array_awarness_Hear_WasIncreased[_factionNumber][lastIndex];
                */
                Array_awarness_Hear[_indexToRepace] = Array_awarness_Hear[_indexFromCoppy];
                Array_hearLastPosition[_indexToRepace] = Array_hearLastPosition[_indexFromCoppy];
            }

            if (m_use_Smell)
            {
                /*
                m_array_awarness_Smell[_factionNumber][i] = m_array_awarness_Smell[_factionNumber][lastIndex];
                m_array_awarness_Smell_WasIncreased[_factionNumber][i] = m_array_awarness_Smell_WasIncreased[_factionNumber][lastIndex];
                m_array_scentLeadingPosition[_factionNumber][i] = m_array_scentLeadingPosition[_factionNumber][lastIndex];
                m_array_scentStrength[_factionNumber][i] = m_array_scentStrength[_factionNumber][lastIndex];
                m_array_scentLeadingNode[_factionNumber][i] = m_array_scentLeadingNode[_factionNumber][lastIndex];
                */
                Array_awarness_Smell[_indexToRepace] = Array_awarness_Smell[_indexFromCoppy];
                Array_scentLeadingPosition[_indexToRepace] = Array_scentLeadingPosition[_indexFromCoppy];
                Array_scentStrength[_indexToRepace] = Array_scentStrength[_indexFromCoppy];
                Array_scentLeadingNode[_indexToRepace] = Array_scentLeadingNode[_indexFromCoppy];
            }

            if (m_use_See || m_use_Hear || m_use_Smell)
            {
                Array_distanceSQR[_indexToRepace] = Array_distanceSQR[_indexFromCoppy];
                Array_angle[_indexToRepace] = Array_angle[_indexFromCoppy];
                Array_awarness_See_WasIncreased[_indexToRepace] = Array_awarness_See_WasIncreased[_indexFromCoppy];
            }

            Array_gameObjects[_indexToRepace] = Array_gameObjects[_indexFromCoppy];
        }

        void setDeffaultValueToArrays(int _1D_Index)
        {
            if (m_use_See)
            {
                /*
                m_array_awarness_See[_factionNumber][i] = 0;
                m_array_awarness_See_WasIncreased[_factionNumber][i] = false;
                m_array_targetSensesWereHandledInFrame[_factionNumber][i] = false;
                m_array_distanceSQR[_factionNumber][i] = -1f;   //+ 1.06
                m_array_angle[_factionNumber][i] = -1f;         //+ 1.06
                */
                Array_awarness_See[_1D_Index] = 0;
                Array_targetSensesWasAlreadyHandledInThisFrame[_1D_Index] = false;
            }

            if (m_use_Hear)
            {
                /*
                m_array_awarness_Hear[_factionNumber][i] = 0;
                m_array_awarness_Hear_WasIncreased[_factionNumber][i] = false;
                */
                Array_awarness_Hear[_1D_Index] = 0;
                Array_hearLastPosition[_1D_Index] = Vector3.zero;
            }

            if (m_use_Smell)
            {
                /*
                m_array_awarness_Smell[_factionNumber][i] = 0f;
                m_array_awarness_Smell_WasIncreased[_factionNumber][i] = false;
                m_array_scentLeadingPosition[_factionNumber][i] = Vector3.zero;
                m_array_scentStrength[_factionNumber][i] = 0;
                m_array_scentLeadingNode[_factionNumber][i] = -1;
                */
                Array_awarness_Smell[_1D_Index] = 0f;
                Array_scentLeadingPosition[_1D_Index] = Vector3.zero;
                Array_scentStrength[_1D_Index] = 0;
                Array_scentLeadingNode[_1D_Index] = -1;
            }

            if (m_use_See || m_use_Hear || m_use_Smell)
            {
                Array_distanceSQR[_1D_Index] = -1f;
                Array_angle[_1D_Index] = -1f;
                Array_awarness_See_WasIncreased[_1D_Index] = false;
            }

            Array_gameObjects[_1D_Index] = null;
        }

        void addAwarness_See(int _1D_Index)
        {
            //int _factionNumber = (int)m_targetSenses.FactionName;
            //float _see_Awarness = m_array_awarness_See[_factionNumber][_index];
            float _see_Awarness = Array_awarness_See[_1D_Index];
            if (_see_Awarness < 100)
            {
                if (_see_Awarness < 0)
                {
                    _see_Awarness = 0;
                }

                _see_Awarness = Mathf.Clamp(_see_Awarness + m_deltaAwarnes, 0, 100);
                Array_awarness_See[_1D_Index] = _see_Awarness;
            }

            Array_awarness_See_WasIncreased[_1D_Index] = true;
            m_targetSenses.SetWasDetected();
        }

        void checkAndAddToDictionary(int _factionNumber, TargetSenses _targetSenses, out int _index)
        {
            if (!m_array_TargetSenses_Index[_factionNumber].TryGetValue(_targetSenses, out _index))
            {
                ushort _array_currentNumberOfTargets = m_array_currentNumberOfTargets[_factionNumber];
                if (_array_currentNumberOfTargets >= m_1DAmountOfSlots[_factionNumber] - 1)
                {
                    //now we need to do hard shit....
                    increaseArrayLenghtWithoutDataLost_DueToAmountOfTargetsInSomeFactionChanged(_factionNumber);
                }

                m_array_TargetSenses_Index[_factionNumber].Add(_targetSenses, _array_currentNumberOfTargets);
                setDeffaultValueToArrays(m_1DBeginingOfFactionIndex[_factionNumber] + _array_currentNumberOfTargets);
                Array_gameObjects[m_1DBeginingOfFactionIndex[_factionNumber] + _array_currentNumberOfTargets] = _targetSenses.gameObject; //added in 3.0
                _index = _array_currentNumberOfTargets;
                _array_currentNumberOfTargets++;
                m_array_currentNumberOfTargets[_factionNumber] = _array_currentNumberOfTargets;
            /* Was:
                 * 
                _array_currentNumberOfTargets++;
                m_array_currentNumberOfTargets[_factionNumber] = _array_currentNumberOfTargets;
                _index = _array_currentNumberOfTargets;
                
                 */
            }
        }

#endregion
#region Enable
        TargetSenses m_targetSenses;
        TargetSenses m_selfTargetSenses;
        bool m_hadSelfTargetSenses;
        bool m_useSeeBooster;
        SeeBooster m_seeBooster;
        ColliderReference m_colliderReference;
        GameObject m_colliderGameObject;
        Obstacle m_obstacle;
        bool m_wasEnabled;
        void enable()
        {
            m_transform = transform;
            m_transform = (m_eyes != null ? m_eyes : this.transform);
            if (m_eyes == null)
            {
                m_eyes = this.transform;
            }

            if (m_use_Hear)
            {
                NoiseRecivers.Instance.RegisterAsNoiseReciver(this);
            }

            if (TryGetComponent<SeeBooster>(out SeeBooster _seeBooster))
            {
                m_seeBooster = _seeBooster;
                m_useSeeBooster = true;
            }

            calculate_SQR_CentralVisionRadius();
            calculate_SQR_PeripheralVisionRadius();
            m_collidersSphereCast = new Collider[Mathf.Max(1, m_initialColliderBufferSize)];
            m_amountOfFactions = SceneFactionManager.Instance.GetFactionCount(); //System.Enum.GetValues(typeof(FactionNames)).Length;
            int _1D_RequiredLength = m_amountOfFactions * n_initialNumberOfTrackedTargets;
            //We not declaring arrays here...
            {
            //createArraysWithlength(_1D_RequiredLength);
            }

            m_array_TargetSenses_Index = new Dictionary<TargetSenses, int>[m_amountOfFactions];
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                m_array_TargetSenses_Index[_i] = new Dictionary<TargetSenses, int>(n_initialNumberOfTrackedTargets);
            }

            m_array_currentNumberOfTargets = new ushort[m_amountOfFactions]; // new int[m_amountOfFactions];
            m_1DBeginingOfFactionIndex = new int[m_amountOfFactions];
            m_1DAmountOfSlots = new ushort[m_amountOfFactions];
            //we set for -1
            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                m_1DBeginingOfFactionIndex[_i] = -1; //get1DBeginingOfFactionIndex(i);
            }

            if (TryGetComponent<TargetSenses>(out TargetSenses _targetSenses))
            {
                m_ownFaction = _targetSenses.FactionName;
                m_ownFactionInt = (int)m_ownFaction;
            }

            //check....
            if (m_use_DisplayFOV && m_fov != null)
            {
                m_fov.Enable(this);
            }
            else
            {
            //need new FOv
            }

            //Added for Gizmos - Only For DebugView
            FactionNames[] _factionNamesArray = new FactionNames[SelectedFactionNameIndices.Count];
            for (int _i = 0; _i < SelectedFactionNameIndices.Count; _i++)
            {
                var _factionNameValue = Enum.GetValues(typeof(FactionNames)).GetValue(SelectedFactionNameIndices[_i]);
                _factionNamesArray.SetValue(_factionNameValue, _i);
            }

            calculateFOVParams();
            m_requiredFactions = _factionNamesArray;
            m_wasEnabled = true;
        }

        void calculateFOVParams()
        {
            float _fov = m_central_VisionAngle;
            if (m_use_PeripheralFOV)
            {
                _fov += m_peripheral_VisionAngle;
            }

            if (_fov >= 360f)
            {
                m_fovMode = FOVMode.Full360;
                return;
            }

            if (_fov > 180f)
            {
                m_fovMode = FOVMode.Wide;
                // Blind spot behind character = 360° - FOV
                float _blind = 360f - _fov;
                float _halfBlindRad = (_blind * 0.5f) * Mathf.Deg2Rad;
                // Signed dot threshold to detect if target is inside wide vision
                m_cosBlindLimit = -Mathf.Cos(_halfBlindRad);
                return;
            }

            // Narrow cone (correct and fast squared-dot)
            m_fovMode = FOVMode.Narrow;
            float _halfRad = (_fov * 0.5f) * Mathf.Deg2Rad;
            float _c = Mathf.Cos(_halfRad);
            m_cosHalfFOV_SQR = _c * _c;
        }

        void createArraysWithlength(int _1D_RequiredLength)
        {
            if (m_use_Hear)
            {
                Array_awarness_Hear = new float[_1D_RequiredLength]; //+ 2.00
                Array_hearLastPosition = new Vector3[_1D_RequiredLength]; //+ 2.00
            }

            if (m_use_See)
            {
                Array_awarness_See = new float[_1D_RequiredLength]; //+ 2.00
                Array_targetSensesWasAlreadyHandledInThisFrame = new bool[_1D_RequiredLength]; //+ 2.00
            }

            if (m_use_Smell)
            {
                Array_awarness_Smell = new float[_1D_RequiredLength]; //+ 2.00
                Array_scentLeadingPosition = new Vector3[_1D_RequiredLength]; //+ 2.00
                Array_scentStrength = new float[_1D_RequiredLength]; //+ 2.00
                Array_scentLeadingNode = new int[_1D_RequiredLength]; //+ 2.00
            }

            if (m_use_See || m_use_Hear || m_use_Smell)
            {
                Array_distanceSQR = new float[_1D_RequiredLength]; //+ 2.00
                Array_angle = new float[_1D_RequiredLength]; //+ 2.00
                Array_awarness_See_WasIncreased = new bool[_1D_RequiredLength]; //+ 2.00
            }

            Array_gameObjects = new GameObject[_1D_RequiredLength]; //+ 3.00
        }

        internal void Recalcuate_SQRs()
        {
            calculate_SQR_CentralVisionRadius();
            calculate_SQR_PeripheralVisionRadius();
        }

        void calculate_SQR_CentralVisionRadius()
        {
            if (m_useSeeBooster)
            {
                if (m_seeBooster.AffectedAspect == AffectedAsspect.cetralVisionRadius)
                {
                    m_radiusCentralVision_SQR = m_central_VisionRadius * m_central_VisionRadius;
                    m_radiusCentralVision_SQR = m_seeBooster.Apply_BuffTo(m_radiusCentralVision_SQR);
                }
                else
                {
                    m_radiusCentralVision_SQR = m_central_VisionRadius * m_central_VisionRadius;
                }
            }
            else
            {
                m_radiusCentralVision_SQR = m_central_VisionRadius * m_central_VisionRadius;
            }
        }

        void calculate_SQR_PeripheralVisionRadius()
        {
            if (m_useSeeBooster)
            {
                if (m_seeBooster.AffectedAspect == AffectedAsspect.peripheralVisionRadius)
                {
                    m_radiusPeripheralVision_SQR = m_peripheral_VisionRadius * m_peripheral_VisionRadius;
                    m_radiusPeripheralVision_SQR = m_seeBooster.Apply_BuffTo(m_radiusPeripheralVision_SQR);
                }
                else
                {
                    m_radiusPeripheralVision_SQR = m_peripheral_VisionRadius * m_peripheral_VisionRadius;
                }
            }
            else
            {
                m_radiusPeripheralVision_SQR = m_peripheral_VisionRadius * m_peripheral_VisionRadius;
            }
        }

#endregion
#region FOV
        string prefabName = "- FOV (part of Senses Asset)";
        void addPrefab()
        {
            GameObject _prefabToAdd = findPrefabByName(prefabName);
            if (!Application.isPlaying)
            {
                if (_prefabToAdd != null)
                {
#if (UNITY_EDITOR)
                    UnityEditor.Undo.RegisterFullObjectHierarchyUndo(gameObject, "FOV");
                    GameObject _newObject = UnityEditor.PrefabUtility.InstantiatePrefab(_prefabToAdd) as GameObject;
                    _newObject.name = prefabName;
                    UnityEditor.Undo.RegisterCreatedObjectUndo(_newObject, "FOV");
                    m_fov = _newObject.GetComponent<FOV>();
#endif
                }
                else
                {
                    Debug.LogError("Prefab with name " + prefabName + " not found.");
                }
            }
            else
            {
                if (_prefabToAdd != null)
                {
                    _prefabToAdd = Instantiate(_prefabToAdd);
                    _prefabToAdd.name = prefabName;
                    m_fov = _prefabToAdd.GetComponent<FOV>();
                }
                else
                {
                    Debug.LogError("Prefab with name " + prefabName + " not found.");
                }
            }
        }

        GameObject findPrefabByName(string name)
        {
            return Resources.Load<GameObject>(name);
        }

#endregion
#region See
        public float Central_VisionAngle => m_central_VisionAngle;

        [field: SerializeField]
        float m_central_VisionAngle;
        public void Set_CentralVisionAngle(float _angleCentralVision)
        {
            m_central_VisionAngle = _angleCentralVision;
            calculateFOVParams();
        }

        public float Central_VisionRadius => m_central_VisionRadius;

        [field: SerializeField]
        float m_central_VisionRadius;
        public void Set_CentralVisionRadius(float _radiusCentralVision)
        {
            m_central_VisionRadius = _radiusCentralVision;
            calculate_SQR_CentralVisionRadius();
        }

        float m_radiusCentralVision_SQR;
        public bool Use_PeripheralFOV => m_use_PeripheralFOV;

        [field: SerializeField]
        bool m_use_PeripheralFOV;
        public void Set_UsePeripheralFOV(bool _state)
        {
            if (_state != m_use_PeripheralFOV)
            {
                m_use_PeripheralFOV = _state;
                calculateFOVParams();
            }
        }

        public float Peripheral_VisionAngle => m_peripheral_VisionAngle;

        [field: SerializeField]
        float m_peripheral_VisionAngle;
        public void Set_PeripheralVisionAngle(float _anglePeripheralVision)
        {
            m_peripheral_VisionAngle = _anglePeripheralVision;
            calculateFOVParams();
        }

        public float Peripheral_VisionRadius => m_peripheral_VisionRadius;

        [field: SerializeField]
        float m_peripheral_VisionRadius;
        public void Set_PeripheralVision_Radius(float _radiusPeripheralVision)
        {
            m_peripheral_VisionRadius = _radiusPeripheralVision;
            calculate_SQR_PeripheralVisionRadius();
        }

        float m_radiusPeripheralVision_SQR;
        public bool Use_DisplayFOV => m_use_DisplayFOV;

        [field: SerializeField]
        bool m_use_DisplayFOV;
        public void Set_UseDisplayFOV(bool _state)
        {
            bool _changed = m_use_DisplayFOV != _state;
            m_use_DisplayFOV = _state;
            if (m_fov == null && _state)
            {
                addPrefab();
                if (m_fov == null)
                {
                    return;
                }

                m_fov.Enable(this);
                _changed = true;
            }

            if (m_fov != null && _changed)
            {
                m_fov.SetFov_State(this, _state);
            }
        }

        [field: SerializeField]
        public bool Use_IgnoreRotationX { get; private set; }

        public void Set_UseIgnoreRotationX(bool _ignoreRotation_X) => Use_IgnoreRotationX = _ignoreRotation_X;
        [field: SerializeField]
        public bool Use_RayCastInFovOnly { get; private set; }

        public void Set_UseRayCastInFovOnly(bool _rayCastInFovOnly) => Use_RayCastInFovOnly = _rayCastInFovOnly;
        [field: SerializeField]
        public bool Use_DebugDrawRay { get; private set; }

        public void Set_DebugDrawRay(bool _debugDrawRay) => Use_DebugDrawRay = _debugDrawRay;
        [field: SerializeField]
        public bool Use_OffsetRotation { get; private set; }

        public void Set_UseOffsetRotation(bool _set_useOffsetRotation) => Use_OffsetRotation = _set_useOffsetRotation;
        [field: SerializeField]
        public Vector3 Offset_Rotation { get; private set; }

        public void Set_OffsetRotation(Vector3 _offset_Rotation) => Offset_Rotation = _offset_Rotation;
        [field: SerializeField]
        public bool Use_OffsetPosition { get; private set; }

        public void Set_UseOffsetPosition(bool _useOverrridePosition) => Use_OffsetPosition = _useOverrridePosition;
        [field: SerializeField]
        public Vector3 Offset_Position { get; private set; }

        public bool NeedsUpdate() => m_needUpdate;
        bool m_needUpdate;
        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                if (m_fov != null)
                {
                    m_fov.Enable(this);
                    m_fov.SetFov_State(this, m_use_DisplayFOV);
                }

                return;
            }

            Updater.Instance.RegisterUpdate(this);
            StopAllCoroutines();
            m_hadSelfTargetSenses = gameObject.TryGetComponent<TargetSenses>(out TargetSenses _targetSenses);
            if (m_hadSelfTargetSenses)
            {
                m_selfTargetSenses = _targetSenses;
            }

            if (m_use_DisplayFOV)
            {
                if (m_fov == null)
                {
                    if (!m_fov)
                    {
                        addPrefab();
                    }

                    m_fov.Enable(this);
                }

                m_fov.SetFov_State(this, true);
            };
            m_needUpdate = false;
        }

        void OnDisable()
        {
            if (Updater.Instance != null)
            {
                Updater.Instance.UnegisterUpdate(this);
                m_needUpdate = true;
            }

            if (m_fov != null)
            {
                m_fov.SetFov_State(this, false);
            }
        }

        public void Set_OffsetPosition(Vector3 _requiredPosition) => Offset_Position = _requiredPosition;
        float m_xD;
        float m_yD;
        float m_zD;
        Vector3 m_transform_Position;
        Vector3 m_transform_Forward;
        Vector3 m_target_Position;
        Vector3 m_target_Direction;
        float m_currentDistance_SQR;
        float m_currentAngle;
        float m_deltaAwarnes;
        bool m_collider_In_Central_VisionRange;
        bool m_collider_In_Central_VisionAngle;
        bool m_collider_In_Periphera_VisionRange;
        bool m_collider_In_Periphera_VisionAngle;
        bool m_collider_In_LineOfSight;
        bool m_target_HadStealthComponent;
        Stealth m_targetStealthComponent;
        RaycastHit m_hitInfo = new RaycastHit();
        float m_remainingRange;
        Collider[] m_collidersSphereCast;
        Collider[] m_collidersTargetSenses;
        readonly Collider[] m_singleTargetCollider = new Collider[1];
        int m_ammountOfColliders = 20;
        int m_hitCount;
        bool m_coudlBeHandled;
        float m_cosHalfFOV_SQR;
        FOVMode m_fovMode;
        float m_cosBlindLimit;
        float m_forwardX;
        float m_forwardZ;
        float m_originX;
        float m_originZ;
        float m_forwardAngleRadians;
        void see_ForceUpdate()
        {
            if (!m_wasEnabled)
            {
                return;
            }

            //grab_frameVaraibles();
            int _targetLayerMask = m_seeLayersSolution == SeeLayersSolution.Fixer33
                ? m_seeTargetLayers.value
                : ~IgnoreLayers.value;
            m_hitCount = overlap_MK2(m_central_VisionRadius, _targetLayerMask);

            for (int _i = 0; _i < m_hitCount; _i++)
            {
                //if (IsInsideFOV_XZ(m_collidersSphereCast[i].bounds.center)) nice idea but fuck up when we rotate....
                {
                    m_colliderReference = GlobalColliderReferenceRegistry.Get(m_collidersSphereCast[_i]);
                    if (m_colliderReference.iS_SensesTarget(out m_targetSenses))
                    {
                        int _faction = (int)m_targetSenses.FactionName;
                        m_coudlBeHandled = true;
                        if (m_use_ignoreOwnFaction)
                        {
                            if (_faction == m_ownFactionInt)
                            {
                                m_coudlBeHandled = false;
                            }
                        }

                        if (m_hadSelfTargetSenses && m_targetSenses == m_selfTargetSenses)
                        {
                            m_coudlBeHandled = false;
                        }

                        if (m_coudlBeHandled)
                        {
                            checkDidWeEverDetectFaction(_faction);
                            checkAndAddToDictionary(_faction, m_targetSenses, out int _index);
                            int _1D_Index = m_1DBeginingOfFactionIndex[_faction] + _index;
                            //Debug.Log($"_faction: {_faction}, _1D_Index: {_1D_Index}");
                            if (!Array_targetSensesWasAlreadyHandledInThisFrame[_1D_Index])
                            {
                                if (m_targetSenses.TargetType == TargetType.Collider)
                                {
                                    m_singleTargetCollider[0] = m_collidersSphereCast[_i];
                                    m_collidersTargetSenses = m_singleTargetCollider;
                                    m_ammountOfColliders = 1;
                                }
                                else if (m_targetSenses.TargetType == TargetType.RigCollider)
                                {
                                    m_collidersTargetSenses = m_targetSenses.Provide_RigCollider_Colliders();
                                    m_ammountOfColliders = m_collidersTargetSenses.Length;
                                }

                                checkIsAnyColliderVisible(_1D_Index);
                                //m_array_targetSensesWereHandledInFrame[_faction][_index] = true;
                                Array_targetSensesWasAlreadyHandledInThisFrame[_1D_Index] = true;
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsInsideFOV_XZ(Vector3 targetPos)
        {
            // Full 360° → Everything is visible
            if (m_fovMode == FOVMode.Full360)
            {
                return true;
            }

            float _dx = targetPos.x - m_originX;
            float _dz = targetPos.z - m_originZ;
            float _dot = _dx * m_forwardX + _dz * m_forwardZ;
            if (m_fovMode == FOVMode.Wide)
            {
                // Wide FOV → check if we're outside the blind spot
                return _dot >= m_cosBlindLimit;
            }

            // Narrow FOV → squared-dot fast cone test (≤180°)
            float _distSqr = _dx * _dx + _dz * _dz;
            return (_dot * _dot) > (_distSqr * m_cosHalfFOV_SQR);
        }

        enum FOVMode
        {
            Narrow, // 0–180°
            Wide, // 180–360°
            Full360 // 360°
        }

        void checkDidWeEverDetectFaction(int _faction)
        {
            if (m_1DBeginingOfFactionIndex[_faction] == -1)
            {
                bool _arraysNotCreated = Array_gameObjects == null;
                if (_arraysNotCreated)
                {
                    createArraysWithlength(n_initialNumberOfTrackedTargets);
                    m_1DBeginingOfFactionIndex[_faction] = 0;
                    m_1DAmountOfSlots[_faction] = n_initialNumberOfTrackedTargets;
                }
                else
                {
                    int _totalAmountOfSloots = howManySlotsAreUsedNow();
                    _totalAmountOfSloots += n_initialNumberOfTrackedTargets;
                    increaseArrayLenghtWithoutDataLost_DueToAmountOfFactionsChanged(_totalAmountOfSloots, out int _1DBeginingIndexOfNewFaction);
                    m_1DBeginingOfFactionIndex[_faction] = _1DBeginingIndexOfNewFaction;
                    m_1DAmountOfSlots[_faction] = n_initialNumberOfTrackedTargets;
                }
            }
        }

        int howManySlotsAreUsedNow()
        {
            int _totalAmountOfSloots = 0;
            for (int _j = 0; _j < m_1DBeginingOfFactionIndex.Length; _j++)
            {
                if (m_1DBeginingOfFactionIndex[_j] > -1)
                {
                    _totalAmountOfSloots += m_1DAmountOfSlots[_j];
                }
            }

            return _totalAmountOfSloots;
        }

        void smell_ForceUpdate()
        {
            if (!m_wasEnabled)
            {
                return;
            }

            //grab_frameVaraibles();
            m_hitCount = overlap_MK2(m_smell_Radius, ScentLayer);

            for (int _i = 0; _i < m_hitCount; _i++)
            {
                m_colliderReference = GlobalColliderReferenceRegistry.Get(m_collidersSphereCast[_i]);
                if (m_colliderReference.iS_ScentNode(out ScentNode _scentNode))
                {
                    m_targetSenses = _scentNode.TargetSenses;
                    int _faction = (int)m_targetSenses.FactionName;
                    bool _coudlBeHandled = true;
                    if (m_use_ignoreOwnFaction)
                    {
                        if (_faction == m_ownFactionInt)
                        {
                            _coudlBeHandled = false;
                        }
                    }

                    if (m_hadSelfTargetSenses && m_targetSenses == m_selfTargetSenses)
                    {
                        _coudlBeHandled = false;
                    }

                    if (_coudlBeHandled)
                    {
                        checkDidWeEverDetectFaction(_faction);
                        checkAndAddToDictionary(_faction, m_targetSenses, out int _index);
                        float _scentDeltaValue = _scentNode.Strength_Relative;
                        Vector3 _position = Vector3.zero;
                        int _1D_Index = m_1DBeginingOfFactionIndex[_faction] + _index;
                        addAwarness_Smell(_1D_Index, _scentDeltaValue);
                        if (_scentNode.Strength_Relative > Array_scentStrength[_1D_Index])
                        {
                            _position = _scentNode.transform.position;
                            Array_scentLeadingPosition[_1D_Index] = _position;
                            Array_scentLeadingNode[_1D_Index] = _scentNode.NodeID;
                            Array_scentStrength[_1D_Index] = _scentNode.Strength_Relative;
                        }
                        else
                        {
                            if (_scentNode.NodeID == Array_scentLeadingNode[_1D_Index])
                            {
                                _position = _scentNode.transform.position;
                                Array_scentLeadingPosition[_1D_Index] = _position;
                                Array_scentStrength[_1D_Index] = _scentNode.Strength_Relative;
                            }
                        }
                    }
                }
            }
        }

        int overlap_MK2(float _radius, LayerMask _layerMask)
        {
            int _hitCount = Physics.OverlapSphereNonAlloc(m_transform_Position, _radius, m_collidersSphereCast, _layerMask, QueryTriggerInteraction.UseGlobal);
            while (_hitCount == m_collidersSphereCast.Length)
            {
                // A full buffer may be truncated. Repeat in this update before processing any results.
                // No copy is needed: the physics query fills the new buffer from the same snapshot.
                int _newCapacity = checked(m_collidersSphereCast.Length * 2);
                m_collidersSphereCast = new Collider[_newCapacity];
                _hitCount = Physics.OverlapSphereNonAlloc(m_transform_Position, _radius, m_collidersSphereCast, _layerMask, QueryTriggerInteraction.UseGlobal);
            }

            return _hitCount;
        }

        void checkIsAnyColliderVisible(int _1D_Index)
        {
            for (int _i = 0; _i < m_ammountOfColliders; _i++)
            {
                m_collider_In_Central_VisionRange = false;
                m_collider_In_Central_VisionAngle = false;
                m_collider_In_Periphera_VisionRange = false;
                m_collider_In_Periphera_VisionAngle = false;
                m_collider_In_LineOfSight = false;
                m_target_Position = m_collidersTargetSenses[_i].bounds.center;
                m_target_Direction = m_target_Position - m_transform_Position;
                m_target_Direction = m_target_Direction.normalized;
                m_currentDistance_SQR = currentDistanceSQR_XZ(m_target_Position);
                Array_distanceSQR[_1D_Index] = m_currentDistance_SQR;
                m_collider_In_Central_VisionRange = m_currentDistance_SQR < m_radiusCentralVision_SQR;
                if (m_collider_In_Central_VisionRange)
                {
                    m_currentAngle = anle_XZ(m_target_Direction);
                    mathf_StupidButSimple_ABS();
                    //m_array_angle[(int)m_targetSenses.FactionName][_index] = m_currentAngle;         //+ 1.06
                    Array_angle[_1D_Index] = m_currentAngle;
                    m_collider_In_Central_VisionAngle = m_currentAngle < (m_central_VisionAngle / 2f);
                    if (m_collider_In_Central_VisionAngle)
                    {
                        if (Use_RayCastInFovOnly)
                        {
                            if (Use_IgnoreRotationX)
                            {
                                m_target_Direction.y = 0f;
                            }
                            else
                            {
                                m_target_Direction.y = m_transform_Forward.y;
                            }
                        }

                        checkIsColliderInLineOfSight(m_central_VisionRadius, _i);
                        if (m_collider_In_LineOfSight)
                        {
                            m_deltaAwarnes = (m_radiusCentralVision_SQR - m_currentDistance_SQR) / m_radiusCentralVision_SQR;
                            if (m_useSeeBooster && m_seeBooster.AffectedAspect == AffectedAsspect.deltaSeeAwarness)
                            {
                                m_deltaAwarnes = m_seeBooster.Apply_BuffTo(m_deltaAwarnes);
                            }

                            calculate_SeeAwarness(_1D_Index);
                        }
                    }
                    else
                    {
                        if (m_use_PeripheralFOV)
                        {
                            m_collider_In_Periphera_VisionRange = m_currentDistance_SQR < m_radiusPeripheralVision_SQR;
                            if (m_collider_In_Periphera_VisionRange)
                            {
                                m_collider_In_Periphera_VisionAngle = Mathf.Abs(m_currentAngle) < (m_central_VisionAngle / 2f) + (m_peripheral_VisionAngle / 2f);
                                if (m_collider_In_Periphera_VisionAngle)
                                {
                                    if (Use_RayCastInFovOnly)
                                    {
                                        if (Use_IgnoreRotationX)
                                        {
                                            m_target_Direction.y = 0f;
                                        }
                                        else
                                        {
                                            m_target_Direction.y = m_transform_Forward.y;
                                        }
                                    }

                                    checkIsColliderInLineOfSight(m_peripheral_VisionRadius, _i);
                                    if (m_collider_In_LineOfSight)
                                    {
                                        m_deltaAwarnes = (m_radiusPeripheralVision_SQR - m_currentDistance_SQR) / m_radiusPeripheralVision_SQR;
                                        if (m_useSeeBooster && m_seeBooster.AffectedAspect == AffectedAsspect.deltaSeeAwarness)
                                        {
                                            m_deltaAwarnes = m_seeBooster.Apply_BuffTo(m_deltaAwarnes);
                                        }

                                        calculate_SeeAwarness(_1D_Index);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void mathf_StupidButSimple_ABS()
        {
            if (m_currentAngle < 0)
            {
                m_currentAngle = -m_currentAngle;
            }
        }

        void grab_frameVaraibles()
        {
            m_transform_Position = m_transform.position;
            m_transform_Forward = m_eyes ? m_eyes.forward : m_transform.forward; //we should use m_eyes forward instead of transform.forward
            if (Use_OffsetPosition)
            {
                m_transform_Position += Offset_Position;
            }

            if (Use_OffsetRotation)
            {
                Quaternion _rotationQuat = Quaternion.Euler(Offset_Rotation);
                m_transform_Forward = _rotationQuat * m_transform_Forward;
            }

            m_forwardX = m_transform_Forward.x;
            m_forwardZ = m_transform_Forward.z;
            m_originX = m_transform_Position.x;
            m_originZ = m_transform_Position.z;
            m_forwardAngleRadians = Mathf.Atan2(m_transform_Forward.z, m_transform_Forward.x);
        }

        void calculate_SeeAwarness(int _index)
        {
            float _delta = m_deltaAwarnes;
            m_target_HadStealthComponent = m_targetSenses.Cached_StealthComponent(out m_targetStealthComponent);
            if (m_target_HadStealthComponent)
            {
                int _maxStealthBuff = 1;
                float _stealthBuff = m_targetStealthComponent.ProvideTotalStealthBuff();
                m_deltaAwarnes = m_deltaAwarnes * (_maxStealthBuff - _stealthBuff);
            }

            if (m_deltaAwarnes < 0)
            {
                m_deltaAwarnes = 0;
            }

            addAwarness_See(_index);
        }

        float anle_XZ(Vector3 _directionVector3)
        {
            float _angle = (m_forwardAngleRadians - Mathf.Atan2(_directionVector3.z, _directionVector3.x)) * Mathf.Rad2Deg;
            if (_angle > 180)
            {
                _angle -= 360;
            }

            if (_angle < -180)
            {
                _angle += 360;
            }

            return _angle;
        }

        void checkIsColliderInLineOfSight(float _radius, int _colliderIndex)
        {
            m_remainingRange = _radius;
            Vector3 _originPoint = m_transform_Position;
            float _rangeLoweringRatio = 0f;
            Collider _targetCollider = m_collidersTargetSenses[_colliderIndex];
            // Targets must also be raycastable for direct hits and Requires Perfect LOS.
            int _blockingLayerMask = m_seeLayersSolution == SeeLayersSolution.Fixer33
                ? m_seeBlockingLayers.value | m_seeTargetLayers.value
                : ~IgnoreLayers.value;
            while (!m_collider_In_LineOfSight && m_remainingRange > 0.1f)
            {
                if (Physics.Raycast(_originPoint, m_target_Direction, out m_hitInfo, m_remainingRange, _blockingLayerMask))
                {
                    Collider _hitCollider = m_hitInfo.collider;
                    // DIRECT TARGET HIT
                    if (_hitCollider == _targetCollider)
                    {
                        m_collider_In_LineOfSight = true;
                        if (Use_DebugDrawRay)
                        {
                            Debug.DrawRay(_originPoint, m_target_Direction * m_hitInfo.distance, Color.red);
                        }
                    }
                    else
                    {
                        // Cache reference ONCE
                        m_colliderReference = GlobalColliderReferenceRegistry.Get(_hitCollider);
                        // PERFECT FOV MODE
                        if (m_use_PerfectLineOfViewCheck)
                        {
                            if (m_colliderReference.isObstacle(out m_obstacle))
                            {
                                _rangeLoweringRatio = m_obstacle.Range_DecreaseFactor;
                                m_remainingRange = (m_remainingRange - m_hitInfo.distance) * (1f - _rangeLoweringRatio);
                            }
                            else
                            {
                                m_remainingRange = 0f;
                                if (Use_DebugDrawRay)
                                {
                                    Debug.DrawRay(_originPoint, m_target_Direction * m_hitInfo.distance, Color.green);
                                }
                            }
                        }
                        else
                        {
                            // Not perfect mode: obstacle → else target → else block
                            if (m_colliderReference.isObstacle(out m_obstacle))
                            {
                                _rangeLoweringRatio = m_obstacle.Range_DecreaseFactor;
                                m_remainingRange = (m_remainingRange - m_hitInfo.distance) * (1f - _rangeLoweringRatio);
                            }
                            else if (m_colliderReference.iS_SensesTarget(out TargetSenses _ts))
                            {
                                if (Use_DebugDrawRay)
                                {
                                    Debug.DrawRay(_originPoint, m_target_Direction * m_hitInfo.distance, Color.yellow);
                                }
                            }
                            else
                            {
                                m_remainingRange = 0f;
                                if (Use_DebugDrawRay)
                                {
                                    Debug.DrawRay(_originPoint, m_target_Direction * m_hitInfo.distance, Color.green);
                                }
                            }
                        }
                    }

                    // Move origin to avoid hitting same surface again
                    _originPoint = m_hitInfo.point + m_target_Direction * 0.01f;
                }
                else
                {
                    m_remainingRange = 0f;
                }
            }
        }

        float currentDistanceSQR_XZ(Vector3 _targetPosition)
        {
            m_xD = _targetPosition.x - m_transform_Position.x;
            m_zD = _targetPosition.z - m_transform_Position.z;
            return m_xD * m_xD + m_zD * m_zD;
        }

#endregion
#region Hear
        [field: SerializeField]
        public HearSensitivity Hear_Sensitivity;
        public void Set_HearSensitivity(HearSensitivity _hear_Sensitivity) => Hear_Sensitivity = _hear_Sensitivity;
        public void Recived_Noise(float _recivedNoise, TargetSenses _originTargetSenses)
        {
            if (_recivedNoise <= 0)
            {
                return;
            }

            if (_recivedNoise <= minimum_dB_hearing_sensitivity())
            {
                return;
            }

            int _factionNumber = (int)_originTargetSenses.FactionName;
            //we need to check did faction exist...
            checkDidWeEverDetectFaction(_factionNumber);
            checkAndAddToDictionary(_factionNumber, _originTargetSenses, out int _index);
            int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _index;
            float _hear_Awarness = Array_awarness_Hear[_1D_Index];
            Array_hearLastPosition[_1D_Index] = _originTargetSenses.transform.position;
            if (_hear_Awarness < 100)
            {
                if (_hear_Awarness < 0)
                {
                    _hear_Awarness = 0;
                }

                Array_awarness_Hear[_1D_Index] = Mathf.Clamp(_hear_Awarness + _recivedNoise, 0, 100);
                Array_awarness_See_WasIncreased[_1D_Index] = true;
            }

            _originTargetSenses.SetWasDetected();
        }

        //here we will add extra slots in middle
        void increaseArrayLenghtWithoutDataLost_DueToAmountOfTargetsInSomeFactionChanged(int _factionNumberReuiringChange)
        {
            ushort _oldAmountOfFactionSlots = m_1DAmountOfSlots[_factionNumberReuiringChange];
            int _newAmountOfFactionSlots = _oldAmountOfFactionSlots * 2;
            int _amountOfSlotsNeededToAddToArrays = _newAmountOfFactionSlots - _oldAmountOfFactionSlots;
            int _amountOfSlotsUsedNow = howManySlotsAreUsedNow();
            int _newLength = _amountOfSlotsUsedNow + _amountOfSlotsNeededToAddToArrays;
            if (m_use_See)
            {
                ResizeArray_DueToAmountOfSlotsChange(ref Array_awarness_See, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_targetSensesWasAlreadyHandledInThisFrame, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
            }

            if (m_use_Hear)
            {
                ResizeArray_DueToAmountOfSlotsChange(ref Array_awarness_Hear, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_hearLastPosition, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
            }

            if (m_use_Smell)
            {
                ResizeArray_DueToAmountOfSlotsChange(ref Array_awarness_Smell, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_scentLeadingPosition, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_scentLeadingNode, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_scentStrength, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
            }

            if (m_use_See || m_use_Hear || m_use_Smell)
            {
                ResizeArray_DueToAmountOfSlotsChange(ref Array_distanceSQR, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_angle, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_gameObjects, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
                ResizeArray_DueToAmountOfSlotsChange(ref Array_awarness_See_WasIncreased, _newLength, _factionNumberReuiringChange, _amountOfSlotsNeededToAddToArrays);
            }

            int _beginingOfFactionNumberReuiringChange = m_1DBeginingOfFactionIndex[_factionNumberReuiringChange];
            for (int _i = 0; _i < m_1DBeginingOfFactionIndex.Length; _i++)
            {
                if (_beginingOfFactionNumberReuiringChange < m_1DBeginingOfFactionIndex[_i])
                {
                    m_1DBeginingOfFactionIndex[_i] += _amountOfSlotsNeededToAddToArrays;
                }
            }

            m_1DAmountOfSlots[_factionNumberReuiringChange] = (ushort)_newAmountOfFactionSlots;
        }

        //here we just add slots add eng of array
        void increaseArrayLenghtWithoutDataLost_DueToAmountOfFactionsChanged(int _newLength, out int _1DBeginingIndexOfNewFaction)
        {
            if (m_use_See)
            {
                ResizeArray_DueToAmountOfFactionChange(ref Array_awarness_See, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_targetSensesWasAlreadyHandledInThisFrame, _newLength);
            }

            if (m_use_Hear)
            {
                ResizeArray_DueToAmountOfFactionChange(ref Array_awarness_Hear, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_hearLastPosition, _newLength);
            }

            if (m_use_Smell)
            {
                ResizeArray_DueToAmountOfFactionChange(ref Array_awarness_Smell, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_scentLeadingPosition, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_scentLeadingNode, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_scentStrength, _newLength);
            }

            if (m_use_See || m_use_Hear || m_use_Smell)
            {
                _1DBeginingIndexOfNewFaction = Array_distanceSQR.Length;
                ResizeArray_DueToAmountOfFactionChange(ref Array_distanceSQR, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_angle, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_gameObjects, _newLength);
                ResizeArray_DueToAmountOfFactionChange(ref Array_awarness_See_WasIncreased, _newLength);
            }
            else
            {
                _1DBeginingIndexOfNewFaction = 0;
            }
        }

        void ResizeArray_DueToAmountOfSlotsChange<T>(ref T[] array, int newLength, int _factionNumberReuiringChange, int _amountOfSlotsWeNeedToAdd)
        {
            T[] _newArray = new T[newLength];
            int _beginingOfFactionNumberReuiringChange = m_1DBeginingOfFactionIndex[_factionNumberReuiringChange];
            for (int _faction = 0; _faction < m_amountOfFactions; _faction++)
            {
                int _oldBegin = m_1DBeginingOfFactionIndex[_faction];
                int _count = m_array_currentNumberOfTargets[_faction];
                int _newBegin = _oldBegin;
                if (_oldBegin > _beginingOfFactionNumberReuiringChange)
                {
                    _newBegin += _amountOfSlotsWeNeedToAdd;
                }

                for (int _i = 0; _i < _count; _i++)
                {
                    _newArray[_newBegin + _i] = array[_oldBegin + _i];
                }
            }

            array = _newArray;
        }

        void ResizeArray_DueToAmountOfFactionChange<T>(ref T[] array, int newLength)
        {
            T[] _newArray = new T[newLength];
            int _copyLength = array.Length;
            for (int _i = 0; _i < _copyLength; _i++)
            {
                _newArray[_i] = array[_i];
            }

            array = _newArray;
        }

        float minimum_dB_hearing_sensitivity()
        {
            switch (Hear_Sensitivity)
            {
                case HearSensitivity.Low:
                {
                    return 40f;
                }

                case HearSensitivity.Medium:
                {
                    return 20f;
                }

                case HearSensitivity.High:
                {
                    return 5f;
                }

                case HearSensitivity.Perfect:
                {
                    return 0f;
                }
            }

            return 0f;
        }

#endregion
#region Smell
        public float Smell_Radius => m_smell_Radius;

        [field: SerializeField]
        float m_smell_Radius;
        public void Set_SmellRadius(float _smell_Radius) => m_smell_Radius = _smell_Radius;
        public bool Use_CustomRefreshRateSmell => m_use_CustomRefreshRateSmell;

        [field: SerializeField]
        bool m_use_CustomRefreshRateSmell;
        public void Set_UseCustomRefreshRateSmell(bool _use_CustomRefreshRateSmell) => m_use_CustomRefreshRateSmell = _use_CustomRefreshRateSmell;
        [field: SerializeField]
        public float Cooldown_SmellTimeAmount { get; private set; }

        public void Set_CooldownSmellTimeAmount(float _cooldown_Smell_TimeAmount)
        {
            Cooldown_SmellTimeAmount = _cooldown_Smell_TimeAmount;
            if (m_previous_cooldown_Smell_TimeAmount != Cooldown_SmellTimeAmount)
            {
                m_previous_cooldown_Smell_TimeAmount = Cooldown_SmellTimeAmount;
                m_cooldownSmell_IntervalEnumerator.ChangeInterval(Cooldown_SmellTimeAmount);
            }
        }

        float m_previous_cooldown_Smell_TimeAmount;
        IntervalEnumerator m_cooldownSmell_IntervalEnumerator;
        [field: SerializeField]
        public LayerMask ScentLayer;
        void addAwarness_Smell(int _1d_Index, float _awarnessDelta)
        {
            float _smell_Awarness = Array_awarness_Smell[_1d_Index];
            if (_smell_Awarness < 100)
            {
                if (_smell_Awarness < 0)
                {
                    _smell_Awarness = 0;
                }

                _smell_Awarness = Mathf.Clamp(_smell_Awarness + _awarnessDelta, 0, 100);
                Array_awarness_Smell[_1d_Index] = _smell_Awarness;
            }

            Array_awarness_See_WasIncreased[_1d_Index] = true;
            m_targetSenses.SetWasDetected();
        }

#endregion
        void OnDestroy()
        {
            if (m_use_Hear)
            {
                NoiseRecivers.Instance.Remove_NoiseReciver(this);
            }

            if (m_use_See && m_use_DisplayFOV && m_fov)
            {
                m_fov.StopDisplayingThisSense(this);
                m_fov = null;
            }
        }

#region API's
        List<TargetSenses> m_targetSensesList = new List<TargetSenses>();
        List<float> m_awarness = new List<float>();
        float currentSeeAwarnessValue(int _factionNumber, int i, out int _1D_Index)
        {
            _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + i;
            float _currentAwarnessValue = 0f;
            float _toAddValue;
            if (m_use_See)
            {
                _toAddValue = Array_awarness_See[_1D_Index];
                if (_toAddValue < 0)
                {
                    _toAddValue = 0;
                }

                _currentAwarnessValue += _toAddValue;
            }

            return _currentAwarnessValue;
        }

        float currentAwarnessValue(int _factionNumber, int i, out int _1D_Index)
        {
            _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + i;
            float _currentAwarnessValue = 0f;
            float _toAddValue;
            if (m_use_See)
            {
                _toAddValue = Array_awarness_See[_1D_Index];
                if (_toAddValue < 0)
                {
                    _toAddValue = 0;
                }

                _currentAwarnessValue += _toAddValue;
            }

            if (m_use_Hear)
            {
                _toAddValue = Array_awarness_Hear[_1D_Index];
                if (_toAddValue < 0)
                {
                    _toAddValue = 0;
                }

                _currentAwarnessValue += _toAddValue;
            }

            if (m_use_Smell)
            {
                _toAddValue = Array_awarness_Smell[_1D_Index];
                if (_toAddValue < 0)
                {
                    _toAddValue = 0;
                }

                _currentAwarnessValue += _toAddValue;
            }

            return _currentAwarnessValue;
        }

        //2.0
        public bool DidSee(TargetSenses _targetSenses, int _requiredAwarness, out float _seeAwarness, out float _distanceSQR, out float _angle)
        {
            _seeAwarness = 0f;
            _distanceSQR = 0f;
            _angle = 0f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_See)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = (int)_targetSenses.FactionName;
            if (m_array_TargetSenses_Index[_factionNumber].TryGetValue(_targetSenses, out int _index))
            {
                int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _index;
                _seeAwarness = Array_awarness_See[_1D_Index];
                if (_seeAwarness > _requiredAwarness)
                {
                    _distanceSQR = Array_distanceSQR[_1D_Index];
                    _angle = Array_angle[_1D_Index];
                    return true;
                }
            }

            return false;
        }

        public bool DidHear(TargetSenses _targetSenses, int _requiredAwarness, out float _hearAwarness, out Vector3 _soundLastPosition)
        {
            _soundLastPosition = Vector3.zero;
            _hearAwarness = 0f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_Hear)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = (int)_targetSenses.FactionName;
            if (m_array_TargetSenses_Index[_factionNumber].TryGetValue(_targetSenses, out int _index))
            {
                int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _index;
                _hearAwarness = Array_awarness_Hear[_1D_Index];
                if (_hearAwarness > _requiredAwarness)
                {
                    _soundLastPosition = Array_hearLastPosition[_1D_Index];
                    return true;
                }
            }

            return false;
        }

        public bool DidSmell(TargetSenses _targetSenses, int _requiredAwarness, out float _smellAwarness, out Vector3 _scentLastPosition, out float _scentStrength)
        {
            _smellAwarness = 0f;
            _scentLastPosition = Vector3.zero;
            _scentStrength = 0f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_Smell)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = (int)_targetSenses.FactionName;
            if (m_array_TargetSenses_Index[_factionNumber].TryGetValue(_targetSenses, out int _index))
            {
                int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _index;
                _smellAwarness = Array_awarness_Smell[_1D_Index];
                if (_smellAwarness > _requiredAwarness)
                {
                    _scentStrength = Array_scentStrength[_1D_Index];
                    if (_scentStrength == 0)
                    {
                        return false;
                    }

                    _scentLastPosition = Array_scentLeadingPosition[_1D_Index];
                    return true;
                }
            }

            return false;
        }

        //Fixed in 2.03
        /* Updated in version 2.03.01 to properly handle requests where _needToBeInCentralRange = false.
         Now, when WasAnythingSeen is called with _needToBeInCentralRange = false, the Senses Component
         additionally checks the distance and angle to remembered SensesComponents that were not
         included in the sphere cast during the update.
        */
        public bool WasAnythingSeen(float _requiredAwarness, out TargetSenses _targetSenses, out float _currentAwarness, out float _distanceSQR, out float _angle, FactionNames[] _factionNames, SeeSenseSearchType _searchType = SeeSenseSearchType.HighestAwarness, bool _needToBeInCentralRange = true)
        {
            _targetSenses = default;
            _distanceSQR = -1f;
            _angle = -1f;
            _currentAwarness = -1f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_See)
            {
                return false;
            }

            if (!_needToBeInCentralRange && !m_needToHandleTargets_OutOfRange)
            {
                m_needToHandleTargets_OutOfRange = true;
            }

            //grab_frameVaraibles();
            int _factionNumber = 0;
            bool _findSomeResult = false;
            float _currentSquaredDistance;
            float _maxAwarness = 0f;
            float _nearestSquaredDistance = float.MaxValue;
            float _currentAngle;
            float _lowestAngle = float.MaxValue;
            int _best_TargetSensesIndex = 0;
            int _best_FactionIndex = 0;
            switch (_searchType)
            {
                case SeeSenseSearchType.Nearest:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_factionNumber, _i, out int _1D_Index);
                            if (_currentAwarness > _requiredAwarness)
                            {
                                _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                if (_currentSquaredDistance != -1 && _currentSquaredDistance < _nearestSquaredDistance)
                                {
                                    _nearestSquaredDistance = _currentSquaredDistance;
                                    _best_TargetSensesIndex = _i;
                                    _best_FactionIndex = _factionNumber;
                                    _distanceSQR = _currentSquaredDistance;
                                    _angle = Array_angle[_1D_Index];
                                    _findSomeResult = true;
                                }
                            }
                        }
                    }

                    if (_needToBeInCentralRange)
                    {
                        _findSomeResult = _nearestSquaredDistance <= m_radiusCentralVision_SQR;
                        if (_findSomeResult)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index);
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                        }

                        return _findSomeResult;
                    }
                    else
                    {
                        if (_findSomeResult) //fix in 1.72
                        { //fix in 1.72
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index); //fix in 1.72
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex); //fix in 1.72
                        } //fix in 1.72

                        return _findSomeResult;
                    }
#endregion
                } //checked vs !NeedToBeInRange

                case SeeSenseSearchType.HighestAwarness:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_factionNumber, _i, out int _1D_Index);
                            {
                                if (_currentAwarness > _requiredAwarness)
                                {
                                    if (_needToBeInCentralRange)
                                    {
                                        _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                        if (_currentSquaredDistance != -1 && _currentSquaredDistance < m_radiusCentralVision_SQR)
                                        {
                                            if (_currentAwarness > _maxAwarness)
                                            {
                                                _nearestSquaredDistance = _currentSquaredDistance; //fix in 1.72
                                                _maxAwarness = _currentAwarness;
                                                _best_TargetSensesIndex = _i;
                                                _best_FactionIndex = _factionNumber;
                                                _distanceSQR = _currentSquaredDistance;
                                                _angle = Array_angle[_1D_Index];
                                                _findSomeResult = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_currentAwarness > _maxAwarness)
                                        {
                                            _maxAwarness = _currentAwarness;
                                            _best_TargetSensesIndex = _i;
                                            _best_FactionIndex = _factionNumber;
                                            _distanceSQR = Array_distanceSQR[_1D_Index];
                                            _angle = Array_angle[_1D_Index];
                                            _findSomeResult = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (_needToBeInCentralRange)
                    {
                        _findSomeResult = _nearestSquaredDistance <= m_radiusCentralVision_SQR;
                        if (_findSomeResult)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index);
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                        }

                        return _findSomeResult;
                    }
                    else
                    {
                        if (_findSomeResult) //fix in 1.72
                        { //fix in 1.72
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index); //fix in 1.72
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex); //fix in 1.72
                        } //fix in 1.72

                        return _findSomeResult;
                    }
#endregion
                } //checked vs !NeedToBeInRange

                case SeeSenseSearchType.LowestAngle:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_factionNumber, _i, out int _1D_Index);
                            if (_currentAwarness > _requiredAwarness)
                            {
                                {
                                    if (_needToBeInCentralRange)
                                    {
                                        _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                        if (_currentSquaredDistance != -1 && _currentSquaredDistance < m_radiusCentralVision_SQR)
                                        {
                                            _currentAngle = Array_angle[_1D_Index];
                                            if (_currentAngle < _lowestAngle)
                                            {
                                                _lowestAngle = _currentAngle;
                                                _best_TargetSensesIndex = _i;
                                                _best_FactionIndex = _factionNumber;
                                                _distanceSQR = _currentSquaredDistance;
                                                _angle = _currentAngle;
                                                _findSomeResult = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _currentAngle = Array_angle[_1D_Index];
                                        if (_currentAngle < _lowestAngle)
                                        {
                                            _lowestAngle = _currentAngle;
                                            _best_TargetSensesIndex = _i;
                                            _best_FactionIndex = _factionNumber;
                                            _distanceSQR = Array_distanceSQR[_1D_Index];
                                            _angle = _currentAngle;
                                            _findSomeResult = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

#endregion
                    if (_needToBeInCentralRange)
                    {
                        _findSomeResult = _findSomeResult && _distanceSQR <= m_radiusCentralVision_SQR;
                        if (_findSomeResult)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index);
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                        }

                        return _findSomeResult;
                    }
                    else
                    {
                        if (_findSomeResult) //fix in 1.72
                        { //fix in 1.72
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index); //fix in 1.72
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex); //fix in 1.72
                        } //fix in 1.72

                        return _findSomeResult;
                    }
                }

                case SeeSenseSearchType.NearestInCentralVisionAngle:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            if (currentSeeAwarnessValue(_factionNumber, _i, out int _1D_Index) > _requiredAwarness)
                            {
                                if (_needToBeInCentralRange)
                                {
                                    _currentAngle = Array_angle[_1D_Index];
                                    if (_currentAngle < m_central_VisionAngle / 2f)
                                    {
                                        _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                        if (_currentSquaredDistance != -1 && _currentSquaredDistance < _nearestSquaredDistance)
                                        {
                                            _nearestSquaredDistance = _currentSquaredDistance;
                                            _best_TargetSensesIndex = _i;
                                            _best_FactionIndex = _factionNumber;
                                            _distanceSQR = _currentSquaredDistance;
                                            _angle = _currentAngle;
                                            _findSomeResult = true;
                                        }
                                    }
                                }
                                else
                                {
                                    _currentAngle = Array_angle[_1D_Index];
                                    if (_currentAngle < m_central_VisionAngle / 2f)
                                    {
                                        _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                        if (_currentSquaredDistance != -1 && _currentSquaredDistance < _nearestSquaredDistance) //removed && _currentSquaredDistance < m_radiusCentralVision_SQR
                                        {
                                            _nearestSquaredDistance = _currentSquaredDistance;
                                            _best_TargetSensesIndex = _i;
                                            _best_FactionIndex = _factionNumber;
                                            _distanceSQR = _currentSquaredDistance;
                                            _angle = _currentAngle;
                                            _findSomeResult = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

#endregion
                    if (_needToBeInCentralRange)
                    {
                        _findSomeResult = _nearestSquaredDistance <= m_radiusCentralVision_SQR;
                        if (_findSomeResult)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index);
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                        }

                        return _findSomeResult;
                    }
                    else
                    {
                        if (_findSomeResult) //fix in 1.72
                        { //fix in 1.72
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index); //fix in 1.72
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex); //fix in 1.72
                        } //fix in 1.72

                        return _findSomeResult;
                    }
                } //checked vs !NeedToBeInRange

                case SeeSenseSearchType.NearestInBothAngles:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_factionNumber, _i, out int _1D_Index);
                            if (_currentAwarness > _requiredAwarness)
                            {
                                {
                                    if (_needToBeInCentralRange)
                                    {
                                        _currentAngle = Array_angle[_1D_Index];
                                        if (_currentAngle < (m_central_VisionAngle / 2f) + (m_peripheral_VisionAngle / 2f))
                                        {
                                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                            if (_currentSquaredDistance != -1 && _currentSquaredDistance < _nearestSquaredDistance)
                                            {
                                                _nearestSquaredDistance = _currentSquaredDistance;
                                                _best_TargetSensesIndex = _i;
                                                _best_FactionIndex = _factionNumber;
                                                _distanceSQR = _currentSquaredDistance;
                                                _angle = _currentAngle;
                                                _findSomeResult = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _currentAngle = Array_angle[_1D_Index];
                                        if (_currentAngle < (m_central_VisionAngle / 2f) + (m_peripheral_VisionAngle / 2f))
                                        {
                                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                                            if (_currentSquaredDistance != -1 && _currentSquaredDistance < _nearestSquaredDistance) //removed && _currentSquaredDistance < m_radiusCentralVision_SQR
                                            {
                                                _nearestSquaredDistance = _currentSquaredDistance;
                                                _best_TargetSensesIndex = _i;
                                                _best_FactionIndex = _factionNumber;
                                                _distanceSQR = _currentSquaredDistance;
                                                _angle = _currentAngle;
                                                _findSomeResult = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

#endregion
                    if (_needToBeInCentralRange)
                    {
                        _findSomeResult = _nearestSquaredDistance <= m_radiusCentralVision_SQR;
                        if (_findSomeResult)
                        {
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index);
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                        }

                        return _findSomeResult;
                    }
                    else
                    {
                        if (_findSomeResult) //fix in 1.72
                        { //fix in 1.72
                            _currentAwarness = currentSeeAwarnessValue(_best_FactionIndex, _best_TargetSensesIndex, out int _1D_Index); //fix in 1.72
                            _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex); //fix in 1.72
                        } //fix in 1.72

                        return _findSomeResult;
                    }
                }
            }

            return false;
        }

        public bool WasAnythingHeard(float _requiredAwarness, out TargetSenses _targetSenses, out float _currentAwarness, out Vector3 _soundLastPosition, FactionNames[] _factionNames, HearSenseSearchType _hearSenseSearchType = HearSenseSearchType.HighestAwarness)
        {
            _targetSenses = default;
            _soundLastPosition = Vector3.zero;
            _currentAwarness = -1f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_Hear)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = 0;
            bool _findSomeResult = false;
            float _currentSquaredDistance;
            float _maxAwarness = 0f;
            float _nearestSquaredDistance = float.MaxValue;
            int _best_TargetSensesIndex = 0;
            int _best_FactionIndex = 0;
            switch (_hearSenseSearchType)
            {
                case HearSenseSearchType.Nearest:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _i;
                            _currentAwarness = Array_awarness_Hear[_1D_Index];
                            if (_currentAwarness > _requiredAwarness)
                            {
                                {
                                    _soundLastPosition = Array_hearLastPosition[_1D_Index];
                                    _currentSquaredDistance = (this.transform.position - _soundLastPosition).magnitude;
                                    if (_currentSquaredDistance < _nearestSquaredDistance)
                                    {
                                        _nearestSquaredDistance = _currentSquaredDistance;
                                        _best_TargetSensesIndex = _i;
                                        _best_FactionIndex = _factionNumber;
                                        _findSomeResult = true;
                                    }
                                }
                            }
                        }
                    }

                    if (_findSomeResult)
                    {
                        int _1D_Index = m_1DBeginingOfFactionIndex[_best_FactionIndex] + _best_TargetSensesIndex;
                        _currentAwarness = Array_awarness_Hear[_1D_Index];
                        _soundLastPosition = Array_hearLastPosition[_1D_Index];
                        _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                    }

                    return _findSomeResult;
#endregion
                }

                case HearSenseSearchType.HighestAwarness:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _i;
                            _currentAwarness = Array_awarness_Hear[_1D_Index];
                            {
                                if (_currentAwarness > _requiredAwarness)
                                {
                                    if (_currentAwarness > _maxAwarness)
                                    {
                                        _maxAwarness = _currentAwarness;
                                        _best_TargetSensesIndex = _i;
                                        _best_FactionIndex = _factionNumber;
                                        _soundLastPosition = Array_hearLastPosition[_1D_Index];
                                        _findSomeResult = true;
                                    }
                                }
                            }
                        }
                    }

                    if (_findSomeResult)
                    {
                        int _1D_Index = m_1DBeginingOfFactionIndex[_best_FactionIndex] + _best_TargetSensesIndex;
                        _currentAwarness = Array_awarness_Hear[_1D_Index];
                        _soundLastPosition = Array_hearLastPosition[_1D_Index];
                        _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                    }

                    return _findSomeResult;
#endregion
                }
            }

            return false;
        }

        public bool WasAnythingSmelled(float _requiredAwarness, out TargetSenses _targetSenses, out float _currentAwarness, out Vector3 _scentLastPosition, out float _scentStrength, FactionNames[] _factionNames, SmellSenseSearchType _smellSenseSearchType = SmellSenseSearchType.HighestAwarness)
        {
            _targetSenses = default;
            _scentLastPosition = Vector3.zero;
            _currentAwarness = -1f;
            _scentStrength = -1f;
            if (!m_wasEnabled)
            {
                return false;
            }

            if (!m_use_Smell)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = 0;
            bool _findSomeResult = false;
            float _maxAwarness = 0f;
            int _best_TargetSensesIndex = 0;
            int _best_FactionIndex = 0;
            switch (_smellSenseSearchType)
            {
                case SmellSenseSearchType.HighestAwarness:
                {
#region MK4: Array Reusage Solution
                    for (int _j = 0; _j < _factionNames.Length; _j++)
                    {
                        _factionNumber = (int)_factionNames[_j];
                        for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                        {
                            int _1D_Index = m_1DBeginingOfFactionIndex[_factionNumber] + _i;
                            _currentAwarness = Array_awarness_Smell[_1D_Index];
                            {
                                if (_currentAwarness > _requiredAwarness)
                                {
                                    if (_currentAwarness > _maxAwarness)
                                    {
                                        _maxAwarness = _currentAwarness;
                                        _best_TargetSensesIndex = _i;
                                        _best_FactionIndex = _factionNumber;
                                        _scentLastPosition = Array_scentLeadingPosition[_1D_Index];
                                        _findSomeResult = true;
                                    }
                                }
                            }
                        }
                    }

                    if (_findSomeResult)
                    {
                        int _1D_Index = m_1DBeginingOfFactionIndex[_best_FactionIndex] + _best_TargetSensesIndex;
                        _currentAwarness = Array_awarness_Smell[_1D_Index];
                        _scentLastPosition = Array_scentLeadingPosition[_1D_Index];
                        _scentStrength = Array_scentStrength[_1D_Index];
                        _targetSenses = get_ByFactionAndValue(_best_FactionIndex, _best_TargetSensesIndex);
                    }

                    return _findSomeResult;
#endregion
                }
            }

            return false;
        }

        //Added in 2.03
        bool DidDetect(TargetSenses _targetSenses, int _requiredAwarness, out float _awarness)
        {
            _awarness = 0f;
            if (!m_wasEnabled)
            {
                return false;
            }

            //grab_frameVaraibles();
            int _factionNumber = (int)_targetSenses.FactionName;
            if (m_array_TargetSenses_Index[_factionNumber].TryGetValue(_targetSenses, out int _index))
            {
                _awarness = currentAwarnessValue(_factionNumber, _index, out int _1D_Index);
                if (_awarness > _requiredAwarness)
                {
                    return true;
                }
            }

            return false;
        }

        bool DidDetectAnything(float _requiredAwarness, out TargetSenses _detectedTargetSenses, FactionNames[] _factionNames)
        {
            if (WasAnythingSeen(_requiredAwarness, out TargetSenses _detectedTargetSenses2, out float _currentAwarnessSee, out float _distanceSQR, out float _angle, _factionNames, SeeSenseSearchType.NearestInBothAngles, true))
            {
                _detectedTargetSenses = _detectedTargetSenses2;
                return true;
            }

            if (WasAnythingHeard(_requiredAwarness, out TargetSenses _detectedTargetSenses3, out float _currentAwarnessHear, out Vector3 _soundLastPosition, _factionNames, HearSenseSearchType.HighestAwarness))
            {
                _detectedTargetSenses = _detectedTargetSenses3;
                return true;
            }

            if (WasAnythingSmelled(_requiredAwarness, out TargetSenses _detectedTargetSenses4, out float _currentAwarness, out Vector3 _scentLastPosition, out float _scentStrength, _factionNames, SmellSenseSearchType.HighestAwarness))
            {
                _detectedTargetSenses = _detectedTargetSenses4;
                return true;
            }

            _detectedTargetSenses = null;
            return false;
        }

        public List<TargetSenses> GetDetectedTargetList(FactionNames[] _factionNames, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            m_targetSensesList.Clear();
            if (!m_wasEnabled)
            {
                return m_targetSensesList;
            }

            //grab_frameVaraibles();
            float _currentSquaredDistance;
            TargetSenses _currentTargetSenses;
            int _factionNumber = 0;
            for (int _j = 0; _j < _factionNames.Length; _j++)
            {
                _factionNumber = (int)_factionNames[_j];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    if (currentAwarnessValue(_factionNumber, _i, out int _1D_Index) > _requiredAwarness)
                    {
                        if (!_needToBeInCentralRange)
                        {
                            _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                            m_targetSensesList.Add(_currentTargetSenses);
                        }
                        else
                        {
                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                            if (_currentSquaredDistance != -1 && _currentSquaredDistance < m_radiusCentralVision_SQR)
                            {
                                _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                                m_targetSensesList.Add(_currentTargetSenses);
                            }
                        }
                    }
                }
            }

            return m_targetSensesList;
        }

        public List<TargetSenses> GetDetectedTargetList(FactionNames _factionNames = FactionNames.None, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            return GetDetectedTargetList(new FactionNames[1] { _factionNames }, _requiredAwarness, _needToBeInCentralRange);
        }

        public List<TargetSenses> GetDetectedTargetList(out List<float> _awarness, FactionNames[] _factionNames, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            m_targetSensesList.Clear();
            m_awarness.Clear();
            _awarness = m_awarness;
            float _currentAwarness;
            if (!m_wasEnabled)
            {
                return m_targetSensesList;
            }

            //grab_frameVaraibles();
            float _currentSquaredDistance;
            TargetSenses _currentTargetSenses;
            int _factionNumber = 0;
            for (int _j = 0; _j < _factionNames.Length; _j++)
            {
                _factionNumber = (int)_factionNames[_j];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    _currentAwarness = currentAwarnessValue(_factionNumber, _i, out int _1D_Index);
                    if (_currentAwarness > _requiredAwarness)
                    {
                        if (!_needToBeInCentralRange)
                        {
                            _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                            m_targetSensesList.Add(_currentTargetSenses);
                            m_awarness.Add(_currentAwarness);
                        }
                        else
                        {
                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                            if (_currentSquaredDistance != -1 && _currentSquaredDistance < m_radiusCentralVision_SQR)
                            {
                                _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                                m_targetSensesList.Add(_currentTargetSenses);
                                m_awarness.Add(_currentAwarness);
                            }
                        }
                    }
                }
            }

            _awarness = m_awarness;
            return m_targetSensesList;
        }

        public List<TargetSenses> GetDetectedTargetList(out List<float> _awarness, FactionNames _factionNames = FactionNames.None, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            return GetDetectedTargetList(out _awarness, new FactionNames[1] { _factionNames }, _requiredAwarness, _needToBeInCentralRange);
        }

        public List<TargetSenses> GetRememberedTargetList(FactionNames[] _factionNames, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            m_targetSensesList.Clear();
            if (!m_wasEnabled)
            {
                return m_targetSensesList;
            }

            //grab_frameVaraibles();
            float _currentAwarness;
            float _currentSquaredDistance;
            int _factionNumber = 0;
            for (int _j = 0; _j < _factionNames.Length; _j++)
            {
                _factionNumber = (int)_factionNames[_j];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    _currentAwarness = currentAwarnessValue(_factionNumber, _i, out int _1D_Index);
                    if (_currentAwarness > 0 && _currentAwarness < _requiredAwarness)
                    {
                        if (!_needToBeInCentralRange)
                        {
                            m_targetSensesList.Add(get_ByFactionAndValue(_factionNumber, _i));
                        }
                        else
                        {
                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                            if (_currentSquaredDistance < m_radiusCentralVision_SQR)
                            {
                                m_targetSensesList.Add(get_ByFactionAndValue(_factionNumber, _i));
                            }
                        }
                    }
                }
            }

            return m_targetSensesList;
        }

        public List<TargetSenses> GetRememberedTargetList(FactionNames _factionNames = FactionNames.None, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            return GetRememberedTargetList(new FactionNames[1] { _factionNames }, _requiredAwarness, _needToBeInCentralRange);
        }

        public List<TargetSenses> GetRememberedTargetList(out List<float> _awarness, FactionNames[] _factionNames, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            m_targetSensesList.Clear();
            m_awarness.Clear();
            _awarness = m_awarness;
            if (!m_wasEnabled)
            {
                return m_targetSensesList;
            }

            //grab_frameVaraibles();
            float _currentAwarness;
            float _currentSquaredDistance;
            int _factionNumber = 0;
            for (int _j = 0; _j < _factionNames.Length; _j++)
            {
                _factionNumber = (int)_factionNames[_j];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    _currentAwarness = currentAwarnessValue(_factionNumber, _i, out int _1D_Index);
                    if (_currentAwarness > 0 && _currentAwarness < _requiredAwarness)
                    {
                        if (!_needToBeInCentralRange)
                        {
                            m_awarness.Add(_currentAwarness);
                            m_targetSensesList.Add(get_ByFactionAndValue(_factionNumber, _i));
                        }
                        else
                        {
                            _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                            if (_currentSquaredDistance < m_radiusCentralVision_SQR)
                            {
                                m_awarness.Add(_currentAwarness);
                                m_targetSensesList.Add(get_ByFactionAndValue(_factionNumber, _i));
                            }
                        }
                    }
                }
            }

            _awarness = m_awarness;
            return m_targetSensesList;
        }

        public List<TargetSenses> GetRememberedTargetList(out List<float> _awarness, FactionNames _factionNames = FactionNames.None, float _requiredAwarness = 1f, bool _needToBeInCentralRange = true)
        {
            return GetRememberedTargetList(out _awarness, new FactionNames[1] { _factionNames }, _requiredAwarness, _needToBeInCentralRange);
        }

        public List<TargetSenses> GetAllTargetList(FactionNames[] _factionNames, bool _needToBeInCentralRange = true)
        {
            m_targetSensesList.Clear();
            if (!m_wasEnabled)
            {
                return m_targetSensesList;
            }

            //grab_frameVaraibles();
            float _currentSquaredDistance;
            TargetSenses _currentTargetSenses;
            int _factionNumber = 0;
            int _1D_Index = 0;
            int _factionBeginingOFArray;
            for (int _j = 0; _j < _factionNames.Length; _j++)
            {
                _factionNumber = (int)_factionNames[_j];
                _factionBeginingOFArray = m_1DBeginingOfFactionIndex[_factionNumber];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    if (!_needToBeInCentralRange)
                    {
                        _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                        m_targetSensesList.Add(_currentTargetSenses);
                    }
                    else
                    {
                        _1D_Index = _factionBeginingOFArray + _i;
                        _currentSquaredDistance = Array_distanceSQR[_1D_Index];
                        if (_currentSquaredDistance != -1 && _currentSquaredDistance < m_radiusCentralVision_SQR)
                        {
                            _currentTargetSenses = get_ByFactionAndValue(_factionNumber, _i);
                            m_targetSensesList.Add(_currentTargetSenses);
                        }
                    }
                }
            }

            return m_targetSensesList;
        }

        public List<TargetSenses> GetAllTargetList(FactionNames _factionNames = FactionNames.None, bool _needToBeInCentralRange = true)
        {
            return GetAllTargetList(new FactionNames[1] { _factionNames }, _needToBeInCentralRange);
        }

        /*
        TargetSenses get_ByFactionAndValue(int _factionNumber, int _indexToLookFor, bool _fullDebug = false)
        {
            foreach (var _key in m_array_TargetSenses_Index[_factionNumber].Keys)
            {
                if (_fullDebug) Debug.Log($"_key: {_key}, _index: {m_array_TargetSenses_Index[_factionNumber][_key]}");

                if (m_array_TargetSenses_Index[_factionNumber][_key] == _indexToLookFor)
                {
                    return _key;
                }
            }
            return default;
        }
        */
        TargetSenses get_ByFactionAndValue(int _factionNumber, int _indexToLookFor, bool _fullDebug = false)
        {
            foreach (var _entry in m_array_TargetSenses_Index[_factionNumber])
            {
                if (_fullDebug)
                {
                    Debug.Log($"_key: {_entry.Key}, _index: {_entry.Value}");
                }

                if (_entry.Value == _indexToLookFor)
                {
                    return _entry.Key;
                }
            }

            return default;
        }

#endregion
#region Added
        public void ResetAwarness()
        {
            for (int _factionNumber = 0; _factionNumber < m_amountOfFactions; _factionNumber++)
            {
                int _beginingOFFactionArray = m_1DBeginingOfFactionIndex[_factionNumber];
                for (int _i = 0; _i < m_array_currentNumberOfTargets[_factionNumber]; _i++)
                {
                    setDeffaultValueToArrays(_beginingOFFactionArray + _i);
                }
            }

            for (int _i = 0; _i < m_amountOfFactions; _i++)
            {
                m_array_TargetSenses_Index[_i].Clear();
                m_array_currentNumberOfTargets[_i] = 0;
            }
        }

#endregion
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (m_use_debug_Detected && m_wasEnabled)
            {
                List<TargetSenses> _detectedTargetSenes = GetDetectedTargetList(out List<float> _awarness, m_requiredFactions, m_requiredAwarness, false);
                for (int _i = 0; _i < _detectedTargetSenes.Count; _i++)
                {
                    EditorGizmoHelpers.DrawRayWithLabel(this, _detectedTargetSenes[_i], _awarness[_i], Color.red);
                }
            }
        }
#endif
    }
}
