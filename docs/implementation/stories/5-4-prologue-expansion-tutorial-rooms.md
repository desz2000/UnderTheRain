---
story_id: '5-4-prologue-expansion-tutorial-rooms'
epic: 'epic-5'
status: review
created: 2026-11-13
---

# Story 5.4: Expansão do Prólogo — Salas de Tutorial

Status: review

**Pilar reforçado:** Sensação de controle antes de qualquer gate (§5.9) e "Ato I ensina", ou seja, ensino pelo level design, sem muro de texto (§5.12, §5.14).

## Story

Como jogador começando *Under The Rain*,
quero atravessar um Prólogo que me ensine, sala a sala, a pular, lutar, atirar, usar a Kitty e descer plataformas,
para chegar ao Abrigo da resistência dominando os controles sem ter lido um tutorial.

## Acceptance Criteria

1. **Ordem das salas:** o Prólogo (`Prologue_Ruins`) tem 6 salas encadeadas **P1 → P2 → P3 → P4 → P5 → P6**, com saídas nos dois sentidos, e P6 leva ao Abrigo (`Act1_Sewers` / `sewers_initial_shelter` / `from_prologue`). Os IDs de P1 (`prologue_metro_platform`) e P5 (`prologue_hidden_lab`) **não mudam**.
2. **P1 Plataforma do Metrô:** mover e pular; **o duto provisório de Kitty sai de P1**. A saída à direita fica livre.
3. **P2 Vão** (`prologue_gap`): um vão curto (resolvido com pulo simples ou *coyote*) e um vão longo que **exige segurar o pulo**; cair no fosso **não causa dano** e dá para sair com um pulo. Uma placa curta explica que segurar o pulo faz ir mais longe. Há uma recompensa (peças) num patamar alto.
4. **P3 Entulho** (`prologue_debris`): **1 Operário** (lento, com mais vida que o batedor) que cai num combo de bastão; uma **célula de bateria** no caminho; e um alvo num patamar alto, fora do alcance do bastão no chão, ensinando o **disparo**. Placas curtas para bastão e disparo.
5. **P4 Duto de Kitty** (`prologue_duct`): uma porta de manutenção emperrada bloqueia a saída à direita; `E` perto do respiradouro envia Kitty, que abre a porta pelo outro lado. A porta **continua aberta** ao recarregar a cena na mesma sessão (`persistentId`).
6. **P5 Laboratório Escondido:** mantém robô batedor, interferência de Kitty, pista do laboratório e Terminal da Domus. A saída para os esgotos passa a ser a saída para **P6**.
7. **P6 Descida** (`prologue_descent`): sala vertical com **plataformas finas (one-way)** empilhadas; uma placa ensina **`S` + `Espaço`** para descer por elas. O caminho também pode ser **subido de volta** só com pulos normais (vindo do Abrigo, sem Wall Jump). A saída no fundo leva ao Abrigo.
8. **Volta do Abrigo:** sair do Abrigo pelo oeste chega em **P6, no fundo** (`prologue_descent` / `from_sewers`).
9. **Nenhum spawn sobre trigger de saída** (regra 6a do `project-context.md`), e nenhum vai e volta entre salas.
10. O projeto compila sem erros nem warnings; os testes EditMode e PlayMode passam, incluindo os novos testes desta história.

## Tasks / Subtasks

- [x] **T1 — Inimigos parametrizados** (AC: 4)
  - [x] Refatorar `EnemySetupUtility.CreateScoutRobot` para uma base `CreateRobot` com especificação (vida, velocidade e amplitude de patrulha, cor, escala, peças).
  - [x] Adicionar `CreateWorkerRobot` (Operário: vida 6, patrulha lenta 1,2, amplitude 2,5, 15 peças) e `CreateDormantScout` (alvo de disparo: vida 2, patrulha mínima).
- [x] **T2 — Reestruturar `PrologueRuinsSetup` com 6 salas** (AC: 1–8)
  - [x] Construir cada sala em coordenadas locais a partir de uma origem (P1 0, P2 34, P3 70, P4 104, P5 140, P6 176).
  - [x] P1 sem duto. P2 com vãos e fosso seguro. P3 com Operário, célula e alvo alto. P4 com duto + porta (`persistentId = prologue_maintenance_door`). P5 com o conteúdo atual reposicionado. P6 vertical com plataformas one-way.
  - [x] Ligar as saídas P1↔P2↔P3↔P4↔P5↔P6 e P6 → `Act1_Sewers`.
