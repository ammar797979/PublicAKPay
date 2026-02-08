using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AKPay.Models;

public partial class AkpayDbContext : DbContext
{
    public AkpayDbContext(DbContextOptions<AkpayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CheckBalance> CheckBalances { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<RegularTransaction> RegularTransactions { get; set; }

    public virtual DbSet<TopUpSource> TopUpSources { get; set; }

    public virtual DbSet<TopUpTransaction> TopUpTransactions { get; set; }

    public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserBeneficiary> UserBeneficiaries { get; set; }

    public virtual DbSet<UserToUserTransaction> UserToUserTransactions { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<VendorPaymentTransaction> VendorPaymentTransactions { get; set; }

    public virtual DbSet<VendorStatus> VendorStatuses { get; set; }

    public virtual DbSet<VendorStatus1> VendorStatuses1 { get; set; }

    public virtual DbSet<VwUserMonthlyStat> VwUserMonthlyStats { get; set; }

    public virtual DbSet<VwVendorPerformanceRanking> VwVendorPerformanceRankings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CheckBalance");

            entity.Property(e => e.Balance)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.LastUpdateTime)
                .HasColumnType("datetime")
                .HasColumnName("lastUpdateTime");
            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.CreatedAt });

            entity.Property(e => e.NotificationId)
                .ValueGeneratedOnAdd()
                .HasColumnName("notificationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.Msg)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("msg");
            entity.Property(e => e.NotifType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("notifType");
            entity.Property(e => e.RecipientId).HasColumnName("recipientID");
            entity.Property(e => e.RecipientType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("recipientType");
            entity.Property(e => e.TxId).HasColumnName("txID");
            entity.Property(e => e.TxType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("txType");
        });

        modelBuilder.Entity<RegularTransaction>(entity =>
        {
            entity.HasKey(e => new { e.RegularTransactionId, e.TxTimeStamp });

            entity.HasIndex(e => new { e.TxTimeStamp, e.FromUserId }, "IX_RegTx_FromUserID");

            entity.HasIndex(e => new { e.TxStatusId, e.TxTimeStamp }, "IX_RegTx_Status_Time");

            entity.HasIndex(e => new { e.TxTimeStamp, e.ToVendorId }, "IX_RegTx_ToVendorID");

            entity.HasIndex(e => e.TxTimeStamp, "IX_RegularTransactions_Analytics");

            entity.Property(e => e.RegularTransactionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("regularTransactionID");
            entity.Property(e => e.TxTimeStamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("txTimeStamp");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.FromUserId).HasColumnName("fromUserID");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Offline")
                .HasColumnName("paymentMode");
            entity.Property(e => e.ToVendorId).HasColumnName("toVendorID");
            entity.Property(e => e.TxStatusId)
                .HasDefaultValue(1)
                .HasColumnName("txStatusID");

            entity.HasOne(d => d.FromUser).WithMany(p => p.RegularTransactions)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegTx_Users");

            entity.HasOne(d => d.ToVendor).WithMany(p => p.RegularTransactions)
                .HasForeignKey(d => d.ToVendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegTx_Vendors");

            entity.HasOne(d => d.TxStatus).WithMany(p => p.RegularTransactions)
                .HasForeignKey(d => d.TxStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegTx_TxStatuses");
        });

        modelBuilder.Entity<TopUpSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__TopUpSou__5ABC0BC0DE93CAD4");

            entity.HasIndex(e => e.SourceName, "UQ__TopUpSou__7435EC7D5A740A74").IsUnique();

            entity.Property(e => e.SourceId).HasColumnName("sourceID");
            entity.Property(e => e.SourceName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("sourceName");
        });

        modelBuilder.Entity<TopUpTransaction>(entity =>
        {
            entity.HasKey(e => new { e.TopUpTransactionId, e.TxTimeStamp });

            entity.HasIndex(e => new { e.TxStatusId, e.TxTimeStamp }, "IX_TopUpTx_Status_Time");

            entity.HasIndex(e => new { e.TxTimeStamp, e.ToUserId }, "IX_TopUpTx_ToUserID");

            entity.Property(e => e.TopUpTransactionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("topUpTransactionID");
            entity.Property(e => e.TxTimeStamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("txTimeStamp");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.SourceId).HasColumnName("sourceID");
            entity.Property(e => e.ToUserId).HasColumnName("toUserID");
            entity.Property(e => e.TxStatusId)
                .HasDefaultValue(1)
                .HasColumnName("txStatusID");

            entity.HasOne(d => d.Source).WithMany(p => p.TopUpTransactions)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopUpTx_TopUpSources");

            entity.HasOne(d => d.ToUser).WithMany(p => p.TopUpTransactions)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopUpTx_Users");

            entity.HasOne(d => d.TxStatus).WithMany(p => p.TopUpTransactions)
                .HasForeignKey(d => d.TxStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopUpTx_TxStatuses");
        });

        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Transact__36257A383A4D63FD");

            entity.HasIndex(e => e.StatusName, "UQ__Transact__6A50C2120BFD0E09").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("statusID");
            entity.Property(e => e.StatusDescription)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("statusDescription");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("statusName");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDFFB030B5F");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("TR_Users_BlockDirectCreate");
                    tb.HasTrigger("TR_Users_BlockDirectDelete");
                });

            entity.HasIndex(e => e.Email, "IX_Users_Email_Active")
                .IsUnique()
                .HasFilter("([Users].[isDeleted]=(0))");

            entity.HasIndex(e => e.Phone, "IX_Users_Phone_Active")
                .IsUnique()
                .HasFilter("([Users].[isDeleted]=(0))");

            entity.HasIndex(e => e.IsDeleted, "IX_Users_isDeleted");

            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dateCreated");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullName");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasComputedColumnSql("(left([email],charindex('@lums.edu.pk',[email])-(1)))", true)
                .HasColumnName("userName");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("userType");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserAcco__CB9A1CDF8C8D1BC2");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("TR_UserAccounts_BlockDirectCreate");
                    tb.HasTrigger("TR_UserAccounts_BlockDirectDelete");
                    tb.HasTrigger("TR_UserAccounts_ValidLastUpdate");
                    tb.HasTrigger("TR_UserAccounts_lastUpdateTime");
                });

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userID");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.LastUpdateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("lastUpdateTime");
            entity.Property(e => e.UserBalance)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("userBalance");

            entity.HasOne(d => d.User).WithOne(p => p.UserAccount)
                .HasForeignKey<UserAccount>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAccounts_Users");
        });

        modelBuilder.Entity<UserBeneficiary>(entity =>
        {
            entity.HasKey(e => new { e.RemitterId, e.BeneficiaryId }).HasName("PK_UserBeneficiary");

            entity.ToTable(tb => tb.HasTrigger("TR_UserBeneficiaries_nickName"));

            entity.Property(e => e.RemitterId).HasColumnName("remitterID");
            entity.Property(e => e.BeneficiaryId).HasColumnName("beneficiaryID");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("addedAt");
            entity.Property(e => e.LastPaymentAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("lastPaymentAmount");
            entity.Property(e => e.LastPaymentTime)
                .HasColumnType("datetime")
                .HasColumnName("lastPaymentTime");
            entity.Property(e => e.NickName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nickName");

            entity.HasOne(d => d.Beneficiary).WithMany(p => p.UserBeneficiaryBeneficiaries)
                .HasForeignKey(d => d.BeneficiaryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBenef_Users_Beneficiary");

            entity.HasOne(d => d.Remitter).WithMany(p => p.UserBeneficiaryRemitters)
                .HasForeignKey(d => d.RemitterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBenef_Users_Remitter");
        });

        modelBuilder.Entity<UserToUserTransaction>(entity =>
        {
            entity.HasKey(e => new { e.UtoUtransactionId, e.TxTimeStamp });

            entity.HasIndex(e => new { e.TxTimeStamp, e.FromUserId }, "IX_U2UTx_FromUserID");

            entity.HasIndex(e => new { e.TxStatusId, e.TxTimeStamp }, "IX_U2UTx_Status_Time");

            entity.HasIndex(e => new { e.TxTimeStamp, e.ToUserId }, "IX_U2UTx_ToUserID");

            entity.Property(e => e.UtoUtransactionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("UToUTransactionID");
            entity.Property(e => e.TxTimeStamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("txTimeStamp");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.FromUserId).HasColumnName("fromUserID");
            entity.Property(e => e.ToUserId).HasColumnName("toUserID");
            entity.Property(e => e.TxStatusId)
                .HasDefaultValue(1)
                .HasColumnName("txStatusID");

            entity.HasOne(d => d.FromUser).WithMany(p => p.UserToUserTransactionFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_U2UTx_Users_FROM");

            entity.HasOne(d => d.ToUser).WithMany(p => p.UserToUserTransactionToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_U2UTx_Users_TO");

            entity.HasOne(d => d.TxStatus).WithMany(p => p.UserToUserTransactions)
                .HasForeignKey(d => d.TxStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_U2UTx_TxStatuses");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__Vendors__EC65C4E3C909E9A6");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("TR_Vendors_ValidLastUpdate");
                    tb.HasTrigger("TR_Vendors_lastUpdateTime");
                });

            entity.Property(e => e.VendorId).HasColumnName("vendorID");
            entity.Property(e => e.LastUpdateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("lastUpdateTime");
            entity.Property(e => e.ManagerName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("managerName");
            entity.Property(e => e.ManagerPhone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("managerPhone");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(2)
                .HasColumnName("statusID");
            entity.Property(e => e.VendorBalance)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("vendorBalance");
            entity.Property(e => e.VendorName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("vendorName");

            entity.HasOne(d => d.Status).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendors_VendorStatuses");
        });

        modelBuilder.Entity<VendorPaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.VendorPaymentTransactionId).HasName("PK__VendorPa__BCEA0AAC10619BEC");

            entity.ToTable("VendorPaymentTransaction");

            entity.HasIndex(e => new { e.TxStatusId, e.TxTimeStamp }, "IX_VendorPayTx_Status_Time");

            entity.HasIndex(e => e.ToVendorId, "IX_VendorPayTx_ToVendorID");

            entity.Property(e => e.VendorPaymentTransactionId).HasColumnName("vendorPaymentTransactionID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.ToVendorId).HasColumnName("toVendorID");
            entity.Property(e => e.TxStatusId)
                .HasDefaultValue(2)
                .HasColumnName("txStatusID");
            entity.Property(e => e.TxTimeStamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("txTimeStamp");

            entity.HasOne(d => d.ToVendor).WithMany(p => p.VendorPaymentTransactions)
                .HasForeignKey(d => d.ToVendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorPayTx_Vendors");

            entity.HasOne(d => d.TxStatus).WithMany(p => p.VendorPaymentTransactions)
                .HasForeignKey(d => d.TxStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VendorPayTx_TxStatuses");
        });

        modelBuilder.Entity<VendorStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VendorStatus");

            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.LastUpdateTime)
                .HasColumnType("datetime")
                .HasColumnName("lastUpdateTime");
            entity.Property(e => e.ManagerName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("managerName");
            entity.Property(e => e.ManagerPhone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("managerPhone");
            entity.Property(e => e.VendorBalance)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("vendorBalance");
            entity.Property(e => e.VendorId).HasColumnName("vendorID");
            entity.Property(e => e.VendorName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("vendorName");
        });

        modelBuilder.Entity<VendorStatus1>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__VendorSt__36257A38D4E1CBD2");

            entity.ToTable("VendorStatuses");

            entity.HasIndex(e => e.StatusName, "UQ__VendorSt__6A50C2125B3656A5").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("statusID");
            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("statusName");
            entity.Property(e => e.TillWhen)
                .HasColumnType("datetime")
                .HasColumnName("tillWhen");
        });

        modelBuilder.Entity<VwUserMonthlyStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserMonthlyStats");

            entity.Property(e => e.AvgTransactionSize).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.TotalSpent).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TransactionMonth).HasMaxLength(4000);
            entity.Property(e => e.UserFullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("userFullName");
        });

        modelBuilder.Entity<VwVendorPerformanceRanking>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_VendorPerformanceRankings");

            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.VendorName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("vendorName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
