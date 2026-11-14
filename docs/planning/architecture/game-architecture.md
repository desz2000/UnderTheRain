---
title: 'Arquitetura de Jogo: Under The Rain'
status: complete
engine: 'Unity 6000.3.1f1 (2D URP)'
created: 2026-11-13
updated: 2026-11-13
---

# Arquitetura de Jogo — Under The Rain (Unity 2D URP)

> Documento técnico para orientar os Épicos 1 a 8 de forma consistente. Segue as convenções de [project-context.md](../../project-context.md): nomes em inglês no código/arquivos, lógica separada da apresentação, dados em ScriptableObjects e sem edição manual de `.unity`/`.prefab` (cenas e prefabs são gerados por scripts de Editor).
>
> **Legenda:** ✅ = implementado (Épicos 1–5.1) · 🔜 = planejado (ainda não existe no código).
>
> **Valores numéricos:** os valores finais vivem nos assets (`Assets/_Project/Data/**`) e nos componentes. Os números citados aqui são do momento em que o documento foi escrito; em caso de divergência, vale o asset.

---

## 1. Estrutura de Pastas (`Assets/_Project/`)

```text
Assets/
  InputSystem_Actions.inputactions   # Action Maps Player e UI (+ fallbacks em runtime)
  _Project/
    Art/            Sprites/ (WhiteSquare.png placeholder), Tilesets/, Animations/
    Audio/          Music/, SFX/, Mixers/
    Data/           # Instâncias de ScriptableObjects (.asset)
      Abilities/    PlayerMovementConfig.asset
      Companion/    CatConfig.asset
      Narrative/    DomusSabotageLog.asset + *Dialogue.asset (terminal, Icarus)
      Weapons/      SimpleBaton.asset
      Enemies/ Items/ World/   # vazias por enquanto
    Prefabs/
      Player/       BatteryProjectile.prefab
      Companion/    Cat.prefab (Kitty, layer Companion)
      Environment/  DroppedPack.prefab, PartsPickup.prefab
      Enemies/ UI/  # vazias por enquanto
    Scenes/
      Core.unity    # cena base do Épico 1
      Zones/        Prologue_Ruins.unity, Act1_Sewers.unity (+ salas de teste: Movement/Combat/CatTestRoom)
    Scripts/
      Editor/       # *Setup.cs — geram cenas/assets via menu "UnderTheRain > Setup > ..."
                    # (ProjectSetupMenu, MovementTestRoomSetup, CombatTestRoomSetup, CatTestRoomSetup,
                    #  PrologueRuinsSetup, Act1SewersSetup, EnemySetupUtility)
      Runtime/      # assembly UnderTheRain.Runtime
        Core/       GameBootstrap; Input/ (GameInputReader, InputDeviceTracker, InputDeviceType)
        Player/     PlayerController, PlayerMotor, PlayerLocomotionState, PlayerMovementConfigSO,
                    PlayerAbilitySet, PlayerMeleeCombat, MeleeComboState, WeaponDataSO,
                    PlayerRangedCombat, BatteryComponent, MedkitComponent, PartsPackComponent,
                    PlayerDeathHandler
        Combat/     Hitbox, Hurtbox, DamageTypes (IDamageable, DamageContext), HealthComponent,
                    CombatFeedback, CameraShake2D, BatteryProjectile, TrainingDummy, DebugCombatHud
        Companion/  CatConfigSO, CatFollowModel, CatController, CatInterference(+Model), CatExploration
        Enemies/    EnemyFaction, ICatAffectable, PatrolInterferenceTarget, ScoutRobotEnemy
        Narrative/  DocumentDataSO, DialogueSequenceSO, NarrativeProgressModel, NarrativeState, DialogueRunner
        World/      RoomBounds, RoomCamera2D, RoomExitTrigger, RoomSpawnPoint, RoomTransitionController,
                    ScreenFader, ShelterRestSpot, BatteryChargeStation, BatteryCellPickup, PartsPickup,
                    DroppedPack, CatExplorationPoint, NarrativeTerminal, NarrativeGate, IcarusNpc,
                    TutorialSignpost, SewerRainAmbience
      Tests/
        EditMode/   # lógica pura (motor, combo, vida, bateria/kit, mochila, Kitty, narrativa, câmera, input)
        PlayMode/   # RoomTransitionTests
    Settings/
```

