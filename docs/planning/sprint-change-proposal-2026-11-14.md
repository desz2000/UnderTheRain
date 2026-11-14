---
title: 'Sprint Change Proposal — Épico 5: Prólogo elevado, arte de mapa e inimigos com percepção'
date: 2026-11-14
workflow: gds-correct-course
mode: batch
scope: major
status: draft
---

# Sprint Change Proposal — Prólogo elevado, arte de mapa e inimigos com percepção

> **Status:** rascunho. Só a Fase A está decidida e implementada (pedido direto do usuário). As Fases B, C e D são *(proposta)* e dependem das aprovações indicadas em cada uma.

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
| GDD / `DESIGN.md` | Registrar a resolução-base da arte (32 px por unidade) e a paleta dos cenários |
| Código | `PrologueRuinsSetup` passa a pintar tiles e fundos em vez de retângulos; colisores continuam gerados pelo script |
| Testes | `Story54`/`Story56` medem salas e portas pelo layout atual; a Fase C exige adaptá-los |

## 3. Abordagem recomendada

**Ajuste direto em quatro fases**, cada uma com seu portão de aprovação. Nada do que já foi entregue é desfeito.

### Fase A — Inimigos com percepção, drones e Dante menor · **feita** (história 5.9, em review)

Ver [5-9-enemy-sight-chase-and-flying-drones.md](../implementation/stories/5-9-enemy-sight-chase-and-flying-drones.md). EditMode 59/59 e PlayMode 36/36. Falta a validação manual e o code review.

### Fase B — Arte de mapa *(proposta; aguarda aprovação visual do lote 1)*

- **Resolução-base:** 32 px por unidade (1 tile = 1 unidade). Com a câmera atual (ortográfica 5,4) a tela mostra ~614×346 px, a mesma faixa de *Owlboy*.
- **Lote 1 gerado** (em `docs/art-review/prologue-maps-v1/`, fora de `Assets/`):
  - 3 tilesets de plataforma 32×32 (16 tiles Wang cada): estação de metrô, laboratório da Domus e túnel de serviço;
  - 3 fundos 640×360 (metrô, laboratório, túnel);
  - 9 props: vagão (de pé e tombado), placa da Linha 7, bilheteria, gerador de emergência, terminal, carcaça do protótipo, rack de servidores e caixotes.
- **História 5.10 *(proposta)* — Arte de mapa no Prólogo:** importar os tilesets com 32 PPU e filtro Point; o setup passa a pintar `Tilemap` por sala escolhendo o tile pelos quatro cantos (a superfície fica no meio do tile, então o colisor continua vindo do retângulo lógico); fundos em camadas de parallax; props no lugar dos `DecorRect`. Falta gerar variações do tile de miolo, paredes de fundo modulares e os props restantes (grafite, dutos, painéis, portas, escotilha).

### Fase C — Prólogo elevado *(proposta; aguarda a decisão da pergunta 1)*

- **História 5.11 *(proposta)* — Prólogo em escala II.** Três alcances possíveis:
  - **A. Aprofundar.** Mesmas 8 salas, cada uma ~1,5× maior (3–5 telas), com uma sub-área nova por sala, segundo segredo e drones nos trechos verticais. Meta de 35–45 min. Menor risco: IDs e grafo não mudam.
  - **B. Expandir.** Tudo de A, mais 3 salas novas *(proposta de lore ambiental, sem personagens novos)*: um pátio de manobras do metrô (combate em vagões), um poço de ventilação vertical (drones) e a linha de montagem do laboratório (arena com esteiras). Meta de 45–60 min. O Prólogo passa a ter 11 salas.
  - **C. Mini-zona.** Tudo de B, com o Prólogo reorganizado em laço (um hub na estação com três alas) e um guardião no laboratório como exame final. Meta de 60–80 min. É quase uma zona inteira antes do Abrigo e empurra o resto do Épico 5.
- Em qualquer opção: P2 continua sem dano, todo degrau da rota principal ≤ 2,8 e o gate de Wall Jump de P4 continua sendo o único gate de retorno do Prólogo.

### Fase D — Sprites e animações dos personagens *(proposta; depois da Fase B aprovada)*

- **História 5.12 *(proposta)*:** Dante, Kitty, Batedor, Operário e Drone no PixelLab, nas proporções da 5.9 (Dante ~22×42 px, Batedor ~32×54, Operário ~45×61, Drone ~32×22), com as animações mínimas: parado, correr, pular/cair, deslizar na parede, combo do bastão, disparo, dano e morte para Dante; patrulha, alerta, perseguição, antecipação, dano e destruição para os robôs.

## 4. Ordem proposta do Épico 5

5.1 ✓ → 5.4 ✓ → 5.6 ✓ → **5.9** (review) → 5.10 → 5.11 → 5.12 → 5.8 → 5.5 → 5.2 → 5.7 → 5.3

## 5. Decisões pendentes

1. **Alcance do Prólogo elevado:** A, B ou C (Fase C).
2. **Aprovação visual do lote 1 de arte de mapa** (Fase B): estilo, paleta e escala.
