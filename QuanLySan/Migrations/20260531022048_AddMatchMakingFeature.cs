using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchMakingFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SportTypeId = table.Column<int>(type: "int", nullable: false),
                    CourtType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourtId = table.Column<int>(type: "int", nullable: true),
                    PlayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    NeededPlayers = table.Column<int>(type: "int", nullable: false),
                    SkillLevel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreferredGender = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreferredAge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPosts_CourtTypes_SportTypeId",
                        column: x => x.SportTypeId,
                        principalTable: "CourtTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchPosts_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MatchApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ApplicantUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillLevel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreviousRating = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchApplications_MatchPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "MatchPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayingGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayingGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayingGroups_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlayingGroups_MatchPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "MatchPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayingGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RatingGiven = table.Column<bool>(type: "bit", nullable: false),
                    RatingReceived = table.Column<double>(type: "float", nullable: false),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayingGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayingGroupMembers_PlayingGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PlayingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchApplications_PostId",
                table: "MatchApplications",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPosts_CourtId",
                table: "MatchPosts",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPosts_SportTypeId",
                table: "MatchPosts",
                column: "SportTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayingGroupMembers_GroupId",
                table: "PlayingGroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayingGroups_BookingId",
                table: "PlayingGroups",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayingGroups_PostId",
                table: "PlayingGroups",
                column: "PostId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchApplications");

            migrationBuilder.DropTable(
                name: "PlayingGroupMembers");

            migrationBuilder.DropTable(
                name: "PlayingGroups");

            migrationBuilder.DropTable(
                name: "MatchPosts");
        }
    }
}
