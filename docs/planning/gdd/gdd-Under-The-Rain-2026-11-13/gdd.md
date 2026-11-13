---
title: 'Game Design Document: Under The Rain'
game_type: metroidvania
platforms: ['PC (Steam)']
status: complete
created: 2026-11-13
updated: 2026-11-13
---

# Under The Rain — Game Design Document (GDD)

**Autor:** Desz2  
**Gênero / Tipo:** Metroidvania 2D (`metroidvania`)  
**Plataforma-Alvo:** PC (Steam)  
**Referências base:** [Game Brief](../../briefs/brief-Under-The-Rain-2026-11-12/brief.md) · [Bíblia Narrativa](../../../narrativa/biblia-narrativa.md) · [Project Context](../../../project-context.md) · [Mapa de Níveis](level-map.md)

> **Valores numéricos:** os valores finais de tuning vivem nos assets (`Assets/_Project/Data/**`) e nos componentes C#. Este documento cita o valor atual e onde ele mora; em caso de divergência, vale o asset.

---

## 1. Resumo Executivo e Contexto

- **Conceito Central:** *"Em uma metrópole neo-noir sufocada pela garoa constante, você é um combatente solitário acompanhado por Kitty, uma gatinha de metal, que desafia uma mente coletiva de máquinas, escalando dos esgotos até o topo das torres corporativas para descobrir a verdade sobre o colapso e o mistério da morte do próprio pai."*
- **Público-Alvo:** Jogadores de PC (Steam, 18–35 anos) fãs de Metroidvanias e ação 2D atmosférica que buscam exploração interconectada, narrativa ambiental forte e desafio justo/acessível (não punitivo como soulslike).
- **Escopo-Alvo:** Campanha enxuta e polida (~4–6 horas), estruturada em 3 atos. Primeiro marco jogável: **Vertical Slice do Ato I**.

---

## 2. Core Gameplay (Pilares, Loop e Condições)

### Pilares de Design
1. **Movimentação Expressiva Antes de Tudo:** Controle ágil e responsivo (pulo variável, *coyote time*, *input buffering*, parkour, escalada, exploração vertical).
2. **Combate Ágil e Progressivo:** De equipamento inicial simples (bastão) até lâmina de alta frequência, combinando corpo a corpo, disparos à base de bateria e acessórios.
3. **Companheira que Quebra a Colmeia:** Kitty, a gatinha de metal, atua na exploração e faz robôs da mente coletiva hesitarem ou travarem, sem tutoriais expositivos.
4. **Mundo em Colapso Gradual e Narrativa Ambiental:** Mapa contínuo onde abrigos marcam o ritmo (tensão → alívio), data centers/bosses derrubados alteram zonas antigas e NPCs/bosses/documentos/itens revelam a história.

### Core Gameplay Loop
Explorar zonas verticais interconectadas → enfrentar inimigos (lâmina/bastão + bateria + Kitty) → encontrar gates legíveis, NPCs, documentos e pistas → derrotar bosses / destruir data centers → obter habilidades, armas/acessórios e enfraquecer a IA → retornar por atalhos e abrigos para abrir novos caminhos, ver o mundo transformado e resolver mistérios.

---

## 3. Movimentação e Game Feel

### 3.1 Movimentação Base (desde o início)
- **Corrida horizontal:** velocidade máx. **8**, aceleração **0,08 s** e desaceleração **0,06 s**, priorizando responsividade e parada seca em plataformas estreitas. Controle aéreo ×0,85.
- **Pulo variável:** altura máx. **3,2**; soltar cedo corta a velocidade vertical (×0,4). Gravidade de subida 38, queda ×1,6, velocidade máx. de queda 22.
- **Assistências de Game Feel:**
  - *Coyote Time:* **40 ms** após deixar uma borda (ajustado em playtest; antes ~100 ms).
  - *Input Buffering:* **120 ms** antes de tocar o chão ou parede.
  - *Corner Correction:* ajuste automático de até **0,25** em quinas de teto para não travar o salto.
  - *Drop-through:* agachar + pulo desce por plataformas finas (**0,25 s** de atravessamento).
- **Fonte dos valores:** `Assets/_Project/Data/Abilities/PlayerMovementConfig.asset` (`PlayerMovementConfigSO`).

