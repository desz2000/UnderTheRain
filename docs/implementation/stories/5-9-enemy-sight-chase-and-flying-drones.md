---
story_id: '5-9-enemy-sight-chase-and-flying-drones'
epic: 'epic-5'
status: review
created: 2026-11-14
---

# Story 5.9: Inimigos que Enxergam e Perseguem, Drones Voadores e Dante Menor

Status: review

**Pilar reforçado:** Combate legível, com telegrafia e tipos de robô complementares (§5.10), Kitty como ferramenta de combate (§5.11) e "Ato I ensina" sem ser fácil demais (§5.12).

## Story

Como jogador de *Under The Rain*,
quero que os robôs da colmeia reajam a mim (me vejam, avisem os vizinhos, venham atrás e desistam quando me perdem), que existam inimigos voadores e que Dante pareça pequeno diante das máquinas,
para que cada encontro seja uma ameaça ativa que pede posicionamento, e não um obstáculo que apenas ronda um ponto fixo.

## Acceptance Criteria

1. Um robô de chão que vê Dante (cone frontal, alcance configurável) mostra um alerta curto e legível e passa a persegui-lo; sem visão, continua patrulhando.
2. Parede sólida bloqueia a linha de visão. Plataformas one-way não bloqueiam.
3. Dano alerta o robô mesmo de costas ou sem visão.
4. O robô que avista Dante alerta os robôs próximos (mente coletiva); os distantes não são afetados.
5. Na perseguição, o robô não anda para fora de beiradas, não atravessa paredes, não sai da sala nem passa da coleira (`leashDistance`).
6. Ao perder Dante de vista, o robô vai até a última posição conhecida, espera a memória acabar e volta ao posto.
7. A interferência de Kitty congela um robô em perseguição e cancela a antecipação do ataque do drone.
8. O drone voador paira no posto, alinha-se acima de Dante quando o vê e ataca com um mergulho **telegrafado** (antecipação visível antes do golpe). Não atravessa chão nem paredes.
9. Dante é menor (mais baixo e mais estreito) que todos os robôs de chão.
10. P2 continua sem inimigos e sem dano. Robôs dormentes não percebem nada até acordarem.
11. Compila sem erros nem warnings; os testes EditMode e PlayMode passam.

## Tasks / Subtasks

- [x] **T1 — Percepção:** `EnemyAwarenessModel` (máquina de estados pura: Idle → Alerted → Chasing → Searching → Returning) e `EnemyAwareness` (cone de visão, raio de percepção próxima, linha de visão, chamado da colmeia, glifo de alerta).
- [x] **T2 — Perseguição no chão:** `PatrolInterferenceTarget` persegue, procura e volta ao posto; checa parede, beirada, coleira e limites da sala; expõe `Facing`, `Origin`, `IsTelegraphing`/`SetTelegraphing`.
- [x] **T3 — Drone:** `FlyingDroneMotor` (pairar, perseguir pelo ar, antecipação → mergulho → recarga, desvio de sólidos).
- [x] **T4 — Dano alerta:** `ScoutRobotEnemy` chama `EnemyAwareness.AlertToTarget()` ao levar dano.
- [x] **T5 — Setup:** `EnemySetupUtility.CreateRobot` passa a receber a posição dos **pés**, liga a percepção e o glifo em todo robô e ganha a spec `Drone` (`CreateDrone`). Novos tamanhos: Dante 0,7×1,3 · Batedor 1,0×1,7 · Operário 1,4×1,9 · Dormente 1,0×1,5 · Drone 1,0×0,7. Drones em P3 (`Drone_ShotLesson`) e P6 (`Drone_Shaft`).
- [x] **T6 — Testes:** `EnemyAwarenessModelTests` (7, EditMode) e `Story59EnemyAiTests` (10, PlayMode).
- [x] **T7 — Validação via CLI:** cenas `Prologue_Ruins` e `Act1_Sewers` regeradas; **EditMode 59/59** e **PlayMode 36/36**; sem warnings.
- [ ] **T8 — Validação manual no Play Mode** (ver "Pontos a observar").
- [ ] **T9 — gds-code-review.**

