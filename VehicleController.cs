using UnityEngine;

// =============================================================
// VehicleController.cs
// Responsabilidade: mover o carro em linha reta e destruí-lo
// ao sair da tela. Não sabe nada sobre jogo ou API.
// =============================================================

public class VehicleController : MonoBehaviour
{
    // Direção (+1 = direita, -1 = esquerda) e velocidade
    // definidas pelo TrafficManager no momento do spawn
    [HideInInspector] public float direcao = 1f;
    [HideInInspector] public float velocidade = 3f;

    [Header("Tamanho visual")]
    [Tooltip("Largura do carro em unidades Unity")]
    public float largura = 1.5f;
    [Tooltip("Altura do carro em unidades Unity")]
    public float altura = 0.6f;

    // Cache do GameManager para evitar FindFirstObjectByType a cada colisão
    private GameManager gameManager;

    // Limite horizontal — calculado a partir do spawn
    private float limiteX = 20f;

    void Awake()
    {
        // Aplica tamanho visual imediatamente
        transform.localScale = new Vector3(largura, altura, 1f);

        // Cache do GameManager
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        // Ajusta limite com base na posição de spawn para garantir
        // que o carro sempre percorra toda a tela antes de ser destruído
        limiteX = Mathf.Abs(transform.position.x) + 25f;
    }

    void Update()
    {
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);

        // Auto-destruição ao sair da tela
        if (Mathf.Abs(transform.position.x) > limiteX)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            if (gameManager != null)
                gameManager.OnPlayerAtingido();
            else
                FindFirstObjectByType<GameManager>()?.OnPlayerAtingido();
        }
    }
}