---
title: 'Backlog Inicial — Épicos e Histórias: Under The Rain'
status: complete
created: 2026-11-13
updated: 2026-11-13
sources:
  - 'docs/planning/gdd/gdd-Under-The-Rain-2026-11-13/gdd.md'
  - 'docs/planning/narrative/narrative-design.md'
  - 'docs/planning/ux/ux-Under-The-Rain-2026-11-13/DESIGN.md'
  - 'docs/planning/ux/ux-Under-The-Rain-2026-11-13/EXPERIENCE.md'
  - 'docs/planning/architecture/game-architecture.md'
  - 'docs/planning/gdd/gdd-Under-The-Rain-2026-11-13/level-map.md'
  - 'docs/planning/sprint-change-proposal-2026-11-13.md'
---

# Backlog Inicial — Under The Rain

> **Regra de Marco:** Os **Épicos 1 a 5** entregam o **Vertical Slice do Ato I** (primeiro marco jogável) antes de ampliar o escopo para os Épicos 6 a 8. Toda história possui critérios de aceite verificáveis no **Unity Play Mode** (ou EditMode Test Runner).
>
> **Valores numéricos:** os valores finais de tuning vivem nos assets (`Assets/_Project/Data/**`) e nos componentes. Números citados aqui são referência do momento; em caso de divergência, vale o asset.

---

## Épico 1 — Fundação do Projeto *(Vertical Slice)*

### 1.1 Estrutura de Pastas, Assembly Definitions e Cena Base
- **Objetivo:** Criar a hierarquia `Assets/_Project/` (`Art`, `Audio`, `Data`, `Prefabs`, `Scenes`, `Scripts`, `Settings`), Assembly Definitions (`UnderTheRain.Runtime`, `UnderTheRain.Editor`, `UnderTheRain.Tests.EditMode`, `UnderTheRain.Tests.PlayMode`) e a cena persistente `Core.unity`.
- **Aceite no Unity:** Abrir `Core.unity` e entrar no Play Mode sem erros ou warnings de compilação; rodar o Unity Test Runner com 1 teste de sanidade verde.

### 1.2 Input System e Detecção de Dispositivo
- **Objetivo:** Configurar `InputSystem_Actions.inputactions` (Action Maps `Gameplay` e `UI` para Teclado e Gamepad) e implementar `InputDeviceTracker` para alternar glifos automaticamente.
- **Aceite no Unity:** No Play Mode, pressionar teclas e botões de gamepad dispara ações mapeadas e alterna o evento de dispositivo ativo (`KeyboardMouse` ↔ `Gamepad`).

### 1.3 Transição de Salas com Fade-to-Black e Câmera 2D
- **Objetivo:** Implementar `RoomTransitionController`, `RoomExitTrigger`, `RoomSpawnPoint` e confinamento de câmera por sala com `RoomCamera2D` + `RoomBounds` (estilo *Hollow Knight*, sem tela de loading; sem Cinemachine).
- **Aceite no Unity:** No Play Mode, caminhar até a saída de uma sala executa fade-to-black rápido (~0,25s), posiciona Dante no `SpawnPoint` da sala vizinha e confina a câmera aos limites da nova sala.

---

## Épico 2 — Movimentação Metroidvania *(Vertical Slice)*

### 2.1 Locomoção Base e Pulo Responsivo (`PlayerMovementConfigSO`)
- **Objetivo:** Implementar `PlayerStateMachine` com corrida (`~0.08s` aceleração / `~0.06s` parada), pulo variável, *coyote time* (`~40ms`, ajustado em playtest), *input buffering* (`~120ms`), *corner correction* e *drop-through* em plataformas finas.
- **Aceite no Unity:** No Play Mode em uma sala de teste de plataformas, Dante para sem deslizar, varia a altura do salto pelo tempo de pressão, consegue pular logo após sair de uma borda (*coyote*) e enfileirar pulo antes de tocar o chão (*buffer*).

### 2.2 Wall Slide & Wall Jump *(1ª Habilidade Desbloqueável)*
- **Objetivo:** Implementar estados `WallSlide` (atrito controlado) e `WallJump`, controlados por flag de habilidade desbloqueada no estado do jogador.
- **Aceite no Unity:** Com `WallJump` bloqueado, Dante não escala poços verticais; após desbloquear a habilidade em teste, ele desliza na parede e salta entre paredes paralelas para alcançar plataformas altas.

---

## Épico 3 — Combate, Bateria e Economia de Peças *(Vertical Slice)*

