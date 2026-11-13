---
story_id: '3-3-parts-pack-death-and-dropped-pack-recovery'
epic: 'epic-3'
status: done
created: 2026-11-13
---

# História 3.3 — Mochila de Peças, Morte e Recuperação (`DroppedPack`)

**Pilar reforçado:** Morte com custo leve (retorno ao último abrigo + recurso recuperável no local da morte) e economia de peças para fabricação/upgrades.

## Objetivo
Implementar `PartsPackComponent` (coleta de peças até `maxCapacity`), fabricação de kit médico usando peças, e fluxo de morte com spawn de `DroppedPack` no local exato da queda + despertar no último abrigo.

## Arquitetura
- `PartsPackModel` / `PartsPackComponent` — armazena `currentParts` e `maxCapacity`, limita ao teto, permite gasto/fabricação de kit médico e extrai peças ao morrer.
- `DroppedPack` — instância no mundo criada no local da morte com as peças perdidas; ao ser tocada pelo jogador, devolve todas as peças (respeitando `maxCapacity`).
- `PartsPickup` — peça dropada por inimigos/alvos ao serem derrotados.
- `PlayerDeathHandler` — escuta `HealthComponent.OnDied`, instancia `DroppedPack`, transporta Dante para o último abrigo (`ShelterRestSpot`) e restaura vida e bateria (kits médicos **não** são restaurados ao morrer).

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `PartsPackTests` passam.
3. No **Play Mode** (`CombatTestRoom`), derrotar o `TrainingDummy` dropa peças que somam na mochila (`Parts: X/50`) sem ultrapassar `maxCapacity`.
4. Apertar `C` com peças suficientes e kit faltando fabrica `+1` carga de Kit Médico descontando `15` peças.
5. Ao morrer no `TestHazard` (`HP -> 0`), Dante acorda no `ShelterRestSpot` com `0` peças e uma `DroppedPack` aparece no local exato da queda; tocar na `DroppedPack` devolve todas as peças perdidas.

## Estado
- [x] `PartsPackComponent`/`PartsPackModel`, `DroppedPack`, `PartsPickup` e `PlayerDeathHandler`.
- [x] `TrainingDummy` dropa `PartsPickup` (`15` peças) ao ser derrotado; `ShelterRestSpot` define o ponto de respawn; `GameInputReader` adiciona `Craft` (`C`).
- [x] Prefabs `Prefabs/Environment/DroppedPack.prefab` e `PartsPickup.prefab` + `CombatTestRoom.unity` atualizada.
- [x] Critérios 1–2: compila sem erros e **35/35** testes EditMode passam (incluindo `PartsPackTests`).
- [x] Critérios 3–5: validação manual no Play Mode aprovada (kits preservados na morte; adicionado `BatteryChargeStation`; `Interact` padronizado em `E` / `Y` Xbox / `Triângulo` PS5).
