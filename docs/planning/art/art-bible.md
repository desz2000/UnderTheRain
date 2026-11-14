---
title: 'Bíblia de Arte — Under The Rain (mundo 2D)'
status: review
created: 2026-11-14
updated: 2026-11-14
scope: 'Arte do mundo, personagens, luz e pós-processamento. HUD e menus continuam em ux/.../DESIGN.md.'
mvp: 'Prólogo (Zona 0). As outras zonas ficam congeladas até o Prólogo ser aprovado.'
render: '2,5D (decisão do usuário em 2026-11-14)'
---

# Bíblia de Arte — Under The Rain

> **Para agentes:** este documento é a fonte de verdade da arte do mundo. Se uma história, um script ou o `project-context.md` disser outra coisa, vale este documento. Itens marcados *(proposta)* ainda dependem de aprovação do usuário.

## 1. Meta em uma frase

**Pixel art moderno e sofisticado em 2,5D: personagens em sprite de pixel art dentro de um cenário com profundidade real, luz dinâmica e atmosfera.** A jogabilidade continua 100% lateral, em um único plano.

**Nível de ambição:** sofisticado e coeso, sem precisar igualar o acabamento de *Replaced*. A régua é: ninguém pode confundir uma captura do jogo com um protótipo.

**MVP:** o Prólogo (Zona 0, os escombros de uma estação de metrô). Nenhuma outra zona é projetada ou recebe arte antes de o Prólogo estar aprovado.

O jogador deve olhar para qualquer tela parada e ver um quadro composto, não um nível montado com blocos.

## 2. Referências: o que trazemos e o que deixamos

| Referência | Trazemos | Deixamos de fora |
|---|---|---|
| **Hollow Knight** | Profundidade em muitas camadas com parallax; silhuetas pretas em primeiro plano; uma identidade visual única por zona; partículas e névoa sempre presentes; vinheta; legibilidade por valor (o plano de jogo se destaca do fundo) | Traço desenhado à mão em alta resolução (o nosso é pixel art) |
| **Replaced** | Sprites de pixel art em cenário com profundidade real; iluminação em tempo real, luz volumétrica, profundidade de campo; sujeira e desgaste em cada superfície; neo-noir retrofuturista | Câmeras dirigidas plano a plano e o nível de acabamento de um estúdio com anos de produção |
| **SacriFire** | Sprites de personagem detalhados e bem animados dentro de cenários 3D com efeitos modernos (bloom, luz, desfoque de profundidade) | Exploração livre em 3D |
| **Katana Zero** *(brief)* | Neon refletido na garoa e nas poças; cor saturada cortando o escuro | Paleta VHS/glitch como linguagem dominante |
| **Owlboy** *(brief)* | Pixel art orgânica, luz quente e acolhedora nos abrigos | Tom de fantasia leve |
| **Moodboard do Pinterest** | *(pendente: o link não pôde ser lido automaticamente; anexar capturas em `docs/planning/art/moodboard/`)* | — |

**Consequência técnica:** Replaced e SacriFire chegam ao visual deles misturando pixel art com render 3D. Adotamos o mesmo princípio: 2,5D (§11).

## 3. Os cinco pilares visuais

1. **A escuridão é o padrão; a luz é desenhada.** Nenhuma sala é iluminada por igual. Toda luz tem uma fonte visível no cenário (lâmpada, terminal, neon, faísca, fresta).
2. **Profundidade sempre.** No mínimo 5 camadas por tela, do fundo distante ao primeiro plano.
3. **Nada é retângulo liso.** Toda superfície tem material, desgaste e variação. A grade de colisão não pode ser lida na imagem.
4. **O mundo se mexe.** Toda tela tem movimento ambiente, mesmo sem inimigos.
5. **Uma zona, uma identidade.** Paleta, materiais, tipo de luz e partículas próprios por zona, coerentes entre chão, parede, fundo e props.

## 4. Câmera, escala e regras de pixel

