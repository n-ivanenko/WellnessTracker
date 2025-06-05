using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessTracker.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserGoalTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkoutGoalMinutes",
                table: "UserGoals",
                newName: "WorkoutGoal");

            migrationBuilder.RenameColumn(
                name: "SleepGoalHours",
                table: "UserGoals",
                newName: "SleepGoal");

            migrationBuilder.RenameColumn(
                name: "DailyCalorieGoal",
                table: "UserGoals",
                newName: "CalorieGoal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkoutGoal",
                table: "UserGoals",
                newName: "WorkoutGoalMinutes");

            migrationBuilder.RenameColumn(
                name: "SleepGoal",
                table: "UserGoals",
                newName: "SleepGoalHours");

            migrationBuilder.RenameColumn(
                name: "CalorieGoal",
                table: "UserGoals",
                newName: "DailyCalorieGoal");
        }
    }
}