### 3.1 Combate Corpo a Corpo (Bastão Simples) e Game Feel
- **Objetivo:** Implementar `WeaponDataSO`, combo de 2–3 golpes, `Hitbox`/`Hurtbox` (`IDamageable`), *hitstop* (`~40–60ms`), *screenshake* sutil e invulnerabilidade pós-dano (`0.5s`, em `HealthComponent`) com trava de knockback de `0.16s`.
- **Aceite no Unity:** No Play Mode, atacar um alvo de teste aplica dano, dispara *hitstop* perceptível e recuo; receber dano ativa invulnerabilidade temporária sem perder vida duas vezes no mesmo golpe.

### 3.2 Disparo à Base de Bateria e Kit Médico
- **Objetivo:** Implementar `BatteryComponent` (disparo à distância consumindo carga, recarga em abrigo e via células dropadas) e `MedkitComponent` (3 cargas iniciais de cura).
- **Aceite no Unity:** No Play Mode, disparar consome bateria até zerar (impedindo novos disparos até recarregar), usar Kit Médico restaura vida consumindo 1 carga, e descansar no abrigo restaura 100% de ambos.

### 3.3 Mochila de Peças, Morte e Recuperação (`DroppedPack`)
- **Objetivo:** Implementar `PartsPackComponent` (coleta de peças até `maxCapacity`), fabricação de kit médico usando peças, e morte com spawn de `DroppedPack` no local exato da queda + despertar no último abrigo.
- **Aceite no Unity:** No Play Mode, derrotar inimigo soma peças; ao morrer, Dante acorda no abrigo com `0` peças e uma `DroppedPack` aparece no local da morte; ao tocá-la, recupera as peças perdidas.

---

## Épico 4 — Kitty, a Companheira *(Vertical Slice)*

### 4.1 Seguir Dante e Subir no Ombro
- **Objetivo:** Implementar `CatController` com Kitty seguindo Dante por plataformas e subindo no ombro/mochila durante escaladas e combates intensos.
- **Aceite no Unity:** No Play Mode, Kitty acompanha Dante ao pular plataformas e realiza transição suave para o ombro ao iniciar `WallSlide`.

### 4.2 Comando Ativo de Interferência (`ICatAffectable`)
- **Objetivo:** Implementar `CatInterference` (comando de Kitty, `R` / `RB`) com cooldown: robôs da colmeia (`CollectiveMachine`) hesitam/travam (`1.35s`, raio `4.5`, cooldown `3s`, custo `50` de bateria — `CatConfig.asset`) com sinal visual/sonoro, enquanto Humanos Misantropos ignoram.
- **Aceite no Unity:** No Play Mode, acionar o comando perto de um robô batedor o deixa travado com glifo/faísca; acionar perto de um Humano Misantropo não interrompe seu ataque.

### 4.3 Interação de Exploração de Kitty (Duto / Terminal)
- **Objetivo:** Implementar `CatExploration` permitindo a Kitty atravessar um duto estreito ou destravar um terminal offline para abrir um atalho.
- **Aceite no Unity:** No Play Mode, interagir (`E` / `Y` / `Triângulo`) com o ponto marcado envia Kitty pelo duto e abre a porta/atalho pelo outro lado.

---

## Épico 5 — Vertical Slice do Ato I *(Marco 1 / MVP)*

> **Mapa de referência:** [level-map.md](gdd/gdd-Under-The-Rain-2026-11-13/level-map.md) (Prólogo = Zona 0 com 6 salas; Zona 1 = Esgotos com 13 salas). Mudança de escopo registrada em [sprint-change-proposal-2026-11-13.md](sprint-change-proposal-2026-11-13.md).
>
> **Ordem de execução sugerida:** 5.1 → 5.4 → 5.5 → 5.2 → 5.3 (as chaves não foram renumeradas).

### 5.1 Salas dos Esgotos, Abrigo Inicial e Confronto Narrativo (Icarus)
- **Chave:** `5-1-sewers-rooms-initial-shelter-and-icarus`
- **Pilar reforçado:** Abrigos como pontos de ritmo (§5.6) e narrativa pelo ritmo de exploração (§5.8).
- **Objetivo:** Entregar a primeira fatia do Prólogo e da Zona 1. No Prólogo (cena `Prologue_Ruins`, subsolo, sem chuva): **P1 Plataforma do Metrô** (`prologue_metro_platform`, mover/pular, duto provisório de Kitty) e **P5 Laboratório Escondido** (`prologue_hidden_lab`: laboratório abandonado da Domus Corp atrás da estação, robô batedor, interferência de Kitty e o Terminal da Domus). Nos Esgotos (cena `Act1_Sewers`, sem chuva): **Z1-01 Abrigo** (`sewers_initial_shelter`, base da resistência com `ShelterRestSpot` e Icarus) e **Z1-02 Passagem** (`sewers_passage`). Ler o terminal e confrontar Icarus abre a comporta leste (gate de história).
- **Aceite no Unity:** No Play Mode, Dante e Kitty atravessam P1 → P5, derrotam/travam o robô batedor, leem o terminal (`E` / `Y`), descem ao Abrigo, confrontam Icarus, a comporta leste abre e a Passagem fica acessível. A volta Abrigo → Prólogo chega pelo lado direito do Laboratório.

