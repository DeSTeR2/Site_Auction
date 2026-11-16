// Creates bids with UNIQUE (lot_id, amount), notifications with the email outbox and suspicious_activities (design/03-data-model.md).
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Auction.Infrastructure.Persistence.Migrations
{
    public partial class BiddingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bids",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    bidder_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    placed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bids", x => x.id);
                    table.CheckConstraint("ck_bids_amount", "amount BETWEEN 1 AND 100000000");
                    table.ForeignKey(
                        name: "fk_bids_lot_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bids_user_bidder_id",
                        column: x => x.bidder_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    recipient_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    lot_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    link = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email_status = table.Column<string>(type: "text", nullable: false),
                    email_attempts = table.Column<int>(type: "integer", nullable: false),
                    email_next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("ck_notifications_email_attempts", "email_attempts BETWEEN 0 AND 3");
                    table.CheckConstraint("ck_notifications_email_status", "email_status IN ('none', 'pending', 'sent', 'failed')");
                    table.CheckConstraint("ck_notifications_type", "type IN ('outbid', 'you_won', 'you_lost', 'lot_sold', 'lot_unsold', 'new_bid_on_your_lot', 'favorite_ending', 'lot_hidden', 'lot_restored', 'complaint_reviewed', 'account_blocked', 'lot_approved', 'lot_rejected')");
                    table.ForeignKey(
                        name: "fk_notifications_user_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "suspicious_activities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    lot_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suspicious_activities", x => x.id);
                    table.CheckConstraint("ck_suspicious_activities_type", "type IN ('own_lot_bid', 'rate_limit', 'new_account_bid', 'login_failures')");
                    table.ForeignKey(
                        name: "fk_suspicious_activities_lot_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_suspicious_activities_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bids_bidder_id_lot_id",
                table: "bids",
                columns: new[] { "bidder_id", "lot_id" });

            migrationBuilder.CreateIndex(
                name: "ix_bids_lot_id_amount",
                table: "bids",
                columns: new[] { "lot_id", "amount" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bids_lot_id_placed_at",
                table: "bids",
                columns: new[] { "lot_id", "placed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_email_next_attempt_at",
                table: "notifications",
                column: "email_next_attempt_at",
                filter: "email_status = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_id_is_read_created_at",
                table: "notifications",
                columns: new[] { "recipient_id", "is_read", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_suspicious_activities_lot_id",
                table: "suspicious_activities",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_suspicious_activities_type_created_at",
                table: "suspicious_activities",
                columns: new[] { "type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_suspicious_activities_user_id",
                table: "suspicious_activities",
                column: "user_id");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bids");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "suspicious_activities");
        }
    }
}