### 3.2 Habilidades de Mobilidade Desbloqueáveis (4 no jogo completo)
1. **Wall Slide & Wall Jump *(1ª habilidade — Vertical Slice Ato I)*:** deslizar em paredes verticais com atrito controlado (velocidade máx. de deslize **3,5**) e saltar entre paredes (velocidade **(6,5; 14)**, trava de input **0,05 s**, *wall coyote* **0,08 s**; ajuste estilo *Hollow Knight*). Obtido na **Oficina (Z1-06)**, no fim da Zona 1 (luvas de tração *(origem: proposta)*). Sustenta a subida dos esgotos para a superfície e salas verticais.
2. **Dash Tático:** impulso horizontal rápido com curto cooldown e janela breve de esquiva; abre vãos largos e reposiciona no combate.
3. **Propulsor Aéreo (Pulo Duplo):** salto adicional no ar para alcance vertical extra e extensão de combate aéreo.
4. **Gancho Magnético:** projétil/ancoragem em pontos metálicos elevados para travessias longas e verticais nas zonas da Cidade Alta e torres da Domus Corp.

## 4. Combate, Armamento e Recursos (Vida, Bateria, Cura, Morte)

### 4.1 Game Feel de Combate
- **Impacto:** *hitstop* de **50 ms** por acerto (×1,4 no finalizador do combo), *screenshake* sutil (intensidade 0,1 por 0,08 s), partículas de faísca e *flash* de dano legível.
- **Invulnerabilidade pós-dano:** **0,5 s** (ajustada em playtest; antes ~0,8 s), com recuo curto (*knockback*) previsível e **trava de controle de 0,16 s** ao levar dano.
- **Bastão Simples (valores atuais):** combo de **3 golpes**, dano **2** por golpe, **finalizador** (3º golpe) com **+1** de dano, hitstop ×1,4 e knockback ×1,35; janela de combo 0,35 s.
- **Disparo de bateria:** custo **5** de carga, dano **1** (o corpo a corpo sempre causa mais dano que o disparo), cadência de 0,25 s.
- **Fonte dos valores:** `Data/Weapons/SimpleBaton.asset` (`WeaponDataSO`), `HealthComponent`, `PlayerController` (trava de knockback) e `PlayerRangedCombat`.
- **Telegrafia:** todo ataque inimigo possui antecipação visual e sonora clara antes do frame ativo.

### 4.2 Evolução da Arma Principal (3 Estágios)
1. **Bastão Simples *(Início / Ato I)*:** combo de 3 golpes com finalizador, alcance curto/médio, ferramenta básica de defesa.
2. **Bastão Eletrificado *(Ato I/II)*:** acoplamento obtido após descoberta/item específico; adiciona carga elétrica com atordoamento leve em robôs e ataque carregado.
3. **Lâmina de Alta Frequência *(Ato II/III)*:** evolução avançada via acoplamento/descoberta; golpes mais velozes, alcance ampliado e capacidade de deflexão/corte preciso.
- *Acessórios e sub-armas:* desbloqueáveis via exploração, loja de abrigo e bosses.

### 4.3 Bateria, Cura, Mochila de Peças e Morte
- **Disparo à base de Bateria:** ataque à distância limitado por carga de bateria (usada também em interações específicas de exploração). Recarrega 100% nos abrigos e parcialmente via células dropadas por inimigos.
- **Cura (Kit Médico):** recurso separado da bateria — começa com **3** cargas (cada uma cura **2** células), recarregáveis nos abrigos ou fabricáveis com **15** peças.
- **Valores atuais de recursos:** vida de **5** células; bateria de **100** de carga (a célula dropada recarrega **35**); mochila com capacidade inicial de **50** peças (`HealthComponent`, `BatteryComponent`, `BatteryCellPickup`, `MedkitComponent`, `PartsPackComponent`).
- **Mochila de Peças (Economia Unificada):** peças coletadas de robôs derrotados servem simultaneamente como **moeda do jogo**, **material para fabricar kits médicos** e **munição/insumo para armas acopladas** adquiridas ao longo da jornada.
- **Morte e Recuperação:** ao morrer, Dante acorda no último **abrigo/posto seguro** (postos da resistência ou de sobreviventes) mantendo mapa, habilidades e equipamentos; a **mochila de peças** fica caída no local exato da morte para ser recuperada.

## 5. Kitty, a Gatinha de Metal (Mecânicas de Companheiro)

