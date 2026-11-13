---
title: 'Game Brief: Under The Rain'
status: complete
created: 2026-11-12
updated: 2026-11-13
---

# Game Brief: Under The Rain

> Documento fundacional para o GDD (`gds-gdd`). Narrativa canônica em [biblia-narrativa.md](../../../narrativa/biblia-narrativa.md) e pilares em [project-context.md](../../../project-context.md).

## Resumo Executivo

**Under The Rain** é um Metroidvania 2D neo-noir em pixel art para PC (Steam), ambientado em uma metrópole brutalista inspirada em São Paulo no ano de 2100. O jogador controla **Dante**, um ex-desenvolvedor que abandona a resistência nos esgotos ao lado de seu **gatinho de metal** para investigar o colapso causado por uma IA coletiva e o mistério da morte do próprio pai. O jogo combina exploração vertical interconectada, combate ágil progressivo e um mundo que se transforma fisicamente conforme a IA colapsa.

---

## 1. Visão e Fantasia Central

- **Fantasia Central:** *"Em uma metrópole neo-noir sufocada pela garoa constante, você é um combatente solitário acompanhado por um gatinho de metal que desafia uma mente coletiva de máquinas, escalando dos esgotos até o topo das torres corporativas para descobrir a verdade sobre o colapso e o mistério da morte do próprio pai."*
- **Experiência Emocional (Contraste Intencional):** O jogo alterna momentos de **melancolia, solidão e descoberta** (exploração sob a chuva, abrigos com sobreviventes, vínculo silencioso com o gato e rastros do pai) com picos de **tensão ágil e precisão cirúrgica** no combate.
- **Declaração de Visão:** Entregar um Metroidvania 2D enxuto, polido e acessível, onde narrativa, mecânica do companheiro e colapso gradual do mundo caminham juntos.

---

## 2. Jogadores-Alvo e Mercado (PC / Steam)

- **Plataforma e Propósito:** Projeto indie solo focado em **PC (Steam)**, com meta de lançamento comercial caso atinja o nível de qualidade pretendido.
- **Público Primário:** Jogadores de PC (18–35 anos) fãs de Metroidvanias e ação 2D atmosférica (*Hollow Knight*, *Katana Zero*, *Ori*, *Owlboy*) que valorizam exploração interconectada, narrativa ambiental e combate responsivo, mas buscam um **desafio justo e acessível** em vez de punição extrema.
- **Público Secundário:** Jogadores atraídos por ficção científica neo-noir melancólica e experiências com companheiro ativo.
- **Expectativa de Sessão:** Sessões de 30–90 minutos com ritmo claro entre exploração/tensão e descanso nos abrigos.
- **Gancho de Mercado:** Um Metroidvania neo-noir de desafio equilibrado onde **um companheiro robótico perturba a mente coletiva dos inimigos** e **o mundo muda fisicamente conforme a IA colapsa**.

---

## 3. Fundamentos de Gameplay e Pilares

- **Gênero:** Metroidvania 2D de ação e exploração narrativa.
- **Core Loop:** Explorar zonas verticais interconectadas → enfrentar inimigos (combate corpo a corpo + bateria + gato) → encontrar gates legíveis, NPCs, documentos e pistas ambientais → derrotar bosses / data centers → desbloquear habilidades, armas/acessórios e enfraquecer a IA → retornar por atalhos e abrigos para abrir novos caminhos, ver o mundo transformado e resolver mistérios da história.
- **Progressão Inicial → Avançada:** Dante começa limitado em golpes, armamento (ex.: bastão simples) e mobilidade, evoluindo gradualmente para equipamentos mais letais (bastão eletrificado / lâmina de alta frequência, novas armas e acessórios) e técnicas avançadas de movimentação.
- **4 Pilares de Gameplay:**
  1. **Movimentação Expressiva Antes de Tudo:** Controle ágil e responsivo (pulo variável, parkour, escalada, exploração vertical) onde atravessar a cidade chuvosa é prazeroso por si só.
  2. **Combate Ágil e Progressivo:** Evolução de arma corpo a corpo simples até lâmina de alta frequência, combinada a disparos à base de bateria e acessórios desbloqueáveis.
  3. **Companheiro que Quebra a Colmeia:** O gatinho de metal atua ativamente na exploração e faz robôs da mente coletiva hesitarem ou travarem, criando janelas táticas e descoberta gradual sem tutoriais expositivos.
  4. **Mundo em Colapso Gradual e Narrativa Ambiental:** Mapa contínuo onde abrigos marcam o ritmo (tensão → alívio) e a destruição de data centers/bosses muda áreas antigas, enquanto NPCs, bosses, documentos, itens e o próprio cenário revelam a história.

