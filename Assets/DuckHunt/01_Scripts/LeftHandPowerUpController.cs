using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gestiona la visualización 3D y activación del Power-Up alojado en la mano/controlador izquierdo.
/// Permite usar el Power-Up presionando el gatillo/botón del mando izquierdo o tocando físicamente el orbe con la mano derecha / arma.
/// </summary>
public class LeftHandPowerUpController : MonoBehaviour
{
    public static LeftHandPowerUpController Instance { get; private set; }

    [Header("Power-Up Almacenado")]
    public PowerUpDataSO currentPowerUp;

    [Header("Visuales en Mano Izquierda")]
    [Tooltip("Desplazamiento relativo al mando izquierdo")]
    public Vector3 localOffset = new Vector3(0f, 0.08f, 0.05f);

    private GameObject visualOrbContainer;
    private MeshRenderer orbRenderer;
    private MeshRenderer innerCoreRenderer;
    private TextMeshPro labelText;
    private AudioSource audioSource;
    private Transform leftControllerTransform;

    // Haptics & Input
    private UnityEngine.XR.InputDevice leftDevice;
    private bool wasButtonPressedLastFrame = false;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        CrearVisualesOrbe();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        BuscarControladorIzquierdo();
        ActualizarVisuales();
    }

    void BuscarControladorIzquierdo()
    {
        // Buscar el objeto "Left Controller" en la jerarquía del XR Origin
        var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            if (t.name.Equals("Left Controller", System.StringComparison.OrdinalIgnoreCase) ||
                t.name.Equals("LeftHand Controller", System.StringComparison.OrdinalIgnoreCase) ||
                t.name.Contains("Left Controller") ||
                t.name.Contains("LeftHand"))
            {
                leftControllerTransform = t;
                transform.SetParent(leftControllerTransform, false);
                transform.localPosition = localOffset;
                transform.localRotation = Quaternion.identity;
                Debug.Log($"[LeftHandPowerUpController] ✅ Vinculado exitosamente al controlador izquierdo: {t.name}");
                break;
            }
        }
    }

    private void CrearVisualesOrbe()
    {
        if (visualOrbContainer != null) return;

        visualOrbContainer = new GameObject("PowerUp_VisualOrb");
        visualOrbContainer.transform.SetParent(transform, false);
        visualOrbContainer.transform.localPosition = Vector3.zero;

        // Orbe exterior brillante
        GameObject orbObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orbObj.name = "OuterOrb";
        orbObj.transform.SetParent(visualOrbContainer.transform, false);
        orbObj.transform.localScale = Vector3.one * 0.09f;

        // Configurar trigger para poder tocarlo con la otra mano o el arma
        SphereCollider col = orbObj.GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 0.65f; // Un poco más grande para facilitar el toque físico
        }

        orbRenderer = orbObj.GetComponent<MeshRenderer>();
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material orbMat = new Material(unlitShader);
        orbRenderer.material = orbMat;

        // Núcleo interno pulsante
        GameObject coreObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coreObj.name = "InnerCore";
        coreObj.transform.SetParent(orbObj.transform, false);
        coreObj.transform.localScale = Vector3.one * 0.55f;
        Collider coreCol = coreObj.GetComponent<Collider>();
        if (coreCol != null) Destroy(coreCol);

        innerCoreRenderer = coreObj.GetComponent<MeshRenderer>();
        innerCoreRenderer.material = new Material(orbMat);

        // Anillo orbital decorativo
        GameObject ringObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringObj.name = "OrbitalRing";
        ringObj.transform.SetParent(orbObj.transform, false);
        ringObj.transform.localScale = new Vector3(1.3f, 0.02f, 1.3f);
        Collider ringCol = ringObj.GetComponent<Collider>();
        if (ringCol != null) Destroy(ringCol);
        var ringRend = ringObj.GetComponent<MeshRenderer>();
        if (ringRend != null) ringRend.material = orbMat;

        // Texto 3D flotante sobre el orbe
        GameObject textObj = new GameObject("PowerUp_Label");
        textObj.transform.SetParent(visualOrbContainer.transform, false);
        textObj.transform.localPosition = new Vector3(0, 0.08f, 0);
        textObj.transform.localScale = Vector3.one * 0.007f;

        labelText = textObj.AddComponent<TextMeshPro>();
        labelText.fontSize = 24;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;

        visualOrbContainer.SetActive(false);
    }

    public void EquiparPowerUp(PowerUpDataSO data)
    {
        currentPowerUp = data;
        ActualizarVisuales();

        if (data != null)
        {
            Debug.Log($"[LeftHandPowerUpController] 🎁 Power-Up '{data.powerUpName}' colocado en la mano izquierda.");

            // Sonido de obtención
            if (audioSource != null)
            {
                audioSource.PlayOneShot(SonidosArmas.SonidoPowerUpSpawn, 0.85f);
            }

            // Vibración háptica en el mando izquierdo
            EnviarVibracionHaptica(0.5f, 0.15f);

            // Animación de aparición
            StopAllCoroutines();
            StartCoroutine(AnimacionSpawnRutina());
        }
    }

    private void ActualizarVisuales()
    {
        if (visualOrbContainer == null) return;

        if (currentPowerUp == null)
        {
            visualOrbContainer.SetActive(false);
        }
        else
        {
            visualOrbContainer.SetActive(true);

            Color col = currentPowerUp.themeColor;
            if (orbRenderer != null && orbRenderer.material != null)
            {
                Color alphaCol = new Color(col.r, col.g, col.b, 0.65f);
                if (orbRenderer.material.HasProperty("_BaseColor")) orbRenderer.material.SetColor("_BaseColor", alphaCol);
                else orbRenderer.material.color = alphaCol;
            }

            if (innerCoreRenderer != null && innerCoreRenderer.material != null)
            {
                Color emissiveCol = currentPowerUp.emissionColor;
                if (innerCoreRenderer.material.HasProperty("_BaseColor")) innerCoreRenderer.material.SetColor("_BaseColor", emissiveCol);
                else innerCoreRenderer.material.color = emissiveCol;
            }

            if (labelText != null)
            {
                labelText.text = $"{currentPowerUp.iconEmoji} {currentPowerUp.powerUpName.ToUpper()}\n<size=65%><color=#FFD700>[GATILLO IZQ / TOCAR]</color></size>";
            }
        }
    }

    private IEnumerator AnimacionSpawnRutina()
    {
        if (visualOrbContainer == null) yield break;

        float elapsed = 0f;
        float duration = 0.25f;
        Vector3 targetScale = Vector3.one;

        visualOrbContainer.transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            visualOrbContainer.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.3f, t);
            yield return null;
        }

        visualOrbContainer.transform.localScale = targetScale;
    }

    void Update()
    {
        if (leftControllerTransform == null)
        {
            BuscarControladorIzquierdo();
        }

        if (currentPowerUp == null) return;

        // Efecto de rotación y flotación suave del orbe
        if (visualOrbContainer != null && visualOrbContainer.activeSelf)
        {
            visualOrbContainer.transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.Self);
            float pulse = 1f + (Mathf.Sin(Time.time * 6f) * 0.08f);
            visualOrbContainer.transform.localScale = Vector3.one * pulse;

            // Orientar el texto hacia la cámara del jugador
            if (labelText != null && Camera.main != null)
            {
                labelText.transform.rotation = Quaternion.LookRotation(labelText.transform.position - Camera.main.transform.position);
            }
        }

        // Comprobar si el jugador presiona para usarlo
        VerificarEntradaParaUsar();
    }

    private void VerificarEntradaParaUsar()
    {
        bool presionado = false;

        // 1. Detectar botones / gatillo del controlador izquierdo en VR
        if (!leftDevice.isValid)
        {
            leftDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        if (leftDevice.isValid)
        {
            leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed);
            leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed);
            leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed);
            leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed);

            if (triggerPressed || primaryPressed || secondaryPressed || gripPressed)
            {
                presionado = true;
            }
        }

        // 2. Soporte para teclado (Pruebas en Editor / Simulator)
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            presionado = true;
        }

        // Ejecutar al pulsar
        if (presionado && !wasButtonPressedLastFrame)
        {
            UsarPowerUp();
        }

        wasButtonPressedLastFrame = presionado;
    }

    /// <summary>
    /// Activa el Power-Up equipado y lo consume.
    /// </summary>
    public void UsarPowerUp()
    {
        if (currentPowerUp == null) return;

        PowerUpDataSO powerUpToUse = currentPowerUp;
        currentPowerUp = null;
        ActualizarVisuales();

        EnviarVibracionHaptica(0.8f, 0.25f);

        // Activar a través del PowerUpManager
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ActivarPowerUp(powerUpToUse);
        }
        else
        {
            Debug.LogWarning("[LeftHandPowerUpController] PowerUpManager no encontrado. Creando instancia...");
            GameObject mgrObj = new GameObject("PowerUpManager");
            var mgr = mgrObj.AddComponent<PowerUpManager>();
            mgr.ActivarPowerUp(powerUpToUse);
        }
    }

    private void EnviarVibracionHaptica(float amplitud, float duracion)
    {
        if (!leftDevice.isValid)
        {
            leftDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        if (leftDevice.isValid)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (leftDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                leftDevice.SendHapticImpulse(0, amplitud, duracion);
            }
        }
    }

    // Detección de toque físico (Tocar el orbe con la mano derecha, el arma o el puntero)
    void OnTriggerEnter(Collider other)
    {
        if (currentPowerUp == null) return;

        // Si colisiona con el arma, la otra mano o una bala
        if (other.CompareTag("Player") || 
            other.name.Contains("Right") || 
            other.name.Contains("Hand") || 
            other.name.Contains("Pistola") || 
            other.name.Contains("Gun") || 
            other.GetComponent<PistolaVR>() != null ||
            other.GetComponent<Bala>() != null)
        {
            UsarPowerUp();
        }
    }
}
