---
title: 'Arquitetura de Jogo: Under The Rain'
status: complete
engine: 'Unity 6000.3.1f1 (2D URP)'
created: 2026-11-13
updated: 2026-11-14
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

---

## 10. Renderização 2,5D — 🔜 *(proposta; aguarda aprovação do usuário e validação no spike 5.10)*

> Fonte de verdade da arte: [art-bible.md](../art/art-bible.md). Esta seção diz **como** o projeto entrega o que a bíblia pede.
>
> **Como as escolhas foram confirmadas:** o manual do URP saiu do pacote e foi para `docs.unity3d.com`, que está bloqueado no ambiente em que esta seção foi escrita. A confirmação foi feita no **código-fonte do pacote `com.unity.render-pipelines.universal` 17.3.0** (repositório `Unity-Technologies/Graphics`, branch `6000.3/staging`), que é a versão instalada (`Packages/manifest.json`). Cada linha das tabelas diz o que foi **confirmado na fonte** e o que fica **a validar no spike**.

### 10.1 Convivência dos dois renderers

| Decisão | Detalhe | Confirmação |
|---|---|---|
| Um único URP Asset (`Assets/Settings/UniversalRP.asset`) com dois renderers | Índice 0 = `Renderer2D` (padrão, não muda). Índice 1 = `Renderer25D.asset` (novo, `UniversalRendererData`) | Fonte: `UniversalRenderPipelineAsset.m_RendererDataList` é um array de `ScriptableRendererData`; `m_DefaultRendererIndex` escolhe o padrão |
| A câmera escolhe o renderer | Só a câmera do Prólogo chama `UniversalAdditionalCameraData.SetRenderer(1)`. As outras cenas ficam com o índice −1 (= padrão, 2D) e não são tocadas | Fonte: `SetRenderer(int index)`; se o índice não existir, o URP avisa no console e volta ao renderer padrão |
| Mudanças no asset compartilhado | Ligar sombras de luzes adicionais (hoje `m_AdditionalLightShadowsSupported: 0`). HDR já está ligado. O renderer 2D ignora luzes 3D, então as cenas 2D não mudam de aparência | A validar no spike: abrir `Act1_Sewers` e as salas de teste e comparar com capturas de antes |
| Pós-processamento | `Volume` global **dentro da cena do Prólogo** (perfil `Assets/_Project/Settings/PostFX/Prologue_PostFX.asset`). Só a câmera do Prólogo liga `renderPostProcessing`. Cenas sem Volume não mudam | — |
| Salas do Prólogo ainda em graybox (P2–P6 e o resto da P1) | Continuam com os sprites do `ZoneArtKit`. No renderer 3D, o `Sprite-Lit-Default` cai no passe `UniversalForward` (sem luz, transparente): ficam com cara de graybox, sem quebrar | Fonte: `Sprite-Lit-Default.shader` tem os passes `Universal2D`, `NormalsRendering` e `UniversalForward`. Aparência a validar no spike |

Todas as alterações de asset (URP Asset, renderer, perfil de Volume, materiais) são feitas por script de Editor, nunca editando YAML.

### 10.2 Renderer 2,5D (`Renderer25D.asset`)

| Item | Escolha | Confirmação |
|---|---|---|
| Caminho de renderização | **Forward+** | Fonte: `RenderingMode { Forward, ForwardPlus, Deferred, DeferredPlus }`. Forward+ tira o limite de luzes por objeto do Forward; com 3 a 5 luzes por tela, mais clarão de disparo e a luz de Kitty, evita luz "pulando" de um objeto para outro |
| Sombras | Luz-chave (Spot) com sombra suave; preenchimento e contraste sem sombra | Fonte: `AdditionalLightsShadowCasterPass` atende Spot e Point. Point gasta até 6 fatias do atlas; por isso a luz-chave é Spot |
| Oclusão de ambiente | Renderer Feature **Screen Space Ambient Occlusion**, fonte `DepthNormals` | Fonte: `ScreenSpaceAmbientOcclusion.cs` (`DepthSource.DepthNormals` é o padrão). Exige passe `DepthNormals` no shader de sprite (§10.4) |
| MSAA | Desligado (borda de pixel art fica dura por recorte de alpha) | — |

### 10.3 Câmera

