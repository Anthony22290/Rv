using UnityEngine;

public class Bala : MonoBehaviour
{
    public float velocidad = 20f;
    public float tiempoVida = 3f;

    void Start()
    {
        // Destruye la bala después de 3 segundos para que no consuma memoria infinita
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        // Mueve la bala constantemente hacia adelante (Eje Z)
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    // Detecta cuando la bala choca con algo
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject); // Destruye el cubo/enemigo
            Destroy(gameObject);       // Destruye la bala
        }
    }
}