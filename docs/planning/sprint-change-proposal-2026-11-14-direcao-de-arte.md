---
title: 'Sprint Change Proposal — Direção de arte: migração para 2,5D com o Prólogo como MVP'
date: 2026-11-14
workflow: gds-correct-course
mode: batch
scope: major
status: partially-approved
supersedes: 'Fase B de sprint-change-proposal-2026-11-14.md'
---

# Sprint Change Proposal — Direção de arte, 2,5D e Prólogo como MVP

> **Status (2026-11-14):** direção decidida pelo usuário: **migrar para 2,5D** e tratar o **Prólogo como MVP**, antes de projetar as outras zonas. Histórias, ordem e textos de regra continuam *(proposta)* até a aprovação. Substitui a Fase B da proposta anterior do mesmo dia. As Fases A (feita) e C (opção B aprovada) não mudam.

## 1. Gatilho

O usuário rejeitou a estética da arte de mapa procedural: o level design vai bem, mas o jogo tem aparência de projeto de faculdade. A direção desejada é **pixel art moderno e sofisticado, com muitos detalhes e jogo de luz**, com *Hollow Knight*, *Replaced* e *SacriFire* como referências. Não precisa igualar o acabamento de *Replaced*; precisa deixar de parecer protótipo.

## 2. Causa raiz

| # | Achado | Evidência |
|---|---|---|
| 1 | Não existe documento de direção de arte do mundo | Brief e GDD têm uma linha ("pixel art orgânica e neo-noir, iluminação 2D"); o `DESIGN.md` cobre só HUD e menus |
| 2 | Um risco previsto no brief não foi tratado | Brief §7, risco 3: a consistência da pixel art gerada exigia paleta e resolução-base rígidas |
| 3 | A rejeição do lote do PixelLab foi resolvida trocando a ferramenta, não travando a paleta | Fase B da proposta anterior; regras 29–31 do `project-context.md` |
| 4 | O método procedural limita o detalhe | `ZoneArtKit`: tiles 64×64 feitos de retângulos e ruído, rampa de 7 cinzas sem desvio de matiz |
| 5 | Não há iluminação | Zero luzes, zero Volume/bloom, zero normal maps em todas as cenas; brilho = sprite a 16% de opacidade |
| 6 | Iluminação está planejada como último passo | `project-context.md` §5.13, item 5 |
| 7 | Pixels desiguais | Câmera 5,4 × 32 PPU = 345,6 px de altura, escala 3,125× em 1080p; `ZoneArtKit.Fit` estica sprites |
| 8 | Personagens sem arte | Dante, Kitty e robôs usam `WhiteSquare.png` tingido; a 5.12 é *(proposta)* e não está no `sprint-status.yaml` |
| 9 | As referências são 2,5D | *Replaced* e *SacriFire* usam sprites de pixel art em cenário 3D; o projeto usa o renderer 2D |

## 3. Decisões do usuário (2026-11-14)

1. **Renderização em 2,5D:** sprites de pixel art em cenário com profundidade real, câmera em perspectiva, jogabilidade lateral em um único plano.
2. **Prólogo como MVP:** a Zona 0 (escombros de uma estação de metrô) é levada ao nível final de arte antes de qualquer trabalho de projeto ou de arte nas outras zonas.
3. **Nível de ambição:** sofisticado e coeso, abaixo do acabamento de *Replaced*.

## 4. Abordagem

**Migrar só a camada visual.** Jogabilidade, física 2D, colisores, layout das salas e testes ficam como estão.

1. Adotar a **Bíblia de Arte** (`docs/planning/art/art-bible.md`) como fonte de verdade.
2. **Spike técnico em uma sala** (P1) antes de migrar o resto: renderer 3D, câmera em perspectiva, sprite iluminado, luz, pós-processamento. O spike é também o *beauty corner* que valida a direção de arte.
3. Rebaixar o `ZoneArtKit` a **graybox**. Arte final em kit modular 3D com textura em pixel art, dentro de um prefab de arte por sala.
4. **Renderers convivendo:** as cenas ainda não migradas (Esgotos, salas de teste) continuam no renderer 2D até a vez delas.
5. **Congelar as outras zonas:** nenhuma história de Esgotos ou de zonas seguintes entra em desenvolvimento antes do MVP aprovado.

Riscos:

| Risco | Mitigação |
|---|---|
| A migração quebrar jogabilidade ou testes | Spike em branch própria; plano de jogo em Z = 0 com física 2D intocada; EditMode e PlayMode verdes como critério de aceite |
| Cenário 3D virar um novo "procedural sem graça" | Kit modular com textura em pixel art e paleta travada; aprovação visual do usuário no beauty corner |
| Escopo de arte grande para dev solo | MVP restrito ao Prólogo; geometria simples, o peso fica em luz e atmosfera |
| Desempenho (luzes e sombras em tempo real) | Meta de 60 FPS em 1080p medida no spike; sombra só na luz-chave |