## Dev Notes

- **Arquitetura:** `PatrolInterferenceTarget` continua sendo o núcleo do robô (facção, hesitação, dormência). A percepção é um componente passivo: quem a avança é o núcleo, e só quando o robô está acordado e sem hesitar. O drone mantém o núcleo e troca apenas o movimento (`FlyingDroneMotor`).
- **Valores atuais (campos serializados; ajustar em playtest):**
  - Percepção: alcance 9 (Operário 7,5; Drone 10), cone de 50° para cada lado (Drone 125°), percepção próxima 2,2, alcance ×1,35 depois de engajado, reação 0,35 s, memória 2,5 s, chamado da colmeia 7.
  - Perseguição: Batedor 4,4 u/s, Operário 2,6 u/s (Dante corre a 8), coleira 14.
  - Drone: vida 2, 5 peças, paira a 1,8 u/s, persegue a 4,5 u/s, alinha 2,6 acima e 2,2 ao lado de Dante, antecipação 0,45 s, mergulho 11 u/s por até 0,7 s, recarga 1,4 s.
- **Telegrafia:** `SetTelegraphing` pinta o corpo de vermelho-alaranjado e é cancelado pela hesitação. A 5.7 (investida do Operário e arrancada do Batedor) reaproveita esse mesmo estado.
- **Sala reativada:** ao reentrar numa sala, os robôs voltam ao posto e ficam desavisados (`OnEnable`).
- **Posição dos robôs:** os setups agora informam o chão onde o robô pisa. Mudar a escala de uma spec não o afunda mais no piso.
- **Alcance do bastão:** a hitbox do bastão é filha de Dante, então encolheu junto com ele (largura efetiva 1,12 → 0,98; altura 1,6 → 1,3). Os robôs ficaram maiores, o que compensa em parte. Confirmar a sensação no playtest.

### Pontos a observar no playtest

- O alerta de 0,35 s dá tempo de reagir? O chamado da colmeia junta robôs demais em P3 (laje + chão)?
- Operário na ponte de P3: ele agora vem até Dante. A ponte ficou difícil demais para um tutorial?
- Drone de P3: dá para acertá-lo com o bastão no mergulho? O disparo virou a resposta natural?
- Drone de P6: atrapalha a descida de forma justa (descer pelas plataformas quebra a linha de visão)?
- Arena de P5: 3 robôs perseguindo ao mesmo tempo, com bateria cheia e vazia.
- Dante menor: leitura na tela, alcance do bastão e passagem pelas portas.

## Dev Agent Record

### Agent Model Used

Claude Fable 5.2 (`claude-fable-5-2`)

### Completion Notes List

- Pedido do usuário em 2026-11-13: inimigos que atacam ao ver Dante, inimigos voadores e Dante menor que os robôs.
- O ataque dos robôs de chão continua sendo o contato; a investida telegrafada deles fica na 5.7.
- O teste do drone pegou um erro de design na primeira versão: com o cone frontal de 50°, o drone não via alvos abaixo dele. O cone do drone passou a ser configurado no `FlyingDroneMotor` (125°).

### File List

- `Assets/_Project/Scripts/Runtime/Enemies/EnemyAwarenessModel.cs`, `EnemyAwareness.cs`, `FlyingDroneMotor.cs` (novos)
- `Assets/_Project/Scripts/Runtime/Enemies/PatrolInterferenceTarget.cs`, `ScoutRobotEnemy.cs` (modificados)
- `Assets/_Project/Scripts/Editor/EnemySetupUtility.cs`, `PrologueRuinsSetup.cs`, `Act1SewersSetup.cs`, `CatTestRoomSetup.cs`, `CombatTestRoomSetup.cs`, `MovementTestRoomSetup.cs` (modificados)
- `Assets/_Project/Scripts/Tests/EditMode/EnemyAwarenessModelTests.cs`, `Tests/PlayMode/Story59EnemyAiTests.cs` (novos)
- `Assets/_Project/Scenes/Zones/Prologue_Ruins.unity`, `Act1_Sewers.unity` (regeradas)
