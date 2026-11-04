# Guía de montaje: progresión de los niveles 2 y 3 + créditos finales

## Resumen del flujo

| Nivel | Paso 1 | Paso 2 | Final |
|---|---|---|---|
| **2: Residencial** | Vencer 5 enemigos → aparece la **regadera** | Tomarla (habilidad Regar) → limpiar **10 charcos** | Se restaura el nivel → cuenta regresiva → Hub (`DesdeNivel2`) |
| **3: Plaza** | Al entrar se entrega la habilidad **Basura** (panel automático) | Recoger **10 basuras** | Se restaura → Hub (`DesdeNivel3`) → **créditos automáticos** |

## Scripts nuevos o modificados

| Script | Qué cambió |
|---|---|
| `Progresion/ObjetivoCharcosLimpiados.cs` (nuevo) | Contador "Charcos limpiados: X/10". Escucha `ZonaPeligrosa.AlLimpiar`. Si la lista está vacía, usa todos los charcos de la escena. |
| `Progresion/ObjetivoBasuraRecolectada.cs` (nuevo) | Contador "Basura recolectada: X/10". Modo **AlRecoger** (al levantarla) o **AlDepositar** (al tirarla en el bote). |
| `Progresion/CreditosFinales.cs` (nuevo) | Va en el Hub. Al volver del nivel 3 abre el `PanelCreditos` una sola vez. |
| `RecompensaHabilidad.cs` | Nuevas opciones: **Entregar Al Iniciar** (nivel 3), evento **AlEntregar** y no repetir el panel si ya tenías la habilidad. |
| `MetaNivel.cs` | Nueva casilla **Completar Al Tocar** (desmárcala en los niveles 2 y 3) y casillas para ocultar charcos/basura al completar. Ya no exige un Collider. |
| `BoteBasura.cs` | Nuevo evento estático `AlDepositarEnCualquierBote`, usado por el modo AlDepositar. |

> En los niveles 2 y 3, `MetaNivel` **no es un trigger**. Va en un objeto vacío **siempre activo** y solo se completa cuando el objetivo final llama a `CompletarNivel()`.

---

## Nivel 2: Residencial

### Jerarquía sugerida
```
Nivel 2
├── Progresion            (objeto vacío, SIEMPRE activo)
│     ├─ ObjetivoEnemigosVencidos
│     ├─ ObjetivoCharcosLimpiados
│     └─ MetaNivel
├── Recompensa_Agua       (activo en la escena; el objetivo de enemigos lo oculta al iniciar)
│     ├─ RecompensaHabilidad + Collider (Is Trigger)
│     └─ Modelo (regadera)
├── Charcos               (10+ charcos, cada uno con ZonaPeligrosa)
├── Residencial_Deteriorado
├── Residencial_Restaurado (desactivado)
└── Canvas
      ├─ TextoEnemigos
      ├─ TextoCharcos
      ├─ MensajeMeta
      └─ TextoCuentaRegresiva
```

### Paso 1: Charcos (`ZonaPeligrosa`)
Cada charco (prefab `Charco_Aceite` o `Charco_Veneno`):
1. Agrega un **Collider con Is Trigger** que haga daño al pisarlo y el componente **`ZonaPeligrosa`**.
2. **Visual Peligro**: arrastra el modelo del charco (se apaga al limpiarlo).
3. **Colisionador Peligro**: arrastra el mismo collider.
4. El agua de `HabilidadRegar` detecta colliders con una esfera: revisa que la **Mascara Capa Salpicadura** del jugador incluya la capa del charco.
5. Coloca **al menos 10**. Si pones más, igual se completa con 10.