## 5. Análise de impacto

| Artefato | Mudança |
|---|---|
| `docs/planning/art/art-bible.md` | **Novo.** Paleta, câmera, luz, camadas, kit do metrô, portão de qualidade |
| `docs/project-context.md` regras 29–31 | **Substituir** pelo texto da §7 |
| `docs/project-context.md` §5.13 | Item 4 passa a incluir o plano de luz da sala; item 5 fica "polimento: áudio e ajuste fino" |
| `docs/project-context.md` linha de Câmera e Salas | Câmera em perspectiva; confinamento pela área visível no plano de jogo |
| `game-architecture.md` | Nova seção de renderização 2,5D: renderer, câmera, material de sprite iluminado, prefab de arte por sala, Volume global, convivência de renderers |
| GDD §Visual e Áudio; brief §6 | "Pixel art em 2,5D"; apontar para a bíblia de arte |
| GDD / `epics.md` | Marco novo: **MVP = Prólogo completo**; zonas 1–5 congeladas até lá |
| `sprint-status.yaml` | 5.10 redefinida; 5.13 e 5.14 novas; 5.2, 5.3, 5.5, 5.7 e 5.8 adiadas para depois do MVP |
| Código | `RoomCamera2D`, `CameraShake2D`, `ParallaxLayer` (deixa de ser necessário nas salas migradas), setups de cena, materiais, `ArtPreviewRenderer` |
| Testes | `RoomCamera2DTests`, `ParallaxLayerTests`, `Story54`/`Story56`, `RoomTransitionTests`: revisar o que depende da câmera ortográfica |
| `DESIGN.md` (UI) | Sem mudança |

## 6. Histórias do Épico 5 *(proposta)*

| ID | Título | Conteúdo | Portão |
|---|---|---|---|
| **5.10** *(redefinida)* | Spike 2,5D + beauty corner da P1 | Renderer 3D e câmera em perspectiva só no Prólogo; sprite iluminado com sombra; uma tela da P1 em qualidade final (kit do metrô, luz, névoa, poeira, pós-processamento); Dante e Kitty em sprite com parado e corrida | **Aprovação visual do usuário** (checklist da bíblia §12) + testes verdes + 60 FPS |
| **5.13** *(nova)* | Fundação 2,5D consolidada | Transformar o spike em padrão: prefab de arte por sala, Volume por zona, material e pipeline de sprites, atualização da arquitetura e do `project-context.md` | Testes verdes |
| **5.11** | Prólogo em escala II | Sem mudança de escopo (11 salas, opção B). Feita em graybox | — |
| **5.12** | Personagens e inimigos | Restante das animações de Dante; Batedor, Operário e Drone; olhos emissivos por estado; efeitos de combate | Aprovação visual |
| **5.14** *(nova)* | Arte do Prólogo completo | Aplicar o padrão às 11 salas | Aprovação visual por sala |
| **5.15** *(nova)* | Fechamento do MVP | HUD mínima do Épico 6.1 no Prólogo, áudio ambiente, playtest do início ao fim | **MVP aprovado** |

**Ordem proposta:** 5.10 (portão) → 5.13 → 5.11 → 5.12 → 5.14 → 5.15. Depois do MVP: 5.8 → 5.5 → 5.2 → 5.7 → 5.3, já em 2,5D.

## 7. Texto novo para o `project-context.md` *(proposta)*

**Arte e renderização (decisão de 2026-11-14, revisada)**

29. **A fonte de verdade da arte é `docs/planning/art/art-bible.md`.** O jogo é 2,5D: sprites de pixel art em cenário com profundidade real. A jogabilidade vive em Z = 0 com física 2D; profundidade é só visual.
30. **O `ZoneArtKit` é graybox.** Serve para blocar e testar level design; não é arte final. Arte final fica em `Art_<roomId>.prefab`, que o script de setup só instancia. Regerar a cena nunca pode apagar arte.
31. **Nenhuma sala é considerada pronta sem plano de luz:** ambiente baixo, luz-chave com sombra, preenchimento e contraste com fonte visível, emissivos e ao menos um elemento animado. Proibido simular luz com sprite semitransparente.
31a. **Densidade de texel única:** 32 por unidade em sprites e texturas, filtro Point. Proibido esticar ou girar livremente sprites de pixel art.
31b. **Cada zona tem uma paleta travada** (24–32 cores com desvio de matiz). Todo sprite ou textura, de qualquer origem, é recolorido para ela antes de entrar no projeto.
31c. **Portão de arte:** arte nova só escala depois de um beauty corner aprovado pelo usuário. **MVP = Prólogo**; as outras zonas ficam congeladas até lá.

