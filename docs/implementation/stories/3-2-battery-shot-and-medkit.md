---
story_id: '3-2-battery-shot-and-medkit'
epic: 'epic-3'
status: done
created: 2026-11-13
---

# História 3.2 — Disparo à Base de Bateria e Kit Médico

**Pilar reforçado:** Recurso limitado de risco e recompensa (bateria para disparos vs. uso em exploração) e ritmo de tensão → alívio nos abrigos.

## Objetivo
Implementar `BatteryComponent` (disparo à distância consumindo carga, recarga em abrigo e via células dropadas) e `MedkitComponent` (3 cargas iniciais de cura, sem desperdício quando a vida está cheia), além do descanso completo no abrigo.

## Arquitetura
- `BatteryModel` / `BatteryComponent` — carga máxima, consumo por disparo, recarga parcial (células) e restauração 100%.
- `MedkitModel` / `MedkitComponent` — 3 cargas iniciais, cura `HealthComponent`, bloqueia uso com vida cheia e restaura 100% no abrigo.
- `BatteryProjectile` + `PlayerRangedCombat` — disparo elétrico direcional consumindo bateria (`Mouse Direito` / `F`).
- `BatteryCellPickup` & `ShelterRestSpot` — célula coletável e ponto de abrigo de teste (`E` / `Interact` restaura vida, bateria e kits).

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `BatteryAndMedkitTests` passam.
3. No **Play Mode** (`CombatTestRoom`), disparar (`Mouse Direito` ou `F`) lança projétil elétrico e consome bateria até zerar, impedindo novos disparos até recarregar.
4. Encostar em uma `BatteryCellPickup` recarrega parcialmente a bateria, permitindo voltar a disparar.
5. Após sofrer dano no `TestHazard`, usar o Kit Médico (`Q`) restaura vida consumindo 1 carga; interagir (`E`) com o `ShelterRestSpot` restaura 100% da vida, bateria e kits.

## Estado
- [x] `BatteryComponent`/`BatteryModel`, `MedkitComponent`/`MedkitModel`, `BatteryProjectile`, `PlayerRangedCombat`, `BatteryCellPickup` e `ShelterRestSpot`.
- [x] `GameInputReader` atualizado com ações `Shoot` (`Mouse Direito` / `F` / `RT`) e `Heal` (`Q` / `LB`).
- [x] Prefab `Prefabs/Player/BatteryProjectile.prefab` e `CombatTestRoom.unity` atualizada com células de bateria e `ShelterRestSpot`.
- [x] Critérios 1–2: compila sem erros e **32/32** testes EditMode passam (incluindo `BatteryAndMedkitTests`).
- [x] Critérios 3–5: validação manual no Play Mode aprovada pelo usuário (disparo elétrico, células de bateria, kit médico e descanso no abrigo).
