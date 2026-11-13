---
story_id: '5-1-sewers-rooms-initial-shelter-and-icarus'
epic: 'epic-5'
status: review
created: 2026-11-13
---

# História 5.1 — Salas dos Esgotos, Abrigo Inicial e Confronto Narrativo (Icarus)

**Pilar reforçado:** Abrigos como pontos de ritmo e narrativa contada pelo ritmo de exploração (Seções 5.6 e 5.8 — tensão/alívio e o incidente incitante do Ato I).

> **Nota de escopo (2026-11-13, correct-course):** esta história cobre **P1 Plataforma do Metrô** (`prologue_metro_platform`) e **P5 Laboratório Escondido** (`prologue_hidden_lab`) no Prólogo (Zona 0), e **Z1-01 Abrigo** (`sewers_initial_shelter`) e **Z1-02 Passagem** (`sewers_passage`) nos Esgotos (Zona 1). As salas P2, P3, P4 e P6 ficam na **5.4**; Z1-03, Z1-04 e a porta do Setor Selado ficam na **5.5**; o restante da Zona 1 fica na **5.2**. O duto de Kitty que hoje está em P1 é provisório e passa para P4 na 5.4. Mapa completo: [level-map.md](../../planning/gdd/gdd-Under-The-Rain-2026-11-13/level-map.md). Proposta de mudança: [sprint-change-proposal-2026-11-13.md](../../planning/sprint-change-proposal-2026-11-13.md).
>
> **Ajustes de playtest já aplicados no código (pendentes de validação):** a caixa de diálogo fecha corretamente (`DialogueRunner.IsBusy`); o robô batedor causa e recebe dano (`ScoutRobotEnemy`); o Prólogo não tem chuva; a volta Abrigo → Prólogo chega pelo lado direito do Laboratório (entrada entre cenas por sala + spawn).
>
> **Revisão pré-validação (2026-11-13):**
> - morrer agora respawna com fade **e ativa a sala do ponto de respawn** (antes, morrer em outra sala deixava Dante numa sala inativa, sem chão);
> - ataque, disparo, cura, fabricação, dash e comando de Kitty ficam bloqueados durante diálogos (`GameInputReader.SetActionsSuppressed`);
> - a porta de manutenção aberta por Kitty continua aberta ao recarregar a cena na mesma sessão (`CatExplorationPoint.persistentId`);
> - a chegada por troca de cena faz fade-in a partir do preto;
> - o spawn `from_lab` em P1 saiu de cima do trigger de saída (evitava um vai e volta infinito);
> - o flash de dano do robô não o deixa mais "preso" na cor de hesitação;
> - a busca de salas não usa mais `FindObjectsByType`;
> - foi removido `useFullKinematicContacts` (desnecessário);
> - a Kitty se chama "Kitty" também em `Act1_Sewers`.
> - Novos testes: `RoomBoundsTests` (2, EditMode) e `Story51SceneFlowTests` (4, PlayMode, sobre as cenas geradas: Prólogo começa na plataforma; morrer no laboratório respawna na plataforma com a sala ativa; ida e volta entre cenas chega na sala e no spawn corretos; a porta da Kitty continua aberta após recarregar).
> - **Validação via CLI (batchmode, 2026-11-13):** compilação sem erros nem warnings; menus de setup geraram `Prologue_Ruins` e `Act1_Sewers`; **EditMode 48/48** e **PlayMode 5/5**. Falta só a validação manual no Play Mode (sensação e fluxo).
>
> **Limitações conhecidas (fora do escopo, Épico 8 — save):** ao recarregar uma cena, inimigos e pickups voltam (dá para "farmar" as peças do P1 indo e voltando), e o respawn do Prólogo é o ponto inicial (o Prólogo não tem abrigo).

## Objetivo
Montar a zona de **Prólogo / Tutorial** numa estação de metrô abandonada no subsolo (`Prologue_Ruins.unity`, sem chuva, com dicas de movimento/pulo/Kitty/combate, duto de manutenção para Kitty e um laboratório secreto da Domus Corp escondido atrás da estação, onde fica o terminal antigo) conectada ao **Abrigo Inicial nos Esgotos** (`Act1_Sewers.unity`, ambiente subterrâneo seco com `ShelterRestSpot`, **Icarus** e o confronto narrativo que abre a comporta leste rumo à subida).

## Arquitetura
- `DocumentDataSO` e `DialogueSequenceSO` (`Runtime/Narrative/`) — dados canônicos em `Assets/_Project/Data/Narrative/`.
- `NarrativeProgressModel` (`Runtime/Narrative/`) — modelo C# puro para documentos coletados e flags narrativas.
- `NarrativeState` e `DialogueRunner` (`Runtime/Narrative/`) — estado de sessão compartilhado entre cenas (`Prologue_Ruins` → `Act1_Sewers`) e execução de diálogos com travamento de controle em `PlayerController.SetControlLocked(bool)`.
- `NarrativeTerminal`, `IcarusNpc`, `NarrativeGate` e `TutorialSignpost` (`Runtime/World/`) — fluxo do incidente incitante até a abertura da comporta leste. `SewerRainAmbience` fica reservado para a superfície.
- `ScoutRobotEnemy` (`Runtime/Enemies/`) + `EnemySetupUtility` (`Editor/`) — robô batedor com vida, dano de contato (desligado enquanto hesita pela Kitty) e drop de peças.
- `PrologueRuinsSetup` e `Act1SewersSetup` (`Editor/`) — geram `Prologue_Ruins.unity` (plataforma do metrô + laboratório Domus escondido, sem chuva) e `Act1_Sewers.unity` (2 salas subterrâneas nos esgotos).
- `NarrativeProgressTests` (`Tests/EditMode/`) — valida coleta única de documentos, flags e fluxo narrativo do Ato I.
- `RoomBoundsTests` (`Tests/EditMode/`) — `RoomBounds.ContainsPoint`, inclusive com a sala inativa (base do respawn por sala).

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes (incluindo `NarrativeProgressTests` e `RoomBoundsTests`) passam — 48/48.
3. No **Play Mode** (`Prologue_Ruins.unity`), Dante e **Kitty** começam na plataforma do metrô abandonado (sem chuva) e praticam movimento/pulo. Mandam Kitty pelo duto (`E` / `Y`) para destravar a porta de manutenção, usam interferência (`R`) e o bastão contra o robô batedor (que causa e recebe dano), leem o `DomusTerminal` (`E` / `Y`) no laboratório escondido; fechar a última fala encerra o diálogo e descem para `Act1_Sewers.unity`.
4. Em `Act1_Sewers.unity` (sem chuva dentro dos esgotos), falar com **Icarus** após ler o terminal dispara o confronto narrativo e abre a `SewerExitGate`, permitindo atravessar para `Room_02_SewerPassage`.

## Estado
- [x] `DocumentDataSO`, `DialogueSequenceSO`, `NarrativeProgressModel`, `NarrativeState`, `DialogueRunner`.
- [x] `NarrativeTerminal`, `IcarusNpc`, `NarrativeGate`, `SewerRainAmbience` e `PlayerController.SetControlLocked`.
- [x] `NarrativeProgressTests` (EditMode).
- [x] `Act1SewersSetup` (Editor menu `UnderTheRain/Setup/Create Act 1 Sewers Scene`).
- [ ] Gerar `Act1_Sewers.unity` no Editor e validar no Play Mode do Unity.
