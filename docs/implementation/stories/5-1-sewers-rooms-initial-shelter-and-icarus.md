---
story_id: '5-1-sewers-rooms-initial-shelter-and-icarus'
epic: 'epic-5'
status: review
created: 2026-11-13
---

# História 5.1 — Salas dos Esgotos, Abrigo Inicial e Confronto Narrativo (Icarus)

**Pilar reforçado:** Abrigos como pontos de ritmo e narrativa contada pelo ritmo de exploração (Seções 5.6 e 5.8 — tensão/alívio e o incidente incitante do Ato I).

## Objetivo
Montar a zona de **Prólogo / Tutorial** nos escombros entre esgotos e superfície (`Prologue_Ruins.unity`, com garoa real sob o céu, dicas de movimento/pulo/Kitty/combate e o terminal antigo da Domus Corp) conectada ao **Abrigo Inicial nos Esgotos** (`Act1_Sewers.unity`, ambiente subterrâneo seco com `ShelterRestSpot`, **Icarus** e o confronto narrativo que abre a comporta leste rumo à subida).

## Arquitetura
- `DocumentDataSO` e `DialogueSequenceSO` (`Runtime/Narrative/`) — dados canônicos em `Assets/_Project/Data/Narrative/`.
- `NarrativeProgressModel` (`Runtime/Narrative/`) — modelo C# puro para documentos coletados e flags narrativas.
- `NarrativeState` e `DialogueRunner` (`Runtime/Narrative/`) — estado de sessão compartilhado entre cenas (`Prologue_Ruins` → `Act1_Sewers`) e execução de diálogos com travamento de controle em `PlayerController.SetControlLocked(bool)`.
- `NarrativeTerminal`, `IcarusNpc`, `NarrativeGate`, `TutorialSignpost` e `SewerRainAmbience` (`Runtime/World/`) — fluxo do incidente incitante até a abertura da comporta leste, com material `Sprites/Default` na chuva.
- `PrologueRuinsSetup` e `Act1SewersSetup` (`Editor/`) — geram `Prologue_Ruins.unity` (2 salas nos escombros sob garoa) e `Act1_Sewers.unity` (2 salas subterrâneas nos esgotos).
- `NarrativeProgressTests` (`Tests/EditMode/`) — valida coleta única de documentos, flags e fluxo narrativo do Ato I.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes (incluindo `NarrativeProgressTests`) passam.
3. No **Play Mode** (`Prologue_Ruins.unity`), Dante e **Kitty** começam nos escombros sob a garoa (azul-acinzentada, sem shader rosa), praticam movimento/pulo/interferência (`R`)/bastão, leem o `DomusTerminal` (`E` / `Y`) e descem para `Act1_Sewers.unity`.
4. Em `Act1_Sewers.unity` (sem chuva dentro dos esgotos), falar com **Icarus** após ler o terminal dispara o confronto narrativo e abre a `SewerExitGate`, permitindo atravessar para `Room_02_SewerPassage`.

## Estado
- [x] `DocumentDataSO`, `DialogueSequenceSO`, `NarrativeProgressModel`, `NarrativeState`, `DialogueRunner`.
- [x] `NarrativeTerminal`, `IcarusNpc`, `NarrativeGate`, `SewerRainAmbience` e `PlayerController.SetControlLocked`.
- [x] `NarrativeProgressTests` (EditMode).
- [x] `Act1SewersSetup` (Editor menu `UnderTheRain/Setup/Create Act 1 Sewers Scene`).
- [ ] Gerar `Act1_Sewers.unity` no Editor e validar no Play Mode do Unity.
