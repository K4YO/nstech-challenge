using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Esta migration sincroniza o modelo com o banco de dados
            // Normalmente, EF Core geraria as mudanças aqui, mas como já temos
            // as tabelas criadas, esta migration funciona apenas para sincronizar
            // o histórico de migrations
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
