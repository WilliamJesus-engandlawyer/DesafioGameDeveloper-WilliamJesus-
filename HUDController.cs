using TMPro;
using UnityEngine;

// =============================================================
// HUDController.cs
// Responsabilidade: exibir dados na tela para o jogador.
// Só recebe dados prontos e os formata — sem lógica de jogo.
// =============================================================

public class HUDController : MonoBehaviour
{
    [Header("Textos do HUD (arraste do Inspector)")]
    public TextMeshProUGUI textoNivel;
    public TextMeshProUGUI textoDadosAPI;
    public TextMeshProUGUI textoTimer;
    public TextMeshProUGUI textoStatus;

    // Atualiza o painel completo com dados recebidos da API
    public void AtualizarDadosAPI(int nivel, Status status, float multClima)
    {
        if (textoNivel != null)
            textoNivel.text = $"Nível: {nivel}";

        if (textoDadosAPI != null)
            textoDadosAPI.text = $"Densidade: {status.vehicleDensity:F1} | " +
                                 $"Vel: {status.averageSpeed:F0} km/h | " +
                                 $"Clima: {status.weather} (x{multClima:F1})";
    }

    // Atualiza só o timer
    public void AtualizarTimer(float segundos)
    {
        if (textoTimer != null)
            textoTimer.text = $"Tempo: {Mathf.Max(0f, segundos):F1}s";
    }

    // Exibe mensagem de status (vitória, derrota, carregando...)
    public void MostrarStatus(string mensagem)
    {
        if (textoStatus != null)
            textoStatus.text = mensagem;
    }
}
