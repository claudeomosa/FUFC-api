using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUFC.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    IsPpv = table.Column<bool>(type: "boolean", nullable: false),
                    Venue = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gyms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HeadCoach = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsGoodFor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gyms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Referees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fighters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NickName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WeightClass = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PredominantStyle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Champion = table.Column<bool>(type: "boolean", nullable: false),
                    InterimChampion = table.Column<bool>(type: "boolean", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    HomeCity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    IsRanked = table.Column<bool>(type: "boolean", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Reach = table.Column<double>(type: "double precision", nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Stance = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    GymId = table.Column<string>(type: "text", nullable: true),
                    FighterImagePath = table.Column<string>(type: "text", nullable: false),
                    Record = table.Column<string>(type: "jsonb", nullable: false),
                    SkillStats = table.Column<string>(type: "jsonb", nullable: true),
                    SocialMedia = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fighters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fighters_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<string>(type: "text", nullable: false),
                    RedCornerId = table.Column<string>(type: "text", nullable: false),
                    BlueCornerId = table.Column<string>(type: "text", nullable: false),
                    IsForTitle = table.Column<bool>(type: "boolean", nullable: false),
                    IsMainEvent = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrelim = table.Column<bool>(type: "boolean", nullable: false),
                    IsInMainCard = table.Column<bool>(type: "boolean", nullable: false),
                    RefereeId = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bouts_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bouts_Fighters_BlueCornerId",
                        column: x => x.BlueCornerId,
                        principalTable: "Fighters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bouts_Fighters_RedCornerId",
                        column: x => x.RedCornerId,
                        principalTable: "Fighters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bouts_Referees_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Referees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bouts_BlueCornerId",
                table: "Bouts",
                column: "BlueCornerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bouts_EventId",
                table: "Bouts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Bouts_RedCornerId",
                table: "Bouts",
                column: "RedCornerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bouts_RefereeId",
                table: "Bouts",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_Fighters_GymId",
                table: "Fighters",
                column: "GymId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bouts");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Fighters");

            migrationBuilder.DropTable(
                name: "Referees");

            migrationBuilder.DropTable(
                name: "Gyms");
        }
    }
}
