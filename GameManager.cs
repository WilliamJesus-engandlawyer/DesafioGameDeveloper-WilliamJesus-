using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================
// GameManager.cs
// Responsabilidade: orquestrar todo o fluxo do jogo.
//   - Chama a API no início de cada nível
//   - Aplica current_status imediatamente
//   - Agenda predicted_status com Coroutines
//   - Gerencia timer, vitória e derrota
// =============================================================

public class GameManager : MonoBehaviour
{
    [Header("Referências")]
    public ApiService apiService;
    public TrafficManager trafficManager;
    public PlayerController2D player;
    public HUDController hud;

    [Header("Posição inicial do Player")]
    public Transform posicaoInicial;

    [Header("Configuração")]
    [Tooltip("Segundos extras adicionados ao timer além do estimated_time da última predição")]
    public float tempoExtra = 30f;

    // Estado interno
    private int nivelAtual = 1;
    private float timerRestante = 0f;
    private bool jogoAtivo = false;
    private bool aguardandoReinicio = false;
    private List<Coroutine> coroutinesPrediction = new List<Coroutine>();

    // Multiplicadores de clima conforme especificação do desafio
    private static readonly Dictionary<string, float> MultiplicadoresClima
        = new Dictionary<string, float>
    {
        { "sunny",      1.0f },
        { "clouded",    0.8f },
        { "foggy",      0.8f },
        { "light rain", 0.6f },
        { "heavy rain", 0.4f }
    };

    void Start()
    {
        IniciarNivel();
    }

    void Update()
    {
        if (aguardandoReinicio && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            aguardandoReinicio = false;
            nivelAtual = 1;
            IniciarNivel();
        }
    }

    // ── Fluxo principal ──────────────────────────────────────

    public void IniciarNivel()
    {
        jogoAtivo = false;
        aguardandoReinicio = false;
        LimparPredictions();
        trafficManager.PararSpawn();

        // Destrói apenas clones de carros gerados em runtime
        // O (Clone) é adicionado automaticamente pelo Unity ao usar Instantiate
        foreach (var carro in FindObjectsByType<VehicleController>(FindObjectsSortMode.None))
        {
            if (carro.gameObject.name.Contains("(Clone)"))
                Destroy(carro.gameObject);
        }

        hud.MostrarStatus($"Nível {nivelAtual} — Carregando...");
        player.Resetar(posicaoInicial.position);
        apiService.BuscarDados(OnDadosRecebidos, OnErroDaApi);
    }

    private void OnDadosRecebidos(TrafficResponse resposta)
    {
        if (resposta == null || resposta.current_status == null)
        {
            OnErroDaApi("Resposta inválida da API.");
            return;
        }

        AplicarStatus(resposta.current_status);

        if (resposta.predicted_status != null && resposta.predicted_status.Count > 0)
        {
            int ultimoMs = resposta.predicted_status[resposta.predicted_status.Count - 1].estimated_time;
            timerRestante = (ultimoMs / 1000f) + tempoExtra;

            foreach (var entrada in resposta.predicted_status)
            {
                if (entrada != null && entrada.predictions != null)
                {
                    Coroutine c = StartCoroutine(AgendarPrediction(entrada));
                    coroutinesPrediction.Add(c);
                }
            }
        }
        else
        {
            timerRestante = tempoExtra;
        }

        jogoAtivo = true;
        hud.MostrarStatus("Atravesse a via!");
        StartCoroutine(LoopTimer());
    }

    private void OnErroDaApi(string erro)
    {
        Debug.LogWarning($"[GameManager] Erro da API: {erro}");
        hud.MostrarStatus("Erro na API. Tentando novamente em 3s...");
        StartCoroutine(TentarNovamente());
    }

    private IEnumerator TentarNovamente()
    {
        yield return new WaitForSeconds(3f);
        IniciarNivel();
    }

    // ── Predições agendadas ──────────────────────────────────

    private IEnumerator AgendarPrediction(PredictedEntry entrada)
    {
        yield return new WaitForSeconds(entrada.estimated_time / 1000f);
        if (!jogoAtivo) yield break;
        Debug.Log($"[GameManager] Predição aplicada em {entrada.estimated_time}ms — Clima: {entrada.predictions.weather}");
        AplicarStatus(entrada.predictions);
    }

    // ── Aplicar status ───────────────────────────────────────

    private void AplicarStatus(Status status)
    {
        trafficManager.AtualizarDados(status);

        float mult = ObterMultiplicadorClima(status.weather);
        player.multiplicadorClima = mult;

        hud.AtualizarDadosAPI(nivelAtual, status, mult);
        Debug.Log($"[GameManager] Status aplicado — Clima: {status.weather} (x{mult})");
    }

    private float ObterMultiplicadorClima(string clima)
    {
        if (!string.IsNullOrEmpty(clima) && MultiplicadoresClima.TryGetValue(clima, out float mult))
            return mult;
        return 1f;
    }

    // ── Timer ────────────────────────────────────────────────

    private IEnumerator LoopTimer()
    {
        while (jogoAtivo && timerRestante > 0f)
        {
            timerRestante -= Time.deltaTime;
            hud.AtualizarTimer(timerRestante);
            yield return null;
        }

        if (jogoAtivo && timerRestante <= 0f)
            GameOver();
    }

    // ── Vitória e Derrota ────────────────────────────────────

    public void OnPlayerChegou()
    {
        if (!jogoAtivo) return;

        jogoAtivo = false;
        LimparPredictions();
        trafficManager.PararSpawn();

        nivelAtual++;
        hud.MostrarStatus($"✓ Nível {nivelAtual - 1} completo! Carregando nível {nivelAtual}...");
        StartCoroutine(ProximoNivelAposDelay());
    }

    private IEnumerator ProximoNivelAposDelay()
    {
        yield return new WaitForSeconds(2f);
        IniciarNivel();
    }

    public void OnPlayerAtingido()
    {
        if (!jogoAtivo) return;
        GameOver();
    }

    private void GameOver()
    {
        jogoAtivo = false;
        LimparPredictions();
        trafficManager.PararSpawn();
        player.Morrer();

        hud.MostrarStatus("GAME OVER — Pressione R para tentar novamente");
        aguardandoReinicio = true;
    }

    // ── Utilitários ──────────────────────────────────────────

    private void LimparPredictions()
    {
        foreach (var c in coroutinesPrediction)
            if (c != null) StopCoroutine(c);
        coroutinesPrediction.Clear();
    }
}