# Guía de integración de audios — RinovyPark

Todo el código ya está listo. Solo hay que **arrastrar los clips** a los campos de sonido en el Inspector. Si un campo se deja vacío, ese sonido simplemente no suena (no da error), así que puedes ir llenándolos poco a poco.

---

## 0. Cómo funciona un campo "Sonido"

Casi todos los campos de sonido son de tipo **SonidoAleatorio** y se ven así en el Inspector:

| Campo | Qué es |
|---|---|
| **Clips** | Lista de clips. Pon 1 o varios: si hay varios, se elige uno al azar sin repetir el anterior. Ideal para pasos, golpes y cortes. |
| **Volumen** | 0–1. |
| **Rango Tono** | Mínimo y máximo del pitch (1 = normal). Por defecto 0.95–1.05 para que varíe un poco. Para jingles y música de UI pon 1–1. |

Todos los sonidos salen automáticamente por el grupo **Efectos** del `MixerPrincipal`, así que respetan el slider de volumen de efectos.

---

## 1. Configurar GestorAudio (una sola vez)

1. Abre la escena del **Menú de Inicio** y selecciona el objeto que tiene `GestorAudio`.
2. En el Inspector, en **Grupos del mixer**:
   - **Grupo Musica** → despliega `MixerPrincipal` en el Project y arrastra el grupo **Musica**.
   - **Grupo Efectos** → arrastra el grupo **Efectos**.
3. Si antes tenías un `AudioSource` en la escena reproduciendo la música (por ejemplo, la de Piki), **elimínalo**: ahora la música la pone `MusicaEscena` (paso 2). Si lo dejas, sonarán dos músicas a la vez.

---

## 2. Música y ambiente por escena — `MusicaEscena`

En **cada escena** (MenuInicio, Hub, Nivel1, Nivel2, Nivel3):

1. Crea un objeto vacío llamado `Musica` → **Add Component → MusicaEscena**.
2. **Musica**: arrastra la pista de esa escena. **Volumen Musica**: 0.5–0.7.
3. **Ambiente** (opcional): pájaros, viento, ciudad. **Volumen Ambiente**: 0.3–0.5.
4. **Duracion Fundido**: 1.5 s (transición suave entre escenas).

> Si dos escenas usan el mismo clip (por ejemplo, Hub y un nivel), la música **no se reinicia** al cambiar de escena.
> Si pruebas un nivel directamente sin pasar por el menú, la música suena igual (sin mixer), para que puedas probar.

**Import Settings recomendados (WebGL):** música y ambiente → *Load Type: Streaming*, *Compression: Vorbis*, *Quality ~50–70*. Efectos cortos → *Decompress On Load*, *Force To Mono* activado.

---

## 3. Jugador (prefab del jugador)

### 3.1 `SonidosJugador` (nuevo)
Abre el prefab del jugador → raíz → **Add Component → SonidosJugador**.

| Campo | Sonido sugerido |
|---|---|
| Sonido Pasos | 4–6 pasos sobre pasto/concreto (Kenney `footstep_grass_00X`) |
| Distancia Entre Pasos | 1.6 (súbelo si los pasos van muy rápido) |
| Sonido Salto | impulso/“whoosh” corto |
| Sonido Aterrizaje | golpe suave en pasto |
| Sonido Dano | quejido / impacto |
| Sonido Muerte | caída / quejido largo |

No hace falta enganchar nada: se suscribe solo a `PlayerHealth` y lee el movimiento del `CharacterController`.

### 3.2 `CombateJugador` (ya existente, mismo prefab)
- **Sonido Ataque**: “swoosh” del golpe (suena siempre).
- **Sonido Impacto**: golpe que conecta (solo si le pegó a un enemigo).

### 3.3 `GestorHabilidades` (mismo prefab)
- **Sonido Cambiar Habilidad**: clic/“select” corto al presionar Q/E.

### 3.4 Habilidades (en los objetos donde están los componentes de habilidad)
| Script | Campo | Sonido sugerido |
|---|---|---|
| `HabilidadRegar` | Sonido Carga *(AudioClip en bucle)* | agua burbujeando / chorro continuo |
| | Volumen / Tono Inicial / Tono Final | 0.6 / 0.8 / 1.4 (el tono sube mientras carga) |
| | Sonido Salpicadura | splash grande |
| `HabilidadRecolectarBasura` | Sonido Aparecer Bote | “pop” / golpe metálico |
| | Sonido Recolectar | bolsa de plástico / pickup |
| | Sonido Inventario Lleno | “error” / buzz corto |