- **Perspectiva, FOV vertical de 25°**, a **25,37 unidades** do plano de jogo (`z = −25,37`). A distância sai da fórmula `d = 5,625 / tan(FOV/2)`, que enquadra exatamente **20 × 11,25** em Z = 0 numa tela 16:9. Em 1080p isso dá 96 px por unidade = ×3 a 32 texels por unidade (×4 em 1440p, ×6 em 4K).
- **`RoomCamera2D` continua sendo a única câmera de sala.** Ela ganha uma função pura `HalfExtentsAtPlayPlane` que devolve a meia-largura e a meia-altura visíveis em Z = 0: `orthographicSize` na câmera ortográfica; `|z| · tan(FOV/2)` na de perspectiva. O `ClampCameraPosition` não muda. Consequência: as cenas 2D e os testes atuais (`RoomCamera2DTests`, `RoomTransitionTests`, que criam câmera ortográfica) continuam passando **sem alteração**.
- **Encaixe de pixel:** a posição final da câmera é arredondada para a grade de pixel de tela no plano de jogo, para os sprites não cintilarem enquanto a câmera anda. A validar no spike (se der tranco perceptível, cai o arredondamento e entra amostragem suavizada no shader).
- `CameraShake2D` (deslocamento em X/Y) e os prompts em IMGUI (`WorldToScreenPoint`) funcionam em perspectiva sem mudança.
- `ParallaxLayer` não é usado em sala migrada (o parallax vem da perspectiva). Continua existindo, com seus testes, para as cenas 2D.
- **Único efeito visível na jogabilidade:** no Prólogo a área visível passa de 19,2 × 10,8 para 20 × 11,25 (+4%), como a bíblia §4 manda.
- A arte de fundo precisa sobrar além dos limites da sala: em Z = +20 a câmera enxerga ~36 unidades de largura.

### 10.4 Material de sprite iluminado (`UnderTheRain/SpriteLit25D`)

O `Sprite-Lit-Default` não serve: no renderer 3D ele não recebe luz, não projeta sombra e não escreve profundidade (ver §10.1). Entra um shader próprio, escrito em HLSL/ShaderLab sobre a biblioteca do URP (arquivo de texto, versionável, sem depender do editor de Shader Graph).

| Característica | Motivo |
|---|---|
| **Opaco com recorte de alpha** (`clip`), sem mistura | Pixel art não tem semitransparência. Opaco escreve profundidade, e é disso que dependem profundidade de campo, SSAO, névoa e sombra |
| Passes `UniversalForward`, `ShadowCaster`, `DepthOnly`, `DepthNormals` | Os mesmos passes do `Lit.shader` do URP 17.3 (fonte). Dão luz, sombra projetada, profundidade e normais para o SSAO |
| Duas faces (`Cull Off`), normal corrigida quando o sprite está espelhado | `SpriteRenderer.flipX` inverte a geometria |
| Normal map pela textura secundária do sprite (`_NormalMap`) + termo de contorno (rim) | Bíblia §6 e §9: volume e contorno de luz nos personagens |
| Emissão por máscara (`_EmissionMap`, cor HDR) | Olhos de Kitty e sensores dos robôs acendem o bloom |

- Usado direto no `SpriteRenderer` (que entrega a textura em `_MainTex`). `shadowCastingMode` e `receiveShadows` são ligados por script, porque o Inspector do `SpriteRenderer` não mostra esses campos. **A validar no spike.** Plano B: um quad com `MeshRenderer` e um componente pequeno que troca o quadro da animação.
- **Ordem de desenho passa a ser por profundidade**, não por sorting layer. Convenção de Z dentro do plano de jogo (a física 2D ignora Z): props de cenário `+0,05`, inimigos `−0,02`, Dante `−0,05`, Kitty `−0,08`, efeitos `−0,10`.
- Efeitos com transparência de verdade (poeira, faísca, feixe de luz) usam os shaders de partícula do URP na fila transparente.

### 10.5 Cenário 3D e prefab de arte por sala

- **Geometria:** peças modulares simples (caixas, planos, perfis) com UV a 32 texels por unidade, material `Universal Render Pipeline/Lit` com textura Point, sem compressão. Malhas geradas por script de Editor e gravadas como asset em `Assets/_Project/Art/Kits/Prologue/`.
- **Um prefab por sala:** `Assets/_Project/Art/Rooms/Prologue/Art_<roomId>.prefab` (por exemplo `Art_prologue_metro_platform.prefab`), com os grupos `Geometry/`, `Props/`, `Lights/`, `FX/`.
- **O script de setup só instancia.** `PrologueRuinsSetup` procura o prefab pelo `roomId`; se existir, instancia como filho da sala (`PrefabUtility.InstantiatePrefab`) e **não** aplica a pele do `ZoneArtKit` nos blocos cobertos; se não existir, a sala fica em graybox. Regerar a cena nunca escreve no prefab.
- **O prefab é montado por um comando separado** (`UnderTheRain > Art > Build Art Prefab…`), que só cria o prefab se ele ainda não existir ou se for chamado de propósito com "recriar". Ajuste fino depois disso é feito no Editor do Unity.
- **Arte não tem lógica:** o prefab de arte não pode conter `Collider2D`, `Rigidbody2D` nem scripts de jogo (só `LightFlicker` e afins). Um teste garante isso.
- Colisores, triggers, spawns e inimigos continuam criados pelo setup, em Z = 0, exatamente como hoje.