| Item | Valor | Estado atual |
|---|---|---|
| Câmera | **Perspectiva**, campo de visão estreito (20°–30°), afastada do plano de jogo | Ortográfica 5,4 |
| Área visível no plano de jogo (Z = 0) | **20 × 11,25 unidades** (equivale a 640×360 a 32 px por unidade) | 19,2 × 10,8 |
| Texels por unidade | **32**, em sprites e em texturas do cenário 3D | 32 nos sprites |
| Escala do pixel no plano de jogo | ×3 em 1080p, ×4 em 1440p, ×6 em 4K | ×3,125 em 1080p |
| Filtro / compressão das texturas | Point / None | Point |

Regras:

- **O plano de jogo é sagrado.** Dante, inimigos, plataformas e colisores vivem em Z = 0, com física 2D. A profundidade é só visual.
- **Densidade de texel única.** Sprites e texturas do cenário usam 32 texels por unidade, para o pixel ter o mesmo tamanho aparente no plano de jogo. Camadas mais fundas ficam naturalmente menores pela perspectiva.
- **Sprite não se estica nem gira livremente.** `ZoneArtKit.Fit` (que estica sprite para caber em colisor) não entra em arte final.
- **O pixel é nítido; a luz é suave.** Luz, bloom, névoa, partículas e desfoque rodam na resolução da tela.
- Proporções dos personagens: as da história 5.9 (Dante ~22×42 px, Batedor ~32×54, Operário ~45×61, Drone ~32×22).

## 5. Cor

### 5.1 Regras

- **Rampa com desvio de matiz.** Sombras puxam para azul/violeta frio; luzes puxam para amarelo/âmbar quente. Rampa de cinza neutro (como a atual `#0B0C0C → #B3B4A6`) está proibida: é ela que dá o aspecto lavado.
- **24 a 32 cores por zona**, em 4 a 5 rampas de material (pedra/concreto, metal, ferrugem/sujeira, vegetação ou tecido, luz).
- **Regra 70/25/5:** 70% tons escuros e dessaturados, 25% tons médios do material, 5% acento saturado de luz. O acento só aparece onde há fonte de luz ou informação de jogo.
- **Preto puro** só nas silhuetas de primeiro plano. **Branco puro** só no núcleo de fontes de luz.
- **Personagens têm paleta própria** (mestre, comum a todas as zonas), mais saturada e mais clara que o cenário. A zona os tinge pela luz, não pela textura.
- Os acentos conversam com a UI (`DESIGN.md`): vida `#C94A4A`, bateria/Kitty `#4ECDC4`, peças `#D49A4A`.

### 5.2 Paleta do Prólogo — estação de metrô e laboratório *(proposta, a validar no beauty corner)*

| Rampa | Tons |
|---|---|
| Sombras (frias) | `#0A0D14` `#121826` `#1C2638` |
| Concreto (médios) | `#2B3547` `#3F4A58` `#5A6166` |
| Concreto iluminado (quentes) | `#7F8278` `#A8A28C` `#D1C6A3` |
| Ferrugem e fuligem | `#3A2420` `#6B3A2A` `#A65A32` |
| Linha 7 (ocre desbotado) | `#5C4E2A` `#8C7A44` `#BFA65A` |
| Luz âmbar (estação) | `#C9822E` `#F0B45C` `#FFD98A` |
| Luz ciano (laboratório) | `#1F6F78` `#4ECDC4` `#B8FFF4` |
| Marca da resistência | `#8E3B32` `#C94A4A` |

### 5.3 Identidade por zona *(proposta; só a Zona 0 está em produção, as demais ficam congeladas até o MVP)*

