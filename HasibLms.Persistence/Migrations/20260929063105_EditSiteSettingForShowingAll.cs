using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HasibLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditSiteSettingForShowingAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomeArticlesCount",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeFeaturedCoursesCount",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeLatestCoursesCount",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomePopularCoursesCount",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCategories",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowFeaturedCourses",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLatestArticles",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLatestCourses",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPopularArticles",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPopularCourses",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStats",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTestimonials",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeArticlesCount",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HomeFeaturedCoursesCount",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HomeLatestCoursesCount",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HomePopularCoursesCount",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowCategories",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowFeaturedCourses",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowLatestArticles",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowLatestCourses",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowPopularArticles",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowPopularCourses",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowStats",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ShowTestimonials",
                table: "SiteSettings");
        }
    }
}
