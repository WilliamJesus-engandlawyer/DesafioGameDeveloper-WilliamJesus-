# DesafioGameDeveloper-WilliamJesus-
## 🎮 Explicando o projeto

<p align="center">
  <a href="https://www.youtube.com/watch?v=0dJSj97pIyI">
    <img src="https://img.youtube.com/vi/0dJSj97pIyI/0.jpg" alt="Demonstração do jogo"/>
  </a>
</p>

[Clique aqui para jogar sem abrir o Unity](https://drive.google.com/drive/folders/1RExN778AbatEEJ_cyOzkQ58Qb7ISUW2V?usp=drive_link)
----------------
# 🚦 VBL Smart Crossing

> Desafio técnico — Centro Von Braun | Time de Inovação  
> Desenvolvido por **William Jesus da Silva**

Jogo 2D em Unity onde o jogador atravessa uma via com tráfego dinâmico controlado **em tempo real por uma API REST**. Todos os parâmetros do jogo — velocidade dos carros, frequência de spawn e penalidade de movimento — são determinados pelos dados recebidos do endpoint `/v1/traffic/status`.

---

## 🎮 Como jogar

- **WASD** — mover o personagem
- Atravesse a via sem ser atingido por nenhum carro
- Cada nível busca novos dados da API e aumenta a dificuldade progressivamente
- **R** — reiniciar após Game Over

---

## 🏗️ Arquitetura

O projeto foi construído com separação estrita de responsabilidades. Cada script tem uma única função e não conhece os detalhes dos outros.

```
Mockoon (API mock)
    └── ApiService.cs          # Única classe que faz requisição HTTP
            └── GameManager.cs # Orquestra todo o fluxo do jogo
                    ├── TrafficManager.cs      # Controla spawn e velocidade dos carros
                    │       └── VehicleController.cs   # Move o carro e detecta colisão
                    ├── PlayerController2D.cs  # Movimento do jogador + multiplicador de clima
                    ├── HUDController.cs       # Exibe dados na tela (sem lógica)
                    └── ZoneChegada.cs         # Detecta quando o jogador atravessou
```

### Fluxo de dados

```
API → TrafficResponse → GameManager → distribui para cada sistema
```

1. `ApiService` faz GET em `/v1/traffic/status` e entrega um `TrafficResponse` via callback
2. `GameManager` aplica `current_status` imediatamente e agenda cada `predicted_status` com Coroutines
3. O timer é calculado a partir do `estimated_time` da última predição + tempo extra configurável
4. Cada sistema recebe apenas o dado que lhe pertence — nenhum acessa a API diretamente

---

## 📁 Scripts

| Arquivo | Responsabilidade |
|---|---|
| `ApiService.cs` | Requisição HTTP GET, timeout, parse do JSON |
| `GameDataModel.cs` | Contrato da API — apenas classes de dados, sem lógica |
| `GameManager.cs` | Orquestração: chama API, agenda predições, gerencia vitória/derrota |
| `TrafficManager.cs` | Spawn de veículos nas duas faixas baseado em `vehicleDensity` e `averageSpeed` |
| `VehicleController.cs` | Movimento linear, auto-destruição fora da tela, notificação de colisão |
| `PlayerController2D.cs` | Movimento WASD com `velocidadeBase × multiplicadorClima` |
| `HUDController.cs` | Exibe nível, dados da API, timer e mensagens de status |
| `ZoneChegada.cs` | Trigger de chegada — notifica `GameManager.OnPlayerChegou()` |

---

## 🌦️ Multiplicadores de clima

Os valores seguem exatamente a especificação do desafio:

| Clima | Multiplicador | Efeito |
|---|---|---|
| `sunny` | 1.0× | Velocidade normal |
| `clouded` | 0.8× | Leve redução |
| `foggy` | 0.8× | Leve redução |
| `light rain` | 0.6× | Redução moderada |
| `heavy rain` | 0.4× | Movimento lento |

O multiplicador é aplicado diretamente na física do jogador:

```csharp
rb.linearVelocity = inputMovimento * velocidadeBase * multiplicadorClima;
```

---

## 📡 Integração com a API

### Endpoint

```
GET http://localhost:3000/v1/traffic/status
```

### Contrato esperado (JSON)

```json
{
  "current_status": {
    "vehicleDensity": 0.5,
    "averageSpeed": 60.0,
    "weather": "light rain"
  },
  "predicted_status": [
    {
      "estimated_time": 10000,
      "predictions": {
        "vehicleDensity": 0.8,
        "averageSpeed": 80.0,
        "weather": "heavy rain"
      }
    }
  ]
}
```

### Campos

| Campo | Tipo | Descrição |
|---|---|---|
| `vehicleDensity` | float (0.1–1.0) | Frequência de spawn — `intervalo = 1 / density` segundos |
| `averageSpeed` | float (0–100) | Velocidade dos carros — normalizada pela `velocidadeReferencia` do Unity |
| `weather` | string | Define o multiplicador de movimento do jogador |
| `estimated_time` | int (ms) | Tempo em milissegundos para aplicar a predição |

### Trocar o Mockoon por outro backend

Basta alterar a URL no Inspector do Unity (campo `apiUrl` do `ApiService`) ou diretamente no código:

```csharp
// ApiService.cs
public string apiUrl = "http://localhost:3000/v1/traffic/status";

// Para Docker local:
public string apiUrl = "http://192.168.1.100:8000/v1/traffic/status";

// Para AWS EC2:
public string apiUrl = "https://sua-instancia.compute.amazonaws.com/v1/traffic/status";
```

Nenhum outro arquivo precisa ser modificado.

---

## 🔧 Como executar localmente

### Pré-requisitos

- Unity 2022.3 LTS ou superior
- [Mockoon](https://mockoon.com/) (para mockar a API)
- Arquivo `vbl-mockoon-environment.json` (incluso no repositório)

### Passos

1. Clone o repositório
   ```bash
   git clone https://github.com/seu-usuario/vbl-smart-crossing.git
   ```

2. Abra o projeto no Unity Hub

3. Importe o ambiente no Mockoon:
   - Abra o Mockoon → **File → Open environment**
   - Selecione `vbl-mockoon-environment.json`
   - Inicie o servidor (porta 3000)

4. No Unity, abra a cena principal e clique em **Play**

---

## 📊 Níveis de dificuldade (JSON de referência)

| Nível | Densidade | Velocidade | Clima |
|---|---|---|---|
| 1 | 0.2 | 30 km/h | sunny |
| 2 | 0.4 | 55 km/h | clouded → light rain |
| 3 | 0.7 | 75 km/h | light rain |
| 4 | 1.0 | 100 km/h | heavy rain |
| Erro | — | — | HTTP 500 (teste de resiliência) |

---

## 👤 Autor

**William Jesus da Silva**  
Engenharia de Computação — Univesp  
Experiência em C#, .NET, Python, APIs REST, Unity, Inteligência Artificial e visualização de dados

---

*Desenvolvido como parte do processo seletivo para o time de Inovação do Centro Von Braun.*
