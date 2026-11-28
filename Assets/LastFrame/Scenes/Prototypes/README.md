# Prototypes: blocking de niveles

## Versiones (no se borra ninguna)

| Versión | Escenas | Menú | Qué es |
|---|---|---|---|
| **v10 (vigente, P1–P5)** | `v10/Prototype N v10.unity` | Tools > Last Frame > Build Prototype Blocking v10 (abandonado) | La v9 con luz de estudio abandonado de verdad: la zona antigua ya no tiene lámparas encendidas, entra luz de tarde por cuatro ventanas polvorientas, un proyector olvidado encendido ilumina la cinta, los fluorescentes están casi todos fundidos y la bombilla de la sala de guiones falla. Hay polvo flotando en cada haz y solo los focos de la IA dan luz limpia. Objetos de las imágenes: proyector, rollos de película, caballetes con dibujos, decorados apoyados, dibujos en el suelo, papeleras, perchero, taburete, archivador, pilas de dibujos y muebles tapados. Pulido: la luz de las ventanas es un sol con cookie de ventana (los cuatro cristales se proyectan en el suelo) y un haz que se desvanece; la recepción no tiene guardia; Ollie no salta ni se sube a los muebles (escalón 0,08 y salto 0); todas las rutas comprobadas con el NavMesh. P2–P5 llevan el mismo acabado sobre su diseño: sus luces de la v3 con charco, cono y polvo, una de cada cuatro luces antiguas falla, ventanas polvorientas en paredes exteriores con espacio libre (P2–P4; el P5 es exterior de noche) y objetos solo donde dejan 2,2 m de paso. Los objetos de juego (arbustos, río, papeleras de rastro, peceras) nunca se tapan con sábanas |
| v9 (estilo GDD, solo P1) | `v9/Prototype 1 v9.unity` | Tools > Last Frame > Build Prototype Blocking v9 (P1, estilo GDD) | El plan de luces de la v8 con la dirección de arte de las imágenes de concepto y la paleta oficial del GDD (`Docs/Referencias/paleta_gdd.png`): estudio de madera cálida (suelos de tablones de nogal, paredes de madera, alfombras ladrillo), sombras ciruela oscura, uno de cada tres muebles tapado con una sábana marfil y dos ventanas polvorientas con luz de tarde que también cuentan para el sigilo. Rojo apagado para los enemigos, azul pizarra reservado para Ollie |
| v8 (luz motivada, solo P1) | `v8/Prototype 1 v8a…v8d.unity` | Tools > Last Frame > Build Prototype Blocking v8 (P1, luz motivada) | Estudio abandonado con zonas que la IA ha reactivado. Sin sol: cada luz sale de un objeto visible (focos de seguridad de la IA, flexos, fluorescentes, dos fundidos y uno que falla, bombilla desnuda, tele, luces de emergencia y cartel de salida) y dibuja su cono y su charco en el suelo. Son luces reales que mide Senses: los charcos son las zonas de peligro y los huecos oscuros, las rutas de sigilo. El fluorescente que falla (`LightFlicker`) se apaga de verdad y abre ventanas de paso. Cuatro variantes: **v8a** sobre la v7b, **v8b** sobre la v7c, **v8c** sobre la v5 y **v8d**, un punto intermedio entre la v8b y la v8c (todos los valores promediados y la paleta mezclada al 50 %) |
| v7 (4 variantes, solo P1) | `v7/Prototype 1 v7a…v7d.unity` | Tools > Last Frame > Build Prototype Blocking v7 (P1, 4 variantes) | Cuatro direcciones de look para elegir: **v7a** lápiz y papel (colores de papel, líneas de grafito, grano de fibras, algunos props como boceto sin colorear), **v7b** alma vs algoritmo (estudio cálido tirando a sepia, los enemigos de IA traen luz y sombra de neón cian), **v7c** penumbra pop (media luz con lámparas magenta, turquesa y ámbar), **v7d** cartoon plano de TV (colores planos saturados, sin sombras, tinta negra gruesa). Los personajes llevan contorno de tinta y sombra de color. Cada look es un `LookProfile` del builder |
| v6 (prueba cartoon, solo P1) | `v6/Prototype 1 v6.unity` | Tools > Last Frame > Build Prototype Blocking v6 (P1, cartoon) | La v5 más luminosa y cartoon, sin el tono sombrío: estudio iluminado con luz cálida, paleta de los 80 más saturada (el azul sigue reservado para Ollie), sombras lavanda claras con un brillo suave en los bordes, lámparas como acento, postproceso ligero (más saturación, casi sin viñeta ni grano). Mismo contorno de tinta, cámara y sprites que la v5. Materiales en `Art/Materials/Blocking/FlatKit_v6/`. Si se aprueba, se aplica al resto |
| v5 (archivo) | `v5/Prototype N v5.unity` | Tools > Last Frame > Build Prototype Blocking v5 (look) | El diseño de la v4 con un look nuevo: estudio de noche con charcos de luz cálida, sombras moradas, contorno de tinta en pantalla (renderer *Look* de `URP_LastFrame`), grano y viñeta, cámara a 57° y Ollie y los enemigos como sprites 2D provisionales encima de los objetos de Senses. Azul reservado para Ollie, rojo para los enemigos (el río pasa a verde azulado). P2–P5 conservan sus luces de la v3, un 40 % más fuertes, y los niveles de noche (P2 y P5) y con luz tenue (P3 y P4) quedan más oscuros que el P1 para que la luz siga contando |
| v4 (archivo) | `v4/Prototype N v4.unity` | Tools > Last Frame > Build Prototype Blocking v4 (archivo) | El diseño de la v3 con el Flat Kit: todos los materiales del blocking y los opacos de los objetos de Senses (enemigos, cámaras, atrezo) pasan a *FlatKit/Stylized Surface*, con un solo paso de sombreado, la sombra tirando al color tinta y un contorno de tinta. Materiales en `Art/Materials/Blocking/FlatKit/` |
| v3 (archivo) | `v3/Prototype N v3.unity` | Tools > Last Frame > Build Prototype Blocking v3 (archivo) | v2 + mecánicas acumulativas: lo que enseña un nivel vuelve en todos los siguientes (luz, visor nocturno, zonas de sigilo, cámaras, cobertura a medias, oído, suelos, latas). P2 con 6 enemigos, P3 con 5 cámaras y 6 enemigos, sala de foley nueva en P4 (tres carriles), P5 de noche con farolas, reflectores y 4 cámaras. P3 y P4 con luz tenue |
| v2 (archivo) | `v2/Prototype N v2.unity` | Tools > Last Frame > Build Prototype Blocking v2 (archivo) | Ajustada al GDD: Ollie a escala 0,7 y muebles ×1,2; enemigos de IA (la nutria patrulla, el capibara se queda quieto) con borrador o lanza gomas; la cinta abre la salida en los 5 niveles; perro, río, basura, visión nocturna, prismáticos y latas adaptados al tema sin quitar mecánicas |
| v1 (archivo) | `v1/Prototype N.unity` | Tools > Last Frame > Build Prototype Blocking v1 (archivo) | Primer blockout, a escala humana y con guardias de seguridad. Lo que sigue en este README describe la v1 |

