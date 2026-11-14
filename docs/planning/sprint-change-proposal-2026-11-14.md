---
title: 'Sprint Change Proposal — Épico 5: Prólogo elevado, arte de mapa e inimigos com percepção'
date: 2026-11-14
workflow: gds-correct-course
mode: batch
scope: major
status: partially-approved
---

# Sprint Change Proposal — Prólogo elevado, arte de mapa e inimigos com percepção

> **Status (2026-11-14):** Fase A feita e validada pelo usuário. Fase B redefinida: a arte de mapa gerada no PixelLab foi **rejeitada** e passa a ser procedural dentro do Unity. Fase C: **opção B aprovada**. Fase D continua *(proposta)*.

## 1. Resumo do pedido

Pedido do usuário em 2026-11-13, com a 5.6 já validada:

1. Elevar o escopo da zona do Prólogo: salas maiores e mais complexas.
2. Gerar a arte pelo PixelLab: primeiro os sprites de mapa; depois de aprovados, os sprites e as animações dos personagens e dos inimigos.
3. Inimigos com inteligência: quem vê Dante vai atacá-lo, em vez de só rondar a área.
4. Inimigos voadores.
5. Dante menor que os robôs inimigos.

## 2. Análise de impacto

| Artefato | Impacto |
|---|---|
| `docs/planning/epics.md` | 5.9 adicionada (feita). 5.10, 5.11 e 5.12 entram quando aprovadas. A ordem do Épico 5 muda: o Prólogo ganha mais uma passada antes da 5.8 |
| `sprint-status.yaml` | +1 chave agora (`5-9-*`); +3 depois das aprovações |
| `project-context.md` §5.2 | A meta do Prólogo hoje é "tutorial de ~20–35 min, 6 salas". A Fase C muda esse número e precisa de aprovação |
| `level-map.md` e proposta de level design | Novos tamanhos de sala e, conforme a opção escolhida, novas salas (IDs novos; os existentes não mudam) |
| GDD / `DESIGN.md` | Registrar a resolução-base da arte (32 px por unidade) e a rampa de cor de cada zona |
| Código | Novo `ZoneArtKit` (arte procedural por zona); `PrologueRuinsSetup` veste os retângulos lógicos com ele; colisores continuam gerados pelo script |
| Testes | `Story54`/`Story56` medem salas e portas pelo layout atual; a Fase C exige adaptá-los |

## 3. Abordagem recomendada

**Ajuste direto em quatro fases**, cada uma com seu portão de aprovação. Nada do que já foi entregue é desfeito.

### Fase A — Inimigos com percepção, drones e Dante menor · **feita e validada** (história 5.9, done)

Ver [5-9-enemy-sight-chase-and-flying-drones.md](../implementation/stories/5-9-enemy-sight-chase-and-flying-drones.md). EditMode 59/59 e PlayMode 36/36. Validada pelo usuário no Play Mode em 2026-11-14; o code review ficou em `deferred-work.md`.

### Fase B — Arte de mapa procedural por zona · **direção definida pelo usuário em 2026-11-14**

- **O que foi rejeitado:** o lote 1 do PixelLab (3 tilesets, 3 fundos, 9 props). Os tilesets não combinavam entre si e ficaram desconexos do fundo.
- **Direção aprovada:**
  - cada zona segue **um único padrão de arte**, como em *Hollow Knight*; chão e paredes precisam conversar com o fundo;
  - a arte de mapa é gerada **com recursos do próprio Unity** (texturas pintadas por script de Editor, sprites em mosaico, brilhos), no estilo de metroidvania 2D em pixel art das referências enviadas;
  - o PixelLab fica reservado para os **personagens e inimigos** (Fase D).
