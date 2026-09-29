using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Objeto recolectable de Power-Up en el espacio 3D/VR.
/// El jugador puede dispararle con cualquier arma o recogerlo al pasar cerca.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PowerUpItem : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    public PowerUpType powerUpType = PowerUpType.DoublePoints;
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 2.5f;
    public float rotationSpeed = 65f;
    public float lifeTime = 25f;

    [Header("Duraciones")]
    public float shieldDuration = 15f;
    public float doublePointsDuration = 15f;
    public int healthAmount = 1;

    private Vector3 initialPosition;
    private bool collected = false;
    private float spawnTime;
    private Light pointLight;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (!gameObject.CompareTag("PowerUp"))
        {
            // Opcional tag si existe
        }
    }

    void Start()
    {
        initialPosition = transform.position;
        spawnTime = Time.time;
        ConfigurarVisuales();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (collected) return;

        // Animación de flotación suave y rotación
        float elapsed = Time.time - spawnTime;
        float yOffset = Mathf.Sin(elapsed * floatFrequency) * floatAmplitude;
        transform.position = initialPosition + new Vector3(0, yOffset, 0);

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;

        PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        AudioClip clipToPlay = null;
        string popupMessage = "";
        Color popupColor = Color.white;

        switch (powerUpType)
        {
            case PowerUpType.ExtraLife:
                if (playerHealth != null) playerHealth.Heal(healthAmount);
                clipToPlay = SonidosArmas.SonidoPowerUpExtraLife;
                popupMessage = "¡+1 VIDA!";
                popupColor = new Color(1f, 0.2f, 0.3f);
                break;

            case PowerUpType.Shield:
                if (playerHealth != null) playerHealth.ActivateShield(shieldDuration);
                clipToPlay = SonidosArmas.SonidoPowerUpShield;
                popupMessage = "¡ESCUDO DE ENERGÍA!";
                popupColor = new Color(0f, 0.9f, 1f);
                break;

            case PowerUpType.DoublePoints:
                PlayerHUD.ActivateDoublePoints(doublePointsDuration);
                clipToPlay = SonidosArmas.SonidoPowerUpDoublePoints;
                popupMessage = "¡DOBLE PUNTOS 2X!";
                popupColor = new Color(1f, 0.85f, 0.1f);
                break;
        }

        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position, 1.0f);
        }

        MostrarPopupTexto(popupMessage, popupColor);

        // Ocultar renderer y destruir
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
        if (pointLight != null) pointLight.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    private void MostrarPopupTexto(string message, Color color)
    {
        GameObject textObj = new GameObject("PowerUp_PopupText");
        textObj.transform.position = transform.position + Vector3.up * 0.5f;
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - cam.transform.position);
        }

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;

        // Pequeño script de flotación y desvanecimiento hacia arriba
        StartCoroutine(AnimarPopupRutina(textObj, tmp));
    }

    private IEnumerator AnimarPopupRutina(GameObject obj, TextMeshPro tmp)
    {
        float elapsed = 0f;
        float duration = 1.2f;
        Vector3 startPos = obj.transform.position;
        Color startCol = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.position = startPos + Vector3.up * (t * 1.5f);
            tmp.color = new Color(startCol.r, startCol.g, startCol.b, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        Destroy(obj);
    }

    private void ConfigurarVisuales()
    {
        Color baseColor = Color.white;
        string label = "";

        switch (powerUpType)
        {
            case PowerUpType.ExtraLife:
                baseColor = new Color(1f, 0.15f, 0.25f);
                label = "❤️ +1";
                break;
            case PowerUpType.Shield:
                baseColor = new Color(0f, 0.8f, 1f);
                label = "🛡️ ESCUDO";
                break;
            case PowerUpType.DoublePoints:
                baseColor = new Color(1f, 0.85f, 0f);
                label = "⭐ 2X";
                break;
        }

        // Crear contenedor visual si no existe
        if (transform.childCount == 0)
        {
            GameObject coreMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreMesh.name = "PowerUp_VisualCore";
            coreMesh.transform.SetParent(transform, false);
            coreMesh.transform.localScale = Vector3.one * 0.75f;
            
            Collider col = coreMesh.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer ren = coreMesh.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            ren.material = mat;

            // Anillo exterior giratorio
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "PowerUp_Ring";
            ring.transform.SetParent(transform, false);
            ring.transform.localScale = new Vector3(1.1f, 0.05f, 1.1f);
            ring.transform.localRotation = Quaternion.Euler(45f, 0, 45f);

            Collider ringCol = ring.GetComponent<Collider>();
            if (ringCol != null) Destroy(ringCol);

            Renderer ringRen = ring.GetComponent<Renderer>();
            Material ringMat = new Material(shader);
            ringMat.SetColor("_BaseColor", Color.white);
            ringRen.material = ringMat;

            // Luz brillante
            GameObject lightObj = new GameObject("Point_Light");
            lightObj.transform.SetParent(transform, false);
            pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = baseColor;
            pointLight.range = 5f;
            pointLight.intensity = 3.5f;

            // Texto flotante 3D
            GameObject textObj = new GameObject("Label_Text");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0, 0.7f, 0);
            
            TextMeshPro labelTMP = textObj.AddComponent<TextMeshPro>();
            labelTMP.text = label;
            labelTMP.fontSize = 3.5f;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.color = baseColor;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Si es una bala o proyectil
        if (other.CompareTag("Bala") || 
            other.GetComponent<Bala>() != null || 
            other.name.ToLower().Contains("bala") || 
            other.name.ToLower().Contains("bullet") ||
            other.name.ToLower().Contains("pellet") ||
            other.name.ToLower().Contains("misil"))
        {
            Collect();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player") || other.GetComponentInParent<PlayerHealth>() != null)
        {
            Collect();
        }
    }
}
