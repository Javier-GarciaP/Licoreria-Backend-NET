namespace Licoreria.Application.Common;

/// <summary>
/// Parámetros de paginación comunes a los listados de la API.
/// </summary>
public class PaginacionRequest
{
    public const int TamanoMaximo = 100;
    public const int TamanoPorDefecto = 20;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = TamanoPorDefecto;

    public int PaginaNormalizada => Page < 1 ? 1 : Page;

    public int TamanoNormalizado
        => PageSize < 1 ? TamanoPorDefecto : (PageSize > TamanoMaximo ? TamanoMaximo : PageSize);
}
