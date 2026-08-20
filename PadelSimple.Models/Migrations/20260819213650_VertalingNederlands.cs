using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PadelSimple.Models.Migrations
{
    /// <inheritdoc />
    public partial class VertalingNederlands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Voornaam = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Achternaam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefoonnummer = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsLid = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsGeblokkeerd = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerwijderd = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerwijderdOp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materialen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AantalInInventaris = table.Column<int>(type: "INTEGER", nullable: false),
                    BeschikbaarAantal = table.Column<int>(type: "INTEGER", nullable: false),
                    Huurprijs = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerwijderd = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerwijderdOp = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materialen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Terreinen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Capaciteit = table.Column<int>(type: "INTEGER", nullable: false),
                    IsIndoors = table.Column<bool>(type: "INTEGER", nullable: false),
                    Uurtarief = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsVerwijderd = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerwijderdOp = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terreinen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservaties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GebruikerId = table.Column<string>(type: "TEXT", nullable: false),
                    TerreinId = table.Column<int>(type: "INTEGER", nullable: false),
                    MateriaalId = table.Column<int>(type: "INTEGER", nullable: true),
                    AantalMateriaal = table.Column<int>(type: "INTEGER", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartUur = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EindUur = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TotalePrijs = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AantalSpelers = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerwijderd = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerwijderdOp = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservaties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservaties_AspNetUsers_GebruikerId",
                        column: x => x.GebruikerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservaties_Materialen_MateriaalId",
                        column: x => x.MateriaalId,
                        principalTable: "Materialen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservaties_Terreinen_TerreinId",
                        column: x => x.TerreinId,
                        principalTable: "Terreinen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservatieMaterialen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservatieId = table.Column<int>(type: "INTEGER", nullable: false),
                    MateriaalId = table.Column<int>(type: "INTEGER", nullable: false),
                    Aantal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservatieMaterialen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservatieMaterialen_Materialen_MateriaalId",
                        column: x => x.MateriaalId,
                        principalTable: "Materialen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservatieMaterialen_Reservaties_ReservatieId",
                        column: x => x.ReservatieId,
                        principalTable: "Reservaties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "ROLE_ADMIN", "a1b2c3d4-0000-0000-0000-000000000001", "Admin", "ADMIN" },
                    { "ROLE_KLANT", "a1b2c3d4-0000-0000-0000-000000000002", "Klant", "KLANT" },
                    { "ROLE_MEDEWERKER", "a1b2c3d4-0000-0000-0000-000000000003", "Medewerker", "MEDEWERKER" }
                });

            migrationBuilder.InsertData(
                table: "Materialen",
                columns: new[] { "Id", "AantalInInventaris", "BeschikbaarAantal", "Huurprijs", "IsActief", "IsVerwijderd", "Naam", "VerwijderdOp" },
                values: new object[,]
                {
                    { 1, 20, 0, 5.00m, true, false, "Padelracket", null },
                    { 2, 30, 0, 2.50m, true, false, "Set Ballen", null },
                    { 3, 15, 0, 1.50m, true, false, "Beschermingsbril", null }
                });

            migrationBuilder.InsertData(
                table: "Terreinen",
                columns: new[] { "Id", "Capaciteit", "IsIndoors", "IsVerwijderd", "Naam", "Uurtarief", "VerwijderdOp" },
                values: new object[,]
                {
                    { 1, 4, true, false, "Terrein 1 (Overdekt)", 18.00m, null },
                    { 2, 4, false, false, "Terrein 2 (Buiten)", 12.00m, null },
                    { 3, 4, true, false, "Terrein 3 (Overdekt VIP)", 25.00m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservatieMaterialen_MateriaalId",
                table: "ReservatieMaterialen",
                column: "MateriaalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservatieMaterialen_ReservatieId",
                table: "ReservatieMaterialen",
                column: "ReservatieId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaties_GebruikerId",
                table: "Reservaties",
                column: "GebruikerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaties_MateriaalId",
                table: "Reservaties",
                column: "MateriaalId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaties_TerreinId",
                table: "Reservaties",
                column: "TerreinId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ReservatieMaterialen");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Reservaties");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Materialen");

            migrationBuilder.DropTable(
                name: "Terreinen");
        }
    }
}
