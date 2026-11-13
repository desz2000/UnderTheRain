---
title: 'DESIGN.md — Under The Rain'
status: complete
created: 2026-11-13
updated: 2026-11-13
tokens:
  colors:
    bg_void: '#0E1014'
    bg_panel: '#161A20'
    surface_metal: '#232933'
    text_ivory: '#EAE6DF'
    text_muted: '#8B939E'
    accent_life: '#C94A4A'
    accent_battery: '#4ECDC4'
    accent_parts: '#D49A4A'
    border_pixel: '#3B424E'
  typography:
    font_display: 'Pixel / Bitmap Clean (títulos e abas)'
    font_body: 'Pixel Readable Sans (descrições e documentos)'
    font_mono: 'Pixel Monospace (números de peças, kits, bateria e coordenadas)'
  rounded:
    hud_element: '0px (pixel-aligned)'
    panel_frame: '0px com cantos chanfrados em pixel art (step 2px/4px)'
  spacing:
    hud_margin_px: 16
    hud_gap_px: 8
    panel_padding_px: 16
  components:
    hud_health: 'Ícones/células em pixel art soltos no canto superior esquerdo'
    hud_battery: 'Barra segmentada fina em ciano abaixo da vida'
    hud_kits_parts: 'Ícone pixel art + contador monospace (kits médicos e peças atual/máx)'
    hud_cat_weapon: 'Ícone compacto de estado/cooldown do gato + ícone da arma equipada'
    menu_panel: 'Moldura pixel-art escura com borda de 1px e canto chanfrado (zero caixas na HUD de jogo)'
---

# DESIGN.md — Under The Rain (Identidade Visual de UI/HUD)

## 1. Brand & Style
- **Direção Escolhida:** **Minimalista Sem Caixas (estilo *Hollow Knight / Silksong*)** na HUD de gameplay + **Molduras Pixel-Art Neo-Noir** nos menus, inventário, mapa e abrigos.
- **Meta Explícita:** Eliminar qualquer estética de "site web / dashboard de IA" (sem cards arredondados, sem gradientes CSS, sem sombras difusas modernas). Toda interface deve parecer feita no mesmo universo pixel-art do jogo.

## 2. Colors
- **Base Monocromática (Chuva e Carvão):**
  - Fundo profundo de menus: `#0E1014` (`{colors.bg_void}`)
  - Superfície de painel (Inventário/Mapa/Abrigo): `#161A20` (`{colors.bg_panel}`)
  - Detalhes metálicos / divisores: `#232933` (`{colors.surface_metal}`) e borda `#3B424E` (`{colors.border_pixel}`)
  - Texto principal (branco marfim): `#EAE6DF` (`{colors.text_ivory}`)
  - Texto secundário / legendas: `#8B939E` (`{colors.text_muted}`)
- **Acentos Funcionais Contidos (apenas 2 principais + 1 de recurso):**
  - Vida / Perigo: `#C94A4A` (`{colors.accent_life}`)
  - Bateria / Pulso do Gato: `#4ECDC4` (`{colors.accent_battery}`)
  - Peças / Máquinas de Abrigo (uso pontual): `#D49A4A` (`{colors.accent_parts}`)

## 3. Typography
- **Títulos e Abas:** fonte pixel/bitmap nítida, alinhada à grade de pixels (sem anti-aliasing borrado).
- **Documentos, Registros e Descrições:** fonte pixel legível em 1080p (mínimo equivalente a 14–16px em tela cheia) para leitura confortável de logs do Pai.
- **Contadores e Custos:** fonte pixel monospace para peças (`120/250`), cargas de kit e bateria.

## 4. Layout & Spacing
- **HUD de Gameplay (Zero Caixas):** elementos soltos diretamente sobre a cena no canto superior esquerdo (vida, bateria, kits, peças) e canto inferior/discreto para arma equipada + cooldown do gato, com margem fixa de `16px` (`{spacing.hud_margin_px}`) e espaçamento de `8px` (`{spacing.hud_gap_px}`).
- **Menus e Inventário:** ocupam a tela com respiro generoso, leitura limpa e hierarquia clara (abas no topo, conteúdo à esquerda, detalhes/ilustração à direita).

## 5. Elevation & Depth
- **Sem sombras CSS/drop-shadow suaves.** Profundidade é criada apenas por escurecimento sutil do cenário atrás do menu (overlay `rgba(14, 16, 20, 0.78)`) e bordas pixeladas de 1px (`{colors.border_pixel}`).

## 6. Shapes
- **Pixel-aligned (`0px` de border-radius):** cantos retos ou chanfrados em degraus de pixel (2px/4px) nas molduras de menus, mapa e abrigos.

## 7. Components
- **HUD de Vida:** células/cápsulas pixel-art que racham/esvaziam ao sofrer dano (sem barra genérica de progresso).
- **HUD de Bateria:** barra fina segmentada em `#4ECDC4` logo abaixo da vida.
- **HUD de Kits Médicos & Peças:** ícone pixel-art + número monospace (`3` kits; `85/150` peças).
- **HUD de Gato & Arma:** ícone da arma equipada + ícone discreto do gato indicando pronto vs. recarregando (cooldown).
- **Prompts Diegéticos no Mundo:** pequeno glifo de botão/tecla acima de terminais, NPCs ou máquina de teletransporte.

## 8. Do's and Don'ts
- **DO:** Manter toda a UI alinhada à grade de pixel art; usar apenas os acentos definidos; deixar a HUD respirar sem painéis ao redor.
- **DON'T:** Nunca usar cantos arredondados suaves, gradientes arco-íris/roxo-neon genéricos, fontes web padrão (Roboto/Inter/Arial) ou poluição visual na HUD.
