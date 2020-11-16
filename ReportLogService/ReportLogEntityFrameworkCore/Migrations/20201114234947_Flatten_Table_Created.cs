using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReportLogEntityFrameworkCore.Migrations
{
    public partial class Flatten_Table_Created : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppFlattenLogData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 20, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 20, nullable: true),
                    MessageId = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Channel = table.Column<string>(maxLength: 300, nullable: true),
                    Severity = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 75, nullable: true),
                    Message = table.Column<string>(maxLength: 300, nullable: true),
                    ActivityId = table.Column<string>(maxLength: 60, nullable: true),
                    RootActivityId = table.Column<string>(maxLength: 60, nullable: true),
                    ParentActivityId = table.Column<string>(maxLength: 60, nullable: true),
                    ActivityName = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFlattenLogData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppFlattenLogData");
        }
    }
}