**Padrão "Model + Component":** a lógica testável fica em classes C# puras (`PlayerMotor`, `HealthModel`, `BatteryModel`, `MedkitModel`, `PartsPackModel`, `CatFollowModel`, `CatInterferenceModel`, `CatExplorationModel`, `MeleeComboState`, `NarrativeProgressModel`) cobertas por testes EditMode. O `MonoBehaviour` só faz a ponte com Unity (input, física, visual).

---

## 2. Dados em ScriptableObjects (Zero Números Mágicos)

| SO | Status | Asset | Conteúdo |
|---|---|---|---|
| `PlayerMovementConfigSO` | ✅ | `Data/Abilities/PlayerMovementConfig.asset` | corrida (máx. 8, acel. 0,08 s, desacel. 0,06 s), pulo (altura 3,2, corte ×0,4), *coyote* 0,04 s, *buffer* 0,12 s, *corner correction* 0,25, *drop-through* 0,25 s, Wall Slide (3,5) / Wall Jump ((6,5; 14), trava 0,05 s, *wall coyote* 0,08 s) |
| `WeaponDataSO` | ✅ | `Data/Weapons/SimpleBaton.asset` | estágio, dano 2, finalizador +1, 3 golpes, janela 0,35 s, *hitstop* 0,05 s (×1,4 no finalizador), knockback (6; 2) (×1,35), *screenshake* |
| `CatConfigSO` | ✅ | `Data/Companion/CatConfig.asset` | follow/ombro, *teleport* 11, interferência (raio 4,5, cooldown 3 s, hesitação 1,35 s, custo 50 de bateria) |
| `DocumentDataSO` | ✅ | `Data/Narrative/DomusSabotageLog.asset` | `documentId`, título, autor, corpo |
| `DialogueSequenceSO` | ✅ | `Data/Narrative/*Dialogue.asset` | lista de `DialogueLine` (falante, cor, texto) |
| `EnemyDataSO` | 🔜 | — | vida, velocidade, facção, multiplicadores por estágio do Colapso. *Hoje os valores do batedor estão em `EnemySetupUtility` + `HealthComponent`.* |
| `ItemDataSO` | 🔜 | — | acoplamentos, acessórios |
| `RoomDataSO` / `ZoneDataSO` | 🔜 | — | revelação no Mapa Criptografado, gates. *Hoje o ID da sala é `RoomBounds.RoomId`; o mapa de salas está em `docs/planning/gdd/.../level-map.md`.* |

Valores que ainda vivem em componentes (serializados): `HealthComponent` (vida 5, invulnerabilidade 0,5 s), `BatteryComponent` (100), `MedkitComponent` (3 cargas, cura 2), `PartsPackComponent` (capacidade 50, kit custa 15), `PlayerRangedCombat` (custo 5, dano 1, cadência 0,25 s), `BatteryCellPickup` (35), `RoomTransitionController` (fade 0,25 s). Constante: `PlayerController.DamageKnockbackLockTime = 0.16 s`.

---

## 3. Jogador

Não existe uma classe `PlayerStateMachine`. O estado é distribuído:

