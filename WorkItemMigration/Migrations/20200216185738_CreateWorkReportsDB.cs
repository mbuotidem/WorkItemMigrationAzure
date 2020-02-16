using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkItemMigration.Migrations
{
    public partial class CreateWorkReportsDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Rev = table.Column<int>(nullable: true),
                    Fields = table.Column<string>(nullable: false),
                    Relations = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    DWId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItem", x => x.DWId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItem");
        }
    }
}
