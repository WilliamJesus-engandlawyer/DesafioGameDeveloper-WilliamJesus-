// TrafficManager.cs


using System.Collections;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [Header("Prefab do Carro - Arraste o PREFAB REAL do Project (não Clone!)")]
    public GameObject carroPrefab;

    [Header("Pontos de Spawn")]
    public Transform spawnF1Esquerda;
    public Transform spawnF1Direita;
    public Transform spawnF2Esquerda;
    public Transform spawnF2Direita;

    [Header("Velocidade de Referência Visual")]
    public float velocidadeReferencia = 8f;

    private static GameObject prefabCache;

    private float vehicleDensity = 0.3f;
    private float averageSpeed = 40f;

    private Coroutine coroutineF1;
    private Coroutine coroutineF2;
    private bool spawnAtivo = false;

    void Awake()
    {
        if (prefabCache == null && carroPrefab != null)
        {
            prefabCache = carroPrefab;
            Debug.Log("[TrafficManager] ✅ Prefab cacheado com sucesso (Awake).");
        }
    }

    // Roda toda vez que a cena recarrega (mesmo com DontDestroyOnLoad)
    void OnEnable()
    {
        if (carroPrefab == null && prefabCache != null)
        {
            carroPrefab = prefabCache;
            Debug.Log("[TrafficManager] 🔄 Prefab restaurado via OnEnable.");
        }
    }

    public void AtualizarDados(Status status)
    {
        if (status == null) return;

        // Restauração FORÇADA em TODAS as chamadas
        if (carroPrefab == null)
            carroPrefab = prefabCache;

        if (carroPrefab == null)
        {
            Debug.LogError("[TrafficManager] ❌ Prefab do carro ainda é null! Verifique o Inspector.");
            return;
        }

        vehicleDensity = Mathf.Clamp(status.vehicleDensity, 0.1f, 1f);
        averageSpeed = Mathf.Clamp(status.averageSpeed, 1f, 100f);

        Debug.Log($"[TrafficManager] Dados atualizados — Densidade: {vehicleDensity} | Vel: {averageSpeed}");

        ReiniciarSpawn();
    }

    private void ReiniciarSpawn()
    {
        if (carroPrefab == null) carroPrefab = prefabCache;

        if (carroPrefab == null)
        {
            Debug.LogError("[TrafficManager] ❌ Nenhum prefab disponível para spawn!");
            return;
        }

        PararSpawn();

        spawnAtivo = true;
        coroutineF1 = StartCoroutine(SpawnFaixa(spawnF1Esquerda, 1f));
        coroutineF2 = StartCoroutine(SpawnFaixa(spawnF2Direita, -1f));

        Debug.Log("[TrafficManager] ✅ Spawn reiniciado com sucesso.");
    }

    private IEnumerator SpawnFaixa(Transform origem, float direcao)
    {
        yield return new WaitForSeconds(0.5f);
        while (spawnAtivo)
        {
            float intervalo = 1f / vehicleDensity;
            float velocidadeUnity = (averageSpeed / 100f) * velocidadeReferencia;

            SpawnarCarro(origem.position, direcao, velocidadeUnity);
            yield return new WaitForSeconds(intervalo);
        }
    }

    private void SpawnarCarro(Vector3 posicao, float direcao, float velocidade)
    {
        if (carroPrefab == null) carroPrefab = prefabCache;
        if (carroPrefab == null) return;

        GameObject carro = Instantiate(carroPrefab, posicao, Quaternion.identity);
        VehicleController vc = carro.GetComponent<VehicleController>();

        if (vc != null)
        {
            vc.direcao = direcao;
            vc.velocidade = velocidade;
        }
        else
        {
            Destroy(carro);
        }
    }

    public void PararSpawn()
    {
        spawnAtivo = false;
        if (coroutineF1 != null) { StopCoroutine(coroutineF1); coroutineF1 = null; }
        if (coroutineF2 != null) { StopCoroutine(coroutineF2); coroutineF2 = null; }
    }
}