# Trabalho Adiado

## Deferred from: code review of 5-6-prologue-scale-and-complexity-pass (2026-11-13)

- Acordar por dano só existe em `ScoutRobotEnemy`. Um alvo `startDormant` sem essa classe nunca acorda ao levar dano. Hoje todos os robôs usam `ScoutRobotEnemy`; rever na 5.7 (novos inimigos).
- Não há validação de `persistentId` duplicado (`ArenaEncounter`, `SecretWallReveal`, `UpgradePickup`, `CatExplorationPoint`). Um id copiado limpa outra arena ou revela outro segredo. Candidato a teste de cena ou validador de Editor.
- As salas de teste (`CatTestRoomSetup`, `CombatTestRoomSetup`, `MovementTestRoomSetup`) criam Dante sem `PlayerUpgradeApplier`. Rever quando houver um helper `CreatePlayer` compartilhado.

## Deferred from: story 5-9-enemy-sight-chase-and-flying-drones (2026-11-14)

- O `gds-code-review` da 5.9 não foi executado: a história foi fechada pela validação do usuário no Play Mode. Rodar a revisão (percepção, perseguição e `FlyingDroneMotor`) antes da 5.7, que reaproveita o estado de telegrafia.
