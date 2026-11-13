---
story_id: '4-3-cat-exploration-duct-or-terminal'
epic: 'epic-4'
status: done
created: 2026-11-13
---

# História 4.3 — Interação de Exploração do Gato (Duto / Terminal)

**Pilar reforçado:** O gato como mecânica de exploração e atalhos unilaterais (Seções 5.2 e 5.11 — abrir caminhos e atalhos que ligam áreas sem resolver o jogo sozinho).

## Objetivo
Implementar `CatExploration` e `CatExplorationPoint` permitindo enviar o gato por um duto estreito ou terminal offline (`E` / `Y` / `Triângulo`) para abrir uma porta/atalho pelo outro lado.

## Arquitetura
- `CatController.SetExternalControl(bool)` — suspende temporariamente a física de seguimento enquanto o gato executa a travessia do duto/terminal.
- `CatExplorationPoint` (`Runtime/World/`) — ponto interativo (`Duct` ou `Terminal`) com posição de entrada, destino do outro lado, prompt visual quando Dante está perto e abertura de porta/atalho ao concluir.
- `CatExploration` (`Runtime/Companion/`) — escuta `GameInputReader.OnInteractPressed` quando Dante está junto a um `CatExplorationPoint` disponível, conduz o gato pela travessia e completa a interação.
- `CatExplorationTests` (`Tests/EditMode/`) — valida regras de disponibilidade, execução única e proteção contra dupla ativação.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `CatExplorationTests` passam.
3. No **Play Mode** (`CatTestRoom`), aproximar-se do duto (`NarrowDuct`) e apertar **`E`** (ou **`Y` / `Triângulo`**) envia o gato pelo duto e abre a `ShortcutDoor` pelo outro lado.
4. Após abrir o atalho, interagir novamente com o mesmo duto não repete a ação.

## Estado
- [x] `CatController.SetExternalControl`, `CatExplorationPoint` e `CatExploration`.
- [x] `CatExplorationTests` (EditMode).
- [x] Atualização de `CatTestRoomSetup` com `NarrowDuct` + `ShortcutDoor`.
- [x] Validação no Play Mode do Unity.
