using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(SichiDbContext))]
[Migration("20260601000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Playlists",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Playlists", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "MediaItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MediaType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "Video")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DurationSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                Checksum = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, defaultValue: "")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FileSize = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediaItems", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Screens",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ScreenKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DeviceId = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CurrentPlaylistId = table.Column<int>(type: "int", nullable: true),
                IsOnline = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                LastPing = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Screens", x => x.Id);
                table.ForeignKey(
                    name: "FK_Screens_Playlists_CurrentPlaylistId",
                    column: x => x.CurrentPlaylistId,
                    principalTable: "Playlists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "PlaylistMedias",
            columns: table => new
            {
                PlaylistId = table.Column<int>(type: "int", nullable: false),
                MediaItemId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                OverrideDuration = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlaylistMedias", x => new { x.PlaylistId, x.MediaItemId });
                table.ForeignKey(
                    name: "FK_PlaylistMedias_MediaItems_MediaItemId",
                    column: x => x.MediaItemId,
                    principalTable: "MediaItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlaylistMedias_Playlists_PlaylistId",
                    column: x => x.PlaylistId,
                    principalTable: "Playlists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UserId = table.Column<int>(type: "int", nullable: false),
                EntityId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                Detail = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditLogs_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId",
            table: "AuditLogs",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PlaylistMedias_MediaItemId",
            table: "PlaylistMedias",
            column: "MediaItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Screens_CurrentPlaylistId",
            table: "Screens",
            column: "CurrentPlaylistId");

        migrationBuilder.CreateIndex(
            name: "IX_Screens_ScreenKey",
            table: "Screens",
            column: "ScreenKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "Screens");
        migrationBuilder.DropTable(name: "PlaylistMedias");
        migrationBuilder.DropTable(name: "MediaItems");
        migrationBuilder.DropTable(name: "Playlists");
        migrationBuilder.DropTable(name: "Users");
    }
}
