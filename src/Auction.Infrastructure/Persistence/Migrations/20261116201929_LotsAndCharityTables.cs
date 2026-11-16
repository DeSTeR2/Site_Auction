// Creates categories with their seed, lots, lot_photos, favorites, lot_views and donation tables, and links complaints to lots.
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auction.Infrastructure.Persistence.Migrations
{
    public partial class LotsAndCharityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "donation_links",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    normalized_url = table.Column<string>(type: "text", nullable: false),
                    original_url = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    submitted_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reviewed_by = table.Column<long>(type: "bigint", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    review_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_donation_links", x => x.id);
                    table.CheckConstraint("ck_donation_links_status", "status IN ('pending', 'approved', 'rejected', 'revoked')");
                    table.ForeignKey(
                        name: "fk_donation_links_user_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_donation_links_user_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lots",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    condition = table.Column<string>(type: "text", nullable: false),
                    start_price = table.Column<long>(type: "bigint", nullable: false),
                    bid_step = table.Column<long>(type: "bigint", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    hidden_reason = table.Column<string>(type: "text", nullable: true),
                    hidden_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    donation_url = table.Column<string>(type: "text", nullable: false),
                    donation_link_id = table.Column<long>(type: "bigint", nullable: true),
                    review_rejection_reason = table.Column<string>(type: "text", nullable: true),
                    source_lot_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    current_price = table.Column<long>(type: "bigint", nullable: false),
                    leader_id = table.Column<long>(type: "bigint", nullable: true),
                    bid_count = table.Column<int>(type: "integer", nullable: false),
                    bidder_count = table.Column<int>(type: "integer", nullable: false),
                    winner_id = table.Column<long>(type: "bigint", nullable: true),
                    final_price = table.Column<long>(type: "bigint", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lots", x => x.id);
                    table.CheckConstraint("ck_lots_active_has_times", "status <> 'active' OR (started_at IS NOT NULL AND ends_at IS NOT NULL)");
                    table.CheckConstraint("ck_lots_bid_counts", "bid_count >= 0 AND bidder_count >= 0 AND bidder_count <= bid_count");
                    table.CheckConstraint("ck_lots_bid_step", "bid_step BETWEEN 1 AND 1000000");
                    table.CheckConstraint("ck_lots_condition", "condition IN ('new', 'like_new', 'used', 'needs_repair')");
                    table.CheckConstraint("ck_lots_current_price", "current_price > 0");
                    table.CheckConstraint("ck_lots_description_length", "char_length(description) BETWEEN 20 AND 5000");
                    table.CheckConstraint("ck_lots_duration", "duration_minutes > 0");
                    table.CheckConstraint("ck_lots_final_price_when_closed", "final_price IS NULL OR status = 'sold' OR status = 'unsold'");
                    table.CheckConstraint("ck_lots_start_price", "start_price BETWEEN 1 AND 10000000");
                    table.CheckConstraint("ck_lots_status", "status IN ('draft', 'pending_review', 'active', 'sold', 'unsold', 'withdrawn')");
                    table.CheckConstraint("ck_lots_title_length", "char_length(title) BETWEEN 5 AND 100");
                    table.ForeignKey(
                        name: "fk_lots_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_donation_links_donation_link_id",
                        column: x => x.donation_link_id,
                        principalTable: "donation_links",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_lots_source_lot_id",
                        column: x => x.source_lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_user_leader_id",
                        column: x => x.leader_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_user_seller_id",
                        column: x => x.seller_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_user_winner_id",
                        column: x => x.winner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "donation_reports",
                columns: table => new
                {
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    winner_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    receipt_key = table.Column<string>(type: "text", nullable: true),
                    reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_donation_reports", x => x.lot_id);
                    table.CheckConstraint("ck_donation_reports_amount", "amount > 0");
                    table.ForeignKey(
                        name: "fk_donation_reports_lot_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_donation_reports_user_winner_id",
                        column: x => x.winner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reminder_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorites", x => new { x.user_id, x.lot_id });
                    table.ForeignKey(
                        name: "fk_favorites_lot_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favorites_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lot_photos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    position = table.Column<short>(type: "smallint", nullable: false),
                    large_key = table.Column<string>(type: "text", nullable: false),
                    thumb_key = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lot_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_lot_photos_lots_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lot_views",
                columns: table => new
                {
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    visitor_key = table.Column<string>(type: "text", nullable: false),
                    view_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lot_views", x => new { x.lot_id, x.visitor_key, x.view_date });
                    table.ForeignKey(
                        name: "fk_lot_views_lots_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "name", "sort_order" },
                values: new object[,]
                {
                    { (short)1, "Монети і банкноти", (short)1 },
                    { (short)2, "Марки", (short)2 },
                    { (short)3, "Вініл і музика", (short)3 },
                    { (short)4, "Книги", (short)4 },
                    { (short)5, "Електроніка і техніка", (short)5 },
                    { (short)6, "Одяг і аксесуари", (short)6 },
                    { (short)7, "Антикваріат", (short)7 },
                    { (short)8, "Мистецтво і хендмейд", (short)8 },
                    { (short)9, "Іграшки та колекційні фігурки", (short)9 },
                    { (short)10, "Спорт і хобі", (short)10 },
                    { (short)11, "Інше", (short)11 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_complaints_lot_id",
                table: "complaints",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_donation_links_normalized_url",
                table: "donation_links",
                column: "normalized_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_donation_links_reviewed_by",
                table: "donation_links",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "ix_donation_links_status_created_at",
                table: "donation_links",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_donation_links_submitted_by",
                table: "donation_links",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "ix_donation_reports_winner_id",
                table: "donation_reports",
                column: "winner_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_lot_id",
                table: "favorites",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_lot_photos_lot_id_position",
                table: "lot_photos",
                columns: new[] { "lot_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_category_id_status_is_hidden",
                table: "lots",
                columns: new[] { "category_id", "status", "is_hidden" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_description",
                table: "lots",
                column: "description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_donation_link_id",
                table: "lots",
                column: "donation_link_id");

            migrationBuilder.CreateIndex(
                name: "ix_lots_leader_id",
                table: "lots",
                column: "leader_id");

            migrationBuilder.CreateIndex(
                name: "ix_lots_seller_id_status",
                table: "lots",
                columns: new[] { "seller_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_source_lot_id",
                table: "lots",
                column: "source_lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_lots_status_closed_at",
                table: "lots",
                columns: new[] { "status", "closed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_lots_status_is_hidden_ends_at",
                table: "lots",
                columns: new[] { "status", "is_hidden", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_title",
                table: "lots",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_lots_winner_id",
                table: "lots",
                column: "winner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_complaints_lots_lot_id",
                table: "complaints",
                column: "lot_id",
                principalTable: "lots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_complaints_lots_lot_id",
                table: "complaints");

            migrationBuilder.DropTable(
                name: "donation_reports");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "lot_photos");

            migrationBuilder.DropTable(
                name: "lot_views");

            migrationBuilder.DropTable(
                name: "lots");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "donation_links");

            migrationBuilder.DropIndex(
                name: "ix_complaints_lot_id",
                table: "complaints");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
