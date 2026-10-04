using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coworking.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Rooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    opening_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closing_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.CheckConstraint("ck_rooms_capacity", "capacity > 0");
                    table.CheckConstraint("ck_rooms_opening_hours", "closing_time > opening_time");
                });

            migrationBuilder.InsertData(
                table: "rooms",
                columns: new[] { "id", "capacity", "closing_time", "is_active", "name", "normalized_name", "opening_time" },
                values: new object[,]
                {
                    { 1, 4, new TimeOnly(20, 0, 0), true, "Sala Alfa", "SALA ALFA", new TimeOnly(8, 0, 0) },
                    { 2, 6, new TimeOnly(20, 0, 0), true, "Sala Beta", "SALA BETA", new TimeOnly(8, 0, 0) },
                    { 3, 10, new TimeOnly(18, 0, 0), true, "Sala Gamma", "SALA GAMMA", new TimeOnly(9, 0, 0) },
                    { 4, 20, new TimeOnly(18, 0, 0), true, "Sala Delta", "SALA DELTA", new TimeOnly(9, 0, 0) },
                    { 5, 8, new TimeOnly(13, 0, 0), false, "Sala Epsilon", "SALA EPSILON", new TimeOnly(9, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_rooms_normalized_name",
                table: "rooms",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rooms");
        }
    }
}
