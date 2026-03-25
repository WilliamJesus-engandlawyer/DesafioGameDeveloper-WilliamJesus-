using System.Collections.Generic;

// =============================================================
// GameDataModel.cs
// Responsabilidade: APENAS representar o contrato da API.
// Nenhuma lógica de jogo aqui — só dados.
// =============================================================

[System.Serializable]
public class TrafficResponse
{
    public Status current_status;
    public List<PredictedEntry> predicted_status;
}

[System.Serializable]
public class PredictedEntry
{
    public int estimated_time;       // milissegundos para o evento acontecer
    public Status predictions;
}

[System.Serializable]
public class Status
{
    public float vehicleDensity;     // 0.1 a 1.0
    public float averageSpeed;       // 0 a 100 km/h
    public string weather;           // sunny, clouded, foggy, light rain, heavy rain
}
