using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCPP.Core.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChargePoint",
                columns: table => new
                {
                    ChargePointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientCertThumb = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargePoint", x => x.ChargePointId);
                });

            migrationBuilder.CreateTable(
                name: "ChargeTags",
                columns: table => new
                {
                    TagId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TagName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentTagId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeKeys", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "ConnectorStatus",
                columns: table => new
                {
                    ChargePointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectorId = table.Column<int>(type: "integer", nullable: false),
                    ConnectorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastStatusTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMeter = table.Column<double>(type: "double precision", nullable: true),
                    LastMeterTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectorStatus", x => new { x.ChargePointId, x.ConnectorId });
                    table.ForeignKey(
                        name: "FK_ConnectorStatus_ChargePoint",
                        column: x => x.ChargePointId,
                        principalTable: "ChargePoint",
                        principalColumn: "ChargePointId");
                });

            migrationBuilder.CreateTable(
                name: "MessageLog",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChargePointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectorId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLog", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_MessageLog_ChargePoint_ChargePointId",
                        column: x => x.ChargePointId,
                        principalTable: "ChargePoint",
                        principalColumn: "ChargePointId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChargePointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectorId = table.Column<int>(type: "integer", nullable: false),
                    StartTagId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MeterStart = table.Column<double>(type: "double precision", nullable: false),
                    StartResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StopTagId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StopTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MeterStop = table.Column<double>(type: "double precision", nullable: true),
                    StopReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_ChargePoint",
                        column: x => x.ChargePointId,
                        principalTable: "ChargePoint",
                        principalColumn: "ChargePointId");
                });

            migrationBuilder.CreateIndex(
                name: "ChargePoint_Identifier",
                table: "ChargePoint",
                column: "ChargePointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_ChargePointId",
                table: "MessageLog",
                column: "ChargePointId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_ChargePointId1",
                table: "MessageLog",
                column: "LogTime");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ChargePointId",
                table: "Transactions",
                column: "ChargePointId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeTags");

            migrationBuilder.DropTable(
                name: "ConnectorStatus");

            migrationBuilder.DropTable(
                name: "MessageLog");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "ChargePoint");
        }
    }
}
