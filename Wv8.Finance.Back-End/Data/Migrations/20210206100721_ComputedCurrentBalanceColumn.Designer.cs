// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinance.Data;

namespace PersonalFinance.Data.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20210206100721_ComputedCurrentBalanceColumn")]
    partial class ComputedCurrentBalanceColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PersonalFinance.Data.Models.AccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("CurrentBalance")
                        .ValueGeneratedOnAddOrUpdate()
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

                    b.HasKey("Id");

                    b.HasIndex("IconId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BudgetEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(12,2)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("date");

                    b.Property<decimal>("Spent")
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
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("ExpectedMonthlyAmount")
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
                        .HasColumnType("decimal(12,2)");

                    b.HasKey("AccountId", "Date");

                    b.ToTable("DailyBalances");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.IconEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount")
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

            modelBuilder.Entity("PersonalFinance.Data.Models.RecurringTransactionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(12,2)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<bool>("NeedsConfirmation")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("NextOccurence")
                        .HasColumnType("date");

                    b.Property<int?>("ReceivingAccountId")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("date");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ReceivingAccountId");

                    b.ToTable("RecurringTransactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.TransactionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(12,2)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("NeedsConfirmation")
                        .HasColumnType("bit");

                    b.Property<bool>("Processed")
                        .HasColumnType("bit");

                    b.Property<int?>("ReceivingAccountId")
                        .HasColumnType("int");

                    b.Property<int?>("RecurringTransactionId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ReceivingAccountId");

                    b.HasIndex("RecurringTransactionId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.AccountEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.IconEntity", "Icon")
                        .WithMany()
                        .HasForeignKey("IconId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.BudgetEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.CategoryEntity", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.DailyBalanceEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.AccountEntity", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.PaymentRequestEntity", b =>
                {
                    b.HasOne("PersonalFinance.Data.Models.TransactionEntity", "Transaction")
                        .WithMany("PaymentRequests")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.RecurringTransactionEntity", b =>
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
                });

            modelBuilder.Entity("PersonalFinance.Data.Models.TransactionEntity", b =>
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

                    b.HasOne("PersonalFinance.Data.Models.RecurringTransactionEntity", "RecurringTransaction")
                        .WithMany()
                        .HasForeignKey("RecurringTransactionId");
                });
#pragma warning restore 612, 618
        }
    }
}
