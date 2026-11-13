# Plano de Implementação do BMAD — Under The Rain

> **Para o agente (Antigravity):** execute este plano fase por fase, na raiz do repositório do jogo. Ao final de cada fase, pare, resuma o que fez e aguarde minha confirmação antes de seguir. Comunique-se comigo em **português do Brasil**. Código, nomes de arquivos e commits em **inglês**. Nunca sobrescreva arquivos existentes sem me perguntar antes.

---

## 0. Contexto do projeto

- **Repositório:** `desz2000/UnderTheRain` (https://github.com/desz2000/UnderTheRain), **novo e zerado**. É um projeto **greenfield**: não há código a migrar.
- **Repositório antigo (`desz2000/new-game-devtest`):** protótipo 2D simples em Unity, usado para aprender. Serve só como **referência opcional**; não copie código dele sem eu pedir.
- **Engine:** Unity 2D (confirmar a versão LTS a usar na Fase 1)
- **Objetivo atual:** transformar o jogo em um **metroidvania** chamado **Under The Rain**, com narrativa forte, level design melhor, novas mecânicas, personagens refinados e uma **UI com identidade própria** (a UI atual tem "cara de IA" e passa pouca credibilidade).
- **Ferramentas de agente:** posso trazer depois o agente `game-dev` e as skills de game design do repositório antigo. O BMAD deve **conviver** com elas, não substituí-las.
- **Fonte narrativa canônica:** Bíblia de Narrativa e Game Design "Under The Rain" (Google Doc). Será salva no repositório na Fase 2.

### Resumo narrativo (base para o `project-context.md`)

- **Mundo:** metrópole fictícia inspirada em São Paulo (não é SP de fato), ano 2100. Cidade cinzenta, garoa constante, prédios corporativos brutalistas. Uma IA coletiva (nascida de um código de livre-arbítrio implantado nos robôs pela **Domus Corp**) dominou o planeta. Humanos vivem em campos de trabalho forçado ou nos esgotos. Há uma facção de **Humanos Misantropos**, aliados das máquinas.
- **Protagonista, Dante (30 anos):** ex-desenvolvedor, combatente solitário. Parte sozinho após a resistência nos esgotos ignorar as provas que ele achou.
- **Gato de metal:** gatinho branco robótico isolado da rede. Companheiro **ativo** (nos moldes do BD-1 de Jedi: Fallen Order). Tem rede neural simples e não entende o próprio papel; robôs travam ou hesitam diante dele. Quem percebe isso, aos poucos, é o jogador/Dante.
- **Pai (55 anos):** dev da Domus Corp que criou o código de livre-arbítrio, vive escondido num laboratório secreto. É encontrado do meio para o final do jogo, com vestígios dele ao longo do caminho.
- **Mente coletiva:** todos os robôs compartilham a mesma mente, com tipos, funções e hierarquias diferentes (operários, policiais, patrulheiros, elite).
- **Clímax:** vírus criado com a matriz neural do gato quebra a colmeia; o **Titã** (avatar físico da IA) absorve a energia da rede e precisa ser destruído na força bruta.
- **Decisões de design já tomadas:** vários bosses ao longo do jogo (inclusive humanos misantropos) que rendem itens; bosses opcionais ligados à história; **colapso gradual da IA** conforme Dante destrói data centers e bosses; boss final usa o que o jogador aprendeu.
- **Gameplay-alvo:** combate corpo a corpo com lâmina de alta frequência + disparos à base de bateria; movimentação metroidvania (pulos dinâmicos, parkour, escalada, exploração vertical); narrativa ambiental; abrigos com comerciantes e sobreviventes.
- **Referências:** Hollow Knight, Katana Zero (jogabilidade); Owlboy e Katana Zero (pixel art, neon na chuva, poças refletindo luz).

---

## Fase 1 — Pré-checagem

1. Confirme que a pasta aberta é o clone de `desz2000/UnderTheRain` (`git remote -v`). Se a pasta não for um repositório Git, me avise.
2. Confirme a branch atual e crie uma branch de trabalho: `feature/bmad-setup`.
3. Verifique `node -v` (**precisa ser v20+**). Se faltar, me avise e pare.
4. Inspecione o que já existe (provavelmente só README, licença e/ou `.gitignore`) e me apresente um resumo curto:
   - há `.gitignore` para Unity? Se não, proponha o padrão oficial de Unity (ignorar `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`; **não** ignorar `.meta`) e crie-o após minha aprovação.
   - há `ProjectSettings/` ou `Assets/`? Se **não**, o projeto Unity ainda não foi criado: me diga qual versão do Unity está instalada na minha máquina e eu crio o projeto 2D pelo Unity Hub nesta pasta. **Não tente gerar o projeto Unity pela linha de comando.**
   - pastas de agentes (`.claude/`, `.agent/`, `AGENTS.md`, `CLAUDE.md`).
5. Não altere nada além do `.gitignore` aprovado. Termine com a lista de pontos de atenção.

## Fase 2 — Instalação do BMAD

1. Antes de instalar, **confirme a documentação atual**: `npx bmad-method install --help` e, se disponível, `--list-options`. A versão e as flags mudam com frequência; priorize o que o CLI instalado mostrar sobre o que está escrito aqui.
2. Instale na raiz do projeto com o comando abaixo (o instalador interativo é o caminho preferido; se o terminal do Antigravity não suportar prompts, use o modo não interativo):

   ```bash
   npx bmad-method install
   ```

   Seleções desejadas:
   - **Módulos:** BMad Method (BMM) + **Game Dev Studio (BMGD)** (workflows para Unity). Opcional: Creative Intelligence Suite (CIS) para brainstorming. Não instale TEA agora.
   - **Ferramenta/IDE:** Antigravity, se constar na lista do instalador. Se não constar, escolha a opção genérica ou a mais próxima e me avise.
   - **Idioma de comunicação e de documentos gerados:** Português (Brasil).
   - **Nome do projeto:** Under The Rain. **Engine:** Unity.
   - **Pastas de saída:** planejamento em `docs/planning/` e implementação em `docs/implementation/` (ajuste se o instalador usar outro padrão e me diga).
3. Verifique o que foi criado (pasta do BMAD, agentes, workflows, arquivos de integração com o Antigravity) e me liste.
4. **Conflitos:** se o instalador tocar em `AGENTS.md`, `CLAUDE.md` ou em pastas de agentes existentes, mostre o diff e **peça confirmação** antes de manter.
5. Faça o commit: `chore: install BMAD method (BMM + BMGD)`.

## Fase 3 — Contexto do jogo dentro do BMAD

1. **Bíblia de narrativa:** crie `docs/narrativa/` e salve nela a bíblia como `biblia-narrativa.md`. O texto completo está no Google Doc que vou colar/anexar; **não invente nem resuma** conteúdo narrativo. Se o arquivo ainda não estiver presente, pare e me peça.
2. Crie `project-context.md` (ou o arquivo equivalente que o BMAD usar para contexto persistente de projeto) contendo:
   - o **resumo narrativo** da seção "Contexto do projeto" deste plano;
   - stack e convenções (Seção 6 deste plano);
   - regras de colaboração: a narrativa é **canônica**; qualquer mudança de história exige minha aprovação explícita.
3. **Greenfield:** não rode workflow de documentação de código existente (`document-project`), pois ainda não há código. Esse passo só entra se, no futuro, eu portar algo do repositório antigo.
4. Commit: `docs: add narrative bible and project context`.

## Fase 4 — Planejamento (BMGD)

Use os agentes e workflows do módulo de games **instalados** (verifique os nomes reais com o help do BMAD). Para cada etapa abaixo, rode o workflow, converse comigo e só avance após aprovação:

1. **Game Brief** de Under The Rain, partindo da bíblia (não recomeçar a ideia do zero).
2. **GDD (Game Design Document)**, cobrindo o que a bíblia **ainda não define** (ela é só narrativa): sistemas de combate, movimentação, progressão metroidvania (habilidades que abrem áreas), economia de itens, comportamento dos tipos de robôs, mecânica do gato, estrutura de bosses e opcionais, colapso gradual da IA.
3. **Design narrativo:** transformar a estrutura em 3 atos em um mapa de beats por região/zona. Pistas do pai, bosses principais e opcionais, comerciantes e abrigos.
4. **UI/UX:** documento de direção visual da UI (diegética, coerente com neo-noir, garoa, neon e pixel art). Meta explícita: **sair do visual genérico de "cara de IA"**. Listar referências, paleta, tipografia e HUD.
5. **Arquitetura de jogo** para Unity 2D: estrutura de pastas, ScriptableObjects para dados (itens, inimigos, habilidades), máquina de estados do jogador, sistema de combate, IA dos robôs (incluindo a reação ao gato), save/load, cenas e carregamento, input system.
6. Commit por documento aprovado: `docs: add <nome-do-documento>`.

## Fase 5 — Backlog inicial (épicos e histórias)

Gere os épicos com o workflow do BMAD e proponha esta ordem como **ponto de partida** (ajustável por mim):

| # | Épico | Objetivo |
|---|-------|----------|
| 1 | Fundação do projeto | Projeto Unity 2D configurado, estrutura de pastas, `.gitignore`, input system, cenas base, arquitetura-alvo |
| 2 | Movimentação metroidvania | Pulo dinâmico, parede, escalada, dash, controle fino |
| 3 | Combate | Lâmina de alta frequência, disparo à base de bateria, feedback de impacto |
| 4 | Gato companheiro | Seguir/explorar, interações no cenário, robôs que hesitam e travam |
| 5 | **Vertical slice do Ato I** | Esgotos → transição → superfície → primeiro Humano Misantropo, com mapa criptografado |
| 6 | UI/UX | HUD, menus e identidade visual conforme o documento de direção |
| 7 | Level design | Passada de refinamento dos mapas; layouts com gates de habilidade |
| 8 | Sistemas (itens, bosses, save) | Itens de boss, opcionais, persistência |

Regras do backlog:
- O **vertical slice do Ato I** é o primeiro marco jogável; priorize os épicos 1 a 5 antes de ampliar o escopo.
- Cada história deve ter critérios de aceite **verificáveis no Unity** (o que eu vejo/faço no Play Mode).
- Rode o workflow de sprint planning e me mostre o resultado antes de começar a implementar.

## Fase 6 — Ciclo de implementação

Para cada história, use o ciclo do BMAD: **criar história → implementar → revisar código → (QA) → concluir**.

- Uma branch por épico (`feature/epic-N-nome`), commits por história.
- Antes de implementar, o agente confirma comigo qualquer decisão de design ou narrativa que não esteja nos documentos.
- Escrever testes (Unity Test Framework: EditMode para lógica, PlayMode para comportamento) quando fizer sentido.
- Ao final de cada épico: retrospectiva curta e atualização do `project-context.md`.

---

## 6. Convenções técnicas (Unity)

- Não commitar `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`; **sempre** commitar os `.meta`.
- Não editar cenas/prefabs `.unity`/`.prefab` na mão (YAML). Se for necessário alterar, descreva o passo a seguir no editor ou use scripts de editor.
- Dados de jogo em **ScriptableObjects**; evitar números mágicos e singletons desnecessários.
- Separar lógica de apresentação (UI, animação, VFX).
- Nomes de classes, métodos e arquivos em inglês; comentários curtos e úteis.

## 7. Regras do agente

1. **Narrativa é canônica.** Não invente personagens, reviravoltas ou lore. Se faltar algo, pergunte.
2. Uma pergunta por vez quando precisar de decisão minha; ofereça 2–3 alternativas com consequências.
3. Não rode comandos destrutivos (`git reset --hard`, `git clean -fd`, remoção de pastas) sem confirmação.
4. Se algo na documentação do BMAD divergir deste plano, **siga a documentação** e me avise.
5. Mantenha as respostas curtas: o que fez, o que mudou, o que precisa de mim.

## 8. Critérios de conclusão do setup (Fases 1 a 3)

- [ ] BMAD instalado, com BMM + BMGD, em português
- [ ] `.gitignore` de Unity aprovado e commitado; projeto Unity criado por mim (ou decisão registrada sobre quando criar)
- [ ] `docs/narrativa/biblia-narrativa.md` e `project-context.md` presentes
- [ ] Commits organizados na branch `feature/bmad-setup`
- [ ] Lista de próximos passos apresentada para eu aprovar a Fase 4