### 10.6 Luz, atmosfera e pós-processamento

| Elemento | Como | Confirmação |
|---|---|---|
| Ambiente | `RenderSettings` da cena: ambiente plano, muito baixo, na cor de sombra da zona | — |
| Luzes práticas | `Light` Spot/Point em tempo real dentro do prefab de arte, sempre junto de uma fonte visível | — |
| Luz animada | Componente `LightFlicker` (pisca, oscila, balança) | — |
| Emissivos | Emissão HDR acima de 1 + Bloom | Fonte: `Bloom.cs` |
| Névoa de profundidade | Névoa da cena (`RenderSettings.fog`), na cor de sombra da zona | A validar no spike com os shaders usados |
| **Luz volumétrica** | **O URP 17.3 não tem névoa nem luz volumétrica** (o changelog só cita "volumetric" para `Light2D`, do renderer 2D). Alternativa: malha de feixe (cone ou plano) com shader aditivo, esmaecimento por profundidade e textura de poeira rolando, **sempre presa a uma luz real**, mais partículas de poeira | Ver §10.8, item 1 |
| Profundidade de campo | **Bokeh**, foco na distância do plano de jogo | Fonte: `DepthOfFieldMode.Gaussian` é "far blur only" (só desfoca o fundo). Para desfocar também o primeiro plano, como a bíblia §7 pede, só o Bokeh serve; ele custa mais (§10.8, item 2) |
| Tonemapping | Neutral ou ACES, decidido no beauty corner | Fonte: `TonemappingMode { None, Neutral, ACES }` |
| Demais efeitos | Color Adjustments, Color Lookup, Vignette, Film Grain; Chromatic Aberration só em eventos | Fonte: `Runtime/Overrides/*.cs` |

### 10.7 Testes

| Teste | Situação |
|---|---|
| `RoomCamera2DTests`, `RoomTransitionTests`, `ParallaxLayerTests` | Não mudam (§10.3) |
| `Story56PrologueScaleTests` (linha que lê o alpha do `SpriteRenderer` de `TicketBooth_Front` na P1) | O objeto lógico continua existindo; se a arte dele migrar para o prefab, o teste é adaptado e a mudança explicada na história |
| Novos, EditMode | `HalfExtentsAtPlayPlane` em ortográfica e em perspectiva (20 × 11,25 a 25° e 25,37 de distância) |
| Novos, PlayMode | Câmera do Prólogo em perspectiva enquadrando 20 × 11,25; prefab de arte sem colisor nem script de jogo; **retrato dos colisores da P1** (posição e tamanho) idêntico ao de antes da migração |

### 10.8 Pontos em que a bíblia não é viável como está escrita

1. **Luz volumétrica (bíblia §6).** Não existe no URP 17.3. A alternativa (malha de feixe aditiva presa a uma luz real) não ilumina nada: quem ilumina é a luz. Mesmo assim ela é uma forma semitransparente, e a regra "não simular luz com sprite semitransparente" precisa de uma exceção explícita para o **feixe visível de uma luz real**. A outra saída é escrever um efeito volumétrico próprio (raymarching), mais fiel, porém com custo alto de tempo e de desempenho; não recomendado para o MVP.
2. **Desfoque do primeiro plano (bíblia §7).** Só com DoF Bokeh. Se o custo estourar os 60 FPS, o plano B é Gaussian (desfoca só o fundo) com o primeiro plano em silhueta nítida.
3. **Pixels uniformes em todas as camadas (bíblia §13).** Em perspectiva, uma textura a 32 texels por unidade em Z = +10 aparece com pixels menores e em escala não inteira. Isso é inerente ao 2,5D (acontece em *Replaced*). A regra passa a valer para o **plano de jogo**; nas camadas fundas, névoa e desfoque escondem a diferença. Resoluções fora de 1080p/1440p/4K (escala não inteira) ficam para depois do MVP.
