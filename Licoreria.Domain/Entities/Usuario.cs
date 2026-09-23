using System;
using System.Collections.Generic;
using System.Text;

namespace Licoreria.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cajero"; // Administrador, Cajero
    public bool Activo { get; set; } = true;
}
