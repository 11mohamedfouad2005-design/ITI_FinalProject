using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMaximumCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumCapacity",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumCapacity",
                table: "Courses");
        }
    }
}