- ✅ **`PlayerMotor`** (C# puro) + **`PlayerLocomotionState`**: `Idle`, `Run`, `JumpAscend`, `Fall`, `WallSlide`, `WallJump`. 🔜 `Dash`, `Grapple`, `Hurt`, `Dead` como estados explícitos.
- ✅ **`PlayerController`**: lê input, detecta chão/teto/parede, aplica o motor ao `Rigidbody2D` (`CollisionDetectionMode2D.Continuous`), trava de knockback e **`SetControlLocked(bool)`** (usado por diálogos).
- ✅ **Ações** em componentes separados: `PlayerMeleeCombat` (combo 1–3), `PlayerRangedCombat`, `MedkitComponent` (Heal), `PartsPackComponent` (Craft), `CatInterference` (comando de Kitty), interações via `GameInputReader.OnInteractPressed`.
- ✅ **`PlayerAbilitySet`** (`[Serializable]` com `wallJump`): flags de habilidade checadas pelo motor. 🔜 persistência em save e demais habilidades.
- ✅ **Morte:** `PlayerDeathHandler` cria a `DroppedPack` na posição da queda, faz fade e respawna no `ShelterRestSpot` padrão.

---

## 4. Combate, Bateria e Mochila de Peças

- ✅ **Detecção:** `Hitbox` / `Hurtbox` nas layers `PlayerHitbox`, `PlayerHurtbox`, `EnemyHitbox`, `EnemyHurtbox`. A matriz de colisão só permite PlayerHitbox↔EnemyHurtbox e EnemyHitbox↔PlayerHurtbox (configurada por `CombatTestRoomSetup.ConfigureCombatCollisionMatrix`, chamada também pelos setups de zona via `EnemySetupUtility.EnsureCombatLayers`). `Hitbox.alwaysActive` serve para contato/hazards.
- ✅ **Recursos desacoplados:** `HealthComponent` (vida em células + invulnerabilidade), `BatteryComponent`, `MedkitComponent`, `PartsPackComponent` (ao morrer, `ExtractAllOnDeath` vai para a `DroppedPack`).
- ✅ **Abrigos:** `ShelterRestSpot` (descanso, respawn) e `BatteryChargeStation`.

---

## 5. Inimigos, Mente Coletiva e Kitty

- ✅ **Kitty (`CatController` + `CatFollowModel`):** segue Dante, sobe no ombro em WallSlide/WallJump, no combate ou em travessia alta, e se teleporta se ficar longe (> 11).
  - **Comando ativo (`CatInterference`, `R` / `RB`):** pulso que busca `ICatAffectable` num raio. Robôs `CollectiveMachine` hesitam; `MisanthropeHuman` ignora o pulso.
  - **Exploração (`CatExploration` + `CatExplorationPoint`):** com `Interact` perto de um ponto marcado, Kitty atravessa o duto e desativa a porta do outro lado.
- ✅ **Inimigos atuais:** `PatrolInterferenceTarget` (patrulha + hesitação) e `ScoutRobotEnemy` (vida 4, contato que fere — desligado durante a hesitação —, drop de 10 peças). Montados por `EnemySetupUtility.CreateScoutRobot`.
- 🔜 **IA por tipo** (Operário, Policial, Elite, Misantropo, bosses) e **`CollectiveCollapseManager`** (nível global 0–3 de colapso, inimigos mais agressivos e novas rotas).

---

## 6. Narrativa

- ✅ **`NarrativeProgressModel`** (C# puro): documentos coletados (únicos) e flags.
- ✅ **`NarrativeState`** (`MonoBehaviour`): envolve um modelo **estático de sessão**, compartilhado entre cenas (`Prologue_Ruins` ↔ `Act1_Sewers`) e zerado ao entrar no Play Mode. Flags atuais: `domus_evidence_found`, `icarus_confronted`, `sewer_exit_unlocked`. 🔜 persistência em save.
- ✅ **`DialogueRunner`**: executa `DialogueSequenceSO` com IMGUI temporário (a UI final virá no Épico 6), trava o controle do jogador e avança com `Interact`. Interagíveis devem checar **`IsBusy`** (e não `IsPlaying`) para que o mesmo `E` que fecha um diálogo não o reabra.
- ✅ **Mundo:** `NarrativeTerminal` (coleta documento + flag), `IcarusNpc` (diálogo conforme as flags), `NarrativeGate` (abre ao receber uma flag), `TutorialSignpost` (placas curtas).

---

## 7. Cenas, Salas e Transições (Estilo *Hollow Knight*)

- ✅ **Uma cena por zona**, com todas as salas dentro dela: cada sala é um `RoomBounds` (trigger + `RoomId` + `RoomSpawnPoint` filhos). Só a sala atual fica ativa.
  - `Prologue_Ruins` = Zona 0 (Prólogo). Salas atuais: `prologue_metro_platform`, `prologue_hidden_lab`.
  - `Act1_Sewers` = Zona 1 (Esgotos). Salas atuais: `sewers_initial_shelter`, `sewers_passage`.
  - Mapa completo de salas: [level-map.md](../gdd/gdd-Under-The-Rain-2026-11-13/level-map.md).
- ✅ **Cada cena de zona é autocontida** (câmera, `GameInputReader`, Dante, Kitty, `DialogueRunner`, fader), gerada por seu `*Setup.cs`. 🔜 Uma cena persistente (`Core.unity` com Dante, Kitty, HUD, save) com zonas carregadas de forma aditiva fica para quando houver HUD e save.
- ✅ **Câmera:** `RoomCamera2D` (ortográfica, segue Dante com *SmoothDamp* e se confina aos limites do `RoomBounds` ativo) + `CameraShake2D`. **Não** há Cinemachine nem `PixelPerfectCamera` no projeto.
- ✅ **Transição entre salas:** `RoomExitTrigger` → `RoomTransitionController.TransitionToRoom` → fade-to-black (0,25 s), troca de sala ativa, Dante no `RoomSpawnPoint` de destino, câmera reconfinada e fade-in, **sem tela de loading**.
- ✅ **Transição entre cenas:** `RoomExitTrigger.ConfigureScene(scene, roomId, spawnId)` → `TransitionToScene` guarda a entrada pendente (sala + spawn); o `RoomTransitionController` da nova cena a aplica no `Start`.

---

## 8. Input System e UI

- ✅ **Input System:** `Assets/InputSystem_Actions.inputactions` (Action Maps `Player` e `UI`). O `GameInputReader` instancia uma cópia em runtime e cria, num mapa separado (`_fallbackMap`), as ações que faltam no asset (`Shoot`, `Heal`, `Craft`, `CatCommand`) e o `Interact` do gamepad.
  - Atual: Interact `E` / `Y` / `Triângulo`; Attack `Mouse Esq.` / `Enter` / botão Oeste (*`J` pendente*); Shoot `Mouse Dir.` / `F` / `RT`; Heal `Q` / `LB`; Craft `C` / `D-Pad baixo`; CatCommand `R` / `RB`; Jump `Espaço` / `A`.
- ✅ **Detecção de dispositivo:** `InputDeviceTracker` (`KeyboardMouse` vs. `Gamepad`), usado pelos prompts do diálogo.
- 🔜 **UI em `uGUI`** (Épico 6): Canvas com `CanvasScaler` 1920×1080, HUD sem caixas, `InventoryMenu` (3 abas), `ShelterNpcUI`, `TeleporterMachineUI`, `PromptGlyphView`. Hoje existem só HUDs de debug (`DebugCombatHud`) e IMGUI temporário.

---

## 9. Save / Load (Slot Único Automático) — 🔜 Épico 8

- **Formato:** JSON local via `JsonUtility` / `Newtonsoft` em `Application.persistentDataPath/save_slot0.json`.
- **Quando salva:** automaticamente ao descansar em um abrigo, concluir compra/upgrade/teletransporte ou sair para o menu.
- **Dados persistidos (`SaveData`):**
  - Último abrigo (`lastShelterId`, `lastRoomId`) e abrigos descobertos (para teletransporte no Ato II+);
  - Vida máxima, kits, bateria máxima, capacidade da mochila, peças atuais;
  - Posição da `DroppedPack` caso Dante tenha morrido e ainda não a recuperou (`hasDroppedPack`, `droppedPackRoomId`, `droppedPackPosition`, `droppedPackAmount`);
  - Estágio da arma, acoplamentos, habilidades desbloqueadas (`PlayerAbilitySet`) e acessórios;
  - Salas reveladas no mapa, gates abertos, documentos/áudios coletados e flags narrativas (`NarrativeProgressModel`), bosses/data centers derrotados e nível de Colapso da IA.