Cada versión vive en su carpeta (`v1/` a `v10/`), sin escenas sueltas en la raíz. Capturas: `Docs/Blockout/v1/` a `Docs/Blockout/v10/`. En `v6/` está también `Prototype 1 v6 (look v5).unity`, una copia del P1 con el look de la v5 para comparar. El registro completo del proceso está en el documento *Last Frame · Blockout de niveles*, en la pestaña "Registro del proceso".

Cinco niveles de blocking ambientados en un **estudio antiguo de caricaturas y televisión**. Cada uno es una zona del estudio: salas, pasillos y platós con mobiliario y decorados hechos con primitivas. Cada nivel añade mecánicas del Senses Pack encima de las anteriores. Los guardias, cámaras, el perro, la hierba y el río son los objetos de Senses de la test room, y funcionan en Play Mode con el jugador de test.

Se regeneran desde **Tools > Last Frame > Build Prototype Blocking v1 (archivo)** (sobrescribe las 5 escenas de `v1/`). Script: `Scripts/Editor/PrototypeBlockingBuilder.cs`.

## Enemigos

Hay dos diseños de comportamiento y cada enemigo, según su sprite, lleva un arma de melé o de distancia:

| | Melé (porra) | Distancia (pistola de tinta) |
|---|---|---|
| **Tímido / estático**: se queda en su sitio | Recepcionista, guardia viendo dibujos, guardia del acuario, técnico de foley, ingeniero de sonido, guardia del archivo | Guardia en el set, guardia con visión nocturna, jardinero, productor, archivero con prismáticos |
| **Patrulla**: recorre una ruta | Vigilante del pasillo, tramoyista, guardia del backlot | Guardia de inventario, músico nervioso |

