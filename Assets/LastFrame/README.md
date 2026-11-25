# Last Frame: estructura del proyecto

Convenciones del GDD (sección 09, "Convenciones del proyecto").

## Carpetas

| Carpeta | Qué va aquí |
|---|---|
| `LastFrame/Art` | Modelos, texturas, sprites, materiales, animaciones, fuentes, UI y VFX del juego |
| `LastFrame/Audio` | Música (`MUS_`), efectos (`SFX_`), ambiente y mixers |
| `LastFrame/Prefabs` | Prefabs del juego (`PF_`) |
| `LastFrame/Scenes` | Escenas del juego (ver abajo) |
| `LastFrame/Scripts` | Código, separado por sistema (Core, Player, Enemies, Weapons...) |
| `LastFrame/ScriptableObjects` | Datos: recuerdos de las cintas (`MemoryData`), armas |
| `LastFrame/Timeline` | Cinemáticas (prólogo y final) |
| `LastFrame/Scenes/TestRoom` | Test room con las 13 demos de Senses 3 (no entra en la build) |

Los paquetes de la Asset Store (`CatBorg AI Framework`, `FlatKit`, `TextMesh Pro`) se quedan donde los instala Unity: no los muevas, porque las actualizaciones los vuelven a poner en su sitio.

## Escenas (orden de Build Settings)

| Escena | Qué es |
|---|---|
| `Menus/00_Splash` | Clasificación E10+ y logo del equipo |
| `Menus/01_MainMenu` | Jugar, Opciones, Créditos, Salir |
| `Cinematics/02_Prologue` | "El último cuadro" |
| `Sets/10_Set1_Tutorial` | El set apagado (siempre primero) |
| `Sets/11_Set2_Props` | Orden aleatorio |
| `Sets/12_Set3_Iluminado` | Orden aleatorio |
| `Sets/13_Set4_Camaras` | Orden aleatorio (diseño por definir) |
| `Sets/14_Set5_Reemplazo` | Jefe final (siempre último) |
| `Menus/20_GameOver` | La borran, vuelta al menú |
| `Cinematics/21_Ending` | La sacan al aire en vivo |
| `Menus/22_Results` | Tiempo, detecciones, cintas y nota S a E |
| `Menus/23_Credits` | Créditos |

Todas las escenas tienen la misma jerarquía: `--- Managers ---`, `--- Environment ---`, `--- Gameplay ---`, `--- UI ---`, `--- Lighting ---`, `--- Audio ---`.

## Nombres

Prefijo por tipo y nombre en PascalCase: `TEX_`, `SPR_`, `MAT_`, `MDL_`, `PF_`, `UI_`, `SFX_`, `MUS_`, `FNT_`, `VFX_`.
Ejemplo: `PF_Enemy_Office`, `TEX_PropBox_01`.

## Reglas de trabajo

- Una persona edita una escena o prefab a la vez.
- Cada quien trabaja en su rama.
- Cada asset va en el mismo commit que su archivo `.meta`.
- Comentarios, issues, PRs y commits en inglés.
- Para volver a generar carpetas o escenas que falten: `Tools > Last Frame > Create Project Structure` (no toca lo que ya existe).
- Git no sube carpetas vacías: cada una lleva un `.gitkeep`. Si creas carpetas nuevas vacías, usa `Tools > Last Frame > Add .gitkeep to Empty Folders`.
