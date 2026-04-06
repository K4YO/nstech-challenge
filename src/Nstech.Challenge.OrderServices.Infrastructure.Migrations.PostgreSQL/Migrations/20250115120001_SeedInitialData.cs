using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow.ToString("O");

            // Insert Products (8 products)
            migrationBuilder.Sql($@"
                INSERT INTO ""Products"" (""Id"", ""Sku"", ""UnitPrice"", ""AvailableQuantity"", ""ReservedQuantity"", ""CreatedAt"", ""UpdatedAt"", ""SequenceNumber"")
                VALUES 
                    ('a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'PROD-001', 99.99,  100, 0, '{now}', '{now}', 0),
                    ('b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e', 'PROD-002', 49.99,   50, 0, '{now}', '{now}', 0),
                    ('c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f', 'PROD-003', 149.99,  25, 0, '{now}', '{now}', 0),
                    ('d4e5f6a7-b8c9-4d5e-8f9a-3b4c5d6e7f8a', 'PROD-004', 29.99,  200, 0, '{now}', '{now}', 0),
                    ('e5f6a7b8-c9d0-4e5f-9a0b-4c5d6e7f8a9b', 'PROD-005', 199.99,  15, 0, '{now}', '{now}', 0),
                    ('f6a7b8c9-d0e1-4f5a-0b1c-5d6e7f8a9b0c', 'PROD-006', 9.99,   500, 0, '{now}', '{now}', 0),
                    ('a7b8c9d0-e1f2-4a5b-1c2d-6e7f8a9b0c1d', 'PROD-007', 74.50,   60, 0, '{now}', '{now}', 0),
                    ('b8c9d0e1-f2a3-4b5c-2d3e-7f8a9b0c1d2e', 'PROD-008', 349.90,  10, 0, '{now}', '{now}', 0);
            ");

            // Insert Orders (8 orders, 2 per status, 3 customers)
            migrationBuilder.Sql($@"
                INSERT INTO ""Orders"" (""Id"", ""CustomerId"", ""Status"", ""Currency"", ""Total"", ""CreatedAt"", ""UpdatedAt"", ""SequenceNumber"")
                VALUES 
                    ('11111111-1111-1111-1111-111111111111', 'f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0', 'Placed',    'USD', 199.98, '{now}', '{now}', 0),
                    ('22222222-2222-2222-2222-222222222222', 'f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0', 'Confirmed', 'USD', 249.97, '{now}', '{now}', 0),
                    ('33333333-3333-3333-3333-333333333333', 'a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0', 'Canceled',  'BRL', 59.98,  '{now}', '{now}', 0),
                    ('44444444-4444-4444-4444-444444444444', 'a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0', 'Placed',    'BRL', 399.98, '{now}', '{now}', 0),
                    ('55555555-5555-5555-5555-555555555555', 'b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1', 'Confirmed', 'EUR', 224.49, '{now}', '{now}', 0),
                    ('66666666-6666-6666-6666-666666666666', 'b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1', 'Canceled',  'EUR', 349.90, '{now}', '{now}', 0),
                    ('77777777-7777-7777-7777-777777777777', 'f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0', 'Placed',    'USD', 89.97,  '{now}', '{now}', 0),
                    ('88888888-8888-8888-8888-888888888888', 'a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0', 'Confirmed', 'BRL', 449.85, '{now}', '{now}', 0);
            ");

            // Insert OrderItems
            migrationBuilder.Sql($@"
                INSERT INTO ""OrderItem"" (""Id"", ""OrderId"", ""ProductId"", ""UnitPrice"", ""Quantity"", ""CreatedAt"", ""UpdatedAt"", ""SequenceNumber"")
                VALUES 
                    -- Order 1 (Placed, USD 199.98): 2x PROD-001
                    ('31111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 99.99,  2, '{now}', '{now}', 0),
                    -- Order 2 (Confirmed, USD 249.97): 1x PROD-002 + 1x PROD-003
                    ('32222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', 'b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e', 49.99,  1, '{now}', '{now}', 0),
                    ('33333333-3333-3333-3333-333333333333', '22222222-2222-2222-2222-222222222222', 'c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f', 149.99, 1, '{now}', '{now}', 0),
                    ('34444444-3333-3333-3333-333333333333', '22222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 49.99,  1, '{now}', '{now}', 0),
                    -- Order 3 (Canceled, BRL 59.98): 2x PROD-004
                    ('34444444-4444-4444-4444-444444444444', '33333333-3333-3333-3333-333333333333', 'd4e5f6a7-b8c9-4d5e-8f9a-3b4c5d6e7f8a', 29.99,  2, '{now}', '{now}', 0),
                    -- Order 4 (Placed, BRL 399.98): 2x PROD-005
                    ('35555555-5555-5555-5555-555555555555', '44444444-4444-4444-4444-444444444444', 'e5f6a7b8-c9d0-4e5f-9a0b-4c5d6e7f8a9b', 199.99, 2, '{now}', '{now}', 0),
                    -- Order 5 (Confirmed, EUR 224.49): 3x PROD-007 + 1x PROD-006
                    ('36666666-6666-6666-6666-666666666666', '55555555-5555-5555-5555-555555555555', 'a7b8c9d0-e1f2-4a5b-1c2d-6e7f8a9b0c1d', 74.50,  3, '{now}', '{now}', 0),
                    ('36666666-6666-6666-6666-777777777777', '55555555-5555-5555-5555-555555555555', 'f6a7b8c9-d0e1-4f5a-0b1c-5d6e7f8a9b0c', 9.99,   1, '{now}', '{now}', 0),
                    -- Order 6 (Canceled, EUR 349.90): 1x PROD-008
                    ('37777777-7777-7777-7777-777777777777', '66666666-6666-6666-6666-666666666666', 'b8c9d0e1-f2a3-4b5c-2d3e-7f8a9b0c1d2e', 349.90, 1, '{now}', '{now}', 0),
                    -- Order 7 (Placed, USD 89.97): 3x PROD-004
                    ('38888888-8888-8888-8888-888888888888', '77777777-7777-7777-7777-777777777777', 'd4e5f6a7-b8c9-4d5e-8f9a-3b4c5d6e7f8a', 29.99,  3, '{now}', '{now}', 0),
                    -- Order 8 (Confirmed, BRL 449.85): 3x PROD-003
                    ('39999999-9999-9999-9999-999999999999', '88888888-8888-8888-8888-888888888888', 'c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f', 149.95, 3, '{now}', '{now}', 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""OrderItem"";");
            migrationBuilder.Sql(@"DELETE FROM ""Orders"";");
            migrationBuilder.Sql(@"DELETE FROM ""Products"";");
        }
    }
}