### 5.2 Gate de Wall Jump, Retorno Recompensado e Transição para a Superfície
- **Chave:** `5-2-wall-jump-gate-rewarded-backtrack-and-surface`
- **Pilar reforçado:** Bloqueio por habilidade (§5.3), progressão por habilidades (§5.4) e backtracking com recompensa (§5.5).
- **Objetivo:** Construir a segunda metade da Zona 1: **Z1-05 Base do Poço** (`sewers_shaft_base`, gate legível com o poço e a luz no topo visíveis), **Z1-06 Oficina** (`sewers_workshop`, Dante ganha o Wall Jump — luvas de tração — com treino seguro), **Z1-07 Poço** (`sewers_shaft`, gate de progressão), **Z1-08 Nicho** (`sewers_alcove`, célula de bateria, gate coletável/suave de precisão), **Z1-09 Topo** (`sewers_shaft_top`, atalho unilateral Topo → Abrigo) e **Z1-10 Saída** (`sewers_exit`), mais a transição para a Superfície (Zona 2) com robô batedor e a anomalia de Kitty.
- **Aceite no Unity:** No Play Mode: (1) o poço é visível da Base antes de Dante ter a habilidade e não pode ser escalado sem ela; (2) a Oficina concede `WallJump` e o poço passa a ser escalável; (3) o atalho Topo → Abrigo só abre pelo lado de cima e permanece aberto depois; (4) o Nicho só é alcançável com Wall Jump; (5) a Saída leva à Transição, onde Kitty trava o robô batedor.

### 5.3 Primeiro Boss (Humano Misantropo) e Mapa Criptografado
- **Chave:** `5-3-first-misanthrope-boss-and-encrypted-map`
- **Pilar reforçado:** Bosses como exame da habilidade e marco narrativo (§5.10).
- **Objetivo:** Implementar a luta do 1º Humano Misantropo (imune a Kitty, com ataques telegrafados) e a recompensa do **Mapa Criptografado**, encerrando o Vertical Slice diante das torres da Domus Corp. Inclui um **ponto de descanso antes da arena**.
- **Aceite no Unity:** No Play Mode, derrotar o boss desbloqueia o Mapa Criptografado no inventário e exibe a vista das torres na chuva; morrer na luta faz Dante acordar no ponto de descanso mais próximo da arena.

### 5.4 Expansão do Prólogo: Salas de Tutorial
- **Chave:** `5-4-prologue-expansion-tutorial-rooms`
- **Pilar reforçado:** Sensação de controle antes de qualquer gate (§5.9) e ensino pelo level design, sem muro de texto (§5.12 — "Ato I ensina").
- **Objetivo:** Completar o Prólogo (Zona 0) com as salas que faltam, ensinando por level design: **P2 Vão** (`prologue_gap`: pulo variável, coyote time e buffer, sem dano), **P3 Entulho** (`prologue_debris`: bastão contra 1 Operário; célula de bateria + disparo), **P4 Duto de Kitty** (`prologue_duct`: Kitty atravessa o duto e abre a passagem — o duto provisório de P1 sai de lá e vem para cá) e **P6 Descida** (`prologue_descent`: descida rumo aos esgotos que leva ao Abrigo Z1-01). Ordem final: P1 → P2 → P3 → P4 → P5 → P6 → Z1-01. Manter os IDs de P1 e P5.
- **Aceite no Unity:** No Play Mode, partindo de P1, o jogador atravessa P1 → P6 → Abrigo sem nenhum texto além de placas curtas; P2 não causa dano em queda; em P3 o Operário cai em golpes de bastão e a célula de bateria permite disparar; em P4 a passagem só abre depois que Kitty atravessa o duto; a volta do Abrigo chega pelo lado correto de P6.

### 5.5 Esgotos: Galeria, Duto e Porta do Setor Selado
- **Chave:** `5-5-sewers-gallery-duct-and-sealed-door`
- **Pilar reforçado:** Gates legíveis (§5.3), Kitty como mecânica de exploração (§5.11) e "eu quero voltar ali" (§5.1).
- **Objetivo:** Construir **Z1-03 Galeria** (`sewers_gallery`, opcional: combate com grupo de Operários, recompensa de peças/bateria), **Z1-04 Duto** (`sewers_duct`, salto de sala: Kitty abre a porta e é a única rota até a Base do Poço) e a **porta do Setor Selado** (`sewers_sealed_sector`, Z1-11) no Abrigo, visível desde o início, que dá feedback de "sem energia" e não abre (a sala jogável fica para o Épico 8, gate do Bastão Eletrificado). Inclui as salas **Z1-12** e **Z1-13** (estrutura detalhada a definir pelo level design; ver `level-map.md`).
- **Aceite no Unity:** No Play Mode, a Galeria é opcional e recompensa o combate; a Base do Poço só é alcançável depois que Kitty atravessa o Duto; interagir com a porta do Selado mostra o feedback "sem energia" e ela continua fechada.