---

## 4. Referências e Diferenciação

- **Hollow Knight:** *Trazemos* mundo contínuo interconectado, gates legíveis, ritmo de abrigos e descoberta. *Deixamos de fora* punição extrema/frustrante e lore excessivamente críptica.
- **Katana Zero:** *Trazemos* estética neo-noir, neon refletido na garoa/poças e combate ágil de impacto cortante. *Deixamos de fora* estrutura linear por fases e morte em 1 golpe (*one-hit kill*).
- **Owlboy:** *Trazemos* pixel art orgânica e expressiva, contraste de luz acolhedora na melancolia e sinergia ativa com o companheiro. *Deixamos de fora* tom de fantasia leve.
- **Jedi: Fallen Order (BD-1):** *Trazemos* companheiro robótico ativo, carismático e útil na exploração. *Deixamos de fora* escala 3D e combate lento baseado em stamina.
- **Diferenciais Genuínos:**
  1. Gatinho de metal que perturba a mente coletiva dos robôs (mecânica e narrativa unidas).
  2. Colapso gradual da IA com impacto visível nas zonas já exploradas.
  3. Progressão acessível e tátil com mistérios revelados por NPCs, bosses, documentos e cenário.

---

## 5. Escopo e MVP (Vertical Slice do Ato I)

- **Equipe e Stack:** Projeto solo (dev + agentes de IA) em **Unity 6000.3.1f1 (2D URP)**, sem prazo comercial rígido.
- **Escopo-Alvo do Jogo Completo:** Metroidvania **enxuto e polido** (~4–6 horas de campanha), estruturado em 3 atos com zonas verticais interconectadas, bosses principais e opcionais.
- **MVP — Vertical Slice do Ato I (Primeiro Marco Jogável):**
  - Trecho dos esgotos + transição para a superfície.
  - Movimento e combate base (bastão inicial + bateria básica) com game feel aprovado.
  - 1 habilidade nova, 1 gate legível e 1 retorno recompensado.
  - Gato ativo em pelo menos 1 interação (ex.: travar/hesitar robô batedor).
  - 1 abrigo funcional e o primeiro Humano Misantropo como boss (rendendo o mapa criptografado).
  - HUD/UI com identidade neo-noir própria (sem visual genérico).

---

## 6. Conteúdo, Mundo e Direção (Arte, UI e Áudio)

- **Mundo e Narrativa:** Metrópole brutalista em 2100 sob garoa constante (esgotos → ruínas da superfície → cidade alta / Domus Corp → laboratório secreto → torre central). História entregue por exploração, NPCs, bosses, documentos, itens e cenário.
- **Direção de Arte e UI:** Pixel art orgânica e neo-noir (chuva, neon refletido em poças, contraste de luz quente nos abrigos). Sprites produzidos com apoio do **PixelLab (MCP)** e ajustes/geração manual quando conveniente. UI diegética com identidade própria.
- **Direção de Áudio:** Garoa abafada nos esgotos vs. aberta na superfície, silêncios melancólicos nos abrigos, impacto seco no combate e sinais sonoros sutis do gato — produzidos com apoio de agentes/ferramentas de áudio design.

---

## 7. Riscos e Perguntas em Aberto para o GDD

- **Riscos Principais:**
  1. *Escopo de arte e animação para dev solo:* mitigado mantendo o jogo enxuto (~4–6h) e validando o pipeline com PixelLab no Vertical Slice.
  2. *Legibilidade do efeito do gato sem tutorial invasivo:* requer playtest e feedback visual/sonoro claro quando robôs hesitam.
  3. *Consistência visual da pixel art gerada:* requer paleta e resolução-base rígidas definidas no documento de UI/Arte.
- **Perguntas para resolver no GDD (`gds-gdd`):**
  - Lista exata de habilidades de mobilidade e evolução de armas/acessórios (tabela chave × fechadura).
  - Regras de bateria, cura e custo de morte nos abrigos.
  - Hierarquia exata de robôs, reação ao gato e estágios do colapso gradual da IA.
  - Estrutura de bosses principais e opcionais.
