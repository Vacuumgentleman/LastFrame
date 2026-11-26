# Prototypes: blocking de niveles

Cinco niveles de blocking ambientados en un **estudio antiguo de caricaturas y televisión**. Cada uno es una zona del estudio: salas, pasillos y platós con mobiliario y decorados hechos con primitivas. Cada nivel añade mecánicas del Senses Pack encima de las anteriores. Los guardias, cámaras, el perro, la hierba y el río son los objetos de Senses de la test room, y funcionan en Play Mode con el jugador de test.

Se regeneran desde **Tools > Last Frame > Build Prototype Blocking** (sobrescribe las 5 escenas). Script: `Scripts/Editor/PrototypeBlockingBuilder.cs`.

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
- En la escena de noche todo se ve rojizo por un ajuste global del proyecto. Pasa igual sin el volumen de postprocesado, así que no viene de esta escena.
- El río del P5 es una malla de 100 m y asoma por fuera de la valla del recinto.