---

## 4. Objetos del mundo

| Objeto (prefab) | Script | Campo | Sonido sugerido |
|---|---|---|---|
| Maleza | `Maleza` | Sonido Corte | 3–4 tijeretazos / arbusto |
| Plantas regables | `Regable` | Sonido Al Regar | brote / crecimiento mágico |
| Zona peligrosa | `ZonaPeligrosa` | Sonido Al Limpiar | fuego apagándose (“tssss”) |
| Bote de basura (prefab del Project) | `BoteBasura` | Sonido Depositar | basura cayendo en el bote |
| Portales | `PortalDeNivel` | Sonido Entrar | teletransporte / “whoosh” mágico |

> ⚠️ **Maleza**: el campo antiguo *Sonido Corte* (un solo AudioClip) se convirtió en la lista nueva. Si ya le habías puesto un clip, vuelve a asignarlo.

### Sonidos en bucle pegados a un objeto (fuego, fuente, aleteo, motor...)
1. En el objeto: **Add Component → Audio Source** → arrastra el clip, activa **Loop** y **Play On Awake**, **Spatial Blend = 1 (3D)**, *Max Distance* ~15–25.
2. **Add Component → FuenteSonidoBucle**.

`FuenteSonidoBucle` lo conecta al mixer, lo pausa con el juego, desfasa el inicio (para que varios enemigos no suenen sincronizados) y aplica el volumen en WebGL. Si el objeto se desactiva (zona limpiada, enemigo oculto), el sonido se apaga solo.

- **Zona peligrosa**: pon el AudioSource en el hijo **Visual Peligro**, así se apaga al regarla.

---

## 5. Enemigos (prefabs terrestre y Murciélago)

### 5.1 `VidaEnemigo`
- **Sonido Dano**: golpe / chillido corto.
- **Sonido Muerte**: “poof” / chillido final. Sigue sonando aunque el enemigo se destruya.

### 5.2 `EnemigoTerrestre` / `EnemigoVolador` (sección **Sonidos (opcional)**)
| Campo | Cuándo suena | Sugerencia |
|---|---|---|
| Sonido Alerta | descubre al jugador | gruñido / chillido de murciélago |
| Sonido Preparacion | aviso antes de atacar | gruñido cargando / chirrido |
| Sonido Embestida | lanza el ataque / picada | “whoosh” rápido |

### 5.3 Bucle de movimiento
- **Murciélago**: AudioSource con aleteo en bucle + `FuenteSonidoBucle` (paso 4).
- **Terrestre**: pasos/respiración en bucle + `FuenteSonidoBucle` (volumen bajo, ~0.3).

---

## 6. Progresión y objetivos

| Objeto | Script | Campos |
|---|---|---|
| Meta de cada nivel | `MetaNivel` | **Sonido Llegada** (campanita), **Sonido Completar** (jingle de victoria), **Sonido Cuenta Regresiva** (tic, suena cada segundo), **Detener Musica Al Completar** ✔ |
| Objetivo de malezas (nivel 1) | `ObjetivoMalezasCortadas` | **Sonido Conteo** (pequeño “ding”), **Sonido Completado** (jingle) |
| Objetivo de enemigos (Hub) | `ObjetivoEnemigosVencidos` | **Sonido Conteo**, **Sonido Completado** (se abre el portal) |
| Panel de tutorial | `PanelTutorial` | **Sonido Aviso** (aparece “Presiona F”), **Sonido Abrir** (papel), **Sonido Cerrar** |
| Panel de habilidad nueva | `PanelHabilidadDesbloqueada` | **Sonido Mostrar** (fanfarria corta, Kenney *Music Jingles*) |

### Comodín: `SonidoEnEvento`
Para poner un sonido en **cualquier** evento sin código (por ejemplo, `AlLlegarMeta`, `AlCompletarNivel`, `Maleza.AlCortar` de una maleza especial, el `OnClick` de un botón concreto):
1. **Add Component → SonidoEnEvento** en cualquier objeto y asigna el sonido. Marca **Espacial** si debe oírse desde esa posición.
2. En el evento, pulsa **+**, arrastra ese objeto y elige **SonidoEnEvento → Reproducir()**.

---

## 7. Menús e interfaz

### 7.1 Botones — `SonidosBotones`
En la **raíz de cada Canvas con botones** (Menú principal, Ajustes, Pausa, Game Over, Créditos, Panel de habilidad, Tutorial):
- **Add Component → SonidosBotones**.
- **Sonido Clic**: clic de UI (Kenney *Interface Sounds* `click_00X`).
- **Sonido Resaltar** (opcional): “tick” suave al pasar el mouse.

