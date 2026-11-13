---
story_id: '1-2-input-system-and-device-tracker'
epic: 'epic-1'
status: done
created: 2026-11-13
---

# História 1.2 — Input System e Detecção de Dispositivo

## Objetivo
Implementar a leitura centralizada de ações a partir de `InputSystem_Actions.inputactions` (`GameInputReader`) com troca entre mapas `Player` (Gameplay) e `UI`, além de `InputDeviceTracker` para detectar automaticamente o último dispositivo ativo (`KeyboardMouse` vs. `Gamepad`).

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila sem erros nem warnings.
2. No Test Runner (**EditMode**), todos os testes em `InputDeviceTrackerTests` passam.
3. No menu **`UnderTheRain → Setup → Wire Input in Core Scene`**, `Core.unity` recebe `GameInputReader` (ligado a `Assets/InputSystem_Actions.inputactions`) e `InputDeviceTracker`.
4. No **Play Mode** em `Core.unity`, pressionar teclas (`W/A/S/D`, `Space`, `E`) ou botões do Gamepad atualiza o dispositivo ativo (`LastDevice`) e dispara as ações sem erros no Console.
