using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessTracker.Migrations
{
    /// <inheritdoc />
    public partial class NullUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WorkoutEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SleepLogEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MoodEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "HabitEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "HabitCompletions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CalorieLogEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WorkoutEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SleepLogEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MoodEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HabitEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HabitCompletions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CalorieLogEntries");
        }
    }
}
