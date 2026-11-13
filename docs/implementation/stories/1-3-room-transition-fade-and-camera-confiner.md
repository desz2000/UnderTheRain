---
story_id: '1-3-room-transition-fade-and-camera-confiner'
epic: 'epic-1'
status: done
created: 2026-11-13
---

# História 1.3 — Transição de Salas com Fade-to-Black e Câmera 2D

## Objetivo
Implementar a infraestrutura de salas interconectadas estilo *Hollow Knight* (sem tela de loading): `RoomBounds`, `RoomSpawnPoint`, `RoomExitTrigger`, `RoomCamera2D` (confinamento ortográfico aos limites da sala), `ScreenFader` (fade-to-black rápido de ~0,25s) e `RoomTransitionController`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes de `RoomCamera2DTests` passam.
3. No menu **`UnderTheRain → Setup → Wire Room Transition in Core Scene`**, `Core.unity` recebe `ScreenFader`, `RoomCamera2D` e `RoomTransitionController`.
4. No **Play Mode**, acionar uma transição entre duas salas executa fade-to-black (~0,25s), posiciona o alvo no `RoomSpawnPoint` de destino e confina a câmera dentro dos limites do `RoomBounds` da nova sala.
