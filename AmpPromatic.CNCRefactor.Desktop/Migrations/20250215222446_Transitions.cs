using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmpPromatic.CNCRefactor.Desktop.Migrations
{
    /// <inheritdoc />
    public partial class Transitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transitions",
                columns: table => new
                {
                    TransitionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldText = table.Column<string>(type: "TEXT", nullable: false),
                    NewText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transitions", x => x.TransitionId);
                    table.ForeignKey(
                        name: "FK_Transitions_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transitions_MachineId",
                table: "Transitions",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transitions");
        }
    }
}
