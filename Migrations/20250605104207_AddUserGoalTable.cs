using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGoalTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyCalorieGoal = table.Column<double>(type: "float", nullable: true),
                    SleepGoalHours = table.Column<double>(type: "float", nullable: true),
                    WorkoutGoalMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGoals");
        }
    }
}
