using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReportLogEntityFrameworkCore.Migrations
{
    public partial class Initial_Create : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMainLogData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Channel = table.Column<string>(nullable: true),
                    Severity = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMainLogData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLogActivityData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogId = table.Column<Guid>(nullable: false),
                    ActivityId = table.Column<string>(nullable: true),
                    RootActivityId = table.Column<string>(nullable: true),
                    ParentActivityId = table.Column<string>(nullable: true),
                    ActivityName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogActivityData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLogActivityData_AppMainLogData_LogId",
                        column: x => x.LogId,
                        principalTable: "AppMainLogData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogActivityData_LogId",
                table: "AppLogActivityData",
                column: "LogId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLogActivityData");

            migrationBuilder.DropTable(
                name: "AppMainLogData");
        }
    }
}
