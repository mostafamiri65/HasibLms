using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HasibLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureStatusToArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FeatureStatus",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeatureStatus",
                table: "Articles");
        }
    }
}
