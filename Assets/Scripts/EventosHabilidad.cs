using UnityEngine.Events;

/// <summary>
/// Eventos reutilizables (UnityEvent con parámetros) usados por el sistema
/// de habilidades. Se agrupan aquí para evitar declaraciones duplicadas
/// en distintos scripts.
/// </summary>
[System.Serializable] public class EventoEntero : UnityEvent<int> { }
[System.Serializable] public class EventoFlotante : UnityEvent<float> { }
[System.Serializable] public class EventoCambioHabilidad : UnityEvent<int, Habilidad> { }
