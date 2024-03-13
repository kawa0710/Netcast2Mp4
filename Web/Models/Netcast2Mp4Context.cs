using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Web.Models
{
    public partial class Netcast2Mp4Context : DbContext
    {
        public Netcast2Mp4Context()
        {
        }

        public Netcast2Mp4Context(DbContextOptions<Netcast2Mp4Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Channel> Channel { get; set; }
        public virtual DbSet<Flim> Flim { get; set; }
        public virtual DbSet<Image> Image { get; set; }
        public virtual DbSet<Netcast> Netcast { get; set; }
        public virtual DbSet<SendMail> SendMail { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserPassword> UserPassword { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.HasKey(e => e.ChnSn);

                entity.HasIndex(e => e.ChnSn)
                    .HasName("IX_Channel");

                entity.Property(e => e.ChnSn)
                    .HasColumnName("CHN_SN")
                    .HasComment("節目序號");

                entity.Property(e => e.ChnCat)
                    .HasColumnName("CHN_CAt")
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.ChnInfo)
                    .IsRequired()
                    .HasColumnName("CHN_Info")
                    .HasColumnType("ntext")
                    .HasComment("節目JSON資訊");

                entity.Property(e => e.ChnStatus)
                    .HasColumnName("CHN_Status")
                    .HasComment("狀態(準備廢棄)");

                entity.Property(e => e.UsrSn)
                    .HasColumnName("USR_SN")
                    .HasComment("User序號");

                entity.HasOne(d => d.UsrSnNavigation)
                    .WithMany(p => p.Channel)
                    .HasForeignKey(d => d.UsrSn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Channel_User");
            });

            modelBuilder.Entity<Flim>(entity =>
            {
                entity.HasKey(e => e.FlmSn);

                entity.Property(e => e.FlmSn)
                    .HasColumnName("FLM_SN")
                    .HasComment("影片序號");

                entity.Property(e => e.FlmCat)
                    .HasColumnName("FLM_CAt")
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.FlmFileName)
                    .IsRequired()
                    .HasColumnName("FLM_FileName")
                    .HasMaxLength(100)
                    .HasComment("下載檔名");

                entity.Property(e => e.FlmIsActive)
                    .HasColumnName("FLM_IsActive")
                    .HasComment("是否可用");

                entity.Property(e => e.FlmMat)
                    .HasColumnName("FLM_MAt")
                    .HasColumnType("datetime")
                    .HasComment("修改時間");

                entity.Property(e => e.FlmSha256)
                    .IsRequired()
                    .HasColumnName("FLM_Sha256")
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("SHA256雜湊碼(hex)");

                entity.Property(e => e.FlmStatus)
                    .HasColumnName("FLM_Status")
                    .HasComment("狀態(0未轉檔,1轉檔中,2轉檔失敗,3未下載,4已下載)");

                entity.Property(e => e.NtcSn)
                    .HasColumnName("NTC_SN")
                    .HasComment("音輯序號");

                entity.HasOne(d => d.NtcSnNavigation)
                    .WithMany(p => p.Flim)
                    .HasForeignKey(d => d.NtcSn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Flim_Netcast");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.ImgSn);

                entity.Property(e => e.ImgSn)
                    .HasColumnName("IMG_SN")
                    .HasComment("圖片序號");

                entity.Property(e => e.ImgExt)
                    .IsRequired()
                    .HasColumnName("IMG_Ext")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasComment("副檔名");

                entity.Property(e => e.ImgGuid16)
                    .IsRequired()
                    .HasColumnName("IMG_Guid16")
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Guid(檔名,長度16)");

                entity.Property(e => e.NtcSn)
                    .HasColumnName("NTC_SN")
                    .HasComment("音輯序號");

                entity.HasOne(d => d.NtcSnNavigation)
                    .WithMany(p => p.Image)
                    .HasForeignKey(d => d.NtcSn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Image_Netcast");
            });

            modelBuilder.Entity<Netcast>(entity =>
            {
                entity.HasKey(e => e.NtcSn);

                entity.Property(e => e.NtcSn)
                    .HasColumnName("NTC_SN")
                    .HasComment("音輯序號");

                entity.Property(e => e.ChnSn)
                    .HasColumnName("CHN_SN")
                    .HasComment("節目序號");

                entity.Property(e => e.NtcGuid)
                    .IsRequired()
                    .HasColumnName("NTC_Guid")
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasComment("Guid");

                entity.Property(e => e.NtcInfo)
                    .IsRequired()
                    .HasColumnName("NTC_Info")
                    .HasColumnType("ntext")
                    .HasComment("音輯JSON資訊");

                entity.HasOne(d => d.ChnSnNavigation)
                    .WithMany(p => p.Netcast)
                    .HasForeignKey(d => d.ChnSn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Netcast_Channel");
            });

            modelBuilder.Entity<SendMail>(entity =>
            {
                entity.HasKey(e => e.SmlSn)
                    .HasName("PK__SendMail__FAA9C38AF66B34EE");

                entity.Property(e => e.SmlSn)
                    .HasColumnName("SML_SN")
                    .HasComment("PKEY");

                entity.Property(e => e.SmlBody)
                    .IsRequired()
                    .HasColumnName("SML_Body")
                    .HasComment("信件內容");

                entity.Property(e => e.SmlCat)
                    .HasColumnName("SML_CAt")
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.SmlSendAt)
                    .HasColumnName("SML_SendAt")
                    .HasColumnType("datetime")
                    .HasComment("發送郵件時間");

                entity.Property(e => e.SmlSubject)
                    .IsRequired()
                    .HasColumnName("SML_Subject")
                    .HasMaxLength(50)
                    .HasComment("信件標題");

                entity.Property(e => e.SmlTo)
                    .IsRequired()
                    .HasColumnName("SML_To")
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasComment("收件人");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UsrSn);

                entity.Property(e => e.UsrSn)
                    .HasColumnName("USR_SN")
                    .HasComment("User序號");

                entity.Property(e => e.UsrCat)
                    .HasColumnName("USR_CAt")
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.UsrCuser)
                    .IsRequired()
                    .HasColumnName("USR_CUser")
                    .HasMaxLength(100)
                    .HasComment("建立者");

                entity.Property(e => e.UsrEmail)
                    .IsRequired()
                    .HasColumnName("USR_Email")
                    .HasMaxLength(100)
                    .HasComment("Email");

                entity.Property(e => e.UsrGuid)
                    .HasColumnName("USR_Guid")
                    .HasComment("Guid");

                entity.Property(e => e.UsrIsActive)
                    .HasColumnName("USR_IsActive")
                    .HasComment("是否可用");

                entity.Property(e => e.UsrMat)
                    .HasColumnName("USR_MAt")
                    .HasColumnType("datetime")
                    .HasComment("修改時間");

                entity.Property(e => e.UsrMuser)
                    .IsRequired()
                    .HasColumnName("USR_MUser")
                    .HasMaxLength(100)
                    .HasComment("修改者");
            });

            modelBuilder.Entity<UserPassword>(entity =>
            {
                entity.HasKey(e => e.UsrSn);

                entity.Property(e => e.UsrSn)
                    .HasColumnName("USR_SN")
                    .HasComment("User序號")
                    .ValueGeneratedNever();

                entity.Property(e => e.UpwCat)
                    .HasColumnName("UPW_CAt")
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.UpwPwd)
                    .IsRequired()
                    .HasColumnName("UPW_PWD")
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("密碼");

                entity.HasOne(d => d.UsrSnNavigation)
                    .WithOne(p => p.UserPassword)
                    .HasForeignKey<UserPassword>(d => d.UsrSn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserPassword_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
