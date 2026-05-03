using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingBoardGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    main_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    names = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    is_suspended = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    street_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    last_message_read_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    unread_messages_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_participants", x => new { x.conversation_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversation_participants_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    maximum_player_number = table.Column<int>(type: "int", nullable: false),
                    minimum_player_number = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                    table.ForeignKey(
                        name: "FK_games_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_sent_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    message_content_as_string = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    message_sender_id = table.Column<int>(type: "int", nullable: false),
                    message_receiver_id = table.Column<int>(type: "int", nullable: false),
                    MessageCategory = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    cash_payment_id = table.Column<int>(type: "int", nullable: true),
                    is_cash_agreement_resolved = table.Column<bool>(type: "bit", nullable: true),
                    is_cash_agreement_accepted_by_buyer = table.Column<bool>(type: "bit", nullable: true),
                    is_cash_agreement_accepted_by_seller = table.Column<bool>(type: "bit", nullable: true),
                    message_image_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rental_request_id = table.Column<int>(type: "int", nullable: true),
                    is_request_resolved = table.Column<bool>(type: "bit", nullable: true),
                    is_request_accepted = table.Column<bool>(type: "bit", nullable: true),
                    request_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    message_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text_message_content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_users_message_receiver_id",
                        column: x => x.message_receiver_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_message_sender_id",
                        column: x => x.message_sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_of_transaction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_confirmed_buyer = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_confirmed_seller = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payment_state = table.Column<int>(type: "int", nullable: false),
                    receipt_file_path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    PaymentCategory = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    game_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    owner_name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_users_client_id",
                        column: x => x.client_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    game_id = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    PaymentTransactionIdentifier = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.id);
                    table.ForeignKey(
                        name: "FK_rentals_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rentals_payments_PaymentTransactionIdentifier",
                        column: x => x.PaymentTransactionIdentifier,
                        principalTable: "payments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_rentals_users_client_id",
                        column: x => x.client_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rentals_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_participants_user_id",
                table: "conversation_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_games_owner_id",
                table: "games",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_cash_payment_id",
                table: "messages",
                column: "cash_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_receiver_id",
                table: "messages",
                column: "message_receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_sender_id",
                table: "messages",
                column: "message_sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_rental_request_id",
                table: "messages",
                column: "rental_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_client_id",
                table: "payments",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_owner_id",
                table: "payments",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_request_id",
                table: "payments",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_client_id",
                table: "rentals",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_game_id",
                table: "rentals",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_owner_id",
                table: "rentals",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_PaymentTransactionIdentifier",
                table: "rentals",
                column: "PaymentTransactionIdentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_messages_payments_cash_payment_id",
                table: "messages",
                column: "cash_payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_messages_rentals_rental_request_id",
                table: "messages",
                column: "rental_request_id",
                principalTable: "rentals",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_rentals_request_id",
                table: "payments",
                column: "request_id",
                principalTable: "rentals",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_games_users_owner_id",
                table: "games");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_users_client_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_users_owner_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_rentals_users_client_id",
                table: "rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_rentals_users_owner_id",
                table: "rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_rentals_payments_PaymentTransactionIdentifier",
                table: "rentals");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "rentals");

            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
