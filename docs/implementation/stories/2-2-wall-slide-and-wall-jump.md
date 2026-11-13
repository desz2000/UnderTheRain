---
story_id: '2-2-wall-slide-and-wall-jump'
epic: 'epic-2'
status: done
created: 2026-11-13
---

# História 2.2 — Wall Slide & Wall Jump *(1ª Habilidade Desbloqueável)*

**Pilar reforçado:** Bloqueio por habilidade (não por nível) e progressão por habilidades — `WallJump` é a primeira habilidade desbloqueável que abre gates verticais no Vertical Slice.

## Objetivo
Implementar os estados `WallSlide` (atrito controlado na parede) e `WallJump` (impulso diagonal afastando da parede), controlados pela flag `wallJump` no estado de habilidades de Dante (`PlayerAbilitySet`).

## Arquitetura
- `PlayerAbilitySet` (`[Serializable]`) — flags de habilidades desbloqueadas no estado do jogador (inicia com `wallJump = false`).
- `PlayerMovementConfigSO` — adiciona parâmetros de tuning: `wallSlideMaxSpeed`, `wallJumpVelocity`, `wallJumpLockTime` e `wallCoyoteTime`.
- `PlayerLocomotionState` — adiciona `WallSlide` e `WallJump`.
- `PlayerMotor` — lógica pura para `WallSlide`, `WallJump`, `wallCoyoteTimer` e `wallJumpLockTimer` (testável em EditMode).
- `PlayerController` — detecção de parede esquerda/direita apenas em superfícies sólidas (`groundMask & ~oneWayMask`) e integração com `PlayerAbilitySet`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `PlayerMotorTests` (incluindo os novos de `WallSlide` e `WallJump`) passam.
3. Na `MovementTestRoom`, com `abilities.wallJump` desmarcado (`false`), Dante não desliza nem salta em paredes e não consegue escalar o poço vertical.
4. Após habilitar `abilities.wallJump` (no Inspector ou via pickup de teste na sala), Dante desliza com velocidade controlada ao pressionar contra a parede em queda (`WallSlide`).
5. Pressionar Pulo durante o `WallSlide` (ou logo após soltar a parede dentro do `wallCoyoteTime`) impulsiona Dante para cima e para longe da parede (`WallJump`), permitindo escalar entre paredes paralelas até a plataforma alta.

## Estado
- [x] `PlayerAbilitySet`, estados `WallSlide`/`WallJump`, parâmetros no `PlayerMovementConfigSO` e detecção de parede sólida no `PlayerController`.
- [x] Poço vertical adicionado à `MovementTestRoom.unity` (`Shaft Wall Left`/`Right` + `High Reward Platform`).
- [x] Critérios 1–2: compila sem erros e 15/15 `PlayerMotorTests` passam no EditMode.
- [x] Critérios 3–5: validação manual no Play Mode aprovada (ajuste estilo Hollow Knight: `wallJumpVelocity = (6.5, 14)`, `wallJumpLockTime = 0.05s`).
