using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;
using TOBem.TelemetriaApi.Models;

#nullable disable

namespace TOBem.TelemetriaApi.Migrations
{
    /// <summary>Cria as tabelas DISPOSITIVO e EVENTO (relação 1:N) no Oracle.</summary>
    [DbContext(typeof(Contexto))]
    [Migration("20261112120000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DISPOSITIVO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    IDOSO_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TIPO = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    COMODO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    ULTIMO_HEARTBEAT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DISPOSITIVO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENTO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    DISPOSITIVO_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TIPO = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    VALOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DATA_HORA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENTO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EVENTO_DISPOSITIVO_DISPOSITIVO_ID",
                        column: x => x.DISPOSITIVO_ID,
                        principalTable: "DISPOSITIVO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EVENTO_DISPOSITIVO_ID_DATA_HORA",
                table: "EVENTO",
                columns: new[] { "DISPOSITIVO_ID", "DATA_HORA" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EVENTO");

            migrationBuilder.DropTable(
                name: "DISPOSITIVO");
        }
    }
}
