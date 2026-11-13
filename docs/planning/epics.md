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
---

# Backlog Inicial — Under The Rain

> **Regra de Marco:** Os **Épicos 1 a 5** entregam o **Vertical Slice do Ato I** (primeiro marco jogável) antes de ampliar o escopo para os Épicos 6 a 8. Toda história possui critérios de aceite verificáveis no **Unity Play Mode** (ou EditMode Test Runner).

---

## Épico 1 — Fundação do Projeto *(Vertical Slice)*

### 1.1 Estrutura de Pastas, Assembly Definitions e Cena Base
- **Objetivo:** Criar a hierarquia `Assets/_Project/` (`Art`, `Audio`, `Data`, `Prefabs`, `Scenes`, `Scripts`, `Settings`), Assembly Definitions (`UnderTheRain.Runtime`, `UnderTheRain.Editor`, `UnderTheRain.Tests.EditMode`, `UnderTheRain.Tests.PlayMode`) e a cena persistente `Core.unity`.
- **Aceite no Unity:** Abrir `Core.unity` e entrar no Play Mode sem erros ou warnings de compilação; rodar o Unity Test Runner com 1 teste de sanidade verde.

### 1.2 Input System e Detecção de Dispositivo
- **Objetivo:** Configurar `InputSystem_Actions.inputactions` (Action Maps `Gameplay` e `UI` para Teclado e Gamepad) e implementar `InputDeviceTracker` para alternar glifos automaticamente.
- **Aceite no Unity:** No Play Mode, pressionar teclas e botões de gamepad dispara ações mapeadas e alterna o evento de dispositivo ativo (`KeyboardMouse` ↔ `Gamepad`).

### 1.3 Transição de Salas com Fade-to-Black e Câmera 2D
- **Objetivo:** Implementar `RoomTransitionController`, `RoomExitTrigger`, `SpawnPoint` e ajuste de `CinemachineConfiner2D` por sala (estilo *Hollow Knight*, sem tela de loading).
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
- **Objetivo:** Implementar `WeaponDataSO`, combo de 2–3 golpes, `Hitbox`/`Hurtbox` (`IDamageable`), *hitstop* (`~40–60ms`), *screenshake* sutil e invulnerabilidade pós-dano (`~0.8s`).
- **Aceite no Unity:** No Play Mode, atacar um alvo de teste aplica dano, dispara *hitstop* perceptível e recuo; receber dano ativa invulnerabilidade temporária sem perder vida duas vezes no mesmo golpe.

### 3.2 Disparo à Base de Bateria e Kit Médico
- **Objetivo:** Implementar `BatteryComponent` (disparo à distância consumindo carga, recarga em abrigo e via células dropadas) e `MedkitComponent` (3 cargas iniciais de cura).
- **Aceite no Unity:** No Play Mode, disparar consome bateria até zerar (impedindo novos disparos até recarregar), usar Kit Médico restaura vida consumindo 1 carga, e descansar no abrigo restaura 100% de ambos.

### 3.3 Mochila de Peças, Morte e Recuperação (`DroppedPack`)
- **Objetivo:** Implementar `PartsPackComponent` (coleta de peças até `maxCapacity`), fabricação de kit médico usando peças, e morte com spawn de `DroppedPack` no local exato da queda + despertar no último abrigo.
- **Aceite no Unity:** No Play Mode, derrotar inimigo soma peças; ao morrer, Dante acorda no abrigo com `0` peças e uma `DroppedPack` aparece no local da morte; ao tocá-la, recupera as peças perdidas.

---

## Épico 4 — Gato Companheiro *(Vertical Slice)*

### 4.1 Seguir Dante e Subir no Ombro
- **Objetivo:** Implementar `CatController` seguindo Dante por plataformas e subindo no ombro/mochila durante escaladas e combates intensos.
- **Aceite no Unity:** No Play Mode, o gato acompanha Dante ao pular plataformas e realiza transição suave para o ombro ao iniciar `WallSlide`.

### 4.2 Comando Ativo de Interferência (`ICatAffectable`)
- **Objetivo:** Implementar `CatInterference` com cooldown: robôs da colmeia (`CollectiveMachine`) hesitam/travam por `~1.0–1.5s` com sinal visual/sonoro, enquanto Humanos Misantropos ignoram.
- **Aceite no Unity:** No Play Mode, acionar o comando perto de um robô batedor o deixa travado por `~1–1.5s` com glifo/faísca; acionar perto de um Humano Misantropo não interrompe seu ataque.

### 4.3 Interação de Exploração do Gato (Duto / Terminal)
- **Objetivo:** Implementar `CatExploration` permitindo ao gato atravessar um duto estreito ou destravar um terminal offline para abrir um atalho.
- **Aceite no Unity:** No Play Mode, interagir com o ponto marcado envia o gato pelo duto e abre a porta/atalho pelo outro lado.

---

## Épico 5 — Vertical Slice do Ato I *(Marco 1 / MVP)*

### 5.1 Salas dos Esgotos, Abrigo Inicial e Confronto Narrativo (Icarus)
- **Objetivo:** Montar as salas iniciais dos esgotos com chuva abafada, terminal com documentos da Domus Corp, abrigo com Icarus/NPC e ruptura de Dante rumo à superfície.
- **Aceite no Unity:** No Play Mode, Dante interage com o terminal, conversa com Icarus e inicia a subida pelos esgotos.

### 5.2 Gate de Wall Jump, Retorno Recompensado e Transição para a Superfície
- **Objetivo:** Construir poço vertical que exige `WallJump`, desbloqueio da habilidade, retorno por atalho recompensado e transição para as Ruínas da Superfície (com robô batedor + anomalia do gato).
- **Aceite no Unity:** No Play Mode, o jogador vê o gate legível antes de ter a habilidade, desbloqueia `WallJump`, abre o atalho de retorno e chega à superfície onde o gato trava o robô batedor.

### 5.3 Primeiro Boss (Humano Misantropo) e Mapa Criptografado
- **Objetivo:** Implementar a luta do 1º Humano Misantropo (imune ao gato, com ataques telegrafados) e a recompensa do **Mapa Criptografado**, encerrando o Vertical Slice diante das torres da Domus Corp.
- **Aceite no Unity:** No Play Mode, derrotar o boss desbloqueia o Mapa Criptografado no inventário e exibe a vista das torres na chuva.

---

## Épico 6 — UI/UX Diegética *(Pós-Vertical Slice)*

### 6.1 HUD Minimalista Sem Caixas (`uGUI` Pixel-Art)
- **Objetivo:** Implementar a HUD conforme `DESIGN.md` (células de vida pixel-art, barra fina de bateria em `#4ECDC4`, contadores de kits e peças `atual/máx`, cooldown do gato e arma equipada), sem painéis/caixas.
- **Aceite no Unity:** No Play Mode, sofrer dano, gastar bateria, usar kit, coletar peças e acionar o gato atualizam imediatamente os respectivos elementos da HUD em 1080p.

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

### 7.2 Construção das 5 Macrozonas, Atalhos e Segredos
- **Objetivo:** Montar as salas das 5 zonas (Esgotos, Superfície, Cidade Alta, Laboratório do Pai, Torre Central) com atalhos unilaterais e segredos sugeridos pelo cenário.
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