### Paso 2: `ObjetivoEnemigosVencidos` (en `Progresion`)
| Campo | Valor |
|---|---|
| **Id Objetivo** | `Nivel2_Enemigos` ⚠️ **cámbialo**: si se queda en `Hub_Portal1`, el objetivo sale como ya completado |
| Enemigos Requeridos | 5 |
| Enemigos Objetivo | vacío (usa todos los de la escena) |
| **Objetos A Desbloquear** | `Recompensa_Agua` |
| Ocultar Enemigos Si Ya Completado | ✔ |
| Texto Contador | `TextoEnemigos` |
| Texto Completado | `¡Apareció la regadera!` |

### Paso 3: `Recompensa_Agua` (`RecompensaHabilidad`)
| Campo | Valor |
|---|---|
| Indice Habilidad | **1** (Regar) |
| Entregar Al Iniciar | ☐ |
| Panel Desbloqueo | el `PanelHabilidadDesbloqueada` del Canvas |
| **AlEntregar** (+) | ① `Progresion` → `ObjetivoCharcosLimpiados.Activar` · ② `TextoEnemigos` → `GameObject.SetActive` (☐ false) |

### Paso 4: `ObjetivoCharcosLimpiados` (en `Progresion`)
| Campo | Valor |
|---|---|
| Charcos Requeridos | 10 |
| Charcos Objetivo | vacío (todos los de la escena) |
| Activar Al Iniciar | ☐ (lo activa la regadera) |
| Id Nivel Omitir Si Completado | `Nivel2_Completado` |
| Texto Contador | `TextoCharcos` |
| **AlCompletarObjetivo** (+) | `Progresion` → `MetaNivel.CompletarNivel` |

### Paso 5: `MetaNivel` (en `Progresion`)
| Campo | Valor |
|---|---|
| Id Nivel Completado | `Nivel2_Completado` |
| **Completar Al Tocar** | ☐ |
| Esperar Objetivo Extra | ☐ (no aplica) |
| Objetos Deteriorados | `Residencial_Deteriorado` |
| Objetos Restaurados | `Residencial_Restaurado` |
| Ocultar Charcos Al Completar | ✔ |
| Indice Habilidad | **-1** (la regadera ya la dio `RecompensaHabilidad`) |
| Mensaje Meta / Texto Cuenta Regresiva | los objetos del Canvas |
| Nombre Escena Hub | el mismo que en el nivel 1 (`ParqueHub`) |
| Id Punto Aparicion Hub | `DesdeNivel2` (debe existir un `PuntoAparicion` con ese Id en el Hub) |

---

## Nivel 3: Plaza

### Jerarquía sugerida
```
Nivel 3
├── Progresion            (objeto vacío, SIEMPRE activo)
│     ├─ ObjetivoBasuraRecolectada
│     ├─ MetaNivel
│     └─ RecompensaBasura (hijo: RecompensaHabilidad + BoxCollider Is Trigger, lo pide el script; puede quedar lejos)
├── Basura                (10+ objetos con ObjetoBasura + Collider Is Trigger)
├── Plaza_Deteriorada
├── Plaza_Restaurada (desactivada)
└── Canvas (TextoBasura, MensajeMeta, TextoCuentaRegresiva, PanelHabilidadDesbloqueada)
```

> ⚠️ `RecompensaHabilidad` se **desactiva a sí misma** al entregar la habilidad. Ponla en un **objeto hijo propio** (ej. `Progresion/RecompensaBasura`), no en `Progresion`, porque ahí también están el objetivo y la meta.

### Paso 1: `RecompensaBasura` (`RecompensaHabilidad`)
| Campo | Valor |
|---|---|
| Indice Habilidad | **2** (Basura) |
| **Entregar Al Iniciar** | ✔ |
| Retraso Al Iniciar | 1 |
| Panel Desbloqueo | el `PanelHabilidadDesbloqueada` |

La habilidad se equipa sola (`DesbloquearHabilidad` la equipa).

