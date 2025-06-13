using System;
using System.Collections.Generic;
using DIP.Models;
using Microsoft.EntityFrameworkCore;

namespace DIP.Data;

public partial class DipDbContext : DbContext
{
    public DipDbContext()
    {
    }

    public DipDbContext(DbContextOptions<DipDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SysUser> SysUsers { get; set; }
    public virtual DbSet<SysRole> SysRoles { get; set; }
    public virtual DbSet<SysUserRole> SysUserRoles { get; set; }
    public virtual DbSet<SysModule> SysModules { get; set; }
    public virtual DbSet<SysRoleModule> SysRoleModules { get; set; }
    public virtual DbSet<KmsKnowledge> KmsKnowledges { get; set; }
    public virtual DbSet<KmsFriendlink> KmsFriendlinks { get; set; }
    public virtual DbSet<KmsType> KmsTypes { get; set; }
    public virtual DbSet<KmsFileComment> KmsFileComments { get; set; } = null!;
    public virtual DbSet<KmsBasicCodeData> KmsBasicCodeData { get; set; }
    public virtual DbSet<KmsFilePraise> KmsFilePraises { get; set; }
    public virtual DbSet<KmsAttention> KmsAttentions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 只有在沒有配置的情況下才使用預設連接（可選）
        if (!optionsBuilder.IsConfigured)
        {
            // 如果需要預設連接，使用 MySQL 而不是 SQL Server
            // var connectionString = "Server=localhost;Database=DIP_DB;User=root;Password=your_password;";
            // optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__SYS_USER__F3BEEBFFB4A90EF6");

            entity.ToTable("SYS_USER");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("USER_ID");
            entity.Property(e => e.Activeflag)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .IsFixedLength()
                .HasColumnName("ACTIVEFLAG");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("CREATE_ID");
            entity.Property(e => e.ImgContentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("IMG_CONTENT_TYPE");
            entity.Property(e => e.TxDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("TX_DATE");
            entity.Property(e => e.TxId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("TX_ID");
            entity.Property(e => e.UserDesc)
                .HasMaxLength(200)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("USER_DESC");
            entity.Property(e => e.UserImg)
                .HasDefaultValue("2e002e002f0069006d0061006700650073002f0075007300650072002f0075007300650072002d0034002e0070006e006700")
                .HasColumnName("USER_IMG");
            entity.Property(e => e.UserLogid)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("USER_LOGID");
            entity.Property(e => e.UserMail)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("USER_MAIL");
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("USER_NAME");
            entity.Property(e => e.UserPwd)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("USER_PWD");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__SYS_ROLE__XXXX"); // 可自定義主鍵名稱

            entity.ToTable("SYS_ROLE");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("ROLE_ID");

            entity.Property(e => e.RolenameCn)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("ROLENAME_CN");

            entity.Property(e => e.RolenameTw)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("ROLENAME_TW");

            entity.Property(e => e.RolenameJp)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("ROLENAME_JP");

            entity.Property(e => e.RolenameUs)
                .HasMaxLength(30)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("ROLENAME_US");

            entity.Property(e => e.SystemId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("('KMWeb')")
                .HasColumnName("SYSTEM_ID");

            entity.Property(e => e.SystemType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValueSql("('2')")
                .HasColumnName("SYSTEM_TYPE");

            entity.Property(e => e.RoleDesc)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("ROLE_DESC");

            entity.Property(e => e.ActiveFlag)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValueSql("('Y')")
                .HasColumnName("ACTIVE_FLAG");

            entity.Property(e => e.CreateId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("CREATE_ID");

            entity.Property(e => e.CreateDate)
                .HasColumnType("date")
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("CREATE_DATE");

            entity.Property(e => e.TxId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("TX_ID");

            entity.Property(e => e.TxDate)
                .HasColumnType("date")
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("TX_DATE");
        });

        modelBuilder.Entity<SysUserRole>(entity =>
        {
            entity.ToTable("SYS_USER_ROLE");

            // 主鍵只設 USER_ID（表示一位使用者只能有一筆角色紀錄）
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_ID");

            entity.Property(e => e.RoleId)
                .HasColumnName("ROLE_ID");

            entity.Property(e => e.CreateId)
                .HasColumnName("CREATE_ID");

            entity.Property(e => e.CreateDate)
                .HasColumnName("CREATE_DATE")
                .HasColumnType("datetime");

            entity.Property(e => e.TxId)
                .HasColumnName("TX_ID");

            entity.Property(e => e.TxDate)
                .HasColumnName("TX_DATE")
                .HasColumnType("datetime");

            // 一對一：User 對 UserRole
            entity.HasOne(ur => ur.User)
                .WithOne(u => u.UserRole) // ← 需在 SysUser 類別中新增 UserRole 導覽屬性
                .HasForeignKey<SysUserRole>(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 多對一：多使用者可以共用同一個角色
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SysModule>(entity =>
        {
            entity.ToTable("SYS_MODULE");

            entity.HasKey(e => e.ModuleId);

            entity.Property(e => e.ModuleId)
                .HasColumnName("MODULE_ID");

            entity.Property(e => e.ModulenameCn)
                .HasColumnName("MODULENAME_CN");

            entity.Property(e => e.ModulenameTw)
                .HasColumnName("MODULENAME_TW");

            entity.Property(e => e.ModulenameJp)
                .HasColumnName("MODULENAME_JP");

            entity.Property(e => e.ModulenameUs)
                .HasColumnName("MODULENAME_US");

            entity.Property(e => e.ModuleImg)
                .HasColumnName("MODULE_IMG");

            entity.Property(e => e.ModuleSrc)
                .HasColumnName("MODULE_SRC");

            entity.Property(e => e.Parentid)
                .HasColumnName("PARENTID");

            entity.Property(e => e.SeqNo)
                .HasColumnName("SEQ_NO");

            entity.Property(e => e.SystemId)
                .HasColumnName("SYSTEM_ID");

            entity.Property(e => e.SystemType)
                .HasColumnName("SYSTEM_TYPE");

            entity.Property(e => e.CreateId)
                .HasColumnName("CREATE_ID");

            entity.Property(e => e.CreateDate)
                .HasColumnName("CREATE_DATE")
                .HasColumnType("date");

            entity.Property(e => e.TxId)
                .HasColumnName("TX_ID");

            entity.Property(e => e.TxDate)
                .HasColumnName("TX_DATE")
                .HasColumnType("date");

            entity.Property(e => e.ActiveFlag)
                .HasColumnName("ACTIVE_FLAG");
        });

        modelBuilder.Entity<SysRoleModule>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ModuleId }).HasName("PK_SYS_ROLE_MODULE");

            entity.ToTable("SYS_ROLE_MODULE");

            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.ModuleId).HasColumnName("MODULE_ID");
            entity.Property(e => e.ActiveFlag)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("ACTIVE_FLAG");
            entity.Property(e => e.CreateId).HasColumnName("CREATE_ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime").HasColumnName("CREATE_DATE");
            entity.Property(e => e.TxId).HasColumnName("TX_ID");
            entity.Property(e => e.TxDate).HasColumnType("datetime").HasColumnName("TX_DATE");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.RoleModules)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_SYS_ROLE_MODULE_ROLE");

            entity.HasOne(d => d.Module)
                .WithMany(p => p.RoleModules)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_SYS_ROLE_MODULE_MODULE");
        });

        modelBuilder.Entity<KmsType>(entity =>
        {
            entity.ToTable("KMS_TYPE");

            entity.HasKey(e => e.KtId);

            entity.Property(e => e.KtId)
                .HasColumnName("KT_ID");

            entity.Property(e => e.KtNameCn)
                .HasColumnName("KT_NAME_CN");

            entity.Property(e => e.KtNameTw)
                .HasColumnName("KT_NAME_TW");

            entity.Property(e => e.KtNameJp)
                .HasColumnName("KT_NAME_JP");

            entity.Property(e => e.KtNameUs)
                .HasColumnName("KT_NAME_US");

            entity.Property(e => e.ParentId)
                .HasColumnName("PARENTID");

            entity.Property(e => e.SeqNo)
                .HasColumnName("SEQ_NO");

            entity.Property(e => e.CreateId)
                .HasColumnName("CREATE_ID");

            entity.Property(e => e.CreateDate)
                .HasColumnName("CREATE_DATE")
                .HasColumnType("datetime");

            entity.Property(e => e.TxId)
                .HasColumnName("TX_ID");

            entity.Property(e => e.TxDate)
                .HasColumnName("TX_DATE")
                .HasColumnType("datetime");

            entity.Property(e => e.ActiveFlag)
                .HasColumnName("ACTIVEFLAG");
        });

        modelBuilder.Entity<KmsKnowledge>(entity =>
        {
            entity.HasKey(e => e.KfFileId);
            entity.Property(e => e.KfFileId).ValueGeneratedNever(); // 若是手動指定PK

            entity.Property(e => e.KfFileContent).HasColumnType("text");

            entity.Property(e => e.KfReadNum).HasDefaultValue(0);
            entity.Property(e => e.KfPraiseNum).HasDefaultValue(0);
            entity.Property(e => e.KfTreadNum).HasDefaultValue(0);
        });

        modelBuilder.Entity<KmsKnowledge>()
            .HasOne(k => k.KfFileType)
            .WithMany()
            .HasForeignKey(k => k.KfFileTypeId);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}