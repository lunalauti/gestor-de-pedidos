﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Orders.Infrastructure.Data;

#nullable disable

namespace Backend.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20250616015800_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("orders")
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Orders.Domain.Entities.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("address");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CustomerEmail")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("customer_email");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("customer_name");

                    b.Property<string>("DeliveryUserEmail")
                        .HasColumnType("text");

                    b.Property<int?>("DeliveryUserId")
                        .HasColumnType("integer");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("order_number");

                    b.Property<int>("OrderStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("phone");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("idx_orders_created_at");

                    b.HasIndex("CustomerEmail")
                        .HasDatabaseName("idx_orders_customer_email");

                    b.HasIndex("OrderNumber")
                        .IsUnique()
                        .HasDatabaseName("idx_orders_order_number");

                    b.HasIndex("OrderStatusId")
                        .HasDatabaseName("idx_orders_status");

                    b.ToTable("orders", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Entities.OrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid")
                        .HasColumnName("order_id");

                    b.Property<string>("ProductId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("product_id");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("product_name");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .HasDatabaseName("idx_order_items_order_id");

                    b.HasIndex("ProductId")
                        .HasDatabaseName("idx_order_items_product_id");

                    b.ToTable("order_items", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Entities.OrderItem", b =>
                {
                    b.HasOne("Orders.Domain.Entities.Order", null)
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_order_items_order_id");
                });

            modelBuilder.Entity("Orders.Domain.Entities.Order", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
