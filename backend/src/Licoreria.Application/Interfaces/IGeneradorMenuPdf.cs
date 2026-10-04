using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Puerto de generación del menú en PDF. La implementación vive en Infrastructure.
/// </summary>
public interface IGeneradorMenuPdf
{
    byte[] Generar(MenuDigitalDto menu);
}
