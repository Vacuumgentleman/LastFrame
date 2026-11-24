using Senses2026;
using UnityEngine;
using UnityEngine.UI;

namespace Senses2026BonusContent
{
    /*
        Bonus Content Example – Demonstration of RedHandedActionSO usage.

        This script provides a simplified example of how to use RedHandedActionSO.
        It demonstrates five types of player actions and their gameplay consequences.

        ─────────────────────────────────────────────────────────────
        Action Type Overview:
        (Action Type #1) Instant actions without repercussions.
            • Example: throwing away a pack of cigarettes.
            • Guards ignore this action; no faction change occurs.

        (Action Type #2) Instant actions with automatic repercussions (even if not detected).
            • Example: sabotaging a generator (older design).
            • In this demo, the screwdriver action now behaves like Type #5 (abortable lasting).

        (Action Type #3) Instant actions with repercussions only if detected.
            • Example: pickpocketing a guard.
            • If detected, the Player is punished (faction change from Prisoners → Runners).

        (Action Type #4) Lasting actions that cannot be aborted, with repercussions if detected.
            • Reserved for future use (e.g., lockpicking or breaking doors).

        (Action Type #5) Lasting actions that can be aborted, with repercussions if detected.
            • Examples: cutting a fence or sabotaging a generator.
        ─────────────────────────────────────────────────────────────

        Repercussions:
        If a Player is caught performing an unauthorized action, they are “caught red-handed”.
        This triggers a faction change from Prisoners → Runners (representing punishment).

        Demonstration Summary:
        • Action Type #1 – Throwing cigarettes (harmless, no punishment)
        • Action Type #2 – Generator sabotage (can be aborted, always punished once completed)
        • Action Type #3 – Pickpocketing (punished only if detected)
        • Action Type #5 – Wire cutting (can be aborted, punished only if detected)

        This example focuses on player interaction and UI logic, using raycasting, 
        distance checks, and progress bars to visualize interactions.
    */

    public class CameraRay : MonoBehaviour
    {
        [SerializeField] Transform m_player;
        RedHanded m_redHanded;
        [SerializeField] float m_alphaFadeValue = .2f;
        [Space]

        // === WIRE CUTTERS ===
        // Demonstrates Action Type #5 – lasting action that can be aborted, punished if detected.
        // The player must hold the mouse button to complete cutting. Detection during this process
        // triggers a faction change from Prisoners → Runners.
        [Header("WireCutters")]
        bool m_wireCutterIsActive = true;
        [SerializeField] RectTransform m_recTransformWireCutters;
        [SerializeField] Animator m_recAnimatorWireCutters;
        [SerializeField] Toggle m_wireCutterToggle;
        [SerializeField] CanvasGroup m_wireCutterCanvasGroup;
        [SerializeField] float m_wireCutterWorkDistance = 3f;
        [SerializeField] float m_timeOfActionRequiredWireCutter = 1f;
        float m_currentTimeOfActionWireCutter = 0f;
        [SerializeField] Image m_progresIndication;
        [SerializeField] RedHandedActionSO m_wireCutActionSO;
        [Space]

        // === SCREWDRIVER (Generator Sabotage) ===
        // Demonstrates Action Type #2 – lasting action that can be aborted, but always punished once completed.
        // Represents sabotaging a power generator. The player must hold the mouse button to perform the action.
        // If released early, the action is aborted with no consequence; if completed, repercussions always occur
        // (the sabotage is too important for guards to ignore).
        [Header("ScrewDriver")]
        bool m_screwDriverIsActive = true;
        [SerializeField] RectTransform m_recTransformScrewDriver;
        [SerializeField] Animator m_recAnimatorScrewDriver;
        [SerializeField] Toggle m_screwDriverToggle;
        [SerializeField] CanvasGroup m_screwDriverCanvasGroup;
        [SerializeField] float m_screwDriverWorkDistance = 3f;
        [SerializeField] float m_timeOfActionRequiredScrewDriver = 1f;
        float m_currentTimeOfActionScrewDriver = 0f;
        [SerializeField] Image m_progresIndicationScrewDriver;
        [SerializeField] RedHandedActionSO m_screwDriverActionSO;
        [Space]