## 8. Pendências do usuário

1. Aprovar a bíblia de arte e esta proposta.
2. Enviar capturas do moodboard do Pinterest para `docs/planning/art/moodboard/` (o link não pôde ser lido automaticamente).
3. Origem das texturas e sprites: PixelLab com paleta travada, Aseprite manual, ou os dois.

## 9. Handoff

- **Escopo:** major. Muda renderização, arquitetura de câmera, regras de projeto e a ordem do Épico 5.
- **Próximo passo:** executar `docs/planning/art/prompt-migracao-2-5d.md` no agente de desenvolvimento.
- **Critério de sucesso:** beauty corner da P1 aprovado pelo checklist da bíblia §12.

---

## 10. Revisão da Fase 1 (gds-correct-course, 2026-11-14) *(aguarda aprovação)*

Esta revisão parte da proposta acima, sem mudar as decisões do usuário (§3). Acrescenta o que a análise do código e do URP 17.3 mostrou, o plano da história 5.10 e as decisões que faltam. O desenho técnico está em `game-architecture.md` §10.

### 10.1 Resultado do checklist

| Seção | Resultado |
|---|---|
| 1. Gatilho e contexto | Feito (§1 e §2 acima) |
| 2. Impacto nos épicos | Só o Épico 5 muda. Épicos 6 a 8 ficam como estão; a 6.1 empresta a HUD mínima para a 5.15 |
| 3. Conflitos entre artefatos | `project-context.md` (regras 29–31, linha de câmera, §5.13), `game-architecture.md` (nova §10, escrita nesta fase), GDD e brief (uma linha cada), `epics.md`, `sprint-status.yaml`. Sem conflito com a narrativa nem com o `DESIGN.md` |
| 4. Caminho | Ajuste direto (opção 1). Não há rollback: nenhuma história feita é desfeita, o graybox continua valendo |
| 5. Proposta | Este documento |
| 6. Handoff | Escopo major; ver §10.6 |

### 10.2 O que a análise mostrou

1. **A migração da câmera é pequena e não exige adaptar testes.** `RoomCamera2D` só precisa calcular a área visível em Z = 0 nos dois modos. Os testes atuais criam câmera ortográfica, que continua suportada.
2. **Os dois renderers convivem no mesmo URP Asset**; cada câmera escolhe o seu. As cenas 2D não são tocadas.
3. **O sprite iluminado precisa de shader próprio** (opaco, com recorte de alpha): o shader de sprite do URP não recebe luz 3D, não projeta sombra e não escreve profundidade.
4. **Três pontos da bíblia não são viáveis como estão escritos** (`game-architecture.md` §10.8): luz volumétrica (o URP não tem), desfoque do primeiro plano (só com DoF Bokeh, mais caro) e pixel uniforme nas camadas fundas (impossível em perspectiva). Cada um tem alternativa proposta.
5. **A P1 tem 48 unidades de largura (2,4 telas).** O spike leva **uma** tela à qualidade final; o resto da P1 fica em graybox até a 5.14.
6. **Ambiente de execução:** a sessão em nuvem em que este plano foi escrito **não tem o Unity instalado** e não alcança os servidores da Unity. Sem resolver isso, o agente não consegue rodar os testes, gerar as capturas nem medir FPS (§10.5, decisão 1).

### 10.3 Plano da história 5.10 — Spike 2,5D + beauty corner da P1

Branch própria. Uma tarefa por commit, testes verdes a cada passo.

