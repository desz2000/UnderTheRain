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
**Referências base:** [Game Brief](../../briefs/brief-Under-The-Rain-2026-11-12/brief.md) · [Bíblia Narrativa](../../../narrativa/biblia-narrativa.md) · [Project Context](../../../project-context.md)

---

## 1. Resumo Executivo e Contexto

- **Conceito Central:** *"Em uma metrópole neo-noir sufocada pela garoa constante, você é um combatente solitário acompanhado por um gatinho de metal que desafia uma mente coletiva de máquinas, escalando dos esgotos até o topo das torres corporativas para descobrir a verdade sobre o colapso e o mistério da morte do próprio pai."*
- **Público-Alvo:** Jogadores de PC (Steam, 18–35 anos) fãs de Metroidvanias e ação 2D atmosférica que buscam exploração interconectada, narrativa ambiental forte e desafio justo/acessível (não punitivo como soulslike).
- **Escopo-Alvo:** Campanha enxuta e polida (~4–6 horas), estruturada em 3 atos. Primeiro marco jogável: **Vertical Slice do Ato I**.

---

## 2. Core Gameplay (Pilares, Loop e Condições)

### Pilares de Design
1. **Movimentação Expressiva Antes de Tudo:** Controle ágil e responsivo (pulo variável, *coyote time*, *input buffering*, parkour, escalada, exploração vertical).
2. **Combate Ágil e Progressivo:** De equipamento inicial simples (bastão) até lâmina de alta frequência, combinando corpo a corpo, disparos à base de bateria e acessórios.
3. **Companheiro que Quebra a Colmeia:** O gatinho de metal atua na exploração e faz robôs da mente coletiva hesitarem ou travarem, sem tutoriais expositivos.
4. **Mundo em Colapso Gradual e Narrativa Ambiental:** Mapa contínuo onde abrigos marcam o ritmo (tensão → alívio), data centers/bosses derrubados alteram zonas antigas e NPCs/bosses/documentos/itens revelam a história.

### Core Gameplay Loop
Explorar zonas verticais interconectadas → enfrentar inimigos (lâmina/bastão + bateria + gato) → encontrar gates legíveis, NPCs, documentos e pistas → derrotar bosses / destruir data centers → obter habilidades, armas/acessórios e enfraquecer a IA → retornar por atalhos e abrigos para abrir novos caminhos, ver o mundo transformado e resolver mistérios.

---

## 3. Movimentação e Game Feel

### 3.1 Movimentação Base (desde o início)
- **Corrida horizontal:** aceleração (~0,08s) e desaceleração (~0,06s) quase instantâneas, priorizando responsividade e parada seca em plataformas estreitas.
- **Pulo variável:** altura controlada pelo tempo de pressão do botão (soltar cedo corta a velocidade vertical).
- **Assistências de Game Feel:**
  - *Coyote Time:* ~100 ms (6 frames a 60 FPS) após deixar uma borda.
  - *Input Buffering:* ~120 ms (7 frames) antes de tocar o chão ou parede.
  - *Corner Correction:* leve ajuste automático em quinas de teto para não travar o salto.
  - *Drop-through:* agachar + pulo permite descer por plataformas finas.

### 3.2 Habilidades de Mobilidade Desbloqueáveis (4 no jogo completo)
1. **Wall Slide & Wall Jump *(1ª habilidade — Vertical Slice Ato I)*:** deslizar em paredes verticais com atrito controlado e saltar entre paredes. Sustenta a subida dos esgotos para a superfície e salas verticais.
2. **Dash Tático:** impulso horizontal rápido com curto cooldown e janela breve de esquiva; abre vãos largos e reposiciona no combate.
3. **Propulsor Aéreo (Pulo Duplo):** salto adicional no ar para alcance vertical extra e extensão de combate aéreo.
4. **Gancho Magnético:** projétil/ancoragem em pontos metálicos elevados para travessias longas e verticais nas zonas da Cidade Alta e torres da Domus Corp.

## 4. Combate, Armamento e Recursos (Vida, Bateria, Cura, Morte)

### 4.1 Game Feel de Combate
- **Impacto:** *hitstop* de ~40–60 ms em acertos críticos/pesados, *screenshake* sutil, partículas de faísca/sangue e *flash* de dano legível.
- **Invulnerabilidade pós-dano:** ~0,8s com recuo curto (*knockback*) previsível.
- **Telegrafia:** todo ataque inimigo possui antecipação visual e sonora clara antes do frame ativo.