        // === CIGARETTES ===
        // Demonstrates Action Type #1 – instant action with no repercussions.
        // Represents throwing away cigarette packs. Guards do not react to this.
        [Header("Cigarets")]
        bool m_cigaertsIsActive = true;
        [SerializeField] Toggle m_cigaretsToggle;
        [SerializeField] RectTransform m_recTransformCiggaerts;
        [SerializeField] CanvasGroup m_cigaertsCanvasGroup;
        [SerializeField] float m_cigaretsWorkDistance = 3f;
        [SerializeField] Transform m_cigaretsPrefab;
        [SerializeField] int m_amountOfCigarets = 3;
        [Space]

        // === PICKPOCKETS ===
        // Demonstrates Action Type #3 – instant action with repercussions only if detected.
        // Represents stealing cigarettes from other characters or picking them up from the ground.
        [Header("PickPockets")]
        bool m_pickPocketsIsActive = true;
        [SerializeField] RectTransform m_recTransformPickPockets;
        [SerializeField] Animator m_recAnimatorPickPockets;
        [SerializeField] Toggle m_pickPocketsToggle;
        [SerializeField] CanvasGroup m_pickPocketsCanvasGroup;
        [SerializeField] float m_pickPocketsWorkDistance = 3f;
        [SerializeField] RedHandedActionSO m_pickPocketsActionSO;
        [SerializeField] RedHandedActionSO m_stealToolActionSO;


        // === THROW STONE (NoiseTrap) ===
        // Demonstrates Action Type #1 – instant action with no repercussions.
        // Represents throwing away rock. Guards do not react to this directly. Although Rock here is NoiseTrap, so if Guard Will Hear it will react to it.
        [Header("ThrowStone")]
        bool m_throwStoneIsActive = true;
        [SerializeField] RectTransform m_recTransformThrowStone;
        [SerializeField] Toggle m_throwStoneToggle;
        [SerializeField] CanvasGroup m_throwStoneCanvasGroup;
        [SerializeField] float m_throwStoneWorkDistance = 3f;
        [SerializeField] RedHandedActionSO m_throwStoneActionSO;
        [SerializeField] Transform m_throwStonePrefab;
        [SerializeField] int m_amountOfThrowStones = 2;


        void Awake()
        {
            m_redHanded = m_player.GetComponent<RedHanded>();
        }

        void Update()
        {
            // === WIRE CUTTERS ===
            if (!m_wireCutterToggle.isOn)
            {
                m_wireCutterIsActive = false;
                m_recTransformWireCutters.gameObject.SetActive(false);
            }
            else
            {
                m_wireCutterIsActive = true;
                m_recTransformWireCutters.gameObject.SetActive(true);
            }

            if (m_wireCutterIsActive)
            {
                if (SensesDemoInput.GetKeyDown(KeyCode.Escape))
                {
                    m_wireCutterToggle.isOn = false;
                }

                moveUIToMouse(m_recTransformWireCutters);

                bool _animatorShouldBeOff = true;
                float distance = 0f;

                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
                WireCutterInteractor _wireCutterInteractor = null;

                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    distance = Vector3.Distance(m_player.position, hit.point);

                    Transform current = hit.collider.transform;
                    while (current != null)
                    {
                        if (current.CompareTag("ActionInteractor") &&
                            current.TryGetComponent(out _wireCutterInteractor))
                        {
                            _animatorShouldBeOff = false;
                            break;
                        }
                        current = current.parent;
                    }
                }

                m_recAnimatorWireCutters.speed = _animatorShouldBeOff ? 0 : 1;
                updateUITransparency(distance);

                // Conducting wire-cutting action; can be aborted if released early.
                if (_wireCutterInteractor != null && distance <= m_wireCutterWorkDistance)
                {
                    if (SensesDemoInput.GetMouseButton(0))
                    {
                        // Called once when the player starts the action
                        if (m_currentTimeOfActionWireCutter == 0f)
                            m_redHanded.StartLastingAction();

                        m_currentTimeOfActionWireCutter += Time.deltaTime;
                        m_progresIndication.fillAmount = Mathf.Clamp01(
                            m_currentTimeOfActionWireCutter / m_timeOfActionRequiredWireCutter);

                        if (m_currentTimeOfActionWireCutter >= m_timeOfActionRequiredWireCutter)
                        {
                            m_redHanded.FinishLastingAction(m_wireCutActionSO);
                            _wireCutterInteractor.ActionWasFinished();
                            m_currentTimeOfActionWireCutter = 0f;
                            m_progresIndication.fillAmount = 0f;
                        }
                    }
                    else
                    {
                        // Action aborted – reset progress, no repercussions.
                        if (m_currentTimeOfActionWireCutter > 0)
                        {
                            m_currentTimeOfActionWireCutter = 0f;
                            m_progresIndication.fillAmount = 0f;
                        }
                    }
                }
                else
                {
                    // Out of range or no valid target – reset state.
                    if (m_progresIndication.fillAmount > 0f)
                        m_progresIndication.fillAmount = 0f;
                    m_currentTimeOfActionWireCutter = 0f;
                }
            }

