﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PersonalFinance.Data.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("PersonalFinance.Data.Models.AccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal>("CurrentBalance")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)")
                        .HasComputedColumnSql("GetCurrentBalance([Id])");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IconId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<bool>("IsObsolete")
                        .HasColumnType("bit");

                    b.Property<byte>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((byte)1);

                    b.HasKey("Id");

                    b.HasIndex("IconId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BaseTransactionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("NeedsConfirmation")
                        .HasColumnType("bit");

                    b.Property<int?>("ReceivingAccountId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ReceivingAccountId");

                    b.ToTable("BaseTransactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BudgetEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("date");

                    b.Property<decimal>("Spent")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("date");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.CategoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("ExpectedMonthlyAmount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<int>("IconId")
                        .HasColumnType("int");

                    b.Property<bool>("IsObsolete")
                        .HasColumnType("bit");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IconId");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.DailyBalanceEntity", b =>
                {
                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<decimal>("Balance")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.HasKey("AccountId", "Date");

                    b.ToTable("DailyBalances");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.IconEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Pack")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Icons");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.PaymentRequestEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PaidCount")
                        .HasColumnType("int");

                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("PaymentRequests");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.SplitDetailEntity", b =>
                {
                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.Property<int>("SplitwiseUserId")
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.HasKey("TransactionId", "SplitwiseUserId");

                    b.ToTable("SplitDetails");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.SplitwiseTransactionEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Imported")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<decimal>("PaidAmount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<decimal>("PersonalAmount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SplitwiseTransactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.SynchronizationTimesEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("SplitwiseLastRun")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SynchronizationTimes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            SplitwiseLastRun = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.RecurringTransactionEntity", b =>
                {
                    b.HasBaseType("PersonalFinance.Data.Models.BaseTransactionEntity");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("date");

                    b.Property<bool>("Finished")
                        .HasColumnType("bit");

                    b.Property<int>("Interval")
                        .HasColumnType("int");

                    b.Property<int>("IntervalUnit")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastOccurence")
                        .HasColumnType("date");

                    b.Property<DateTime?>("NextOccurence")
                        .HasColumnType("date");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("date");

                    b.ToTable("RecurringTransactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.TransactionEntity", b =>
                {
                    b.HasBaseType("PersonalFinance.Data.Models.BaseTransactionEntity");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<bool?>("IsConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("Processed")
                        .HasColumnType("bit");

                    b.Property<int?>("RecurringTransactionId")
                        .HasColumnType("int");

                    b.Property<int?>("SplitwiseTransactionId")
                        .HasColumnType("int");

                    b.HasIndex("RecurringTransactionId");

                    b.HasIndex("SplitwiseTransactionId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.AccountEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.IconEntity", "Icon")
                        .WithMany()
                        .HasForeignKey("IconId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Icon");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BaseTransactionEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.AccountEntity", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PersonalFinance.Data.Models.CategoryEntity", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("PersonalFinance.Data.Models.AccountEntity", "ReceivingAccount")
                        .WithMany()
                        .HasForeignKey("ReceivingAccountId");

                    b.Navigation("Account");

                    b.Navigation("Category");

                    b.Navigation("ReceivingAccount");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BudgetEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.CategoryEntity", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.CategoryEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.IconEntity", "Icon")
                        .WithMany()
                        .HasForeignKey("IconId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PersonalFinance.Data.Models.CategoryEntity", "ParentCategory")
                        .WithMany("Children")
                        .HasForeignKey("ParentCategoryId");

                    b.Navigation("Icon");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.DailyBalanceEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.AccountEntity", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.PaymentRequestEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.BaseTransactionEntity", null)
                        .WithMany("PaymentRequests")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.SplitDetailEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.BaseTransactionEntity", null)
                        .WithMany("SplitDetails")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.RecurringTransactionEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.BaseTransactionEntity", null)
                        .WithOne()
                        .HasForeignKey("PersonalFinance.Data.Models.RecurringTransactionEntity", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.TransactionEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.BaseTransactionEntity", null)
                        .WithOne()
                        .HasForeignKey("PersonalFinance.Data.Models.TransactionEntity", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.HasOne("PersonalFinance.Data.Models.RecurringTransactionEntity", "RecurringTransaction")
                        .WithMany()
                        .HasForeignKey("RecurringTransactionId");

                    b.HasOne("PersonalFinance.Data.Models.SplitwiseTransactionEntity", "SplitwiseTransaction")
                        .WithMany()
                        .HasForeignKey("SplitwiseTransactionId");

                    b.Navigation("RecurringTransaction");

                    b.Navigation("SplitwiseTransaction");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BaseTransactionEntity", b =>
                {
                    b.Navigation("PaymentRequests");

                    b.Navigation("SplitDetails");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.CategoryEntity", b =>
                {
                    b.Navigation("Children");
                });
#pragma warning restore 612, 618
        }
    }
}
