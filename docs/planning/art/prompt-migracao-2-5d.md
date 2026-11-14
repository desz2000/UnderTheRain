# Prompt — Migração do Prólogo para 2,5D

Cole o bloco abaixo no agente de desenvolvimento, na raiz do repositório.

```text
Vamos migrar o Under The Rain de 2D para 2,5D, começando pelo Prólogo, que passa a ser o nosso MVP. Nenhuma outra zona é projetada ou recebe arte antes de o Prólogo estar aprovado.

LEIA ANTES DE QUALQUER COISA
1. docs/planning/art/art-bible.md (fonte de verdade da arte; vence qualquer outro documento em caso de conflito)
2. docs/planning/sprint-change-proposal-2026-11-14-direcao-de-arte.md
3. docs/project-context.md e docs/planning/architecture/game-architecture.md

O PROBLEMA
O level design está bom, mas o jogo parece projeto de primeiro semestre: cenário gerado por script em tons de cinza, nenhuma luz, personagens como quadrados brancos. Quero pixel art moderno e sofisticado em 2,5D, com muito detalhe e jogo de luz. Referências: Hollow Knight (profundidade e atmosfera), Replaced e SacriFire (sprites de pixel art em cenário 3D iluminado). Não precisa chegar ao acabamento do Replaced. Precisa ser coeso e sofisticado o bastante para ninguém confundir com protótipo.

O QUE É 2,5D AQUI
- Personagens, inimigos e props pequenos: sprites de pixel art.
- Cenário: geometria 3D simples e modular com textura em pixel art (32 texels por unidade, filtro Point), em várias profundidades.
- Câmera em perspectiva com campo de visão estreito, enquadrando 20 x 11,25 unidades no plano de jogo.
- Luz em tempo real com sombras, emissivos com bloom, névoa, poeira em suspensão e profundidade de campo com foco no plano de jogo.

O QUE NÃO PODE MUDAR
- Jogabilidade, sensação de controle, física 2D, colisores e layout das salas. O plano de jogo fica em Z = 0; profundidade é só visual.
- A lógica em classes puras e o fluxo de salas e transições.
- Todos os testes EditMode e PlayMode continuam passando. Se um teste depender da câmera ortográfica, adapte o teste e explique por quê.
- As cenas ainda não migradas (Act1_Sewers e salas de teste) continuam abrindo e funcionando no renderer 2D.

O CENÁRIO: ESCOMBROS DE UMA ESTAÇÃO DE METRÔ
Estação da Linha 7 abandonada e parcialmente desabada, no subsolo, seca, que esconde um laboratório da Domus Corp. Use o kit e os momentos de luz da bíblia de arte (seção 10.1): plataforma com borda amarela gasta, azulejo de metrô, colunas com vergalhão exposto, teto desabado, trilhos e vagão descarrilado, bilheteria, catracas, escada rolante quebrada, placas, cartazes rasgados, cabos pendurados, fluorescentes falhando, entulho, infiltração, pichações da resistência. O túnel deve recuar de verdade para o escuro, com luzes ao longe. Luz âmbar de emergência na estação; ciano frio no laboratório.

COMO TRABALHAR
Fase 1 — Planejamento (não escreva código ainda)
- Rode gds-correct-course a partir da proposta existente e me apresente o plano.
- Atualize a arquitetura com gds-game-architecture: renderer, câmera, material de sprite iluminado, prefab de arte por sala (Art_<roomId>.prefab, que o script de setup só instancia), convivência dos dois renderers. Confirme cada escolha na documentação do URP da versão instalada (Unity 6000.3, URP 17.3) em vez de assumir.
- Pare e espere minha aprovação.

Fase 2 — Spike e beauty corner (história 5.10)
- Em uma branch própria, migre UMA tela da P1 (prologue_metro_platform) e leve-a à qualidade final, com Dante e Kitty em sprite (parado e corrida).
- Gere capturas em docs/art-review/ e me mostre.
- Pare e espere minha aprovação visual pelo checklist da seção 12 da bíblia.

Fase 3 — Só depois de aprovado
- Consolide o padrão (5.13), siga com 5.11, 5.12, 5.14 e 5.15, uma história por vez, com gds-create-story e gds-dev-story.
- Atualize project-context.md (regras 29 a 31), epics.md e sprint-status.yaml.

CRITÉRIOS DE ACEITE DO SPIKE
- Checklist da seção 12 da bíblia de arte cumprido.
- Jogabilidade na P1 idêntica à de antes.
- Testes verdes.
- 60 FPS em 1080p com tudo ligado.

NÃO FAÇA
- Não refine o ZoneArtKit como arte final; ele vira só graybox.
- Não use ruído aleatório, cinza neutro ou retângulos chapados como textura.
- Não simule luz com sprite semitransparente.
- Não estique nem gire livremente sprites de pixel art.
- Não migre mais de uma sala antes da minha aprovação.
- Não marque nada como pronto sem eu ter visto a captura.

Se algo na bíblia de arte for inviável tecnicamente, não contorne em silêncio: me diga o que é, por quê, e proponha a alternativa.
```
