using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <summary>
    /// Habilita la concurrencia optimista mapeando la columna de sistema xmin de
    /// PostgreSQL. No genera cambios de esquema (xmin ya existe en cada tabla).
    /// </summary>
    public partial class ConcurrenciaXmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