### 4.2 Evolução da Arma Principal (3 Estágios)
1. **Bastão Simples *(Início / Ato I)*:** combo curto de 2–3 golpes, alcance curto/médio, ferramenta básica de defesa.
2. **Bastão Eletrificado *(Ato I/II)*:** acoplamento obtido após descoberta/item específico; adiciona carga elétrica com atordoamento leve em robôs e ataque carregado.
3. **Lâmina de Alta Frequência *(Ato II/III)*:** evolução avançada via acoplamento/descoberta; golpes mais velozes, alcance ampliado e capacidade de deflexão/corte preciso.
- *Acessórios e sub-armas:* desbloqueáveis via exploração, loja de abrigo e bosses.

### 4.3 Bateria, Cura, Mochila de Peças e Morte
- **Disparo à base de Bateria:** ataque à distância limitado por carga de bateria (usada também em interações específicas de exploração). Recarrega 100% nos abrigos e parcialmente via células dropadas por inimigos.
- **Cura (Kit Médico):** recurso separado da bateria — começa com 3 cargas fixas, recarregáveis nos abrigos ou fabricáveis usando peças.
- **Mochila de Peças (Economia Unificada):** peças coletadas de robôs derrotados servem simultaneamente como **moeda do jogo**, **material para fabricar kits médicos** e **munição/insumo para armas acopladas** adquiridas ao longo da jornada.
- **Morte e Recuperação:** ao morrer, Dante acorda no último **abrigo/posto seguro** (postos da resistência ou de sobreviventes) mantendo mapa, habilidades e equipamentos; a **mochila de peças** fica caída no local exato da morte para ser recuperada.

## 5. O Gatinho de Metal (Mecânicas de Companheiro)

### 5.1 Presença e Comportamento
- **Companheiro Ativo:** acompanha Dante pelo cenário, sobe no ombro/mochila durante escaladas e combates intensos, e reage sutilmente (olhar/som) perto de segredos ou perigos.

### 5.2 Anomalia na Mente Coletiva (Comando Ativo)
- **Comando Tático de Interferência:** o jogador pode acionar um botão para enviar o gato até um robô ou ponto próximo, causando uma breve **hesitação/travamento** (~1,0–1,5s de "erro de leitura", com cooldown para manter o equilíbrio do combate).
- **Leitura Narrativa:** o jogador utiliza a mecânica taticamente desde o início, mas nem Dante nem o gato entendem *por que* isso acontece — o significado real da matriz neural isolada só se revela aos poucos até o reencontro com o Pai.

### 5.3 Interações de Exploração (distribuídas ao longo dos 3 Atos)
1. **Dutos e espaços pequenos:** atravessar passagens estreitas para abrir atalhos pelo outro lado ou coletar itens/documentos.
2. **Distração tática:** emitir sinal visual/sonoro para chamar atenção e reposicionar patrulhas.
3. **Leitura de terminais:** escanear e destravar terminais antigos e portas de serviço fora da rede.

## 6. Design Específico de Metroidvania (Mapa Macro, Chave × Fechadura, Backtracking e Abrigos)

### 6.1 Mapa Macro (5 Zonas Interconectadas em Salas)
Cada zona é composta por diversas salas interconectadas para exploração livre (ao estilo *Hollow Knight*), organizadas em eixo vertical:
1. **Esgotos e Subsolo *(Ato I)*:** base da resistência, umidade, túneis de serviço e dutos estreitos.
2. **Ruínas da Superfície *(Ato I → II)*:** transição, ruas alagadas, garoa aberta, primeiro Humano Misantropo e obtenção do mapa criptografado.
3. **Cidade Alta e Setor Corporativo *(Ato II)*:** prédios brutalistas, neon, patrulhas densas e data centers locais da IA.
4. **Laboratório Secreto do Pai *(Ato II — Ponto Médio)*:** setor oculto sob o setor corporativo, conectado por atalho ao subsolo.
5. **Torre Central da Domus Corp *(Ato III)*:** escalada final até o núcleo da colmeia e o Titã.

### 6.2 Tabela Chave × Fechadura e Fluxo de Progressão
- **Fluxo Guiado por Exploração:** os gates principais são **gates duros** que exigem a habilidade/ferramenta correspondente, incentivando o jogador a explorar as salas, se perder no mapa e descobrir conexões antes de avançar de zona (sem sequence breaks facilitados).

| Chave (Habilidade / Ferramenta) | Fechadura (Gate Legível) | Onde abre |
|---|---|---|
| **Gato (Dutos / Terminais)** | Dutos estreitos e painéis offline | Atalhos nos esgotos e portas de serviço |
| **Wall Slide & Wall Jump *(1ª)*** | Poços verticais e paredes paralelas | Subida dos esgotos → superfície e torres |
| **Dash Tático** | Vãos largos e corredores com lasers/armadilhas | Superfície e Cidade Alta |
| **Bastão Eletrificado** | Fusíveis/bobinas descarregadas e escudos leves | Data centers e rotas secretas |
| **Propulsor Aéreo (Pulo Duplo)** | Plataformas elevadas sem parede de apoio | Cidade Alta e Laboratório |
| **Gancho Magnético** | Pontos de ancoragem metálicos em altura | Torres da Domus Corp e atalhos verticais |

