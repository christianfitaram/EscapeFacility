# Game Design Document

## Escape the Facility

Proyecto académico desarrollado en Unity  
Versión del documento: `1.0`  
Fecha: `29/04/2026`  
Autor: `Christian Fita`

---

## Índice

1. [Portada](#game-design-document)
2. [Game Design](#game-design)
3. [Descripción de personajes](#descripción-de-personajes)
4. [Diseño de escenas](#diseño-de-escenas)
5. [Mecánicas de juego](#mecánicas-de-juego)
6. [Otras pantallas](#otras-pantallas)
7. [Arte y diseño](#arte-y-diseño)
8. [Notas técnicas](#notas-técnicas)

---

## Game Design

### Idea y objetivos del juego

`Escape the Facility` es un juego 3D en tercera persona ambientado en un complejo sci-fi situado en una superficie lunar. El jugador debe explorar un nivel exterior, recoger tres núcleos de energía, llegar a la instalación interior, cargar esos núcleos y finalmente emitir una señal de rescate.

Objetivos de diseño:

- Proponer una experiencia breve, clara y entregable para una asignatura.
- Combinar exploración, orientación espacial y gestión de objetivo en dos escenas distintas.
- Mantener una curva sencilla: recoger, sobrevivir, avanzar, activar el sistema y completar la misión.
- Reforzar la lectura del progreso mediante HUD, puertas bloqueadas, terminales y música contextual.

### Género

- Aventura 3D en tercera persona.
- Exploración con objetivos.
- Puzzle ligero de progresión.

### Público objetivo

- Jugadores casuales o académicos que necesiten una experiencia breve y fácil de entender.
- Público acostumbrado a controles básicos de juegos 3D en PC.
- Nivel de dificultad: bajo a medio.

### Historia

El protagonista es un operario atrapado durante el turno nocturno en un complejo de contención lunar. El exterior del complejo se ha vuelto inestable y está rodeado por zonas letales. Para escapar, el jugador debe recuperar tres núcleos de energía dispersos por el patio exterior, acceder al interior de la instalación, recargarlos en el panel de control y lanzar un mensaje de rescate antes de que se agote el tiempo de la operación.

### Controles

- Movimiento: `W`, `A`, `S`, `D`
- Cámara: `Ratón`
- Salto: `Espacio`
- Interacción con terminales: `E`
- Pausa: `Esc`

### Condiciones de inicio de la partida

- La partida estándar comienza en `MainMenu`.
- Al pulsar `Begin Run`, se inicia una nueva sesión en `Level1_Outdoor`.
- El menú de opciones permite también arrancar directamente en `Level1_Outdoor` o `Level2_Indoor` para pruebas.
- Cada nueva partida reinicia tiempo, puntuación, coleccionables y estado de misión.

### Condiciones de fin de la partida

Victoria:

- Haber recogido los `3` núcleos de energía.
- Activar el terminal de carga en `Level2_Indoor`.
- Activar el terminal de broadcast para enviar la señal de rescate.

Derrota:

- Agotar el tiempo máximo de partida: `30:00`.

Reinicio parcial:

- Entrar en una `killzone` no termina la sesión completa, pero reinicia la escena actual y resta la puntuación/coleccionables obtenidos en esa escena.

---

## Descripción de personajes

### Protagonista

- Nombre funcional: `PlayerArmature`
- Prefab preferente en runtime: `Assets/Characters/Mixamo/PlayerArmature_Mixamo_External.prefab`
- Base técnica: controlador en tercera persona de `Starter Assets`
- Modelo: variante Mixamo generada desde `Swat.fbx` o, si no está disponible, desde `character_external.fbx`
- Rol jugable: explorar, saltar, recoger núcleos y activar terminales
- Fortalezas jugables: movilidad clara, cámara libre y lectura visual limpia
- Debilidad principal: no tiene combate ni vida; la presión proviene del tiempo y del entorno

### NPCs y enemigos

- No hay NPCs aliados ni enemigos implementados en la versión actual.
- La amenaza del juego es ambiental: zonas letales, trazado del escenario y límite de tiempo.

### Entidades interactivas

- `Collectible`: núcleo de energía flotante que suma `100` puntos por unidad.
- `MissionTerminal`: terminales de carga y de broadcast.
- `DoorController`: puertas que abren la progresión entre escenas.
- `Killzone`: superficies letales del exterior.

---

## Diseño de escenas

### Escenas incluidas

- `MainMenu`
- `OptionsMenu`
- `Level1_Outdoor`
- `Level2_Indoor`
- `EndScreen`

### Mapa del mundo

#### Level1_Outdoor

Tema: patio exterior lunar con rocas, balizas, lava letal y acceso a la instalación.

Objetivo:

- Recoger `3/3` núcleos de energía.
- Llegar a la puerta de acceso interior.

Distribución aproximada:

```text
NORTE

+------------------------------------------------------+
| Killzone perimetral            Core_03               |
|                                      Extraction Pad  |
|                                       Exit / Facility|
|                                                      |
|                     Rocas y laberinto                |
|                                                      |
| Core_01                                              |
|                                                      |
|          Core_02                                     |
| Spawn / Arrival Pad                                  |
+------------------------------------------------------+

SUR
```

Puntos relevantes:

- Spawn del jugador cerca de `ArrivalPad`.
- Núcleos colocados en tres zonas separadas para obligar a recorrer el nivel.
- `Killzone` perimetral y sectores de riesgo que castigan rutas incorrectas.
- Puerta final del exterior conectada con `Level2_Indoor`.

#### Level2_Indoor

Tema: instalación interior compacta, iluminada y centrada en terminales.

Objetivo:

- Cargar los núcleos en el `ChargePanel`.
- Emitir el mensaje de rescate desde `BroadcastConsole`.

Distribución aproximada:

```text
NORTE

+-----------------------------------------+
|          BroadcastConsole               |
|          Broadcast Array                |
|                                         |
|                       Reactor Surface   |
|                       ChargePanel       |
|                                         |
|                                         |
| Airlock / puerta de entrada             |
+-----------------------------------------+

SUR
```

Puntos relevantes:

- El aire cambia de exploración abierta a navegación contenida.
- El terminal de carga está junto a la zona del reactor.
- El terminal de broadcast cierra la misión y lleva a `EndScreen`.

### Mapa del HUD

```text
+------------------------------------------------------+
| [StatusPanel]                                        |
| Score                                                |
| Time Left                                            |
| Objective                                            |
|                                                      |
|                    Vista de juego                    |
|                                                      |
|               [PausePanel si Esc]                    |
|                                                      |
|            [Interaction Prompt: Press E]             |
+------------------------------------------------------+
```

Elementos del HUD:

- `StatusPanel` arriba a la izquierda.
- `InteractionPrompt` en la zona inferior central.
- `PausePanel` centrado, visible sólo en pausa.

---

## Mecánicas de juego

### Exploración y movimiento

- Movimiento libre en tercera persona con cámara orbital.
- Salto para salvar desniveles y reforzar la lectura del terreno.

### Recolección

- Cada núcleo de energía flota, gira y hace un movimiento de bobbing para mejorar su visibilidad.
- Cada núcleo suma `100` puntos.
- En la versión actual hay `3` núcleos en total, por lo que la puntuación base máxima por coleccionables es `300`.

### Progresión por objetivos

Flujo principal:

1. Recoger `3` núcleos en el exterior.
2. Abrir la progresión hacia el interior.
3. Cargar los núcleos en el terminal de carga.
4. Activar el terminal de broadcast.
5. Mostrar pantalla final.

### Puertas y desbloqueo

- Las puertas se desbloquean automáticamente cuando se cumple el objetivo configurado de la escena.
- La puerta exterior conduce del patio a la instalación.
- La finalización real de la partida se produce al transmitir el mensaje de rescate.

### Interacción contextual

- Cuando el jugador entra en el área de un terminal, aparece un mensaje contextual.
- La acción se ejecuta con `E`.
- Los terminales muestran distinto estado visual según estén bloqueados, listos o activados.

### Riesgo ambiental

- Las `killzones` reinician la escena actual.
- Al morir en una escena, se elimina del total lo recogido en esa misma escena para evitar duplicar puntuación.

### Tiempo y presión

- La sesión completa tiene un límite de `30` minutos.
- El HUD muestra `Time Left`.
- Si el contador llega a `00:00`, la partida termina en derrota.

### Audio reactivo

- El juego cambia de tema musical según escena: menú, exterior, interior y final.
- Los sonidos de recogida, apertura y fallo refuerzan la lectura del estado.

---

## Otras pantallas

### Menú principal

Funciones:

- Iniciar partida con `Begin Run`
- Ir a opciones con `Mission Settings`
- Salir con `Abort Shift`

Rol visual:

- Presentar tono sci-fi.
- Resumir la misión desde el primer contacto.

### Menú de opciones

Funciones:

- Activar o desactivar música con `Toggle Audio Feed`
- Cargar directamente `Level1_Outdoor`
- Cargar directamente `Level2_Indoor`
- Volver al menú principal

Rol:

- Pantalla de configuración ligera y menú de acceso rápido para pruebas.

### Menú de pausa

Funciones:

- Reanudar partida
- Volver al menú principal
- Salir del juego

Rol:

- Congelar la acción y desbloquear el cursor.

### Pantalla final

Estados:

- `Victory`
- `Defeat`

Información mostrada:

- Puntuación final
- Tiempo restante
- Número de objetos recogidos

Acciones:

- Repetir la partida
- Salir del juego

---

## Arte y diseño

Nota:

- En las tablas siguientes se distinguen recursos `confirmados en el repositorio` y recursos `inferidos por coincidencia de paquete`.
- Cuando el repositorio no conserva el enlace original del autor, se enlaza la fuente más fiable disponible o se deja indicado como `origen no documentado`.

### Personajes

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `PlayerArmature_Mixamo_External.prefab` | Confirmado en repo | [Prefab local](Assets/Characters/Mixamo/PlayerArmature_Mixamo_External.prefab) | Sin miniatura raster exportada en el repo | Protagonista jugable |
| `Swat.fbx` / `character_external.fbx` | Confirmado en repo | [Modelo local](Assets/Characters/Mixamo/Swat.fbx) | Sin miniatura raster exportada en el repo | Fuente del modelo Mixamo |
| `Starter Assets - ThirdPerson | URP` | Confirmado por paquete | https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-urp-196526 | `![Starter Assets](Assets/StarterAssets/TutorialInfo/Icons/ReadMeImg.PNG)` | Controlador, cámara, animador base |

Animaciones del personaje:

- `Idle Walk Run Blend`
- `JumpStart`
- `FreeFall`
- `JumpLand`

Estas animaciones se gestionan mediante `StarterAssetsThirdPerson.controller`.

### Objetos recolectables

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| Núcleo de energía procedural (`Collectible`) | Confirmado en repo | [Script local](Assets/Scripts/Collectible.cs) | Sin textura dedicada; esfera con material emisivo generada en escena | Objetivo principal del nivel exterior |
| Partículas del coleccionable | Confirmado en repo | [Builder local](Assets/Scripts/Editor/EscapeFacilitySceneBuilder.cs) | Sin sprite importado independiente | Aura visual del núcleo |

### Objetos decorativos

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `Lunar Landscape 3D` | Confirmado por nombre de paquete | https://assetstore-fallback.unity.com/packages/3d/environments/landscapes/lunar-landscape-3d-132614 | `![Lunar Landscape](Assets/Lunar%20Landscape%203D/Resources/Textures/Ground_00_D.png)` | Terreno exterior, rocas, texturas lunares |
| `Sci-Fi Styled Modular Pack` | Inferido por coincidencia de carpeta/prefabs | https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913 | `![Sci-Fi Modular](Assets/Sci%20Fi%20Modular%20Pack/Textures/Box3/Box3_DefaultMaterial_AlbedoTransparency.png)` | Paredes, suelos, puertas, cajas y ventanales del interior |
| `Rocks Stylized` / rocas auxiliares | Confirmado en repo | [Textura local](Assets/PolyOne/Rocks%20Stylized/Texture/Rocks_Stylized_Color.png) | `![Rocks](Assets/PolyOne/Rocks%20Stylized/Texture/Rocks_Stylized_Color.png)` | Masa rocosa y apoyo visual exterior |

### Música de fondo

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `MainTrack.mp3` | Confirmado en repo, origen externo no documentado | [Audio local](Assets/Resources/Audio/MainTrack.mp3) | `![Music Reference](Assets/Lunar%20Landscape%203D/Resources/Textures/Milkyway.png)` | Tema principal del exterior |
| Temas `Menu`, `Indoor`, `EndVictory`, `EndDefeat` | Confirmado en repo | [AudioManager.cs](Assets/Scripts/AudioManager.cs) | No aplica | Música procedural generada por código |

### Efectos de sonido

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `Player_Footstep_01-10.wav` y `Player_Land.wav` | Confirmado en repo | [Carpeta local](Assets/StarterAssets/ThirdPersonController/Character/Sfx) | No aplica | Pasos y aterrizaje del personaje |
| `Pickup`, `DoorOpen`, `Failure` | Confirmado en repo | [AudioManager.cs](Assets/Scripts/AudioManager.cs) | No aplica | Sonidos generados por código para recoger, abrir y fallar |
| `SFX_FireSmall_L.wav`, `SFX_FireMedium_L.wav`, `SFX_FireBig_L.wav` | Confirmado en repo | [Carpeta local](Assets/Vefects/Free%20Fire%20VFX%20URP/Audio) | No aplica | Banco de audio del pack VFX importado |

### Objetos de iluminación

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `Light1.prefab` y `Light2.prefab` | Confirmado en repo | [Prefab local](Assets/Sci%20Fi%20Modular%20Pack/Prefabs/Light1.prefab) | `![Light](Assets/Sci%20Fi%20Modular%20Pack/Textures/Light2/Light2_DefaultMaterial_AlbedoTransparency.png)` | Luminarias del interior |
| Balizas, spotlights y point lights procedurales | Confirmado en repo | [Scene Builder](Assets/Scripts/Editor/EscapeFacilitySceneBuilder.cs) | No aplica | Iluminación exterior, interior y focos de objetivo |
| `NightSkyController` | Confirmado en repo | [Script local](Assets/Scripts/NightSkyController.cs) | `![Sky](Assets/Lunar%20Landscape%203D/Resources/Textures/Milkyway.png)` | Cielo nocturno, niebla, ambiente y estrellas |

### Sistema de partículas

| Asset | Estado | Link | Imagen | Uso |
| --- | --- | --- | --- | --- |
| `Free Fire VFX` | Confirmado por nombre de paquete | https://assetstore.unity.com/packages/vfx/particles/fire-explosions/free-fire-vfx-266227 | `![Fire VFX](docs/gdd-images/fire-vfx-preview.png)` | Pack importado para fuego/partículas |
| Partículas de estrellas | Confirmado en repo | [NightSkyController.cs](Assets/Scripts/NightSkyController.cs) | No aplica | Campo de estrellas procedural del nivel exterior |
| Partículas de coleccionables | Confirmado en repo | [EscapeFacilitySceneBuilder.cs](Assets/Scripts/Editor/EscapeFacilitySceneBuilder.cs) | No aplica | Resaltar los núcleos de energía |

Observación:

- El repositorio contiene el pack `Free Fire VFX`, pero en las escenas jugables actuales no se ha confirmado una instancia directa de sus prefabs de fuego.
- Sí se usan partículas creadas por código para estrellas y coleccionables.

---

## Notas técnicas

- Motor: `Unity`
- Escenas en build: `MainMenu`, `OptionsMenu`, `Level1_Outdoor`, `Level2_Indoor`, `EndScreen`
- Sistema de entrada: `Unity Input System` + `Starter Assets`
- Cámara: `Cinemachine`
- Render pipeline: `URP`
- Música configurable mediante `PlayerPrefs`

### Resumen de bucle jugable

```text
MainMenu
  -> Level1_Outdoor
  -> recoger 3 núcleos
  -> entrar en Level2_Indoor
  -> cargar núcleos
  -> emitir broadcast
  -> EndScreen
```