### 5.1 Presença e Comportamento
- **Companheira Ativa:** Kitty acompanha Dante pelo cenário, sobe no ombro/mochila durante escaladas e combates intensos, e reage sutilmente (olhar/som) perto de segredos ou perigos.

### 5.2 Anomalia na Mente Coletiva (Comando Ativo)
- **Comando Tático de Interferência (`R` / `RB`):** Kitty emite um pulso que causa uma breve **hesitação/travamento** nos robôs da colmeia ao redor (**1,35 s** de "erro de leitura", raio **4,5**, cooldown **3 s**, custo de **50** de bateria — `Data/Companion/CatConfig.asset`). Humanos Misantropos ignoram o pulso.
- **Leitura Narrativa:** o jogador utiliza a mecânica taticamente desde o início, mas nem Dante nem Kitty entendem *por que* isso acontece — o significado real da matriz neural isolada só se revela aos poucos até o reencontro com o Pai.

### 5.3 Interações de Exploração (distribuídas ao longo dos 3 Atos)
1. **Dutos e espaços pequenos:** atravessar passagens estreitas para abrir atalhos pelo outro lado ou coletar itens/documentos.
2. **Distração tática:** emitir sinal visual/sonoro para chamar atenção e reposicionar patrulhas.
3. **Leitura de terminais:** escanear e destravar terminais antigos e portas de serviço fora da rede.

## 6. Design Específico de Metroidvania (Mapa Macro, Chave × Fechadura, Backtracking e Abrigos)

### 6.1 Mapa Macro (Prólogo + 5 Zonas Interconectadas em Salas)
Salas, IDs e grafos detalhados em [level-map.md](level-map.md).

0. **Prólogo — Zona 0 *(Ato I, tutorial)*:** fica **fora** das 5 macrozonas. É uma estação de metrô abandonada no subsolo (sem chuva) que esconde um laboratório abandonado da Domus Corp, onde se testava um robô de guerra que não foi adiante. Ali Dante encontra o Terminal da Domus. São 6 salas e ~20–35 min, ensinando por level design (mover, pular, bastão, bateria, duto e interferência de Kitty), sem muro de texto.

Cada zona é composta por diversas salas interconectadas para exploração livre (ao estilo *Hollow Knight*), organizadas em eixo vertical:
1. **Esgotos e Subsolo *(Ato I)*:** base da resistência (abrigo revisitado com frequência, com mercadores e recarga), umidade sem chuva, túneis de serviço e dutos estreitos. São 13 salas, incluindo a Oficina onde Dante ganha o Wall Jump e a porta do Setor Selado.
2. **Ruínas da Superfície *(Ato I → II)*:** transição, ruas alagadas, garoa aberta, primeiro Humano Misantropo e obtenção do mapa criptografado.
3. **Cidade Alta e Setor Corporativo *(Ato II)*:** prédios brutalistas, neon, patrulhas densas e data centers locais da IA.
4. **Laboratório Secreto do Pai *(Ato II — Ponto Médio)*:** setor oculto sob o setor corporativo, conectado por atalho ao subsolo.
5. **Torre Central da Domus Corp *(Ato III)*:** escalada final até o núcleo da colmeia e o Titã.

### 6.2 Tabela Chave × Fechadura e Fluxo de Progressão
- **Taxonomia de gates (4 tipos):**
  1. **Progressão:** sem a habilidade, não se muda de zona (gates duros).
  2. **Salto de sala:** a habilidade é a única forma de ir de uma sala a outra da mesma zona, abrindo atalhos ou rotas.
  3. **Coletável:** vida, bateria, mochila, documentos do Pai (gates suaves, de precisão).
  4. **Retorno:** voltar a uma zona antiga com a habilidade nova.
  - **Regra:** todo gate é **legível**, ou seja, visível antes de poder ser superado.
- **Fluxo Guiado por Exploração:** os gates principais são **gates duros** que exigem a habilidade/ferramenta correspondente, incentivando o jogador a explorar as salas, se perder no mapa e descobrir conexões antes de avançar de zona (sem sequence breaks facilitados).