            // === SCREWDRIVER (GENERATOR SABOTAGE) ===
            if (!m_screwDriverToggle.isOn)
            {
                m_screwDriverIsActive = false;
                m_recTransformScrewDriver.gameObject.SetActive(false);
            }
            else
            {
                m_screwDriverIsActive = true;
                m_recTransformScrewDriver.gameObject.SetActive(true);
            }

            if (m_screwDriverIsActive)
            {
                if (SensesDemoInput.GetKeyDown(KeyCode.Escape))
                {
                    m_screwDriverToggle.isOn = false;
                }

                moveUIToMouse(m_recTransformScrewDriver);

                bool _animatorShouldBeOff = true;
                float distance = 0f;

                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
                ScrewDriverInterator _screwDriverInteractor = null;

                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    distance = Vector3.Distance(m_player.position, hit.point);

                    Transform current = hit.collider.transform;
                    while (current != null)
                    {
                        if (current.CompareTag("ActionInteractor") &&
                            current.TryGetComponent(out _screwDriverInteractor))
                        {
                            _animatorShouldBeOff = false;
                            break;
                        }
                        current = current.parent;
                    }
                }

                m_recAnimatorScrewDriver.speed = _animatorShouldBeOff ? 0 : 1;
                updateUITransparency(distance);

                // Conducting generator sabotage; can be aborted like wire cutting.
                if (_screwDriverInteractor != null && distance <= m_screwDriverWorkDistance)
                {
                    if (SensesDemoInput.GetMouseButton(0))
                    {
                        if (m_currentTimeOfActionScrewDriver == 0f)
                            m_redHanded.StartLastingAction();

                        m_currentTimeOfActionScrewDriver += Time.deltaTime;
                        m_progresIndicationScrewDriver.fillAmount = Mathf.Clamp01(
                            m_currentTimeOfActionScrewDriver / m_timeOfActionRequiredScrewDriver);

                        if (m_currentTimeOfActionScrewDriver >= m_timeOfActionRequiredScrewDriver)
                        {
                            m_redHanded.FinishLastingAction(m_screwDriverActionSO);
                            _screwDriverInteractor.ActionWasFinished();
                            m_currentTimeOfActionScrewDriver = 0f;
                            m_progresIndicationScrewDriver.fillAmount = 0f;
                        }
                    }
                    else
                    {
                        // Action aborted – reset progress, no repercussions.
                        if (m_currentTimeOfActionScrewDriver > 0)
                        {
                            m_currentTimeOfActionScrewDriver = 0f;
                            m_progresIndicationScrewDriver.fillAmount = 0f;
                        }
                    }
                }
                else
                {
                    // Out of range or no valid target – reset state.
                    if (m_progresIndicationScrewDriver.fillAmount > 0f)
                        m_progresIndicationScrewDriver.fillAmount = 0f;
                    m_currentTimeOfActionScrewDriver = 0f;
                }
            }

            // === CIGARETTES ===
            if (!m_cigaretsToggle.isOn)
            {
                m_cigaertsIsActive = false;
                m_recTransformCiggaerts.gameObject.SetActive(false);
            }
            else
            {
                m_cigaertsIsActive = true;
                m_recTransformCiggaerts.gameObject.SetActive(true);
            }

            if (m_cigaertsIsActive)
            {
                if (SensesDemoInput.GetKeyDown(KeyCode.Escape))
                {
                    m_cigaretsToggle.isOn = false;
                }

                moveUIToMouse(m_recTransformCiggaerts);

                float distance = 0f;
                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    distance = Vector3.Distance(m_player.position, hit.point);
                }