Se agrega solo a todos los Button y Toggle del Canvas, incluidos los de paneles ocultos. Si quieres que un botón concreto suene distinto, ponle a mano un **SonidoBoton** con sus propios clips: no se sobrescribe.

### 7.2 Paneles
| Script | Campos |
|---|---|
| `MenuPausa` | **Sonido Abrir**, **Sonido Cerrar** |
| `PantallaGameOver` | **Sonido Game Over** (jingle de derrota), **Detener Musica** ✔ (vuelve sola al reiniciar) |
| `PanelCreditos` | **Musica Creditos** (AudioClip) + volumen. Al cerrar vuelve la música que sonaba antes. |

---

## 8. Detalles técnicos (para la tesis)

- **`ReproductorAudio`** (clase estática): punto único para reproducir efectos. Usa un *pool* de AudioSources (6 planos + 16 en 3D) que vive entre escenas (`DontDestroyOnLoad`), así un sonido no se corta si su objeto se destruye (enemigo muerto, maleza cortada) o si cambia la escena (portal). No reserva memoria en cada sonido, lo que importa en WebGL.
- **Margen al cargar la escena**: los sonidos 3D se ignoran durante los primeros 0.3 s de cada escena. Así no suenan cuando `EstadoPersistente` restaura las plantas regadas y las zonas limpiadas al volver al Hub.
- **`SonidoAleatorio`**: clase serializable con lista de clips, volumen y rango de tono. Evita repetir el mismo clip dos veces seguidas.
- **`GestorAudio`** ahora también maneja dos canales en bucle (música y ambiente) con fundido usando `Time.unscaledDeltaTime`, para que funcione en pausa.
- **WebGL**: el AudioMixer de Unity tiene soporte limitado en Web y puede no aplicar el volumen. Por eso, solo en builds WebGL, el volumen de los sliders también se aplica por código a todo el audio creado por estos scripts y a `FuenteSonidoBucle`. En PC y en el editor no cambia nada. **Prueba los sliders en el build de itch.io.**
- **WebGL y autoplay**: los navegadores no reproducen audio hasta el primer clic o tecla del jugador. La música del menú empezará después del primer clic; esto es normal.

---

## 9. Checklist de prueba

- [ ] `GestorAudio` tiene asignados Grupo Musica y Grupo Efectos.
- [ ] Cada escena tiene su `MusicaEscena`, y la música cambia con fundido entre Menú → Hub → Nivel.
- [ ] Los sliders de música y efectos del menú de ajustes cambian el volumen.
- [ ] Los pasos suenan al caminar, no suenan quieto ni en el aire; el salto y el aterrizaje suenan.
- [ ] El golpe suena siempre; el impacto solo al pegarle a un enemigo.
- [ ] El enemigo suena al detectarte, al prepararse y al embestir; el sonido de muerte no se corta.
- [ ] Al volver al Hub **no** suenan de golpe las plantas regadas ni las zonas limpiadas.
- [ ] La carga de agua sube de tono y se corta al soltar o al cambiar de habilidad.
- [ ] El aleteo del murciélago se pausa con el menú de pausa.
- [ ] Los botones suenan, también con el juego en pausa.
- [ ] Meta: llegada → jingle → tic de la cuenta regresiva → Hub con su música.
- [ ] Game Over detiene la música y vuelve al reiniciar.

## 10. Problemas comunes

| Síntoma | Causa / solución |
|---|---|
| No suena nada | Revisa que el campo tenga clips y Volumen > 0. Revisa que el `AudioListener` esté en la cámara (solo uno por escena). |
| Los sliders no afectan los sonidos | Falta asignar Grupo Efectos / Grupo Musica en `GestorAudio`, o los parámetros expuestos del mixer no se llaman `VolumenMusica` / `VolumenEfectos`. |
| Suenan dos músicas | Quedó un AudioSource viejo con la música en la escena. Elimínalo. |
| Los pasos suenan muy rápido o muy lento | Ajusta **Distancia Entre Pasos** en `SonidosJugador`. |
| El salto no suena | Baja **Velocidad Minima Salto** (por ejemplo, a 1). |
| Un sonido 3D casi no se oye | Súbele el Volumen, o pasa su mezcla a plano (por ejemplo, con `SonidoEnEvento` sin marcar Espacial). |