| Zona | Material dominante | Luz-chave | Luz de contraste | Atmosfera |
|---|---|---|---|---|
| 0. Prólogo (metrô/lab) | Concreto, azulejo, aço rebitado | Âmbar de emergência | Ciano de laboratório | Poeira no ar, faíscas, lâmpadas falhando |
| 1. Esgotos | Tijolo úmido, ferro, limo | Fogo e lâmpadas quentes do abrigo | Verde-água da água parada | Vapor, gotas, reflexo na água, insetos na luz |
| 2. Ruínas da superfície | Concreto quebrado, vergalhão, mato | Céu cinza-azulado | Neon distante refletido em poças | Chuva em 3 camadas, respingos, névoa baixa |
| 3. Cidade alta / setor corporativo | Brutalismo, vidro, painéis | Neon magenta e ciano | Branco frio de data center | Chuva fina, hologramas, drones ao longe |
| 4. Laboratório do pai | Madeira, papel, cobre | Luz quente de abajur | Azul de monitor antigo | Poeira parada, silêncio visual |
| 5. Torre central | Metal polido, cabos, núcleo | Branco-azulado clínico | Vermelho de alerta | Estática, pulsos de luz, glitches |

## 6. Luz (URP, renderer 3D)

Hoje não existe nenhuma luz no projeto; a "luz" é um sprite circular a 16% de opacidade. Passa a valer:

| Elemento | Regra |
|---|---|
| **Luz ambiente** | Muito baixa, tingida com a sombra da zona. A cena sem luzes práticas deve ficar quase preta. |
| **Luzes práticas** | Point e Spot em tempo real em toda fonte visível. Cada tela tem **1 luz-chave**, **1 a 3 de preenchimento** e **1 de contraste** em cor oposta. |
| **Sombras** | Sombras em tempo real ligadas na luz-chave de cada tela. Personagens projetam sombra (sprite com recorte por alpha). |
| **Sprites iluminados** | Personagens e props em sprite usam material iluminado, com normal map, para receber as luzes da cena e ganhar contorno (rim light). |
| **Emissivos** | Neon, telas, LEDs e olhos de robô usam material emissivo com cor HDR acima de 1, para acender o bloom. |
| **Luz volumétrica** | Feixes e cones de luz com poeira dentro, mais névoa de profundidade. |
| **Oclusão** | Oclusão de ambiente (SSAO) leve, para assentar objetos no chão e nos cantos. |
| **Luz conta história** | A luz aponta o caminho, o segredo e o perigo (pilar de sinalização diegética do `project-context.md`). Planeja-se no desenho da sala, não no polimento final. |
| **Animação de luz** | Pelo menos uma luz por tela oscila, pisca ou balança. |

### Pós-processamento (Volume global + override por zona)

| Efeito | Faixa inicial |
|---|---|
| HDR | ligado |
| Bloom | threshold 0,9–1,0 · intensidade 0,6–1,2 · scatter 0,6–0,7 |
| Tonemapping | Neutral ou ACES (decidir no beauty corner) |
| Color Adjustments / LUT | um por zona; contraste +10 a +20 |
| Vignette | 0,25–0,35 |
| Film Grain | fino, 0,15–0,25 |
| Depth of Field | ligado: foco no plano de jogo, fundo distante e primeiro plano levemente desfocados |
| Chromatic Aberration / distorção | só em eventos (dano, glitch da IA) |

## 7. Profundidade e composição

| Camada | Conteúdo | Profundidade (Z) | Tratamento |
|---|---|---|---|
| 1. Fundo distante | O túnel sumindo no escuro, estrutura ao longe | +20 ou mais | Baixo contraste, engolido pela névoa, desfocado |
| 2. Fundo médio | Colunas, tubulações, máquinas grandes, o outro lado da plataforma | +6 a +15 | Contraste médio |
| 3. Névoa | Faixas de névoa e poeira em suspensão | entre as camadas | Resolução de tela |
| 4. Parede próxima | Azulejo, painéis, cartazes, grafite | +1 a +3 | Recebe luz e sombra |
| 5. **Plano de jogo** | Sólidos, props com colisão, personagens | 0 | Em foco; maior contraste; personagens são o ponto mais claro |
| 6. Primeiro plano | Cabos, grades, vigas, entulho | −1 a −4 | Silhueta quase preta |
| 7. Primeiro plano desfocado *(opcional)* | Formas grandes fora de foco | −5 ou menos | Desfocado pela lente |

