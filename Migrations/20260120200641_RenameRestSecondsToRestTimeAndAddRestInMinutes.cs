using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TupiJua.Migrations
{
    /// <inheritdoc />
    public partial class RenameRestSecondsToRestTimeAndAddRestInMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecommendedRestSeconds",
                table: "WorkoutPlanExercise",
                newName: "RecommendedRestTime");

            migrationBuilder.RenameColumn(
                name: "RestSeconds",
                table: "LoggedExercises",
                newName: "RestTime");

            migrationBuilder.AddColumn<bool>(
                name: "RestInMinutes",
                table: "WorkoutPlanExercise",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RestInMinutes",
                table: "LoggedExercises",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestInMinutes",
                table: "WorkoutPlanExercise");

            migrationBuilder.DropColumn(
                name: "RestInMinutes",
                table: "LoggedExercises");

            migrationBuilder.RenameColumn(
                name: "RecommendedRestTime",
                table: "WorkoutPlanExercise",
                newName: "RecommendedRestSeconds");

            migrationBuilder.RenameColumn(
                name: "RestTime",
                table: "LoggedExercises",
                newName: "RestSeconds");
        }
    }
}
