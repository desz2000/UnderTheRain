# Épicos de Desenvolvimento — Under The Rain

> Derivado do [GDD](gdd.md) e alinhado ao [plano-implementacao-bmad.md](../../../../plano-implementacao-bmad.md). Os Épicos **1 a 5** entregam o **Vertical Slice do Ato I** antes da expansão para o jogo completo.

---

## Épico 1 — Fundação do Projeto (Vertical Slice)
- **Objetivo:** Estrutura base do projeto Unity 2D URP, Input System (teclado + gamepad), arquitetura de ScriptableObjects, fluxo de cenas e testes iniciais.
- **Critério de marco:** Cena de teste rodando a 60 FPS com Input System respondendo.

## Épico 2 — Movimentação Metroidvania (Vertical Slice)
- **Objetivo:** Locomoção responsiva (aceleração ~0,08s, frenagem ~0,06s, pulo variável, *coyote time* ~100ms, *input buffering* ~120ms, *corner correction*) e **Wall Slide & Wall Jump** (1ª habilidade).
- **Critério de marco:** Atravessar circuito vertical de teste com sensação de controle aprovada no Play Mode.

## Épico 3 — Combate, Bateria e Economia de Peças (Vertical Slice)
- **Objetivo:** Bastão simples com *hitstop* (~40–60ms) e *screenshake*, disparo à base de bateria, coleta de peças, fabricação/uso de kit médico, morte com drop da mochila de peças e despertar no abrigo.
- **Critério de marco:** Derrotar inimigo de teste, coletar peças, morrer e recuperar a mochila no local da queda.

## Épico 4 — Gato Companheiro (Vertical Slice)
- **Objetivo:** Gato seguindo Dante, comando ativo de interferência que faz robôs hesitarem/travarem (~1,0–1,5s com cooldown) e pelo menos 1 interação de exploração (duto/terminal).
- **Critério de marco:** Acionar o gato contra um robô batedor e vê-lo hesitar enquanto um Humano Misantropo permanece imune.

## Épico 5 — Vertical Slice do Ato I (Marco 1 / MVP)
- **Objetivo:** Trecho completo conectando Esgotos → Transição → Superfície, com 1 abrigo funcional, 1 gate de Wall Jump + retorno recompensado, 1 robô batedor e o **1º Humano Misantropo (Boss)** rendendo o Mapa Criptografado.
- **Critério de marco:** Ciclo completo jogável do início ao fim no Play Mode.

## Épico 6 — UI/UX Diegética
- **Objetivo:** HUD neo-noir (vida, bateria, kits, peças), menus, tela do Mapa Criptografado (decodificação por zona e marcação de gates) e loja/fabricação no abrigo.

## Épico 7 — Level Design e Expansão das Zonas
- **Objetivo:** Construção das 5 zonas interconectadas, atalhos unilaterais, segredos sugeridos e gates para Dash Tático, Bastão Eletrificado, Propulsor Aéreo e Gancho Magnético.

## Épico 8 — Sistemas Avançados, Colapso da IA e Bosses
- **Objetivo:** Evolução completa da arma (3 estágios), teletransporte pago entre abrigos (Ato II+), estágios de colapso gradual da IA, bosses principais (até o Titã) + 2 opcionais, e sistema de Save/Load completo.