| Chave (Habilidade / Ferramenta) | Fechadura (Gate Legível) | Onde abre |
|---|---|---|
| **Kitty (Dutos / Terminais)** | Dutos estreitos e painéis offline | Prólogo (P4), Esgotos (Z1-04) e portas de serviço |
| **Wall Slide & Wall Jump *(1ª)*** | Poços verticais, chaminés, nichos e paredes paralelas | Esgotos (Z1-07, Z1-08) → superfície e torres |
| **Dash Tático** *(proposta)* | Vãos largos e corredores com lasers/armadilhas | Superfície (Lasers, rota alternativa ao DC 1) e Cidade Alta |
| **Bastão Eletrificado** *(proposta)* | Fusíveis/bobinas descarregadas, escudos leves e portas sem energia | Data centers, rotas secretas e o Setor Selado (Z1-11) |
| **Propulsor Aéreo (Pulo Duplo)** *(proposta)* | Plataformas isoladas sem parede de apoio | Cidade Alta (Telhados) e Laboratório (Antecâmara) |
| **Gancho Magnético** *(proposta)* | Pontos de ancoragem metálicos em altura | Torres da Domus Corp e telhados altos |

> A tabela ampliada, com onde cada chave é obtida e o tipo de gate, está em [level-map.md](level-map.md) §5. As distribuições marcadas *(proposta)* aguardam aprovação.

### 6.3 Abrigos, Mapa e Teletransporte
- **Abrigos (Postos da Resistência / Sobreviventes):** pontos de save, despertar pós-morte, recarga completa (vida/bateria/kits), fabricação/loja com peças e diálogos com NPCs.
- **Teletransporte Futurista *(Ato II+)*:** viagem rápida entre abrigos já descobertos, mediante **custo em peças** do jogador.
- **Mapa Criptografado:** obtido no fim do Ato I e decodificado por zonas/terminais conforme Dante explora, revelando salas visitadas e marcando gates ainda bloqueados.
- **Backtracking Recompensado:** atalhos unilaterais, segredos sugeridos pelo cenário (som/luz/assimetria) e mudanças físicas do colapso da IA (Seção 7).

## 7. Inimigos, Mente Coletiva, Colapso Gradual da IA e Bosses

### 7.1 Hierarquia de Inimigos
- **Robôs da Mente Coletiva** *(sofrem hesitação/travamento com o comando de Kitty)*:
  - **Operários:** lentos, padrão previsível (introduzem combate básico no Ato I).
  - **Patrulheiros / Batedores:** ágeis, dão alerta de posição (Ato I/II; alvo principal de distração/interferência).
  - **Policiais:** escudo frontal ou disparo à distância (Ato II; exigem flanqueio, Dash ou Bastão Eletrificado).
  - **Elite:** combos rápidos e alta resistência (Ato II/III e incursão ao laboratório).
- **Humanos Misantropos** *(não pertencem à colmeia → **imunes à interferência de Kitty**)*: combatentes ágeis e agressivos que quebram a dependência do jogador em Kitty e forçam domínio do combate corpo a corpo/bateria.
- **Robô batedor (valores atuais):** **4** de vida (2 golpes de bastão), dano por contato e **10** peças ao morrer. Enquanto hesita pela interferência de Kitty, o contato não causa dano (`ScoutRobotEnemy`).

### 7.2 Colapso Gradual da IA (Escalada de Caos no Mapa)
- Conforme Dante destrói **Data Centers** e bosses, a colmeia entra em colapso e **culmina em caos pelo mapa** (inspirado na transição de *Crossroads* em *Hollow Knight*):
  - Os inimigos remanescentes tornam-se **mais agressivos, erráticos e fortes** (sobrecarga de emergência / perda de contenção), elevando a tensão no backtracking.
  - Sistemas de contenção/barreiras antigas colapsam, abrindo **novas rotas, atalhos e segredos** em áreas já visitadas.
  - Sinais ambientais refletem o caos crescente (luzes de alerta, faíscas, terminais em pane e chuva/estática mais intensa).

### 7.3 Estrutura de Bosses *(escopo base ajustável durante a produção)*
- **5 Bosses Principais:**
  1. *Fim do Ato I (Vertical Slice):* **Primeiro Humano Misantropo** → rende o **Mapa Criptografado**.
  2. *Ato II (Data Center 1):* **Unidade Executora / Policial Pesado** → rende item/habilidade de progressão.
  3. *Ato II (Cidade Alta / Data Center 2):* **Unidade de Elite ou Líder Misantropo** → rende habilidade de mobilidade avançada.
  4. *Fim do Ato II (Invasão ao Laboratório):* **Incursão de Elite** → batalha de fuga com o Pai.
  5. *Ato III (Clímax):* **O Titã** → exige domínio de todo o repertório aprendido, com o Pai abrindo falhas no chassi e o apoio silencioso de Kitty.
