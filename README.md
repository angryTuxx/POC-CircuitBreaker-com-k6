# POC - Resiliência com Circuit Breaker

## Contexto

Esta Prova de Conceito visa atender o cenário de chamadas para APIs externas instáveis ou indisponíveis, resolver o problema de alta latência e falhas em cascata quando dependências externas falham, e demonstrar como o padrão Circuit Breaker com Polly + Refit protege a aplicação principal mantendo baixa latência e alta taxa de sucesso mesmo durante falhas externas.

---

## Arquitetura
```
K6 Load Test → API Principal (porta 4000) → API Externa (porta 5000)
                         |
                   Circuit Breaker
                   (Polly + Refit)
```

### Componentes

- **API Principal** (porta 4000): Consome API externa com/sem circuit breaker
- **API Externa** (porta 5000): Simula instabilidade e indisponibilidade controlada
- **K6**: Automação de cenários de teste e coleta de métricas

---

## Cenários de Teste

O teste K6 executa 4 fases automatizadas em sequência:

| Fase | Tempo | Comportamento | Objetivo |
|------|-------|---------------|----------|
| 1. Baseline | 0-20s | API externa responde normalmente | Medir latência e taxa de sucesso em condições normais |
| 2. Instabilidade | 20-45s | API externa adiciona latência (150-300ms) | Validar comportamento com timeout e latência alta |
| 3. Indisponibilidade | 45-60s | API externa retorna erro 500 | Validar circuit breaker abrindo e fallback |
| 4. Recuperação | 60-75s | API externa volta ao normal | Validar circuit breaker fechando automaticamente |

---

## Configuração do Circuit Breaker

### Parâmetros Polly
```
Threshold: 5 falhas consecutivas
Duração circuito aberto: 3 segundos
Timeout por requisição: 150ms
Condições de falha: Status 5xx ou Timeout
```

### Fluxo de Estados
```
Closed (normal) → 5 falhas → Open (aberto) → aguarda 3s → Half-Open (teste) → sucesso → Closed
                                    ↓                              ↓
                              Fallback imediato                falha → Open
```

---

## Métricas Coletadas

### K6 Custom Metrics

- **latency**: Tempo de resposta (P50, P95, P99)
- **http_2xx**: Taxa de requisições bem-sucedidas
- **http_non_2xx**: Taxa de falhas
- **timeout_count**: Número de timeouts
- **server_error_count**: Número de erros 5xx
- **high_latency_count**: Requisições acima de 800ms

### Thresholds Definidos
```
P95 latência < 100ms
P99 latência < 150ms
Taxa de sucesso > 90%
Taxa de falha < 10%
```

---

## Resultados Esperados

### Endpoint COM Circuit Breaker (/resiliencia)

| Cenário | P95 | P99 | Taxa Sucesso | Observação |
|---------|-----|-----|--------------|------------|
| Normal | ~100ms | ~150ms | >95% | Chamadas diretas para API externa |
| Instável | ~150ms | ~200ms | >90% | Alguns timeouts, mas mantém estabilidade |
| Indisponível | <50ms | <80ms | >90% | Fallback rápido sem chamada externa |
| Recuperação | ~100ms | ~150ms | >95% | Circuit fecha e volta ao normal |

### Endpoint SEM Circuit Breaker (/simples)

| Cenário | P95 | P99 | Taxa Sucesso | Observação |
|---------|-----|-----|--------------|------------|
| Normal | ~100ms | ~150ms | >95% | Funciona normalmente |
| Instável | ~300ms | ~500ms | ~50% | Muitos timeouts e alta latência |
| Indisponível | timeout | timeout | 0% | Falha total da aplicação |

---

## Como Executar

### 1. Subir as APIs

Terminal 1 - API Externa (Receiver):
```bash
cd Poc_Refit_CircuitBreaker_Receiver
dotnet run
```

Terminal 2 - API Principal:
```bash
cd Poc_Refit_CircuitBreaker
dotnet run
```

### 2. Executar testes K6
```bash
k6 run script.js
```

### 3. Observar resultados

- Logs no console mostram estado do circuit (Open/Closed/Half-Open)
- K6 exibe métricas em tempo real e sumário final
- Verifique se P95/P99 e taxa de sucesso atendem os thresholds

---

## Endpoints Disponíveis

### API Principal (porta 4000)

- `GET /api/poc/resiliencia` - Endpoint COM circuit breaker e fallback
- `GET /api/poc/simples` - Endpoint SEM circuit breaker (comparação)

### API Externa (porta 5000) - Controle de Simulação

- `GET /api/poc/instable` - Liga/desliga simulação de instabilidade
- `GET /api/poc/unavailable` - Liga/desliga simulação de indisponibilidade
- `GET /api/poc/clear` - Reset (volta tudo ao normal)

---

## Validações Principais

1. Circuit breaker abre após 5 falhas consecutivas
2. Fallback retorna status 200 mesmo com API externa indisponível
3. Latência P99 permanece abaixo de 150ms durante indisponibilidade
4. Circuit fecha automaticamente após API externa recuperar
5. Taxa de sucesso mantém-se acima de 90% em todos os cenários

---

## Tecnologias Utilizadas

- .NET 5
- Refit (cliente HTTP tipado)
- Polly (resiliência e circuit breaker)
- K6 (testes de carga e performance)


---

## Captura de testes realizados

- Sem resiliencia
- <img width="524" height="389" alt="withouth-circuit" src="https://github.com/user-attachments/assets/5fa801ba-e1c8-490d-afa8-6f927e536829" />

- Com Resiliencia
- <img width="507" height="393" alt="with-circuit" src="https://github.com/user-attachments/assets/f9beca69-6a71-48cb-8806-9ca921884020" />
