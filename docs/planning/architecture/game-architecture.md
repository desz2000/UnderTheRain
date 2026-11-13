---
title: 'Arquitetura de Jogo: Under The Rain'
status: complete
engine: 'Unity 6000.3.1f1 (2D URP)'
created: 2026-11-13
updated: 2026-11-13
---

# Arquitetura de Jogo — Under The Rain (Unity 2D URP)

> Documento técnico para orientar os Épicos 1 a 8 de forma consistente. Segue as convenções de [project-context.md](../../project-context.md): nomes em inglês no código/arquivos, lógica separada da apresentação, dados em ScriptableObjects e sem edição manual de `.unity`/`.prefab`.

---

## 1. Estrutura de Pastas (`Assets/_Project/`)

```text
Assets/
  _Project/
    Art/
      Sprites/          # Pixel art (Dante, Cat, Enemies, Bosses, Props, UI)
      Tilesets/         # Tilemaps por zona
      Animations/       # AnimatorControllers e AnimationClips
    Audio/
      Music/
      SFX/
      Mixers/
    Data/               # Instâncias de ScriptableObjects (.asset)
      Weapons/
      Abilities/
      Enemies/
      Items/
      Narrative/
      World/
    Prefabs/
      Player/
      Companion/
      Enemies/
      Environment/      # Gates, Abrigos, Teletransporte, Mochila caída
      UI/
    Scenes/
      Core.unity        # Cena persistente (Player, Cat, Camera, HUD, Save)
      Zones/            # Cenas/salas por macrozona (Sewers, Surface, HighCity, Lab, Tower)
    Scripts/
      Core/             # Bootstrap, EventChannels, GameState, SaveSystem
      Player/           # StateMachine, Movement, Combat, Inventory, Battery
      Companion/        # CatController, CatInterference, CatExploration
      Enemies/          # EnemyAI, CollectiveMindModifier, MisanthropeAI, Bosses
      World/            # RoomTransition, Gates, Shelter, TeleporterMachine, DroppedPack
      UI/               # uGUI HUD, InventoryMenu, MapView, ShelterUI, InputGlyphs
      Tests/
        EditMode/       # Lógica pura (economia, colapso IA, save serialization)
        PlayMode/       # Critérios de aceite em Play Mode
    Settings/           # Configurações específicas do projeto
```

---

## 2. Dados em ScriptableObjects (Zero Números Mágicos)

Toda configuração balanceável vive em `ScriptableObject`s em `Assets/_Project/Data/`:
- **`PlayerMovementConfigSO`**: velocidades, aceleração (`~0.08s`), desaceleração (`~0.06s`), parâmetros de pulo variável, *coyote time* (`~0.10s`), *input buffer* (`~0.12s`), parâmetros de *Wall Slide/Jump*, *Dash*, *Double Jump* e *Grapple*.
- **`WeaponDataSO`**: estágio (`SimpleBaton`, `ElectrifiedBaton`, `HighFreqBlade`), dano, alcance, janela de combo, *hitstop* (`~0.04–0.06s`), custo/efeito de acoplamento.
- **`EnemyDataSO`**: vida, velocidade, tipo de facção (`CollectiveMachine`, `MisanthropeHuman`, `OfflinePrototype`), `isAffectedByCat` (`true` apenas para `CollectiveMachine`), multiplicadores de agressividade por estágio do **Colapso da IA**.
- **`ItemDataSO` / `DocumentDataSO`**: itens de acoplamento, acessórios, textos e registros de áudio do Pai / Domus Corp.
- **`RoomDataSO` / `ZoneDataSO`**: ID da sala, zona macro, conexões, estado de revelação no **Mapa Criptografado** e gates presentes.

---

## 3. Máquina de Estados do Jogador (`PlayerStateMachine`)

Separa **Física/Movimento**, **Combate** e **Apresentação (Animator/VFX/Audio)**:
- **Estados de Locomoção:** `Idle`, `Run`, `JumpAscend`, `Fall`, `WallSlide`, `WallJump`, `Dash`, `Grapple`, `Hurt`, `Dead`.
- **Sub-camada de Ação / Combate:** `MeleeAttack` (combo 1–3), `BatteryShot`, `HealMedkit`, `CommandCat`, `Interact`.
- **Invariantes:**
  - `Dead` bloqueia input, gera `DroppedPack` na posição exata da queda, toca fade-out e chama `RespawnAtLastShelter()` no abrigo salvo.
  - Desbloqueios de habilidade (ex.: `WallJump`, `Dash`) são flags persistidas no estado do jogador e checadas nas transições de estado.

---

## 4. Sistema de Combate, Bateria e Mochila de Peças

