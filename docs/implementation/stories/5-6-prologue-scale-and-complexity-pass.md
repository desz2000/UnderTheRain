---
story_id: '5-6-prologue-scale-and-complexity-pass'
epic: 'epic-5'
status: review
created: 2026-11-13
---

# Story 5.6: Prólogo em Escala — Tamanho, Complexidade e Segredos

Status: review

**Pilar reforçado:** Loop central e "eu quero voltar ali" (§5.1), descoberta e segredos (§5.8), "Ato I ensina" sem ser fácil demais (§5.12) e abrigos como pontos de ritmo (§5.6).

## Story

Como jogador começando *Under The Rain*,
quero um Prólogo com salas grandes, em camadas, com rotas opcionais, segredos sugeridos pelo cenário e combates que usam o terreno,
para sentir desde o início um metroidvania de verdade, em que explorar compensa e cada mecânica é ensinada, testada e combinada.

## Acceptance Criteria

1. P1–P6 têm pelo menos 2 telas cada (P1 48×16, P2 56×16, P3 60×18, P4 44×20, P5 64×22, P6 28×44), com dois níveis jogáveis por sala.
2. Cada sala de P1 a P6 (exceto a calma P4) tem um segredo sugerido pelo cenário (`SecretWallReveal`), e eles continuam revelados ao recarregar a cena.
3. A arena de P5 (2 Batedores + 1 Operário, dormentes até Dante cair na câmara) tranca a saída até os 3 robôs caírem. Morrer reabre as travas.
4. O gerador de emergência de P3 (ponto de descanso) vira o ponto de respawn.
5. O Túnel de Serviço só abre por P4 (painel de Kitty) e leva de volta a P1. P1 não tem entrada para ele.
6. A chaminé de P4 é visível e não pode ser subida sem Wall Jump. No alto está o primeiro rabisco do Pai.
7. O Arquivo (P5a, opcional, aberto por um painel de Kitty) traz os registros do protótipo e um upgrade de +25 de bateria, que persiste entre cenas na sessão.
8. P2 continua sem dano, P6 continua escalável sem Wall Jump e nenhum spawn fica sobre um trigger.
9. Compila sem erros nem warnings; os testes EditMode e PlayMode passam.

## Tasks / Subtasks

- [x] **T1 — Runtime:**
  - [x] `SecretWallReveal` (fade + `persistentId`);
  - [x] `ArenaEncounter` (travas, robôs registrados, abortar na morte, `persistentId`);
  - [x] robô dormente (`PatrolInterferenceTarget.startDormant`/`wakeRadius`/`Wake()`; acorda por dano no `ScoutRobotEnemy`; sem dano de contato enquanto dorme);
  - [x] `UpgradePickup` + `PlayerUpgradeSession` + `PlayerUpgradeApplier`;
  - [x] `HealthModel.IncreaseMax` e `BatteryModel.IncreaseMax`;
  - [x] `NarrativeState.FlagFatherSketchFound`.
- [x] **T2 — `EnemySetupUtility`:** `RobotSpec.startDormant`/`wakeRadius` + `AsDormant(radius)`; o `DormantScout` nasce dormente.
- [x] **T3 — `PrologueRuinsSetup` reescrito:**
  - [x] P1–P6 + P5a + Túnel em coordenadas locais;
  - [x] helpers `Rect`/`DecorRect`/`OneWay`/`WallWithDoors`/`LinkDoor`/`KittyPoint`/`SecretWall`/`CreateArena`/`CreateTerminal`/`CreateRestSpot`/`CreateUpgrade`;
  - [x] portas em alturas diferentes (P1↔P2 baixa e alta, P2↔P3 baixa e alta, P4↔P5 alta, P5↔P5a alta).
- [x] **T4 — `Act1SewersSetup`:** `PlayerUpgradeApplier` no Dante.
- [x] **T5 — Testes:**
  - [x] `Story51SceneFlowTests` e `Story54PrologueTests` atualizados para o novo layout;
  - [x] novo `Story56PrologueScaleTests` (7);
  - [x] novo `UpgradeTests` (3, EditMode).