- [x] **T3 — `Act1SewersSetup`** (AC: 8): a saída oeste aponta para `prologue_descent` / `from_sewers`.
- [x] **T4 — Testes** (AC: 1, 3–5, 7–10)
  - [x] Atualizar `Story51SceneFlowTests` para as novas posições (o laboratório agora fica em x ≈ 140; a volta dos esgotos chega em P6).
  - [x] Novo `Story54PrologueTests` (PlayMode): as 6 salas existem; atravessar cada saída à direita leva à sala seguinte; P2 não tem hitbox de dano; P3 tem um Operário com vida 6; P6 tem plataformas one-way.
- [x] **T5 — Validação via CLI:** gerar as duas cenas, rodar EditMode e PlayMode.
- [x] **T6 — Documentação:** `level-map.md` (grafo atual), história, `sprint-status`.

## Dev Notes

### Estado atual (arquivos a alterar)

- **`Assets/_Project/Scripts/Editor/PrologueRuinsSetup.cs`**: hoje gera 2 salas em coordenadas de mundo. P1 tem o duto/porta (`CreateCatDuctShortcut`) e P5 fica centrada em x = 38, com a saída para os esgotos em x = 53. **Preservar:** criação de Dante/Kitty/input/fader/diálogo, `EnsureTerminalDialogueAsset` (que sempre regrava as falas), placas, decoração e terminal.
- **`Assets/_Project/Scripts/Editor/EnemySetupUtility.cs`**: `CreateScoutRobot` (vida 4, `PatrolInterferenceTarget` + `ScoutRobotEnemy` + hurtbox/hitbox + `Rigidbody2D` cinemático). **Preservar a assinatura** `CreateScoutRobot(name, parent, sprite, pos)` (usada pelo `Act1SewersSetup`).
- **`Assets/_Project/Scripts/Editor/Act1SewersSetup.cs`**: a saída oeste hoje aponta para `prologue_hidden_lab` / `from_sewers`.
- **`Assets/_Project/Scripts/Tests/PlayMode/Story51SceneFlowTests.cs`**: usa x = 40 (laboratório) e x = 51 (volta dos esgotos). Atualizar.

### Geometria e física (valores do asset `PlayerMovementConfig`)

- Pulo máximo de **3,2** de altura; corrida de **8**; gravidade de subida 38, queda ×1,6. Com o pulo segurado, o alcance horizontal é de ~5,9; tocando de leve, bem menos. Por isso: **vão curto ≈ 3,5**, **vão longo ≈ 5,0** (exige segurar), **fosso de 2,5** (dá para sair pulando).
- Plataforma one-way: layer `OneWayPlatform` + `BoxCollider2D.usedByEffector` + `PlatformEffector2D` (`useOneWay`, `surfaceArc = 160`), igual ao `MovementTestRoomSetup`. **Espaçamento vertical ≤ 2,8** para conseguir subir de volta.
- Descer: `PlayerController` atravessa a plataforma com **baixo (S) ou `Crouch` + pulo**. ⚠️ **Conflito conhecido:** `Crouch` e `Craft` usam a mesma tecla `C` no teclado. Por isso a placa ensina **`S` + `Espaço`**. Fica registrado para o Épico 6 (remapeamento).
- Dante tem 0,8 de largura. Os triggers de saída têm 1,4 de largura. Spawns ficam a ≥ 1,8 do centro do trigger.

### Padrões a seguir

- Cenas são geradas por script de Editor; **nunca editar `.unity` à mão**.
- `FindProperty` com o nome **exato** do campo serializado.
- Setups de cena chamam `EnemySetupUtility.EnsureCombatLayers()`.
- Objetos em salas inativas: use `RoomTransitionController.FindRoomContaining` / `SetActiveRoomImmediate` nos testes; `FindFirstObjectByType` não encontra objetos inativos.

### Project Structure Notes

- Sem arquivos de runtime novos; mudanças em `Scripts/Editor/` e `Scripts/Tests/PlayMode/`.
- A cena continua `Prologue_Ruins.unity` (nome mantido para não quebrar as ligações entre cenas).