O parallax deixa de ser simulado por script (`ParallaxLayer`): ele vem da própria perspectiva.

Regras de composição:

- **Legibilidade por valor:** onde se pisa tem borda iluminada; o que mata tem cor de acento; o fundo nunca compete em contraste com o plano de jogo.
- **Quebrar a grade:** bordas irregulares, quinas quebradas, vigas tortas, entulho e props que ultrapassam o colisor. O retângulo lógico continua existindo só na física.
- **Ponto focal:** toda tela tem um objeto-herói (o vagão, a carcaça do protótipo, o terminal) para onde a luz-chave aponta.
- **Assimetria manual:** props posicionados com intenção, não espalhados por sorteio.

## 8. Densidade de detalhe

| Item | Mínimo por tela (640×360) |
|---|---|
| Variações de textura de cada material (parede, piso, teto) | 4 |
| Decalques (rachaduras, manchas, infiltração, cartazes, pichações) | 6 |
| Props distintos | 8 a 12, sendo 1 a 3 objetos-herói |
| Elementos animados de ambiente | 3 (luz piscando, cabo balançando, gota, vapor, faísca, rato, papel voando) |
| Sistemas de partículas de atmosfera | 1 a 2 (poeira, chuva, brasa, vapor) |
| Luzes | 3 a 5 |

- Peças modulares com transições (cantos, bordas, quebras), não um bloco de 64×64 repetido.
- Objetos-herói são desenhados inteiros, em tamanho grande (128 px ou mais), não montados com tiles.
- Texturas feitas com ruído aleatório uniforme estão proibidas como arte final.

## 9. Personagens e animação

- **Leitura:** personagens são o elemento mais claro e saturado da tela e recebem rim light da luz de contraste.
- **Contorno:** sem linha preta; contorno seletivo em tom mais escuro do próprio material.
- **Animação:** 10–12 fps. Mínimos para Dante: parado 6 quadros, correr 8, pular/cair 3+3, deslizar na parede 2, combo do bastão 4–5 por golpe, disparo 4, dano 2, morte 8. Com antecipação e acomodação em cada ação.
- **Efeitos:** rastro do bastão, faíscas de impacto, poeira ao pousar e ao correr, clarão do disparo com luz 2D de curta duração.
- **Kitty:** olhos e detalhes emissivos em `#4ECDC4`; ela é uma fonte de luz ambulante.
- **Robôs:** olho/sensor emissivo que muda de cor com o estado de percepção (patrulha, alerta, perseguição).
- **Ferramenta:** PixelLab para a base, com a paleta mestre travada, e retoque manual em Aseprite (pacote `2d.aseprite` já instalado).

## 10. Produção do cenário

- **O `ZoneArtKit` deixa de ser arte final.** Continua valendo como *graybox* (blocagem) para iterar level design.
- **Cenário = kit modular 3D de baixa complexidade com textura em pixel art** (32 texels por unidade, filtro Point), mais props em sprite onde fizer sentido. Geometria simples: o que vende a imagem é luz, textura, profundidade e atmosfera, não a contagem de polígonos.
- **Arte separada da lógica:** a arte de cada sala vive em um prefab próprio (`Art_<roomId>.prefab`) que o script de setup apenas instancia. Regerar a cena nunca apaga arte.
- **Texturas e sprites** vêm do PixelLab e/ou do Aseprite, sempre recoloridos para a paleta da zona (§5) antes de entrar no projeto. Foi a falta de paleta travada que causou a rejeição do primeiro lote.
- **Objetos-herói** (vagão, bilheteria, carcaça do protótipo) são peças próprias, não montadas com tiles.

### 10.1 Kit do Prólogo: escombros de uma estação de metrô