- **Como foi implementado (`ZoneArtKit`):**
  - uma **rampa de 7 tons de uma só família de cor** por zona, mais dois acentos de luz. Prólogo: cinzas sujos e quentes de estação abandonada, ferrugem e fuligem, o ocre desbotado da Linha 7, luz âmbar na estação e ciano no laboratório (a primeira versão, azulada e com arcos no fundo, foi rejeitada em 2026-11-14);
  - todas as texturas saem dessa rampa: massa escura de chão e parede, pedra talhada das estruturas, borda iluminada em todo topo exposto, linha de luz nas faces laterais, sombra por baixo, parede de fundo (azulejo de metrô na estação, painéis rebitados no laboratório), colunas, canos, cabos, grade das plataformas one-way e brilho das lâmpadas;
  - objetos de metrô pintados pelo mesmo kit: vagão descarrilado, bilheteria, trilhos com dormentes, caixotes, bancos, placa da Linha 7, cartazes rasgados, grades de ventilação, carcaça do protótipo e o grafite da resistência;
  - montes de escombros espalhados automaticamente sobre os pisos expostos (sem colisão, longe das portas);
  - fundo em camadas com parallax (`ParallaxLayer`): parede distante, colunas e canos, e cabos pretos em primeiro plano;
  - 32 px por unidade, filtro Point, texturas em `Assets/_Project/Art/Tilesets/Prologue/`;
  - o setup continua construindo retângulos lógicos com colisor; a arte entra como filhos, e a borda iluminada só aparece onde nenhum vizinho cobre o topo.
- **Prévia:** `UnderTheRain > Art > Render Prologue Previews` grava imagens das salas em `docs/art-review/prologue-unity/`.
- **Falta:** aprovação visual do usuário; sprites próprios para gerador, terminais, painéis de Kitty e portas (hoje persianas tingidas ou retângulos); aplicar um kit à Zona 1 (Esgotos) com rampa própria.

### Fase C — Prólogo elevado · **opção B aprovada**

- **História 5.11 — Prólogo em escala II. Decisão do usuário (2026-11-14): opção B, Expandir.**
  - As 8 salas atuais ficam ~1,5× maiores (3–5 telas), cada uma com uma sub-área nova, um segundo segredo e drones nos trechos verticais.
  - **3 salas novas** *(lore ambiental; os textos e nomes continuam *(proposta)* até a aprovação)*: um pátio de manobras do metrô (combate em vagões), um poço de ventilação vertical (drones) e a linha de montagem do laboratório (arena com esteiras). O Prólogo passa a ter 11 salas.
  - Meta de 45–60 min. As opções A (só aprofundar, 35–45 min) e C (mini-zona com hub e guardião, 60–80 min) foram descartadas.
- Regras mantidas: P2 continua sem dano, todo degrau da rota principal ≤ 2,8 e o gate de Wall Jump de P4 continua sendo o único gate de retorno do Prólogo.

### Fase D — Sprites e animações dos personagens *(proposta; PixelLab, depois da Fase B aprovada)*

- **História 5.12 *(proposta)*:** Dante, Kitty, Batedor, Operário e Drone no PixelLab, nas proporções da 5.9 (Dante ~22×42 px, Batedor ~32×54, Operário ~45×61, Drone ~32×22), com as animações mínimas: parado, correr, pular/cair, deslizar na parede, combo do bastão, disparo, dano e morte para Dante; patrulha, alerta, perseguição, antecipação, dano e destruição para os robôs.

## 4. Ordem proposta do Épico 5

5.1 ✓ → 5.4 ✓ → 5.6 ✓ → 5.9 ✓ → **5.10** (arte procedural do Prólogo) → 5.11 (Prólogo B) → 5.12 (personagens) → 5.8 → 5.5 → 5.2 → 5.7 → 5.3

## 5. Decisões

1. **Alcance do Prólogo elevado:** opção **B** (decidido em 2026-11-14).
2. **Arte de mapa:** procedural no Unity, um padrão por zona (decidido em 2026-11-14). **Pendente:** aprovação visual da primeira versão do kit do Prólogo.
3. **Pendente:** nomes e textos das 3 salas novas (lore ambiental *(proposta)*).