### Project Context Rules

- Pilares: ensinar pelo level design (§5.12), gates legíveis (§5.3), sem muro de texto (§5.14). Placas com **uma linha**.
- Taxonomia de gates: o duto de P4 é um **salto de sala** (a habilidade de Kitty é a única rota de P4 para P5).
- Prólogo sem chuva (subsolo). A garoa só aparece na superfície.
- Valores finais vivem nos assets. A vida do Operário (6) e do alvo (2) ficam no `HealthComponent` configurado pelo setup.
- Código e commits em inglês; documentos em PT-BR.

### References

- [Source: docs/planning/epics.md#5.4 Expansão do Prólogo: Salas de Tutorial]
- [Source: docs/planning/gdd/gdd-Under-The-Rain-2026-11-13/level-map.md#3. Zona 0 — Prólogo (6 salas)]
- [Source: docs/planning/gdd/gdd-Under-The-Rain-2026-11-13/gdd.md#3.1, #4.1, #7.1]
- [Source: docs/project-context.md#6. Aprendizados Técnicos]
- [Source: docs/implementation/stories/5-1-sewers-rooms-initial-shelter-and-icarus.md]

### Previous Story Intelligence (5.1)

- Bugs que já apareceram: nome de `FindProperty` errado (`bodyRenderer`), spawn em cima de trigger (vai e volta), respawn em sala inativa e porta da Kitty fechando ao recarregar. Todos já corrigidos, e os padrões estão no `project-context.md`.
- O `Story51SceneFlowTests` (PlayMode) roda sobre a cena gerada. Esse tipo de teste pegou os bugs de fluxo que o EditMode não pega.
- A validação por CLI funciona com o Editor fechado: `-executeMethod` para os setups e `-runTests` para os testes.

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (`claude-opus-5-5`)

### Debug Log References

### Completion Notes List

- Ultimate context engine analysis completed - comprehensive developer guide created.
- `EnemySetupUtility`: `RobotSpec` + `CreateRobot`; presets `Scout` (vida 4), `Worker` (vida 6, patrulha 1,2, 15 peças) e `DormantScout` (vida 2, quase parado). A assinatura `CreateScoutRobot` foi mantida.
- `PrologueRuinsSetup`: 6 salas em coordenadas locais (origens x = 0/34/70/104/140/176); `LinkRooms` cria saídas e spawns dos dois lados (trigger a 0,8 da borda, spawn a 2,6). O duto saiu de P1 e foi para P4 (porta em x local 9, `persistentId` mantido). P6 é vertical (24×25) com 5 plataformas one-way espaçadas em 2,8, que começam sob a beirada da entrada.
- `Act1SewersSetup`: a saída oeste vai para `prologue_descent` / `from_sewers`.
- Validação via CLI (2026-11-13): cenas geradas sem erros nem warnings; **EditMode 48/48** e **PlayMode 13/13** (`Story51SceneFlowTests` atualizado + `Story54PrologueTests` com 8 testes: 6 salas na ordem; cadeias de saídas →/← sem vai e volta; P2 sem dano; P3 com Operário, célula e alvo alto; duto só em P4; plataformas de P6 escaláveis; volta dos esgotos chega no fundo de P6).
- Conflito conhecido (não resolvido aqui): `Crouch` e `Craft` usam a mesma tecla `C`. A placa de P6 ensina `S` + `Espaço`. Remapeamento no Épico 6.
- Falta: validação manual no Play Mode (sensação dos vãos, combate com o Operário, alcance do disparo e descida de P6).

### File List

- `Assets/_Project/Scripts/Editor/EnemySetupUtility.cs` (modificado)
- `Assets/_Project/Scripts/Editor/PrologueRuinsSetup.cs` (modificado)
- `Assets/_Project/Scripts/Editor/Act1SewersSetup.cs` (modificado)
- `Assets/_Project/Scripts/Tests/PlayMode/Story51SceneFlowTests.cs` (modificado)
- `Assets/_Project/Scripts/Tests/PlayMode/Story54PrologueTests.cs` (novo)
- `Assets/_Project/Scenes/Zones/Prologue_Ruins.unity` (regenerada)
- `Assets/_Project/Scenes/Zones/Act1_Sewers.unity` (regenerada)
