---
story_id: '4-1-cat-follow-and-shoulder-perch'
epic: 'epic-4'
status: done
created: 2026-11-13
---

# História 4.1 — Seguir Dante e Empoleirar no Ombro

**Pilar reforçado:** O gato como mecânica de exploração e companheiro ativo (Seção 5.11 — presente no mundo ao lado de Dante, reagindo dinamicamente ao ritmo de escalada e combate).

## Objetivo
Implementar `CatController`, `CatFollowModel` e `CatConfigSO` para que o gato acompanhe Dante por plataformas, salte para alcançá-lo e suba suavemente no ombro/mochila durante escaladas (`WallSlide` / `WallJump`) e combates intensos.

## Arquitetura
- `CatConfigSO` (`Data/Companion/CatConfig.asset`) — ScriptableObject com offsets de seguimento e ombro, velocidades, pulo, gravidade, distância de teleporte de segurança e tempos de transição/retenção em combate.
- `CatFollowModel` — modelo puro de simulação (`Follow`, `Perching`, `Perched`) testável em EditMode, com seguimento horizontal, pulo de plataforma, teleporte de segurança e interpolação *smoothstep* para o ombro.
- `CatController` — MonoBehaviour na layer dedicada `Companion` (colide apenas com `Ground` e `OneWayPlatform`, sem empurrar Dante), conectando `PlayerController`, `PlayerMeleeCombat` e `PlayerRangedCombat` ao `CatFollowModel`.
- `PlayerRangedCombat` — expõe `OnShotFired` para sinalizar atividade de combate ao gato.
- `CatTestRoomSetup` — menu `UnderTheRain/Setup/Create Cat Test Room` criando `CatConfig.asset`, `Prefabs/Companion/Cat.prefab` e `Scenes/Zones/CatTestRoom.unity`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `CatFollowTests` passam (além dos 35 testes anteriores).
3. No **Play Mode** (`CatTestRoom`), o gato acompanha Dante ao caminhar e pular pelas plataformas de diferentes alturas.
4. Ao iniciar `WallSlide` / `WallJump` no poço de escalada à direita, o gato realiza transição suave para o ombro de Dante e acompanha o `FacingDirection`, retornando ao modo `Follow` após sair da parede.
5. Ao atacar o `TrainingDummy` (melee ou disparo de bateria), o gato sobe no ombro durante o combate e desce após o intervalo de retenção.

## Estado
- [x] `CatConfigSO`, `CatFollowModel`, `CatController` e `PlayerRangedCombat.OnShotFired`.
- [x] `CatFollowTests` (4 testes EditMode).
- [x] `CatTestRoomSetup` (`UnderTheRain/Setup/Create Cat Test Room`).
- [x] Executar `UnderTheRain/Setup/Create Cat Test Room`, rodar EditMode tests e validar critérios 3–5 no Play Mode do Unity.
