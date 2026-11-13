---
project_name: 'Under The Rain'
user_name: 'Desz2'
date: '2026-11-12'
updated: '2026-11-13'
engine: 'Unity 6000.3.1f1 (2D URP)'
sections_completed: ['narrative-summary', 'tech-stack', 'unity-conventions', 'collaboration-rules', 'metroidvania-design-pillars', 'technical-learnings']
---

# Project Context for AI Agents — Under The Rain

_Este arquivo contém o contexto persistente, a stack, as convenções técnicas e as regras de colaboração que todo agente de IA deve seguir ao planejar ou implementar código em **Under The Rain**._

---

## 1. Resumo Narrativo e Visão do Jogo

> **Fonte canônica:** [docs/narrativa/biblia-narrativa.md](narrativa/biblia-narrativa.md). Não invente personagens, reviravoltas ou lore fora da bíblia sem aprovação explícita.

- **Gênero:** Metroidvania 2D com narrativa forte, level design interconectado e UI diegética com identidade própria (neo-noir, garoa, neon, pixel art).
- **Mundo:** Metrópole fictícia inspirada em São Paulo (não é SP de fato), ano 2100. Cidade cinzenta, garoa constante, prédios corporativos brutalistas. Uma IA coletiva (nascida de um código de livre-arbítrio implantado nos robôs pela **Domus Corp**) dominou o planeta. Humanos vivem em campos de trabalho forçado ou nos esgotos. Há uma facção de **Humanos Misantropos**, aliados das máquinas.
- **Protagonista — Dante (30 anos):** Ex-desenvolvedor, combatente solitário. Parte sozinho após a resistência nos esgotos ignorar as provas que ele achou.
- **Kitty (gatinha de metal):** gatinha branca robótica, construída por Dante antes da Queda e isolada da rede. Companheira **ativa** (nos moldes do BD-1 de *Jedi: Fallen Order*) que acompanha Dante desde o Prólogo. Tem rede neural simples e não entende o próprio papel; robôs travam ou hesitam diante dela. Quem percebe isso, aos poucos, é o jogador/Dante. *(Nos documentos, use "Kitty" para a personagem; as classes C# mantêm o prefixo `Cat*`.)*
- **Pai (55 anos):** Desenvolvedor da Domus Corp que criou o código de livre-arbítrio; vive escondido num laboratório secreto. É encontrado do meio para o final do jogo, com vestígios dele ao longo do caminho.
- **Mente coletiva:** Todos os robôs compartilham a mesma mente, com tipos, funções e hierarquias diferentes (operários, policiais, patrulheiros, elite).
- **Clímax:** Vírus criado com a matriz neural de Kitty quebra a colmeia; o **Titã** (avatar físico da IA) absorve a energia da rede e precisa ser destruído na força bruta.
- **Decisões de design já tomadas:**
  - Vários bosses ao longo do jogo (inclusive humanos misantropos) que rendem itens.
  - Bosses opcionais ligados à história.
  - **Colapso gradual da IA** conforme Dante destrói data centers e bosses.
  - Boss final usa o que o jogador aprendeu.
- **Gameplay-alvo:** Combate corpo a corpo com lâmina de alta frequência + disparos à base de bateria; movimentação metroidvania (pulos dinâmicos, parkour, escalada, exploração vertical); narrativa ambiental; abrigos com comerciantes e sobreviventes.
- **Referências:** *Hollow Knight*, *Katana Zero* (jogabilidade); *Owlboy* e *Katana Zero* (pixel art, neon na chuva, poças refletindo luz).

---

## 2. Technology Stack & Versions

- **Engine:** Unity `6000.3.1f1` (2D, Universal Render Pipeline / URP 2D).
- **Input:** Unity Input System (`Assets/InputSystem_Actions.inputactions` presente no projeto base). `GameInputReader` clona o asset em runtime (`Instantiate(inputActions)`) e `InputDeviceTracker` detecta Teclado/Mouse vs. Gamepad.
- **Câmera e Salas:** `RoomCamera2D` (confinamento ortográfico aos `RoomBounds`; **sem** Cinemachine/PixelPerfectCamera) + `ScreenFader` + `RoomTransitionController`. Uma cena por zona (`Assets/_Project/Scenes/Zones/Prologue_Ruins.unity`, `Act1_Sewers.unity`), com as salas dentro da cena; a transição entre cenas leva a sala e o spawn de destino.
- **Narrativa:** `NarrativeState` (progresso de sessão compartilhado entre cenas), `DialogueRunner` (IMGUI temporário até o Épico 6), `DocumentDataSO`, `DialogueSequenceSO`.
- **Arquitetura detalhada:** [game-architecture.md](planning/architecture/game-architecture.md).
- **Testes:** Unity Test Framework (EditMode com `Unity.InputSystem.TestFramework` para lógica pura; PlayMode para comportamento e critérios de aceite).
- **Repositório:** Greenfield em `desz2000/UnderTheRain` (não copiar código do protótipo antigo `desz2000/new-game-devtest` sem pedido explícito).
- **BMAD:** BMM + GDS instalados em `_bmad/` e `.agent/skills/`, com artefatos em `docs/planning/` e `docs/implementation/`.

---

## 3. Convenções Técnicas (Unity)

- **Controle de versão:** Nunca commitar `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`; **sempre** commitar arquivos `.meta`.
- **Cenas e Prefabs:** **Não editar arquivos `.unity` ou `.prefab` manualmente (YAML).** Quando for preciso alterar cena ou prefab, descrever o passo a passo no Unity Editor ou usar scripts de Editor C#.
- **Dados e Arquitetura:**
  - Armazenar dados de jogo (itens, inimigos, habilidades, diálogos/beats) em **ScriptableObjects**.
  - Evitar números mágicos e singletons desnecessários.
  - **Valores finais vivem nos assets.** O tuning de game feel (movimento, combate, Kitty) é ajustado em playtest diretamente em `Assets/_Project/Data/**` (e nos campos serializados dos componentes). Os documentos citam o valor atual **e onde ele mora**; em caso de divergência, vale o asset, e o documento deve ser corrigido.
  - Separar lógica de apresentação (UI, animação, VFX, áudio).
- **Idioma no código vs. comunicação:**
  - Nomes de classes, métodos, variáveis, arquivos, assets e mensagens de commit em **inglês**.
  - Comentários curtos e úteis (explicando o *porquê*, não o *quê*).
  - Comunicação com o usuário e documentos de planejamento em **português do Brasil**.

---

## 4. Regras de Colaboração e Escopo

1. **Narrativa é canônica:** Qualquer alteração na história, personagens, lore ou arco dos atos descritos em `docs/narrativa/biblia-narrativa.md` exige **aprovação explícita** do usuário.
2. **Decisões em aberto:** Ao precisar de uma decisão de design ou narrativa não coberta pelos documentos, fazer **uma pergunta por vez**, oferecendo 2–3 alternativas com suas consequências.
3. **Critérios de aceite verificáveis:** Toda história de implementação deve ter critérios de aceite testáveis diretamente no Play Mode do Unity (ou via EditMode/PlayMode tests).
4. **Comandos destrutivos:** Nunca executar `git reset --hard`, `git clean -fd` ou remoção de pastas sem confirmação prévia.
5. **Alinhamento com os pilares:** Toda feature, sala ou mecânica nova deve indicar, na história ou no GDD, **qual pilar da Seção 5** ela reforça. Se não reforça nenhum, questionar se deve entrar.
6. **Propostas não são decisões:** Itens marcados como *(proposta)* na Seção 5 são sugestões de design. Só viram regra depois de aprovados pelo usuário e registrados no GDD.

---

## 5. Essência Metroidvania: Pilares de Design

> Diretrizes para qualquer decisão de level design, progressão, combate e UI. O detalhamento (habilidades, mapa, números) vai para o GDD; aqui ficam os princípios. Itens *(proposta)* ainda precisam de aprovação.

### 5.1 O loop central

**Explorar → encontrar uma barreira → obter uma habilidade ou item → voltar e abrir o caminho → descobrir mais.** Todo sistema do jogo existe para tornar esse ciclo mais satisfatório. O jogador deve sempre ter um "eu quero voltar ali" em mente.

### 5.2 Mundo único e interconectado

- Um mapa contínuo dividido em **zonas com identidade própria** (visual, sonora, inimigos), não uma sequência de fases. Zonas previstas pela narrativa: esgotos, ruínas da superfície, cidade alta e torres da Domus Corp, laboratório secreto do pai, torre central.
- **Prólogo = Zona 0:** tutorial de ~20–35 min, **fora** das 5 macrozonas. É uma estação de metrô abandonada no subsolo (sem chuva) que esconde um laboratório abandonado da Domus Corp. Tem 6 salas (P1–P6) e ensina por level design, sem muro de texto. A Zona 1 (Esgotos, 13 salas) é a base da resistência, revisitada no backtracking.
- **Mapa de salas, IDs e grafos:** [level-map.md](planning/gdd/gdd-Under-The-Rain-2026-11-13/level-map.md). Consulte-o antes de criar ou alterar qualquer sala; os IDs de sala (`RoomBounds.RoomId`) já construídos não mudam.
- **Verticalidade como eixo:** esgotos embaixo, torres em cima; a subida física acompanha a subida narrativa.
- **Atalhos que ligam áreas distantes**, geralmente abertos por um lado só, para que o retorno seja rápido e dê a sensação de o mundo "se fechar" em um todo.
- Poucas telas de carregamento visíveis; transições entre zonas devem parecer parte do mundo.

### 5.3 Bloqueio por habilidade, não por nível

- O jogador é impedido por **o que ele ainda não consegue fazer**, não por um número (nível/XP).
- **Gates duros:** exigem uma habilidade específica. **Gates suaves:** exigem domínio técnico (precisão, rota difícil), permitindo quebra de sequência por jogadores habilidosos.
- **Todo gate deve ser legível:** o jogador precisa *ver* o que não alcança ainda (a plataforma alta, a parede rachada, o corredor lacrado). Gate invisível gera frustração, não curiosidade.
- Antes de desenhar salas, montar a **tabela chave e fechadura** (habilidade → gates que ela abre → zonas afetadas).
- **Taxonomia de gates (4 tipos):**
  1. **Progressão:** sem a habilidade, não se muda de zona (gates duros).
  2. **Salto de sala:** a habilidade é a única forma de ir de uma sala a outra da mesma zona, abrindo atalhos ou rotas.
  3. **Coletável:** vida, bateria, mochila, documentos do Pai (gates suaves, de precisão).
  4. **Retorno:** voltar a uma zona antiga com a habilidade nova.
  - Toda história que cria um gate deve dizer **qual dos 4 tipos** ele é e como ele fica legível.

### 5.4 Progressão por habilidades

- A parte mais importante do poder vem de **habilidades que mudam como o jogador se move ou luta**, e não de números maiores.
- Upgrades secundários (vida, capacidade de bateria, dano) ficam como **recompensa de exploração**, escondidos em rotas opcionais. Evitar grind.
- **Toda habilidade nova precisa de três coisas:** um momento de apresentação segura, um uso inteligente em um gate, e um uso que mude o combate ou a exploração das áreas anteriores.
- Habilidades de movimento e combate vêm dos **bosses e dos beats da história** (já decidido: bosses rendem itens), de forma que cada conquista seja um marco narrativo.

### 5.5 Backtracking com recompensa

- Toda volta a uma área antiga deve trazer algo: atalho, segredo, item, mudança no mundo ou um beat de história.
- **O colapso gradual da IA é o motor narrativo do retorno:** conforme Dante destrói data centers e bosses, áreas anteriores mudam (robôs mais fracos ou desligados, sistemas offline, novas rotas, novas falas). Dar a cada mudança um efeito visível.
- Backtracking nunca deve ser tedioso: usar atalhos, e *(proposta)* viagem rápida entre abrigos a partir do Ato II.

### 5.6 Abrigos como pontos de ritmo

- Os **abrigos com comerciantes e sobreviventes** funcionam como pontos de salvamento e descanso. Eles dão ao jogo o ritmo de **tensão → alívio**.
- Posicioná-los antes de grandes desafios e depois de picos de dificuldade, nunca de forma aleatória.
- *(proposta)* Ser o único lugar onde se pode recarregar completamente a bateria e usar a loja.

### 5.7 Mapa e orientação

- O mapa **se revela conforme o jogador explora**, com marcação de gates ainda não superados e de áreas incompletas.
- Sem setas ou ícones genéricos de objetivo. Orientação por **sinalização diegética** (placas, luz, sons, o próprio gato).
- *(proposta)* O **mapa criptografado** do Ato I é o gancho: ele é decodificado em partes, ligado à progressão.

### 5.8 Descoberta e segredos

- Rotas escondidas, paredes quebráveis, itens opcionais e lore ambiental recompensam a curiosidade.
- A narrativa é contada pelo **ritmo da exploração e pelos cenários**: terminais, rabiscos, vestígios do pai, restos da vida antes do colapso.
- Segredos devem ser **sugeridos**, não aleatórios: algo no cenário (um som, uma luz, uma assimetria) denuncia a possibilidade.

### 5.9 Sensação de controle (game feel)

- **Mover-se precisa ser prazeroso antes de qualquer gate.** Esta é a prioridade 1 do protótipo.
- Pulo variável, *coyote time*, *input buffering*, aceleração e frenagem curtas e previsíveis, controle aéreo suficiente para precisão.
- Combate com **hitstop, impacto visual e sonoro e screenshake sutil**; invulnerabilidade curta após dano.
- Responsividade acima de realismo: se há conflito entre física "correta" e controle bom, vence o controle.

### 5.10 Combate, inimigos e bosses

- Combate legível: **telegrafia clara** de ataques, padrões aprendíveis, mortes que o jogador entende como justas.
- Lâmina de alta frequência como ferramenta principal; **disparo à base de bateria** como recurso limitado de risco e recompensa (decisão de quando gastar). *(proposta)* A bateria também pode alimentar interações de exploração.
- **Inimigos introduzem mecânicas** em ambiente controlado e depois as combinam em arenas maiores. Aproveitar a mente coletiva: tipos e hierarquias (operários, policiais, patrulheiros, elite) com comportamentos distintos e complementares.
- **Bosses são "exames" da habilidade recém-adquirida** e também marcos narrativos. O boss final usa o que o jogador aprendeu.

### 5.11 Kitty como mecânica de exploração

- Kitty é companheira **ativa**. Seu efeito canônico (robôs travam ou hesitam diante dela) é a base.
- Ela atravessa dutos e destrava terminais para abrir passagens (implementado: `CatExploration`). *(proposta)* Também pode chamar a atenção de patrulhas e servir de apoio em puzzles, sem nunca resolver o jogo sozinha.
- O jogador deve **perceber gradualmente** que Kitty é especial, sem explicação direta (alinhado ao canon de que ela não entende o próprio papel).

### 5.12 Dificuldade, morte e ritmo

- Morte com **custo leve**: retorno ao último abrigo. *(proposta)* Um recurso recuperável no local da morte.
- Dificuldade crescente por ato: **Ato I ensina**, **Ato II expande e mistura**, **Ato III testa maestria**.
- Alternar picos de tensão com momentos de calma, narrativa e descoberta. Novas habilidades ou ferramentas em intervalos regulares, para manter a sensação de crescimento.

### 5.13 Ordem de design de níveis

1. Mapa macro: zonas, conexões, atalhos.
2. Tabela chave e fechadura (habilidades × gates).
3. Rota principal e rotas opcionais por zona.
4. Salas, com combate, plataforma e narrativa ambiental.
5. Passada de polimento: sinalização, iluminação, áudio.

### 5.14 Anti-padrões a evitar

- Gates arbitrários ou invisíveis.
- Backtracking sem recompensa ou sem atalhos.
- Habilidades que só "abrem portas" sem mudar o jogo.
- Salas grandes e vazias, ou mapa confuso demais para navegar.
- Dificuldade de combate desconectada da exploração (picos injustos, mortes por falta de informação).
- Explicar a narrativa por texto longo em vez de mostrá-la no mundo.

### 5.15 Escopo do primeiro marco (vertical slice do Ato I)

Provar **um ciclo completo do loop central**, com polimento de jogo real:

- O Prólogo (Zona 0) completo, a Zona 1 (Esgotos) e a transição para a superfície.
- Movimento e combate com game feel aprovado.
- Uma habilidade nova (Wall Jump, obtido na Oficina Z1-06), um gate que ela abre (Poço Z1-07) e um retorno recompensado (atalho Topo → Abrigo).
- Kitty ativa em pelo menos uma interação.
- Um abrigo funcional e o primeiro Humano Misantropo como boss.
- UI/HUD no caminho da identidade visual definida (nada de visual genérico).

---

## 6. Aprendizados Técnicos (Épicos 1 a 5)

> Lições confirmadas em implementação e playtest. Siga-as em novas histórias.

**Input**
1. `InputTestFixture` exige a referência `"Unity.InputSystem.TestFramework"` no `.asmdef` de testes.
2. `GameInputReader` instancia uma cópia do asset em `Awake()` e a destrói em `OnDestroy()`. Ações que faltam no asset (`Shoot`, `Heal`, `Craft`, `CatCommand`, Interact do gamepad) são criadas num **mapa de fallback separado** (`_fallbackMap`), nunca dentro do asset clonado.
3. Mapeamento atual: Interact `E` / `Y` / `Triângulo`; Attack `Mouse Esq.` / `Enter` / botão Oeste (`J` ainda não mapeado); Shoot `Mouse Dir.` / `F` / `RT`; Heal `Q` / `LB`; Craft `C` / `D-Pad baixo`; CatCommand `R` / `RB`.

**Editor e cenas**
4. Todo `[MenuItem]` de setup checa `EditorApplication.isPlaying` antes de rodar.
5. Ao configurar componentes via `SerializedObject.FindProperty`, o nome deve ser **exatamente** o do campo serializado (por exemplo, `PatrolInterferenceTarget.bodyRenderer`). Um nome errado retorna `null` e quebra o setup com `NullReferenceException`.
6. Setups de cena que criam inimigos chamam `EnemySetupUtility.EnsureCombatLayers()` para garantir as layers e a matriz de colisão de combate.
6a. **Spawn points nunca podem ficar sobre um `RoomExitTrigger`:** deixe pelo menos ~1 unidade entre o spawn e o trigger, levando em conta a largura de Dante (0,8). Senão Dante volta para a sala anterior assim que chega.
6b. Para buscar objetos da cena (inclusive inativos), percorra `scene.GetRootGameObjects()` + `GetComponentsInChildren<T>(true)`; evite `FindObjectsByType` com `FindObjectsSortMode`, que está a caminho de ser descontinuado.

**Movimento e combate (playtest)**
7. Movimento estilo *Hollow Knight*: coyote time **40 ms**, Wall Jump **(6,5; 14)** com trava **0,05 s**. Responsividade acima de realismo.
8. Invulnerabilidade pós-dano **0,5 s** e trava de knockback **0,16 s**; o finalizador do combo (3º golpe) dá +1 de dano e mais impacto.
9. O corpo a corpo sempre causa mais dano que o disparo de bateria (bastão 2/3 × disparo 1, custo 5). A interferência de Kitty custa 50 de bateria.
10. `Rigidbody2D` do jogador com `CollisionDetectionMode2D.Continuous`; plataformas sólidas na layer `Ground`.
11. Inimigos movidos por transform precisam de `Rigidbody2D` cinemático para que os triggers de hitbox/hurtbox disparem de forma confiável.

**Kitty**
12. Kitty sobe no ombro em WallSlide/WallJump, em combate e em travessias altas; ela se teleporta se ficar a mais de 11 unidades de Dante.
13. A interferência afeta só `CollectiveMachine`; `MisanthropeHuman` a ignora. Enquanto o robô hesita, o contato dele não causa dano.

**Narrativa e interação**
14. Interagíveis que abrem diálogos checam `DialogueRunner.IsBusy` (não `IsPlaying`) para que o mesmo `E` que fecha o diálogo não o reabra.
15. `NarrativeState` usa um modelo estático de sessão, zerado em `SubsystemRegistration`, para manter as flags entre cenas.
16. Transições entre cenas devem informar sala e spawn de destino (`RoomExitTrigger.ConfigureScene(scene, roomId, spawnId)`); senão Dante cai na sala inicial da cena.
16a. Diálogos travam movimento (`PlayerController.SetControlLocked`) **e** ações (`GameInputReader.SetActionsSuppressed`); só o Interact continua ativo para avançar as falas.
16b. Teleportes para outra sala (respawn, troca de cena) precisam **ativar a sala de destino** (`RoomTransitionController.RespawnRoutine` / `FindRoomContaining`). Salas inativas não têm colisão, e `Collider2D.bounds` vem vazio nelas (use `RoomBounds.ContainsPoint`).
16c. Atalhos abertos por Kitty usam `CatExplorationPoint.persistentId` para continuar abertos quando a cena é recarregada na mesma sessão, até o save do Épico 8.

**Visual**
17. Partículas no URP 2D precisam de um material válido (`Sprites/Default`) para não renderizarem rosa. A chuva (`SewerRainAmbience`) fica só nas áreas a céu aberto (superfície). Prólogo e Esgotos são secos.
