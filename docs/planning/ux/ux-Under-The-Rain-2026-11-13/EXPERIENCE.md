---
title: 'EXPERIENCE.md — Under The Rain'
status: complete
created: 2026-11-13
updated: 2026-11-13
design_ref: 'DESIGN.md'
---

# EXPERIENCE.md — Under The Rain (Arquitetura de UX, HUD e Fluxos)

## 1. Foundation
- **Plataforma e Superfície:** PC (Steam, 1080p+ 16:9), jogado com **Gamepad** (recomendado) ou **Teclado**.
- **Engine / Sistema de UI:** Unity 6000.3.1f1 (2D URP). Identidade visual definida em [DESIGN.md](DESIGN.md).

## 2. Information Architecture
- **Fluxo de Menus:**
  - *Tela Título:* Continuar · Novo Jogo · Opções · Sair (simples, sobre cena atmosférica com chuva).
  - *Menu Start / Pausa:* Retomar · Opções (Áudio, Vídeo, Controles) · Voltar ao Menu.
  - *Menu de Inventário / Diário (botão dedicado):* 3 abas principais:
    1. **Mapa Criptografado:** salas exploradas, zona atual, abrigos, máquina de teletransporte e marcação de gates ainda bloqueados.
    2. **Arsenal & Acoplamentos:** arma principal e estágio atual, sub-armas/acessórios equipados e capacidade da mochila de peças.
    3. **Registros & Documentos:** documentos de texto e registros de áudio encontrados (incluindo pistas do Pai).

## 3. HUD & Diegetic UI
- **Sempre visível em gameplay (sem caixas/painéis, `{components.hud_health}`):**
  1. **Vida** (células em pixel art).
  2. **Bateria** (barra segmentada fina em `{colors.accent_battery}`).
  3. **Kits Médicos** (cargas restantes).
  4. **Peças na Mochila** (`atual/máximo`).
  5. **Cooldown do Gato** (ícone discreto: pronto vs. recarregando).
  6. **Arma Selecionada** (ícone compacto).
- **Diegético no Mundo:**
  - Prompts contextuais pequenos acima de NPCs, terminais, portas e máquina de teletransporte.
  - Feedback visual/sonoro no próprio robô quando o gato causa hesitação (faísca/glifo de erro acima do inimigo).
  - Mochila de peças caída no local da morte com brilho discreto na chuva.

## 4. Abrigos e Máquina de Teletransporte
- **NPCs de Abrigo (Ferreiro, Vendedor, Curandeiro):** abrir interação exibe painel limpo no estilo `{components.menu_panel}` com apenas as opções daquele NPC (acoplar item na arma, comprar/fabricar kits, expandir capacidade da mochila de peças ou vida máxima em troca de peças/itens/favores).
- **Máquina de Teletransporte *(Ato II+)*:** **sem NPC** — Dante interage diretamente com um terminal/máquina física no abrigo, escolhe o abrigo de destino já descoberto e paga o **custo em peças** para viajar.

## 5. Input Schemes & Prompts
- **Detecção Automática de Dispositivo:** glifos dos prompts (ex.: `[E]` / `[Tab]` vs. `(X)` / `(Select)`) alternam automaticamente conforme o último dispositivo usado (Teclado ou Gamepad).

## 6. Game Feel & Juice de UI
- Ao sofrer dano, a célula de vida correspondente racha/pisca antes de esvaziar; ao recuperar a mochila de peças, contador sobe com flash breve em `{colors.accent_parts}`.
- Abertura do Inventário/Mapa tem transição rápida (~120 ms), sem animações lentas que travem o ritmo.

## 7. Accessibility Floor
- Contraste alto entre texto `{colors.text_ivory}` e fundos `{colors.bg_void}` / `{colors.bg_panel}`.
- Áudios de pistas do Pai sempre acompanhados de **legenda/transcrição em texto** na aba de Registros.
- Opção futura de ajuste de *screenshake* nas Opções.

## 8. Key Flows (Jornada do Jogador)
1. **Fluxo de Exploração e Gate:** Dante explora sala nos esgotos → vê plataforma alta (gate legível) → abre **Mapa Criptografado** (vê sala incompleta) → obtém *Wall Jump* → retorna, escala e descobre novo abrigo.
2. **Fluxo de Morte e Recuperação:** Dante cai em combate → acorda no último abrigo (vida/bateria/kits restaurados, mochila vazia) → volta ao local da queda → recupera a mochila de peças caída.
3. **Fluxo de Teletransporte (Ato II+):** Dante interage com a **máquina de teletransporte** no abrigo → seleciona abrigo descoberto → paga peças → chega ao destino.