---

## Épico 6 — UI/UX Diegética *(Pós-Vertical Slice)*

### 6.1 HUD Minimalista Sem Caixas (`uGUI` Pixel-Art)
- **Objetivo:** Implementar a HUD conforme `DESIGN.md` (células de vida pixel-art, barra fina de bateria em `#4ECDC4`, contadores de kits e peças `atual/máx`, cooldown de Kitty e arma equipada), sem painéis/caixas.
- **Aceite no Unity:** No Play Mode, sofrer dano, gastar bateria, usar kit, coletar peças e acionar Kitty atualizam imediatamente os respectivos elementos da HUD em 1080p.

### 6.2 Inventário em 3 Abas e Tela do Mapa Criptografado
- **Objetivo:** Implementar o menu de Inventário (botão dedicado) com abas **Mapa Criptografado**, **Arsenal & Acoplamentos** e **Registros & Documentos** (texto + legenda de áudio).
- **Aceite no Unity:** No Play Mode, abrir o Inventário exibe salas descobertas, detalhes da arma equipada e documentos/áudios coletados, fechando com transição rápida.

### 6.3 UI de Abrigos (Ferreiro/Vendedor/Curandeiro) e Máquina de Teletransporte
- **Objetivo:** Implementar painéis pixel-art para NPCs de abrigo (acoplamento, compra/kits, expansão de vida/mochila) e a interface da **Máquina de Teletransporte** (sem NPC, com custo em peças).
- **Aceite no Unity:** No Play Mode, comprar expansão de mochila aumenta `maxCapacity` na HUD; usar a máquina de teletransporte deduz peças e transporta Dante para outro abrigo descoberto.

---

## Épico 7 — Level Design e Expansão das Zonas *(Pós-Vertical Slice)*

### 7.1 Habilidades Restantes (Dash Tático, Propulsor Aéreo, Gancho Magnético)
- **Objetivo:** Implementar os 3 estados restantes no `PlayerStateMachine` com seus respectivos gates legíveis.
- **Aceite no Unity:** Cada habilidade atravessa seu gate correspondente no Play Mode (vão largo, plataforma alta, ponto de ancoragem).

### 7.2 Construção das Macrozonas 2 a 5, Atalhos e Segredos
- **Objetivo:** Montar as salas das zonas 2 a 5 (Superfície, Cidade Alta, Laboratório do Pai, Torre Central) conforme o [level-map.md](gdd/gdd-Under-The-Rain-2026-11-13/level-map.md), com atalhos unilaterais e segredos sugeridos pelo cenário. A Zona 1 (Esgotos) é entregue no Épico 5, exceto a sala jogável do Setor Selado (Z1-11), que fica no Épico 8. O Prólogo (Zona 0) também é entregue no Épico 5.
- **Aceite no Unity:** É possível navegar entre todas as zonas por conexões contínuas e abrir atalhos que reduzem o backtracking.

---

## Épico 8 — Sistemas Avançados, Colapso da IA e Bosses *(Pós-Vertical Slice)*

### 8.1 Evolução da Arma (Bastão Eletrificado → Lâmina de Alta Frequência)
- **Objetivo:** Implementar os estágios 2 e 3 da arma principal via acoplamentos encontrados no mundo/abrigos.
- **Aceite no Unity:** Acoplar módulos altera dano, alcance, atordoamento elétrico e capacidade de deflexão no Play Mode.

### 8.2 Colapso Gradual da IA (`CollectiveCollapseManager`)
- **Objetivo:** Implementar os estágios de caos no mapa conforme Data Centers caem (inimigos mais agressivos/fortes e abertura de novas rotas).
- **Aceite no Unity:** Destruir um Data Center incrementa o nível de colapso e altera imediatamente o comportamento/status dos robôs da zona.

### 8.3 Bosses do Ato II, Opcionais, Titã e Save/Load Completo (Slot Único)
- **Objetivo:** Implementar os bosses restantes (Data Centers 1 e 2, Fuga do Laboratório, 2 Opcionais, e **O Titã** com apoio do Pai) e persistência JSON completa em `save_slot0.json`.
- **Aceite no Unity:** Salvar em um abrigo, fechar e reabrir o Play Mode restaura posição, habilidades, mapa, documentos, estágio do colapso e eventual `DroppedPack` caída.