### 6.3 Abrigos, Mapa e Teletransporte
- **Abrigos (Postos da Resistência / Sobreviventes):** pontos de save, despertar pós-morte, recarga completa (vida/bateria/kits), fabricação/loja com peças e diálogos com NPCs.
- **Teletransporte Futurista *(Ato II+)*:** viagem rápida entre abrigos já descobertos, mediante **custo em peças** do jogador.
- **Mapa Criptografado:** obtido no fim do Ato I e decodificado por zonas/terminais conforme Dante explora, revelando salas visitadas e marcando gates ainda bloqueados.
- **Backtracking Recompensado:** atalhos unilaterais, segredos sugeridos pelo cenário (som/luz/assimetria) e mudanças físicas do colapso da IA (Seção 7).

## 7. Inimigos, Mente Coletiva, Colapso Gradual da IA e Bosses

### 7.1 Hierarquia de Inimigos
- **Robôs da Mente Coletiva** *(sofrem hesitação/travamento com o comando do gato)*:
  - **Operários:** lentos, padrão previsível (introduzem combate básico no Ato I).
  - **Patrulheiros / Batedores:** ágeis, dão alerta de posição (Ato I/II; alvo principal de distração/interferência).
  - **Policiais:** escudo frontal ou disparo à distância (Ato II; exigem flanqueio, Dash ou Bastão Eletrificado).
  - **Elite:** combos rápidos e alta resistência (Ato II/III e incursão ao laboratório).
- **Humanos Misantropos** *(não pertencem à colmeia → **imunes à interferência do gato**)*: combatentes ágeis e agressivos que quebram a dependência do jogador no gato e forçam domínio do combate corpo a corpo/bateria.

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
  5. *Ato III (Clímax):* **O Titã** → exige domínio de todo o repertório aprendido, com o Pai abrindo falhas no chassi e o apoio silencioso do gato.
- **2 Bosses Opcionais:** ligados a side quests e lore, rendendo acessórios e peças de história.

## 8. Controles, Progressão, Curva de Dificuldade e Especificações Técnicas

### 8.1 Controles e Input
- **Dispositivos suportados:** Controle (Gamepad — recomendado) e Teclado (+ Mouse opcional para mira/UI), com remapeamento futuro previsto.
- **Ações principais:** Mover, Pular (variável), Atacar (corpo a corpo), Disparar (bateria), Dash, Gancho, Usar Kit Médico, Comando do Gato (interferência / interação contextual), Interagir, Mapa/Inventário.

### 8.2 Curva de Dificuldade e Ritmo
- **Ato I (Ensina):** introduz movimentação, bastão, bateria, comando do gato e o primeiro Humano Misantropo em ambiente controlado.
- **Ato II (Expande e Mistura):** combina robôs hierárquicos + misantropos + verticalidade + backtracking guiado pelo colapso da IA.
- **Ato III (Testa Maestria):** exige combinação fluida de mobilidade, combate e uso tático do gato até o Titã.

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
Detalhamento completo em [epics.md](epics.md). Os Épicos **1 a 5** formam o **Vertical Slice do Ato I** (primeiro marco jogável):

| # | Épico | Foco | Marco |
|---|---|---|---|
| 1 | Fundação do Projeto | Input System, cenas base, arquitetura de dados | Vertical Slice |
| 2 | Movimentação Metroidvania | Corrida, pulo variável, game feel, Wall Slide/Jump | Vertical Slice |
| 3 | Combate e Recursos | Bastão, bateria, mochila de peças, kits médicos, morte | Vertical Slice |
| 4 | Gato Companheiro | Seguir, comando de interferência em robôs, interações | Vertical Slice |
| 5 | Vertical Slice do Ato I | Esgotos → superfície, abrigo, 1º Misantropo, mapa criptografado | **Marco 1 (MVP)** |
| 6 | UI/UX Diegética | HUD neo-noir, menus, mapa criptografado legível | Pós-VS |
| 7 | Level Design e Zonas | 5 zonas interconectadas, atalhos, gates de Dash/Pulo Duplo/Gancho | Pós-VS |
| 8 | Sistemas Avançados e Conteúdo | Evolução de arma, teletransporte, colapso da IA, bosses e save completo | Pós-VS |
