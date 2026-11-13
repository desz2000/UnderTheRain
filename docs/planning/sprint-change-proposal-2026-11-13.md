---
title: 'Sprint Change Proposal — Épico 5: novo design de mapa'
date: 2026-11-13
workflow: gds-correct-course
mode: batch
scope: moderate
status: approved
---

# Sprint Change Proposal — Épico 5: novo design de mapa

## 1. Resumo do problema

Durante a História 5.1 o design do mapa do Ato I mudou:

- O Prólogo/Tutorial virou uma **Zona 0** própria, com 6 salas, fora das 5 macrozonas do GDD. É uma **estação de metrô abandonada** no subsolo (sem chuva) que esconde um **laboratório abandonado da Domus Corp**.
- A Zona 1 (Esgotos) passa a ter **13 salas**: 11 definidas (Z1-01 a Z1-11) e 2 *(proposta)* (Z1-12 e Z1-13) cuja estrutura fica a cargo do level design. Todas se organizam em torno da **base da resistência** (Abrigo Z1-01), que será revisitada com frequência no backtracking.
- O **Wall Jump** sai da "subida" genérica e vai para a **Oficina (Z1-06)**, no fim da Zona 1, como luvas de tração.
- A taxonomia de gates passa a ter 4 tipos: progressão, salto de sala, coletável e retorno.
- Vários valores de game feel foram ajustados em playtest nos Épicos 2–4. Os documentos ficaram para trás.

**Evidência:** código e assets atuais (`Assets/_Project/Data/**`, `Scripts/Runtime/**`), cenas `Prologue_Ruins` e `Act1_Sewers` geradas pelos scripts de Editor, e as decisões do usuário em 2026-11-13.

## 2. Análise de impacto

| Artefato | Impacto |
|---|---|
| `docs/planning/epics.md` | 5.1 reescrita (escopo real), 5.2 redefinida (Z1-05 a Z1-10), 5.3 ganha descanso pré-boss *(proposta)*, novas 5.4 e 5.5, 7.2 restrito às zonas 2–5 |
| `sprint-status.yaml` | +2 chaves (`5-4-*`, `5-5-*`), sem rebaixar status |
| GDD (`gdd.md`, `decision-log.md`, novo `level-map.md`) | Zona 0, taxonomia de gates, valores de game feel, Kitty |
| `game-architecture.md` | alinhar à implementação real (sem Cinemachine, `Narrative/`, cenas por zona) |
| `narrative-design.md` | numeração dos beats, Prólogo, local do Wall Jump |
| `project-context.md` | Zona 0, Kitty, gates, `level-map.md`, aprendizados, "valores vivem nos assets" |
| Código | **nenhum** (tarefa só de documentação) |

## 3. Abordagem recomendada

**Ajuste direto** (Direct Adjustment): adicionar histórias ao Épico 5 sem renumerar chaves existentes e atualizar os documentos. Não é preciso desfazer nada. Esforço baixo e risco baixo. Ordem sugerida: **5.1 → 5.4 → 5.5 → 5.2 → 5.3**.

## 4. Propostas de mudança (resumo)

### 4.1 Divergências resolvidas nesta proposta

1. **IDs das salas do Prólogo já construídas.** O prompt de origem usa `prologue_rubble_entry` (P1) e `prologue_collapsed_archive` (P5), mas o código atual, refeito em 2026-11-13 para o tema metrô, usa **`prologue_metro_platform`** (P1, Plataforma do Metrô) e **`prologue_hidden_lab`** (P5, Laboratório Escondido). Como a regra é "salas já construídas mantêm seus IDs", **valem os IDs do código**. O nome da cena continua `Prologue_Ruins`.
2. **Duto da Kitty.** Hoje ele está em P1 (porta de manutenção emperrada). A História 5.4 o move para **P4 (`prologue_duct`)**.
3. **Bíblia narrativa.** Uma linha sobre o Prólogo no metrô foi adicionada em 2026-11-13, antes desta regra. Ela foi **revertida**. A mudança fica como proposta na seção 6.

### 4.2 Histórias

- **5.1** (chave mantida, `review`): Prólogo P1 + P5, Abrigo Z1-01 (Icarus) e Passagem Z1-02.
- **5.2** (chave mantida): Z1-05 a Z1-10 + transição para a Superfície.
- **5.3** (chave mantida): + ponto de descanso antes da arena *(proposta)*.
- **5.4 nova** `5-4-prologue-expansion-tutorial-rooms`: P2, P3, P4, P6.
- **5.5 nova** `5-5-sewers-gallery-duct-and-sealed-door`: Z1-03, Z1-04 e a porta do Setor Selado (Z1-11, sem energia).
- **7.2:** zonas 2–5. A Zona 1 é entregue no Épico 5, exceto a sala do Selado (Épico 8).

### 4.3 Valores (de → para), extraídos dos assets/código

| Valor | Antes (docs) | Agora (fonte) |
|---|---|---|
| Coyote time | ~100 ms | **40 ms** (`PlayerMovementConfig.asset`) |
| Invulnerabilidade pós-dano | ~0,8 s | **0,5 s** (`HealthComponent`) |
| Trava de knockback ao levar dano | — | **0,16 s** (`PlayerController.DamageKnockbackLockTime`) |
| Wall Jump | — | **velocidade (6,5; 14)**, **trava 0,05 s**, wall coyote **0,08 s**, wall slide máx. **3,5** |
| Combo do bastão | 2–3 golpes | **3 golpes**, dano **2**, finalizador **+1** dano, hitstop ×1,4, knockback ×1,35 (`SimpleBaton.asset`) |
| Hitstop | ~40–60 ms | **50 ms** base |
| Interferência da Kitty | ~1,0–1,5 s | **1,35 s**, raio **4,5**, cooldown **3 s**, custo **50** de bateria (`CatConfig.asset`) |

## 5. Handoff

- **Escopo:** Moderado (reorganização de backlog + documentação).
- **Responsável:** Developer (este agente) aplica as edições. Depois, rodar `gds-create-story` para a **5.4**.
- **Critério de sucesso:** `gds-sprint-planning` lista 5.4 e 5.5; `level-map.md` é carregado pelo `create-story`; nenhum documento contradiz os assets.

## 6. Mudança proposta para a bíblia (aguarda aprovação)

Adicionar à §7 (Decisões Adicionais):

> **Prólogo na estação de metrô:** o Prólogo/Tutorial acontece no subsolo da cidade, sem chuva, numa estação de metrô desativada. Atrás dela há um laboratório abandonado da Domus Corp onde se testava um novo modelo de robô de guerra que não foi adiante. Ali Dante encontra o terminal com as provas do acobertamento. Kitty já acompanha Dante desde o início. A garoa só aparece quando Dante chega à superfície.

E ajustar a §4 (Roteiro do Ato I), item 2: Dante encontra as provas **no laboratório escondido do Prólogo** e então confronta Icarus.