En la escena, el arma es un objeto en la mano del guardia y la ruta de patrulla se dibuja en el suelo. Los guardias de Senses todavía no se mueven: solo el perro sigue su ruta. Ni los ataques de los enemigos ni sus patrullas tienen lógica todavía.

## El objeto "--- Plan (ocultable) ---"

Todo lo que es planificación está bajo ese objeto, sin colliders ni lógica. Desactívalo para ver el nivel limpio.

| Marca | Significa |
|---|---|
| Disco gris claro + texto | Enemigo tímido/estático, con su arma y su sentido |
| Disco cian + postes y líneas | Enemigo que patrulla y su ruta |
| Disco rojo | Tu melé por detrás (Q) |
| Disco naranja + línea | Tu disparo a distancia (clic izq., 1 tinta) desde el disco hasta el enemigo |
| Bote morado | Munición de tinta +1 (máx. 3) |
| Caja amarilla | Cinta de memoria (coleccionable) |
| Placa azul + luz ON AIR | Salida al siguiente nivel |

## Niveles

| # | Zona del estudio | Mecánica nueva de Senses | Enemigos |
|---|---|---|---|
| 1 | **Pasillos del estudio**: recepción, pasillo de oficinas, animación, sala de guiones, camerinos | Agacharse (Stealth), guardias que ven, cobertura baja (sofás, mesas) y alta (estanterías) | 2 estáticos melé, 1 patrulla melé |
| 2 | **Plató B de noche**: muelle de carga, set del bosque encantado, bastidores | Luz y sombra (los focos del plató te delatan), guardia con visión nocturna | 1 estático distancia bajo los focos, 1 patrulla melé en bastidores, 1 estático distancia con visión nocturna |
| 3 | **Almacén de atrezo y set del jardín** | Arbustos de atrezo (zonas de sigilo), cámaras de seguridad (no se destruyen), acuario de atrezo (solo tapa a medias) | 1 patrulla distancia, 1 estático melé, 1 estático distancia |
| 4 | **Estudio de sonido**: pasillo de grabación, sala de foley, control, discoteca, estudio A | Guardias que oyen, madera x1,5 / alfombra x0,5, latas que hacen ruido | 2 estáticos melé (oído), 1 estático distancia, 1 patrulla distancia |
| 5 | **Backlot y archivo de cintas**: patio trasero, set del río, calle de fachadas, archivo | Perro que sigue tu rastro, el río lo corta, la basura lo enmascara, guardia con prismáticos | Perro (patrulla), 1 patrulla melé, 1 estático distancia (prismáticos), 1 estático melé (oído) |

## Notas de diseño

- Hay rutas alternativas pensadas para el sigilo:
  - en el P1, el pasillo detrás de los sofás de recepción;
  - en el P5, el callejón detrás de las fachadas, donde hay munición;
  - en el P4, las alfombras acústicas.
- Ideas pendientes para cuando existan las kills:
  - Si eliminas a un guardia a la vista de una cámara, ¿salta la alarma?
  - ¿Se puede eliminar al perro?
- El tinte rojizo venía de la niebla (FlatKitFog) del *Room-URP Asset* de la demo del Flat Kit, que era el pipeline del proyecto. Desde el 27 de noviembre el proyecto usa `Assets/LastFrame/Settings/Rendering/URP_LastFrame.asset`: es una copia sin esa niebla, con dos renderers (0 normal y 1 *Look*, con el contorno de tinta y Forward+). Se regenera con **Tools > Last Frame > Setup Rendering (URP Last Frame)**. Los sprites provisionales salen de **Tools > Last Frame > Build Placeholder Sprites** (`Art/Sprites/Placeholders/`).
- El río del P5 es una malla de 100 m y asoma por fuera de la valla del recinto.
