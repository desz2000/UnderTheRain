---
story_id: '3-1-melee-combat-simple-baton-and-game-feel'
epic: 'epic-3'
status: done
created: 2026-11-13
---

# História 3.1 — Combate Corpo a Corpo (Bastão Simples) e Game Feel

**Pilar reforçado:** Combate legível e tátil (game feel do vertical slice), com separação entre lógica de dano, apresentação (hitstop/screenshake) e dados em ScriptableObject (`WeaponDataSO`).

## Objetivo
Implementar a base do sistema de combate corpo a corpo de Dante com o Bastão Simples: `WeaponDataSO`, combo de 2–3 golpes, sistema `Hitbox` / `Hurtbox` (`IDamageable` + `DamageContext`), *hitstop* (~40–60ms), *screenshake* sutil, recuo e invulnerabilidade pós-dano (~0,5s, ajustado após playtest) via `HealthComponent`.

## Arquitetura
- `WeaponDataSO` — estágio da arma (`SimpleBaton`, `ElectrifiedBaton`, `HighFreqBlade`), dano, alcance, janela de combo, *hitstop*, *knockback* e *screenshake*.
- `IDamageable`, `DamageContext`, `DamageType` — contrato desacoplado de dano.
- `HealthComponent` — células discretas de vida e janela de invulnerabilidade pós-dano (`~0.8s`), com eventos para HUD/VFX.
- `Hitbox` / `Hurtbox` — detecção por trigger em camadas físicas dedicadas (`PlayerHitbox`, `EnemyHurtbox`, `EnemyHitbox`, `PlayerHurtbox`), garantindo 1 acerto por alvo por golpe.
- `PlayerMeleeCombat` — combo de 3 golpes lendo `GameInputReader.OnAttackPressed` e aplicando direção do facing.
- `CombatFeedback` + `CameraShake2D` — *hitstop* (via `Time.timeScale`) e tremor sutil de câmera.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `HealthComponentTests` e `MeleeComboTests` passam.
3. No **Play Mode**, atacar um alvo de teste (`TrainingDummy`) aplica dano, gera recuo e dispara *hitstop* (~50ms) e *screenshake* sutil perceptíveis.
4. Receber dano de um `Hazard` / `EnemyHitbox` de teste reduz vida e ativa invulnerabilidade temporária (~0,5s), impedindo perder vida duas vezes no mesmo golpe/contato imediato.

## Estado
- [x] `DamageTypes`, `HealthComponent`/`HealthModel`, `Hitbox`/`Hurtbox`, `WeaponDataSO`, `MeleeComboState`, `PlayerMeleeCombat`, `CombatFeedback`, `CameraShake2D` e `TrainingDummy`.
- [x] Layers de combate (`PlayerHitbox`, `PlayerHurtbox`, `EnemyHitbox`, `EnemyHurtbox`) + matriz de colisão, asset `Data/Weapons/SimpleBaton.asset` e cena `Scenes/Zones/CombatTestRoom.unity` (`UnderTheRain → Setup → Create Combat Test Room`).
- [x] Critérios 1–2: compila sem erros e **28/28** testes EditMode passam (incluindo `HealthComponentTests` e `MeleeComboTests`).
- [x] Critérios 3–4: validação manual no Play Mode aprovada (invulnerabilidade ajustada para `0.5s`, knockback com trava de `0.16s` e finalizador do combo com bônus de dano/impacto).
