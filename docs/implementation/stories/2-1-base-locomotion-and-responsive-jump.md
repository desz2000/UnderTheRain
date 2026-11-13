---
story_id: '2-1-base-locomotion-and-responsive-jump'
epic: 'epic-2'
status: done
created: 2026-11-13
---

# História 2.1 — Locomoção Base e Pulo Responsivo

**Pilar reforçado:** Movimento com game feel aprovado (vertical slice) e progressão por habilidades (base para o Wall Jump da 2.2).

## Objetivo
Implementar a locomoção de Dante: corrida (~0,08s de aceleração / ~0,06s de parada), pulo variável, *coyote time* (~40ms, reduzido após playtest para não facilitar demais escalada/parkour), *input buffering* (~120ms), *corner correction* e *drop-through* em plataformas finas. Todos os valores ficam em `PlayerMovementConfigSO`.

## Arquitetura
- `PlayerMovementConfigSO` — dados de tuning (sem números mágicos).
- `PlayerMotor` — lógica pura (testável em EditMode): velocidade, timers, estado.
- `PlayerLocomotionState` — `Idle`, `Run`, `JumpAscend`, `Fall` (demais estados em histórias futuras).
- `PlayerController` — `Rigidbody2D` + checagem de chão/teto, corner correction, drop-through; lê `GameInputReader`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `PlayerMotorTests` passam.
3. Play Mode em sala de teste de plataformas: Dante para sem deslizar ao soltar a direção.
4. A altura do pulo varia pelo tempo de pressão do botão.
5. É possível pular logo após sair de uma borda (*coyote*) e enfileirar o pulo antes de tocar o chão (*buffer*).
6. Cabeçada na quina de uma plataforma é corrigida lateralmente (*corner correction*).
7. Baixo + Pulo sobre plataforma fina (layer `OneWayMask`) atravessa a plataforma.

## Estado
- [x] Código, config asset (`Data/Abilities/PlayerMovementConfig.asset`) e layers `Ground`/`OneWayPlatform`.
- [x] Sala de teste `Scenes/Zones/MovementTestRoom.unity` (menu `UnderTheRain → Setup → Create Movement Test Room`): chão com vão (coyote), bloco de teto (corner correction), plataforma fina (drop-through).
- [x] Critérios 1–2: compila e 9/9 `PlayerMotorTests` passam.
- [x] Critérios 3–7: validação manual no Play Mode (feel) pelo usuário (coyote time ajustado para 40ms).