- **Detecção:** `Hitbox` / `Hurtbox` em camadas físicas dedicadas (`PlayerHitbox`, `EnemyHurtbox`, `EnemyHitbox`, `PlayerHurtbox`), usando interfaces `IDamageable` com `DamageContext` (valor, direção de *knockback*, tipo elétrico/físico, *hitstop*).
- **Recursos Desacoplados:**
  - `HealthComponent`: células discretas de vida + invulnerabilidade pós-dano (`~0.8s`).
  - `BatteryComponent`: carga para disparos e interações de exploração (recarrega 100% em abrigos e parcialmente com células dropadas).
  - `MedkitComponent`: cargas fixas (inicia em `3`), recarregáveis em abrigos ou fabricáveis com peças.
  - `PartsPackComponent`: armazena `currentParts` e `maxCapacity` (ampliável por NPCs de abrigo). Ao morrer, `currentParts` é transferido para uma instância de `DroppedPack` na posição da morte.

---

## 5. IA dos Robôs, Mente Coletiva e Gato Companheiro

- **Gato (`CatController`):**
  - Segue Dante com *path/platform follow* simples e sobe no ombro durante escaladas/combate rápido.
  - **Comando Ativo (`CatInterference`):** envia pulso/avanço curto com cooldown. Procura alvos com `ICatAffectable`:
    - Robôs da colmeia (`CollectiveMachine`) entram em estado `Hesitating` (`~1.0–1.5s`, ícone/faísca diegética + cooldown por inimigo).
    - **Humanos Misantropos** e **Protótipos offline** (`isAffectedByCat == false`) ignoram o pulso.
  - **Exploração:** `CatExploration` interage com dutos estreitos e terminais marcados.
- **Colapso Gradual da IA (`CollectiveCollapseManager`):**
  - Mantém o nível global de colapso (`0`, `1`, `2`, `3` — escalando conforme Data Centers e bosses caem).
  - Inimigos consultam o nível atual ao spawnar/atualizar: estágios mais altos aumentam velocidade, cadência de ataque e agressividade (caos no mapa, estilo *Infected Crossroads*), além de abrir barreiras ligadas a cada Data Center.

---

## 6. Cenas, Salas e Transições (Estilo *Hollow Knight*)

- **Cena Persistente (`Core.unity`):** contém Dante, Gato, `CinemachineCamera`, `PixelPerfectCamera`, Canvas de HUD/Menus, `AudioManager` e `GameSessionManager`.
- **Salas por Macrozona:** cada macrozona é composta por múltiplas **salas pequenas** interconectadas.
- **Transição entre Salas:** ao cruzar um `RoomExitTrigger`, o `RoomTransitionController` executa um **fade-to-black rápido** (~0.25s), posiciona Dante no `SpawnPoint` correspondente, ajusta os limites da câmera (`CinemachineConfiner2D` para a nova sala) e faz fade-in — **sem tela de loading**.
  - *Nota de implementação:* salas da mesma macrozona podem ficar em uma cena de zona com ativação/desativação de setores ou carregamento aditivo transparente em background, mantendo a transição visual idêntica (fade-to-black instantâneo).

---

## 7. Input System e UI (`uGUI`)

- **Input System:** baseado em `Assets/InputSystem_Actions.inputactions` com Action Maps `Gameplay` e `UI`.
- **Detecção de Dispositivo:** `InputDeviceTracker` monitora o último dispositivo ativo (`KeyboardMouse` vs. `Gamepad`) e dispara evento para os `PromptGlyphView` trocarem ícones automaticamente.
- **UI em `uGUI`:** Canvas com `CanvasScaler` (Scale With Screen Size 1920×1080, pixel-snapped), sem caixas na HUD de gameplay; telas separadas para `InventoryMenu` (3 abas: Mapa Criptografado, Arsenal, Registros), `ShelterNpcUI` e `TeleporterMachineUI`.

---

## 8. Save / Load (Slot Único Automático no MVP)

- **Formato:** JSON local via `JsonUtility` / `Newtonsoft` em `Application.persistentDataPath/save_slot0.json`.
- **Quando salva:** automaticamente ao descansar em um abrigo, concluir compra/upgrade/teletransporte ou sair para o menu.
- **Dados persistidos (`SaveData`):**
  - Último abrigo (`lastShelterId`, `lastRoomId`) e abrigos descobertos (para teletransporte no Ato II+);
  - Vida máxima, kits, bateria máxima, capacidade da mochila, peças atuais;
  - Posição da `DroppedPack` caso Dante tenha morrido e ainda não a recuperou (`hasDroppedPack`, `droppedPackRoomId`, `droppedPackPosition`, `droppedPackAmount`);
  - Estágio da arma, acoplamentos, habilidades desbloqueadas e acessórios;
  - Salas reveladas no mapa, gates abertos, documentos/áudios coletados, bosses/data centers derrotados e nível de Colapso da IA.
