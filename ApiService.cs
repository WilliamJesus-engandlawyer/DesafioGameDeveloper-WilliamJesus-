using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// =============================================================
// ApiService.cs
// Responsabilidade: comunicação com a API (Mockoon).
// Não sabe nada sobre jogo — só busca e entrega dados.
// =============================================================

public class ApiService : MonoBehaviour
{
    [Header("Configuração da API")]
    public string apiUrl = "http://localhost:3000/v1/traffic/status";

    // Busca os dados da API e chama onSuccess com o resultado,
    // ou onError se algo der errado.
    public void BuscarDados(Action<TrafficResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(RequisicaoGet(onSuccess, onError));
    }

    private IEnumerator RequisicaoGet(Action<TrafficResponse> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string erro = $"Erro na API: {request.error}";
                Debug.LogWarning(erro);
                onError?.Invoke(erro);
                yield break;
            }

            try
            {
                TrafficResponse resposta = JsonUtility.FromJson<TrafficResponse>(request.downloadHandler.text);

                if (resposta == null || resposta.current_status == null)
                {
                    onError?.Invoke("JSON inválido ou vazio.");
                    yield break;
                }

                Debug.Log($"[ApiService] Dados recebidos — Densidade: {resposta.current_status.vehicleDensity} | Vel: {resposta.current_status.averageSpeed} | Clima: {resposta.current_status.weather}");
                onSuccess?.Invoke(resposta);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Erro ao parsear JSON: {e.Message}");
            }
        }
    }
}
