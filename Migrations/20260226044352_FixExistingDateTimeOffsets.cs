using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TupiJua.Migrations
{
    /// <inheritdoc />
    public partial class FixExistingDateTimeOffsets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrige registros existentes que foram migrados de datetime para datetimeoffset
            // com offset +00:00. SWITCHOFFSET preserva o instante UTC, apenas muda a representação
            // para o fuso horário de São Paulo (GMT-3).
            migrationBuilder.Sql(
                "UPDATE WorkoutSessions SET Date = SWITCHOFFSET(Date, '-03:00');");

            migrationBuilder.Sql(
                "UPDATE WorkoutPlans SET CreatedAt = SWITCHOFFSET(CreatedAt, '-03:00');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte para UTC (+00:00)
            migrationBuilder.Sql(
                "UPDATE WorkoutSessions SET Date = SWITCHOFFSET(Date, '+00:00');");

            migrationBuilder.Sql(
                "UPDATE WorkoutPlans SET CreatedAt = SWITCHOFFSET(CreatedAt, '+00:00');");
        }
    }
}