- [x] **T6 — Validação via CLI:** cenas geradas; **EditMode 52/52** e **PlayMode 22/22**; sem warnings.
- [ ] **T7 — Validação manual no Play Mode** (sensação, tempo de 20–32 min, legibilidade dos segredos).

## Dev Notes

- **Layout:** segue a [proposta de level design](../../planning/gdd/gdd-Under-The-Rain-2026-11-13/level-design-proposal-zone0-zone1.md) §3, com estas adaptações:
  - o Arquivo (P5a) fica **acima de P6** (origem 280, 12), para não sobrepor salas;
  - o **Túnel** corre por baixo de P1–P4 (origem 88, −13); entra-se pelo alçapão no chão de P4 (trigger horizontal `Hatch_ToTunnel`) e sai-se na escotilha de P1;
  - a chaminé de P4 fica **sobre o corredor de serviço** (paredes começam 2,5 acima do piso, então dá para passar por baixo);
  - a arena de P5 tranca com um **obturador** no buraco do deck e um **portão** para a sala do servidor;
  - o alvo de disparo de P3 está em **y 5,5** (fora do alcance do pulo + bastão).
- **Origens:** P1 0 · P2 56 · P3 118 · P4 174 · P5 232 · P6 282 (chão em y 0; P6 desce até −33,6).
- **Física respeitada** (asset `PlayerMovementConfig`):
  - degraus ≤ 2,8;
  - vãos da rota principal ≤ 5,0;
  - passarela opcional de P2 com 5,3 e 5,5;
  - portas com 2,5 de altura e triggers de 0,6 rentes à borda;
  - spawns a 2,6 da borda.
- **Lore aprovada (2026-11-13):** grafite (só imagem), placa da Linha 7, 3 registros do protótipo (texto neutro: teste interrompido, custos, projeto engavetado) e rabisco do Pai ("Essa letra... é do meu pai. Ele esteve aqui.").
- **Economia:** ~70 peças em pickups no Prólogo + drops dos robôs; mochila com 50. As recompensas foram variadas (6 células de bateria, upgrade e lore).

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (`claude-opus-5-5`)

### Completion Notes List

- Todas as decisões do usuário de 2026-11-13 foram implementadas: Prólogo inteiro, descanso em P3 + Túnel, pacote de código pequeno (o médio fica na 5.7).
- Os textos dos registros do protótipo e do rabisco do Pai ficam em placas e em `Data/Narrative/FatherSketch*.asset`; dá para ajustar sem mexer em código.
- **Pontos a observar no playtest:**
  - vão de 5,0 em P2;
  - a ponte de P3 com knockback;
  - a arena de P5 com bateria cheia ou vazia;
  - se o grafite e as luzes denunciam os segredos o suficiente.

### File List

- `Assets/_Project/Scripts/Runtime/World/SecretWallReveal.cs` (novo)
- `Assets/_Project/Scripts/Runtime/World/ArenaEncounter.cs` (novo)
- `Assets/_Project/Scripts/Runtime/World/UpgradePickup.cs` (novo)
- `Assets/_Project/Scripts/Runtime/Player/PlayerUpgradeSession.cs` (novo)
- `Assets/_Project/Scripts/Runtime/Enemies/PatrolInterferenceTarget.cs`, `ScoutRobotEnemy.cs` (modificados)
- `Assets/_Project/Scripts/Runtime/Player/BatteryComponent.cs`, `Combat/HealthComponent.cs`, `Narrative/NarrativeState.cs` (modificados)
- `Assets/_Project/Scripts/Editor/PrologueRuinsSetup.cs` (reescrito), `EnemySetupUtility.cs`, `Act1SewersSetup.cs` (modificados)
- `Assets/_Project/Scripts/Tests/PlayMode/Story56PrologueScaleTests.cs` (novo), `Story54PrologueTests.cs`, `Story51SceneFlowTests.cs` (modificados)
- `Assets/_Project/Scripts/Tests/EditMode/UpgradeTests.cs` (novo)
- `Assets/_Project/Data/Narrative/FatherSketch01.asset`, `FatherSketchDialogue.asset` (novos)
- `Assets/_Project/Scenes/Zones/Prologue_Ruins.unity`, `Act1_Sewers.unity` (regeradas)
