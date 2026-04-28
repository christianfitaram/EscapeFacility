# Escape the Facility

Proyecto académico: un juego 3D en tercera persona desarrollado en Unity. El jugador explora un entorno sci‑fi, recoge núcleos de energía, evita zonas letales y completa objetivos para escapar de la instalación.

---

## Contenido

- [Descripción](#descripción)
- [Estado del proyecto](#estado-del-proyecto)
- [Instalación y apertura del proyecto](#instalación-y-apertura-del-proyecto)
- [Ejecutar en el Editor](#ejecutar-en-el-editor)
- [Instrucciones de build](#instrucciones-de-build)
- [Controles y jugabilidad](#controles-y-jugabilidad)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Lista de scripts y sistemas clave](#lista-de-scripts-y-sistemas-clave)
- [Recursos, licencias y créditos](#recursos-licencias-y-créditos)
- [Contacto y autoría](#contacto-y-autoría)

---

## Descripción

`Escape the Facility` es un prototipo jugable diseñado para cumplir los requisitos de una asignatura de desarrollo de videojuegos. Mezcla exploración en un nivel exterior (terreno) y un nivel interior (instalación), con coleccionables, zonas letales (killzones), menús y HUD que muestran progreso, tiempo y puntuación.

Objetivo principal: recoger los núcleos de energía requeridos, cargarlos en el sistema y activar el broadcast de rescate para salir de la instalación.

## Estado del proyecto

- Estado: jugable en Editor (borrador entregable).  
- Menús principales implementados: Main Menu, Options, Pause, End Screen.  
- Escenas: Nivel exterior (Terrain) y Nivel interior (instalado y decorado).  
- Builds: crear desde Editor (consultar la sección de build).  


## Instalación y apertura del proyecto

1. Clona el repositorio:

```bash
git clone <repo-url> EscapeFacility
cd EscapeFacility
```

2. Abre Unity Hub y selecciona la versión de Unity correspondiente.
3. En Unity Hub, elige `Add` y asigna la carpeta del proyecto `EscapeFacility`.
4. Abre el proyecto desde Unity Hub. Cuando Unity importe los assets, espera a que termine la indexación.
5. Si se usan paquetes o URP, ve a `Window → Package Manager` y aplica las recomendaciones del proyecto.

## Ejecutar en el Editor

- Abre la escena principal desde `Assets/Scenes` (por ejemplo `MainMenu` o `Level1_Outdoor`).
- Pulsa el botón `Play` en la parte superior del Editor.

Recomendaciones:
- Comprueba la consola (Window → General → Console) para errores de dependencias o scripts faltantes.
- Si el cursor no aparece en menús, revisa las llamadas a `Cursor.lockState` en `MainMenuController` o `PauseMenu`.

## Instrucciones de build

1. Abre `File → Build Settings` y añade las escenas requeridas al `Scenes In Build`.
2. Selecciona la plataforma objetivo (Windows o Linux) y configura `Player Settings` según el objetivo.
3. Pulsa `Build` o `Build And Run`.

Requisitos de entrega (según la asignatura): builds para Windows y Linux obligatorios — revisa [Unity_Game_Requirements.md](Unity_Game_Requirements.md) para la lista completa.

## Controles y jugabilidad

- Movimiento: WASD
- Saltar: Espacio
- Cámara: Ratón
- Pausa / Menú: ESC

Flujo de juego:
1. Empieza en Main Menu → Start
2. Level 1 (exterior) — recolectar núcleos y evitar killzones
3. Acceder al interior, completar carga de núcleos
4. Activar broadcast → abrir salida → End Screen

## Estructura del repositorio

Estructura principal (resumen):

- Assets/: recursos del proyecto (scripts, escenas, prefabs, materiales)
- Packages/: paquetes de Unity
- ProjectSettings/: configuración del proyecto Unity
- README_Escape_the_Facility.md — versión alternativa del README
- Escape_The_Facility_README.md — notas de diseño y sistema
- Unity_Game_Requirements.md — requisitos y criterios de evaluación

Para detalles de carpetas y generación de escenas automáticas, consulta `Assets/Editor/` (herramientas de generación del proyecto).

## Lista de scripts y sistemas clave

Entre los scripts más relevantes están:

- `GameManager.cs` — Lógica global de sesión y estado
- `Collectible.cs` — Comportamiento de los núcleos de energía
- `DoorController.cs` — Apertura / bloqueo de puertas según objetivos
- `Killzone.cs` — Detección de muerte y reinicio de nivel
- `AudioManager.cs` — Gestión global de música y SFX
- `GameplayHUD.cs` — Actualización de puntuación, tiempo y contadores
- `MainMenuController.cs`, `OptionsMenuController.cs`, `EndScreenController.cs` — UI y navegación

Estos archivos están en `Assets/Scripts/` (o subcarpetas).

## Recursos, licencias y créditos

- Activos 3D, animaciones, Mmúsica y efectos: se usan recursos (Mixamo o paquetes gratuitos). Respeta las licencias de cada asset.  


## Contacto y autoría

Proyecto: `Escape the Facility`  
Autor : Christian Fita.