- **2 Bosses Opcionais:** ligados a side quests e lore, rendendo acessórios e peças de história.

## 8. Controles, Progressão, Curva de Dificuldade e Especificações Técnicas

### 8.1 Controles e Input
- **Dispositivos suportados:** Controle (Gamepad — recomendado) e Teclado (+ Mouse opcional para mira/UI), com remapeamento futuro previsto.
- **Ações principais:** Mover, Pular (variável), Atacar (corpo a corpo), Disparar (bateria), Dash, Gancho, Usar Kit Médico, Fabricar, Comando de Kitty (interferência), Interagir, Mapa/Inventário.
- **Mapeamento atual** (`Assets/InputSystem_Actions.inputactions` + fallbacks em `GameInputReader`):

| Ação | Teclado / Mouse | Gamepad (Xbox / PS) |
|---|---|---|
| Mover | WASD / setas | Analógico esquerdo |
| Pular | Espaço | A / X |
| Atacar | Mouse Esquerdo / Enter *(J pendente — não mapeado)* | X / Quadrado |
| Disparar (`Shoot`) | Mouse Direito / F | RT / R2 |
| Kit Médico (`Heal`) | Q | LB / L1 |
| Fabricar (`Craft`) | C | D-Pad baixo |
| Interagir (`Interact`; também envia Kitty a dutos/terminais) | E | Y / Triângulo |
| Comando de Kitty (`CatCommand`) | R | RB / R1 |

### 8.2 Curva de Dificuldade e Ritmo
- **Ato I (Ensina):** introduz movimentação, bastão, bateria, comando de Kitty e o primeiro Humano Misantropo em ambiente controlado.
- **Ato II (Expande e Mistura):** combina robôs hierárquicos + misantropos + verticalidade + backtracking guiado pelo colapso da IA.
- **Ato III (Testa Maestria):** exige combinação fluida de mobilidade, combate e uso tático de Kitty até o Titã.

### 8.3 Especificações Técnicas (Nível GDD)
- **Plataforma:** PC (Steam), Windows.
- **Desempenho-alvo:** 60 FPS estáveis em 1080p durante combate intenso e chuva/partículas.
- **Visual e Áudio:** Pixel art orgânica e neo-noir (PixelLab MCP + ajustes manuais) com iluminação 2D (neon/poças) e áudio atmosférico por zona.

## 9. Fora de Escopo (v1.0) e Épicos de Desenvolvimento

### 9.1 Fora de Escopo para v1.0
- Multiplayer / co-op.
- Modos extras no lançamento (Boss Rush, Randomizer).
- Ports para consoles ou mobile antes da validação da versão PC (Steam).

### 9.2 Resumo dos Épicos de Desenvolvimento
Detalhamento completo em [docs/planning/epics.md](../../epics.md), a fonte usada pelos workflows BMAD. O [epics.md](epics.md) desta pasta é um resumo antigo, já substituído. Os Épicos **1 a 5** formam o **Vertical Slice do Ato I** (primeiro marco jogável):

| # | Épico | Foco | Marco |
|---|---|---|---|
| 1 | Fundação do Projeto | Input System, cenas base, arquitetura de dados | Vertical Slice |
| 2 | Movimentação Metroidvania | Corrida, pulo variável, game feel, Wall Slide/Jump | Vertical Slice |
| 3 | Combate e Recursos | Bastão, bateria, mochila de peças, kits médicos, morte | Vertical Slice |
| 4 | Kitty, a Companheira | Seguir, comando de interferência em robôs, interações | Vertical Slice |
| 5 | Vertical Slice do Ato I | Prólogo (Zona 0) → Esgotos (Zona 1) → superfície, abrigo, Wall Jump, 1º Misantropo, mapa criptografado | **Marco 1 (MVP)** |
| 6 | UI/UX Diegética | HUD neo-noir, menus, mapa criptografado legível | Pós-VS |
| 7 | Level Design e Zonas | Zonas 2–5 interconectadas, atalhos, gates de Dash/Pulo Duplo/Gancho | Pós-VS |
| 8 | Sistemas Avançados e Conteúdo | Evolução de arma, teletransporte, colapso da IA, bosses e save completo | Pós-VS |