| # | Tarefa | Verificação |
|---|---|---|
| 1 | **Retrato de antes:** capturas da P1 atual e teste que registra posição e tamanho de todos os colisores da P1 | Teste novo verde antes de qualquer mudança |
| 2 | **Renderer:** `Renderer25D.asset` (Forward+, SSAO) no índice 1 do URP Asset; sombras de luzes adicionais ligadas | `Act1_Sewers` e salas de teste abrem e ficam iguais às capturas de antes |
| 3 | **Câmera:** `RoomCamera2D.HalfExtentsAtPlayPlane`; câmera do Prólogo em perspectiva (25°, 25,37 de distância, renderer 1) | Testes atuais sem alteração + testes novos de enquadramento 20 × 11,25 |
| 4 | **Shader `SpriteLit25D`** e material; sombra e recebimento de sombra no `SpriteRenderer` | Dante (ainda quadrado) recebe luz, projeta sombra e fica em foco no DoF |
| 5 | **Prefab de arte:** `Art_prologue_metro_platform.prefab` instanciado pelo setup; comando separado para montar o prefab | Regerar a cena não altera o prefab; teste "arte sem colisor nem lógica" |
| 6 | **Kit do metrô para uma tela:** plataforma com borda amarela, parede de azulejo com faixa da Linha 7, colunas com vergalhão, teto com trecho desabado, trilhos, vagão descarrilado (objeto-herói), bilheteria, placas, cartazes, cabos, entulho, pichação; túnel recuando com luzes ao longe | Mínimos da bíblia §8 conferidos por contagem |
| 7 | **Luz e atmosfera:** ambiente quase preto; luz-chave com sombra (fresta do teto ou interior do vagão), preenchimento âmbar de emergência, contraste frio; fluorescente falhando (`LightFlicker`); névoa, poeira, feixe; Volume com bloom, tonemapping, DoF, vinheta, grão | Cada luz tem fonte visível; ao menos uma animada |
| 8 | **Dante e Kitty em sprite:** parado (6 quadros) e corrida (8), paleta mestre, normal map, olhos de Kitty emissivos e luz própria | Pixels nítidos e uniformes em 1080p e 1440p |
| 9 | **Capturas e medição:** `docs/art-review/beauty-corner-p1/` (1080p e 1440p; com e sem personagens; antes e depois) e FPS em 1080p | Checklist da bíblia §12 preenchido item a item |
| 10 | **Parada:** nada é marcado como pronto; aguarda aprovação visual do usuário | — |

Tela escolhida *(proposta)*: o trecho do **vagão e da bilheteria** (centro em x ≈ 9 da P1), porque junta dois objetos-herói, a via e a boca do túnel.

### 10.4 Riscos novos

| Risco | Mitigação |
|---|---|
| Qualidade da pixel art (texturas, vagão, Dante, Kitty) depende de quem a produz | Decisão 2 abaixo; paleta da bíblia §5.2 travada em qualquer caso; aprovação visual antes de escalar |
| `SpriteRenderer` não projetar ou receber sombra como esperado | Plano B com quad e `MeshRenderer` (`game-architecture.md` §10.4) |
| DoF Bokeh + SSAO + sombra derrubarem os 60 FPS | Medir no passo 9; plano B: DoF Gaussian, SSAO em meia resolução |
| Sprites cintilando com a câmera em movimento | Encaixe da câmera na grade de pixel; plano B no shader |
| Graybox das outras salas do Prólogo ficar estranho com pós-processamento | Conferir no passo 3; se preciso, Volume local só na área migrada |

### 10.5 Decisões que faltam (do usuário)

1. **Onde o Unity roda na Fase 2.**
   - **A. Na sua máquina** *(recomendado)*: o agente escreve código, shaders e scripts de Editor; você roda o setup, os testes e o comando de captura e envia as imagens. Mais lento por rodada, mas mede FPS de verdade.
   - **B. Na nuvem:** instalar o Unity 6000.3.1f1 no ambiente da sessão. Exige liberar os servidores da Unity na rede do ambiente e uma licença ativável sem interface gráfica. Sem placa de vídeo, serve para testes e capturas, mas **não** para medir FPS.
2. **Quem produz a pixel art** (pendência 3 da §8).
   - **A. PixelLab com paleta travada + retoque seu no Aseprite** (o que a bíblia §9 prevê). Precisa de acesso à API do PixelLab na sessão, ou de você gerar e enviar os arquivos.
   - **B. O agente desenha por código** (scripts que pintam cada textura e quadro com a paleta travada, desenho intencional, sem ruído). Não depende de ninguém, mas o teto de qualidade é mais baixo, principalmente em personagens e animação.
   - **C. Misto** *(recomendado)*: cenário e texturas pelo agente (B); Dante e Kitty pelo PixelLab/Aseprite (A).
3. **Luz volumétrica:** aceitar o feixe em malha aditiva preso a uma luz real como exceção à regra "não simular luz com sprite semitransparente"? (`game-architecture.md` §10.8, item 1)
4. **Pixel uniforme:** aceitar que a regra vale para o plano de jogo, e não para as camadas fundas? (§10.8, item 3)
5. **Tela do beauty corner:** vagão + bilheteria, ou outra?
6. **Moodboard:** as capturas do Pinterest ainda não estão em `docs/planning/art/moodboard/`. Sem elas, o item "lado a lado com as referências" do checklist será feito só com Hollow Knight, Replaced e SacriFire.

### 10.6 Handoff

- **Escopo:** major (confirmado).
- **Feito nesta fase:** esta revisão e `game-architecture.md` §10. Nenhum código foi escrito.
- **Não feito de propósito:** `project-context.md`, `epics.md` e `sprint-status.yaml` só mudam depois da aprovação (Fase 3 do prompt), para que nenhum documento afirme o que ainda não foi validado.
- **Próximo passo depois da aprovação:** `gds-create-story` para a 5.10 com as tarefas da §10.3, em branch própria.
