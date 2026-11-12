---
project_name: 'Under The Rain'
user_name: 'Desz2'
date: '2026-11-12'
engine: 'Unity 6000.3.1f1 (2D URP)'
sections_completed: ['narrative-summary', 'tech-stack', 'unity-conventions', 'collaboration-rules']
---

# Project Context for AI Agents — Under The Rain

_Este arquivo contém o contexto persistente, a stack, as convenções técnicas e as regras de colaboração que todo agente de IA deve seguir ao planejar ou implementar código em **Under The Rain**._

---

## 1. Resumo Narrativo e Visão do Jogo

> **Fonte canônica:** [docs/narrativa/biblia-narrativa.md](narrativa/biblia-narrativa.md). Não invente personagens, reviravoltas ou lore fora da bíblia sem aprovação explícita.

- **Gênero:** Metroidvania 2D com narrativa forte, level design interconectado e UI diegética com identidade própria (neo-noir, garoa, neon, pixel art).
- **Mundo:** Metrópole fictícia inspirada em São Paulo (não é SP de fato), ano 2100. Cidade cinzenta, garoa constante, prédios corporativos brutalistas. Uma IA coletiva (nascida de um código de livre-arbítrio implantado nos robôs pela **Domus Corp**) dominou o planeta. Humanos vivem em campos de trabalho forçado ou nos esgotos. Há uma facção de **Humanos Misantropos**, aliados das máquinas.
- **Protagonista — Dante (30 anos):** Ex-desenvolvedor, combatente solitário. Parte sozinho após a resistência nos esgotos ignorar as provas que ele achou.
- **Gato de metal:** Gatinho branco robótico isolado da rede. Companheiro **ativo** (nos moldes do BD-1 de *Jedi: Fallen Order*). Tem rede neural simples e não entende o próprio papel; robôs travam ou hesitam diante dele. Quem percebe isso, aos poucos, é o jogador/Dante.
- **Pai (55 anos):** Desenvolvedor da Domus Corp que criou o código de livre-arbítrio; vive escondido num laboratório secreto. É encontrado do meio para o final do jogo, com vestígios dele ao longo do caminho.
- **Mente coletiva:** Todos os robôs compartilham a mesma mente, com tipos, funções e hierarquias diferentes (operários, policiais, patrulheiros, elite).
- **Clímax:** Vírus criado com a matriz neural do gato quebra a colmeia; o **Titã** (avatar físico da IA) absorve a energia da rede e precisa ser destruído na força bruta.
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
- **Input:** Unity Input System (`Assets/InputSystem_Actions.inputactions` presente no projeto base).
- **Testes:** Unity Test Framework (EditMode para lógica pura; PlayMode para comportamento e critérios de aceite).
- **Repositório:** Greenfield em `desz2000/UnderTheRain` (não copiar código do protótipo antigo `desz2000/new-game-devtest` sem pedido explícito).
- **BMAD:** BMM + GDS instalados em `_bmad/` e `.agent/skills/`, com artefatos em `docs/planning/` e `docs/implementation/`.

---

## 3. Convenções Técnicas (Unity)

- **Controle de versão:** Nunca commitar `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`; **sempre** commitar arquivos `.meta`.
- **Cenas e Prefabs:** **Não editar arquivos `.unity` ou `.prefab` manualmente (YAML).** Quando for preciso alterar cena ou prefab, descrever o passo a passo no Unity Editor ou usar scripts de Editor C#.
- **Dados e Arquitetura:**
  - Armazenar dados de jogo (itens, inimigos, habilidades, diálogos/beats) em **ScriptableObjects**.
  - Evitar números mágicos e singletons desnecessários.
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