                updateUITransparency(distance);

                if (distance > m_cigaretsWorkDistance) return;

                if (SensesDemoInput.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out RaycastHit hitClick, 100))
                    {
                        if (hitClick.collider.gameObject.CompareTag("Ground"))
                        {
                            Transform _cigarets = Instantiate(m_cigaretsPrefab);
                            _cigarets.transform.position = new Vector3(hitClick.point.x, hitClick.point.y + .1f, hitClick.point.z);
                            Quaternion _rotation = Quaternion.identity;
                            _rotation.eulerAngles = new Vector3(0, Random.Range(-180, 180), 0);
                            _cigarets.rotation = _rotation;

                            m_amountOfCigarets--;
                            if (m_amountOfCigarets <= 0)
                            {
                                m_cigaretsToggle.interactable = false;
                                m_cigaretsToggle.isOn = false;
                                return;
                            }
                        }
                    }
                }
            }

            // === PICKPOCKETS ===
            if (!m_pickPocketsToggle.isOn)
            {
                m_pickPocketsIsActive = false;
                m_recTransformPickPockets.gameObject.SetActive(false);
            }
            else
            {
                m_pickPocketsIsActive = true;
                m_recTransformPickPockets.gameObject.SetActive(true);
            }

            if (m_pickPocketsIsActive)
            {
                if (SensesDemoInput.GetKeyDown(KeyCode.Escape))
                {
                    m_pickPocketsToggle.isOn = false;
                }

                moveUIToMouse(m_recTransformPickPockets);

                float distance = 0f;
                bool _animatorShouldBeOff = true;

                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    distance = Vector3.Distance(m_player.position, hit.point);
                    if (hit.collider.gameObject.TryGetComponent<StealItemCigarets>(out StealItemCigarets _stealItemCigarets))
                    {
                        _animatorShouldBeOff = false;
                    }
                    else if (hit.collider.gameObject.TryGetComponent<PickupItemCigarets>(out PickupItemCigarets _pickupItemCigarets))
                    {
                        _animatorShouldBeOff = false;
                    }
                    else if (hit.collider.gameObject.TryGetComponent<PickupItemScrewDriver>(out PickupItemScrewDriver _pickupItemScrewDriver))
                    {
                        _animatorShouldBeOff = false;
                    }
                    else if (hit.collider.gameObject.TryGetComponent<PickupItemWireCutter>(out PickupItemWireCutter _pickupItemWireCutter))
                    {
                        _animatorShouldBeOff = false;
                    }
                    else if (hit.collider.gameObject.TryGetComponent<PickupItemRock>(out PickupItemRock _pickupItemRock))
                    {
                        _animatorShouldBeOff = false;
                    }
                }

                m_recAnimatorPickPockets.speed = _animatorShouldBeOff ? 0 : 1;
                updateUITransparency(distance);

                if (distance > m_pickPocketsWorkDistance) return;

                if (SensesDemoInput.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out RaycastHit hitClick, 100))
                    {
                        // Stealing from a guard
                        if (hitClick.collider.gameObject.TryGetComponent<StealItemCigarets>(out StealItemCigarets _stealItemCigarets))
                        {
                            m_redHanded.FinishInstantAction(m_pickPocketsActionSO);
                            Destroy(_stealItemCigarets);
                            m_amountOfCigarets++;
                            if (m_amountOfCigarets > 0) m_cigaretsToggle.interactable = true;
                        }
                        // Picking up dropped cigarettes
                        else if (hitClick.collider.gameObject.TryGetComponent<PickupItemCigarets>(out PickupItemCigarets _pickupItemCigarets))
                        {
                            Destroy(hitClick.collider.gameObject);
                            m_amountOfCigarets++;
                            if (m_amountOfCigarets > 0) m_cigaretsToggle.interactable = true;
                        }
                        // Picking up dropped screwdriver
                        else if (hitClick.collider.gameObject.TryGetComponent<PickupItemScrewDriver>(out PickupItemScrewDriver _pickupItemScrewDriver))
                        {
                            m_redHanded.FinishInstantAction(m_stealToolActionSO);
                            Destroy(hitClick.collider.gameObject);
                            m_screwDriverToggle.interactable = true;
                        }
                        // Picking up dropped wirecutter
                        else if (hitClick.collider.gameObject.TryGetComponent<PickupItemWireCutter>(out PickupItemWireCutter _pickupItemWireCutter))
                        {
                            m_redHanded.FinishInstantAction(m_stealToolActionSO);
                            Destroy(hitClick.collider.gameObject);
                            m_wireCutterToggle.interactable = true;
                        }
                        // Picking up dropped rock
                        else if (hitClick.collider.gameObject.TryGetComponent<PickupItemRock>(out PickupItemRock _pickupItemRock))
                        {
                            Destroy(hitClick.collider.gameObject);
                            m_amountOfThrowStones++;
                            if (m_amountOfThrowStones > 0) m_throwStoneToggle.interactable = true;
                        }

                    }
                }
            }

            // === THROWSTONE ===
            if (!m_throwStoneToggle.isOn)
            {
                m_throwStoneIsActive = false;
                m_recTransformThrowStone.gameObject.SetActive(false);
            }
            else
            {
                m_throwStoneIsActive = true;
                m_recTransformThrowStone.gameObject.SetActive(true);
            }

            if (m_throwStoneIsActive)
            {
                if (SensesDemoInput.GetKeyDown(KeyCode.Escape))
                {
                    m_throwStoneToggle.isOn = false;
                }

                moveUIToMouse(m_recTransformThrowStone);

                float distance = 0f;

                Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    //we should trow on empty space
                    distance = Vector3.Distance(m_player.position, hit.point);
                }

                updateUITransparency(distance);

                if (distance > m_throwStoneWorkDistance) return;

                if (SensesDemoInput.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out RaycastHit hitClick, 100))
                    {
                        if (hitClick.collider.gameObject.CompareTag("Ground"))
                        {
                            Transform _throwStonePrefab = Instantiate(m_throwStonePrefab);
                            _throwStonePrefab.localScale = new Vector3(.7f, .7f, .7f);

                            // Direction from player → hit point
                            Vector3 direction = (hitClick.point - m_player.transform.position).normalized;

                            // Spawn position slightly in front of player
                            float spawnOffset = 0.2f;   // adjust as needed
                            Vector3 spawnPos = m_player.transform.position + direction * spawnOffset + new Vector3(0,1.5f, 0);

                            _throwStonePrefab.position = spawnPos;

                            // Apply random rotation
                            Quaternion _rotation = Quaternion.identity;
                            _rotation.eulerAngles = new Vector3(0, Random.Range(-180, 180), 0);
                            _throwStonePrefab.rotation = _rotation;

                            // Throw Rigidbody
                            Rigidbody rb = _throwStonePrefab.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                float throwForce = 10f;
                                rb.AddForce(direction * throwForce, ForceMode.VelocityChange);
                            }

                            // Update remaining stones
                            m_amountOfThrowStones--;
                            if (m_amountOfThrowStones <= 0)
                            {
                                m_throwStoneToggle.interactable = false;
                                m_throwStoneToggle.isOn = false;
                                return;
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Moves a UI RectTransform to follow the current mouse position.
        /// </summary>
        void moveUIToMouse(RectTransform _rectTransform)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                SensesDemoInput.MousePosition,
                null,
                out mousePos);

            _rectTransform.anchoredPosition = mousePos;
        }

        /// <summary>
        /// Updates the alpha transparency of UI elements based on interaction distance.
        /// </summary>
        void updateUITransparency(float distance)
        {
            if (m_wireCutterIsActive)
                m_wireCutterCanvasGroup.alpha = (distance <= m_wireCutterWorkDistance) ? 1f : m_alphaFadeValue;

            if (m_screwDriverIsActive)
                m_screwDriverCanvasGroup.alpha = (distance <= m_screwDriverWorkDistance) ? 1f : m_alphaFadeValue;

            if (m_cigaertsIsActive)
                m_cigaertsCanvasGroup.alpha = (distance <= m_cigaretsWorkDistance) ? 1f : m_alphaFadeValue;

            if (m_pickPocketsIsActive)
                m_pickPocketsCanvasGroup.alpha = (distance <= m_pickPocketsWorkDistance) ? 1f : m_alphaFadeValue;

            if (m_throwStoneIsActive)
                m_throwStoneCanvasGroup.alpha = (distance <= m_throwStoneWorkDistance) ? 1f : m_alphaFadeValue;
        }
    }
}
