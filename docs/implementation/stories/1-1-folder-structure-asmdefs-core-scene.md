---
story_id: '1-1-folder-structure-asmdefs-core-scene'
epic: 'epic-1'
status: done
created: 2026-11-13
---

# História 1.1 — Estrutura de Pastas, Assembly Definitions e Cena Base

## Objetivo
Criar a hierarquia `Assets/_Project/` (`Art`, `Audio`, `Data`, `Prefabs`, `Scenes`, `Scripts`, `Settings`), Assembly Definitions (`UnderTheRain.Runtime`, `UnderTheRain.Editor`, `UnderTheRain.Tests.EditMode`, `UnderTheRain.Tests.PlayMode`), teste inicial de sanidade e script de Editor para gerar a cena persistente `Assets/_Project/Scenes/Core.unity`.

## Critérios de Aceite (Verificáveis no Unity)
1. O projeto compila no Unity Editor sem erros nem warnings nos scripts em `Assets/_Project/`.
2. O menu `UnderTheRain/Setup/Create Core Scene` gera `Assets/_Project/Scenes/Core.unity` com a `Main Camera` 2D e o objeto `GameBootstrap`.
3. Abrir `Core.unity` e entrar no Play Mode roda sem erros no Console.
4. No `Window > General > Test Runner` (EditMode), o teste `SanityTests.RuntimeAssembly_IsLoaded` passa.