### Paso 2: `ObjetivoBasuraRecolectada`
| Campo | Valor |
|---|---|
| Basuras Requeridas | 10 |
| **Modo Conteo** | `AlRecoger` (cuenta al levantarla) o `AlDepositar` (cuenta al tirarla en el bote) |
| Activar Al Iniciar | ✔ |
| Id Nivel Omitir Si Completado | `Nivel3_Completado` |
| Texto Contador | `TextoBasura` |
| **AlCompletarObjetivo** (+) | `MetaNivel.CompletarNivel` |

> El jugador carga **5 basuras como máximo**. Con `AlRecoger` tiene que vaciar el inventario en el bote (tecla de la habilidad) para seguir recogiendo. Con `AlDepositar`, la basura cuenta hasta que la tira en el bote.

### Paso 3: `MetaNivel`
Igual que en el nivel 2, con estos cambios: **Id** `Nivel3_Completado`, **Completar Al Tocar** ☐, objetos `Plaza_Deteriorada` / `Plaza_Restaurada`, **Ocultar Basura Al Completar** ✔, Indice Habilidad **-1** e **Id Punto Aparicion Hub** `DesdeNivel3`.

---

## Créditos finales (Hub)

1. En el Canvas del Hub ya debe estar el **`PanelCreditos`** (panel, contenido con scroll y botones *Continuar* → `Cerrar()` y *Menú principal* → `SalirAMenuPrincipal()`).
   - Revisa que **Nombre Escena Menu Principal** sea el nombre real de la escena del menú (`MenuPrincipal`; por defecto dice `MenuInicio`).
2. En el objeto `Progresion` del Hub (siempre activo), agrega **`CreditosFinales`**:
   - **Panel Creditos**: arrastra el `PanelCreditos`.
   - Ids Requeridos: `Nivel3_Completado` (ya viene así).
   - Retraso: 1.5 s.
3. Opcional: si quieres que el jugador pueda volver a verlos, deja la `ZonaCreditos` con un `ActivadorPorProgreso` (`Nivel3_Completado`).

Flujo final: al terminar el nivel 3 y regresar al Hub, los créditos se abren solos (el juego se pausa) → **Continuar** → el jugador sigue explorando. No vuelven a salir solos en esa partida; se reinician al empezar una partida nueva desde el menú.

---

## Checklist de prueba
- [ ] Nivel 2: el contador de enemigos sube y al llegar a 5 aparece la regadera.
- [ ] Al tomarla: panel de Regar, se oculta el contador de enemigos y aparece "Charcos limpiados: 0/10".
- [ ] Cada charco regado suma 1; al llegar a 10 se restaura el nivel y empieza la cuenta regresiva.
- [ ] Regreso al Hub en `DesdeNivel2` y se abre el camino al portal 3.
- [ ] Nivel 3: al entrar aparece el panel de Basura y la habilidad queda equipada.
- [ ] El contador de basura sube; al llegar a 10 → restauración → Hub en `DesdeNivel3`.
- [ ] En el Hub los créditos se abren solos; con **Continuar** el cursor se bloquea y se puede mover.

## Problemas comunes
| Síntoma | Causa |
|---|---|
| La regadera aparece desde el inicio | El `Id Objetivo` sigue en `Hub_Portal1`, que ya está completado desde el Hub. |
| Los charcos no cuentan | La ZonaPeligrosa no tiene un collider que alcance la salpicadura (máscara de capas), o el objetivo no se activó (revisa `AlEntregar`). |
| Warning "solo hay N charcos/basuras" | Hay menos de 10 en la escena, o están desactivados al momento de activar el objetivo. |
| El nivel 3 no entrega la habilidad | Se probó la escena sin pasar por el Hub (no existe el jugador persistente). |
| El contador de basura no sube | `Modo Conteo` está en AlDepositar y aún no se ha tirado la basura en el bote. |
| Los créditos no salen | Falta `CreditosFinales` en el Hub o su `Panel Creditos` está vacío. |
| Al repetir un nivel ya completado no hay objetivo | Es intencional: el nivel se ve restaurado. Deja un portal de regreso al Hub en cada nivel para salir. |
