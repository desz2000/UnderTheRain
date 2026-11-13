# Retrospectiva — Épico 1: Fundação do Projeto

- **Data:** 2026-11-13
- **Branch:** `feature/epic-1-foundation`
- **Histórias concluídas:** `1.1`, `1.2`, `1.3` (todas com testes EditMode/PlayMode verdes no Unity).

## O que funcionou bem
- Separação em 4 Assembly Definitions (`Runtime`, `Editor`, `Tests.EditMode`, `Tests.PlayMode`) manteve compilação rápida e testes limpos.
- Scripts de Editor (`ProjectSetupMenu.cs`) geraram e conectaram `Core.unity` sem editar `.unity` YAML manualmente.
- `RoomCamera2D` + `ScreenFader` + `RoomTransitionController` entregaram a transição estilo *Hollow Knight* (fade-to-black sem loading) sem depender de pacotes extras.

## Aprendizados técnicos (adicionados ao `project-context.md`)
1. `InputTestFixture` exige a referência `"Unity.InputSystem.TestFramework"` no `.asmdef` de testes.
2. `GameInputReader` deve instanciar uma cópia de runtime (`Instantiate(inputActions)`) em `Awake()` e destruí-la em `OnDestroy()` para evitar erros de estado de `InputActionMap` no Play Mode.
3. Comandos `[MenuItem]` que usam `EditorSceneManager.OpenScene` devem checar `EditorApplication.isPlaying` antes de rodar.
