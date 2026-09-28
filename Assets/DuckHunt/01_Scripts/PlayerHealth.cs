using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuracion de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Escenas")]
    public string menuSceneName = "MainMenu";

    [Header("Feedback Visual")]
    public float invulnerabilityDuration = 1.0f;

    private bool isDead = false;
    private bool isInvulnerable = false;
    private Canvas damageCanvas;
    private Image damageOverlay;

    void Awake()
    {
        currentHealth = maxHealth;
        CrearDamageOverlay();
    }

    void CrearDamageOverlay()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            GameObject canvasObj = new GameObject("DamageEffect_Canvas");
            canvasObj.transform.SetParent(cam.transform, false);
            canvasObj.transform.localPosition = new Vector3(0, 0, 0.15f);
            canvasObj.transform.localRotation = Quaternion.identity;

            damageCanvas = canvasObj.AddComponent<Canvas>();
            damageCanvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(2f, 2f);

            GameObject imgObj = new GameObject("DamageFlash");
            imgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.sizeDelta = Vector2.zero;

            damageOverlay = imgObj.AddComponent<Image>();
            damageOverlay.color = new Color(1f, 0f, 0f, 0f);
            damageOverlay.raycastTarget = false;
        }
    }

    void Start()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead || isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        StartCoroutine(DamageFeedbackRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        isInvulnerable = true;

        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0.45f);
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.45f, 0f, elapsed / duration);
                damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, alpha);
                yield return null;
            }

            damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0f);
        }

        yield return new WaitForSeconds(invulnerabilityDuration - 0.4f);
        isInvulnerable = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        var mover = GetComponent<VRAutoForwardMover>();
        if (mover != null) mover.avanzar = false;

        StartCoroutine(VolverAlMenuRutina());
    }

    private IEnumerator VolverAlMenuRutina()
    {
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.8f, 0.0f, 0.0f, 0.85f);
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 centerPos = cam.transform.position + cam.transform.forward * 3f;
            Quaternion rotation = Quaternion.LookRotation(cam.transform.forward);

            // Titulo Principal
            GameObject titleObj = new GameObject("GameOverTitle");
            titleObj.transform.position = centerPos + Vector3.up * 0.8f;
            titleObj.transform.rotation = rotation;
            var tmpTitle = titleObj.AddComponent<TMPro.TextMeshPro>();
            tmpTitle.text = "HAS PERDIDO\n<size=35%><color=white>Apunta y haz clic (o dispara) para elegir</color></size>";
            tmpTitle.fontSize = 7f;
            tmpTitle.alignment = TMPro.TextAlignmentOptions.Center;
            tmpTitle.color = Color.red;

            // Boton Volver a Jugar
            CrearBotonVR("VOLVER A JUGAR", centerPos + Vector3.up * 0.1f, rotation, true);

            // Boton Menu
            CrearBotonVR("IR AL MENU", centerPos - Vector3.up * 0.5f, rotation, false);
        }

        yield return null;
    }

    private void CrearBotonVR(string texto, Vector3 pos, Quaternion rot, bool esReinicio)
    {
        GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        btnObj.name = texto;
        btnObj.transform.position = pos;
        btnObj.transform.rotation = rot;
        btnObj.transform.localScale = new Vector3(2.5f, 0.45f, 0.1f);
        
        btnObj.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        var collider = btnObj.GetComponent<BoxCollider>();
        collider.isTrigger = false; // Interactable needs a solid collider usually, or trigger works for Ray
        
        // Hacerlo clickeable con el rayo VR
        var interactable = btnObj.AddComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener((args) => 
        {
            if (esReinicio) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else SceneManager.LoadScene(!string.IsNullOrEmpty(menuSceneName) ? menuSceneName : "MainMenu");
        });

        // Hacerlo disparable
        var action = btnObj.AddComponent<GameOverAction>();
        action.isRestart = esReinicio;
        action.menuSceneName = menuSceneName;

        GameObject textObj = new GameObject("Texto");
        textObj.transform.SetParent(btnObj.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.6f);
        textObj.transform.localRotation = Quaternion.identity;
        
        var tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = 2f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        // Ajustar escala visual
        textObj.transform.localScale = new Vector3(1f/2.5f, 1f/0.45f, 1f/0.1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.GetComponent<EnemyTarget>() != null)
        {
            TakeDamage(1);
            var enemy = other.GetComponent<EnemyTarget>();
            if (enemy != null)
            {
                enemy.OnHit(false);
            }
        }
    }
}
 