| Grupo | Peças |
|---|---|
| Estrutura | Plataforma com faixa tátil e borda amarela gasta; parede de azulejo com faixas da Linha 7; colunas de concreto com vergalhão exposto; teto com laje rachada e trecho desabado; boca de túnel |
| Via | Trilhos, dormentes, terceiro trilho, brita, vagão descarrilado e tombado, cabine do condutor |
| Estação | Bilheteria, catracas, escada rolante quebrada, bancos, lixeiras, relógio parado, mapa da rede, placas de saída e da Linha 7, cartazes e anúncios rasgados |
| Escombros | Montes de entulho, blocos de laje, vigas tortas, vergalhões, poeira acumulada, vidro quebrado |
| Infraestrutura | Eletrocalhas, cabos pendurados, dutos de ventilação, tubulação, quadros de energia, lâmpadas fluorescentes (acesas, piscando e mortas), luzes de emergência |
| Laboratório Domus | Painéis rebitados, portas de segurança, tanques, bancadas, terminais, cabos grossos, carcaça do protótipo de guerra |
| Vida que voltou | Infiltração, limo, raízes pelo teto, ratos, pichações da resistência, restos de acampamento |

Momentos de luz do Prólogo: fresta no teto desabado com feixe cinza-azulado e poeira; lâmpadas de emergência âmbar; fluorescentes falhando; faíscas de cabos partidos; interior do vagão iluminado por dentro; o túnel recuando para o escuro com luzes ao longe; o ciano frio tomando conta ao entrar no laboratório.

## 11. Decisão de renderização

**Decidido pelo usuário em 2026-11-14: 2,5D.** O renderer 2D do URP dá lugar ao renderer 3D, com câmera em perspectiva. A opção de manter o 2D com luzes 2D foi descartada.

O que **não** muda:

- Jogabilidade, física 2D, colisores, layout das salas e os testes existentes.
- A lógica em classes puras (`PlayerMotor`, modelos) e o fluxo de salas e transições.

O que muda:

- Renderer, câmera (`RoomCamera2D` passa a confinar pela área visível no plano de jogo), materiais dos sprites, cenário, luz e pós-processamento.
- As cenas que ainda não migraram (Esgotos e salas de teste) continuam funcionando no renderer 2D até a vez delas.

A migração começa por um **spike técnico** em uma única sala, antes de tocar o resto (§12).

## 12. Portão de qualidade: beauty corner

Antes de produzir arte para qualquer outra sala, **uma tela** da P1 (`prologue_metro_platform`) é levada à qualidade final em 2,5D, com Dante e Kitty já em sprite e com animação de parado e corrida. Ela é ao mesmo tempo o spike técnico da migração e a prova da direção de arte.

A tela só é aprovada pelo usuário se cumprir tudo:

- [ ] Lado a lado com capturas das referências, não parece de outra categoria de jogo.
- [ ] Parada, sem personagens, a tela funciona como ilustração.
- [ ] Não dá para identificar os retângulos de colisão olhando a imagem.
- [ ] Atende aos mínimos da §8.
- [ ] Tem luz-chave, preenchimento e contraste, com fonte visível para cada uma.
- [ ] Dante se destaca do cenário em qualquer ponto da tela.
- [ ] Pixels dos personagens nítidos e uniformes em 1080p e 1440p.
- [ ] A jogabilidade na sala é idêntica à de antes da migração.
- [ ] 60 FPS em 1080p com tudo ligado (meta do GDD).

Aprovado o beauty corner, os valores usados nele (paleta, bloom, intensidades de luz) são gravados neste documento e as marcas *(proposta)* saem.

## 13. Anti-padrões (o que dá "cara de projeto de faculdade")

- Retângulos de cor chapada ou com ruído uniforme.
- Rampa de cinza sem desvio de matiz.
- Sala iluminada por igual; "luz" feita com círculo branco semitransparente.
- Fundo vazio ou de uma camada só; cenário 3D sem textura em pixel art ou com filtro borrado.
- Sprite esticado, girado ou com pixels de tamanhos diferentes na mesma tela.
- Props espalhados por sorteio, simétricos ou repetidos em sequência.
- Personagem como forma geométrica tingida.
- Tela sem nenhum movimento ambiente.
- Tilesets de origens diferentes na mesma zona sem recolorir para a paleta.
