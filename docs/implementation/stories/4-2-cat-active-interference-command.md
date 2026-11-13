---
story_id: '4-2-cat-active-interference-command'
epic: 'epic-4'
status: done
created: 2026-11-13
---

# História 4.2 — Comando Ativo de Interferência (`ICatAffectable`)

**Pilar reforçado:** O gato como companheiro ativo e narrativa ambiental (Seção 5.11 — robôs da mente coletiva hesitam/travam diante do gato, enquanto humanos misantropos e protótipos offline são imunes).

## Objetivo
Implementar `CatInterference` com cooldown e interface `ICatAffectable`: ao acionar o comando do gato (`R` no teclado / `RB` no gamepad), robôs da colmeia (`CollectiveMachine`) dentro do alcance entram em estado `Hesitating` (`~1.35s`) com sinal visual diegético, enquanto Humanos Misantropos ignoram o pulso.

## Arquitetura
- `GameInputReader` — adiciona `OnCatCommandPressed` (`R` / `<Gamepad>/rightShoulder`).
- `EnemyFaction` & `ICatAffectable` (`Runtime/Enemies/`) — define facções (`CollectiveMachine`, `MisanthropeHuman`, `OfflinePrototype`) e contrato de interferência.
- `CatConfigSO` — adiciona `interferenceRadius`, `interferenceCooldown`, `hesitationDuration`, `perTargetCooldown` e `interferenceBatteryCost` (`50`, no máximo 2 usos por carga completa; disparo à distância ajustado para `15`).
- `CatInterferenceModel` — modelo puro de cooldown + aplicação em alvos `ICatAffectable`, testável em EditMode.
- `CatInterference` — MonoBehaviour no gato que escuta `OnCatCommandPressed`, consome bateria via `BatteryComponent`, detecta alvos `ICatAffectable` por `Physics2D.OverlapCircleAll` e exibe pulso visual diegético.
- `PatrolInterferenceTarget` (`Runtime/Enemies/`) — ator de teste/base para validar patrulha, hesitação de robô (`CollectiveMachine`) e imunidade de Humano Misantropo (`MisanthropeHuman`) na `CatTestRoom`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `CatInterferenceTests` passam.
3. No **Play Mode** (`CatTestRoom`), apertar `R` (ou `RB`) perto do robô batedor (`CollectiveMachine`) o trava em `Hesitating` por `~1.35s` com sinal visual/faísca e entra em cooldown.
4. Apertar `R` (ou `RB`) perto do Humano Misantropo (`MisanthropeHuman`) não interrompe sua patrulha/ataque.

## Estado
- [x] `EnemyFaction`, `ICatAffectable`, `CatInterferenceModel`, `CatInterference` (com consumo de bateria) e `GameInputReader.OnCatCommandPressed`.
- [x] `PatrolInterferenceTarget` e atualização de `CatTestRoomSetup` com `ScoutRobot` e `MisanthropeGuard`.
- [x] `CatInterferenceTests` (EditMode).
- [x] Validação no Play Mode do Unity.
