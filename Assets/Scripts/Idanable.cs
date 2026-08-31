/// <summary>
/// Cualquier objeto que pueda recibir daño del ataque base del jugador
/// (o de otras fuentes) debe implementar esta interfaz.
/// </summary>
public interface IDanable
{
    void RecibirDano(float cantidad);
}
 