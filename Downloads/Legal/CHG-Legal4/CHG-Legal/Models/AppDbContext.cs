using CHG_Legal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CHG_Legal.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Existing DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardType> BoardTypes { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<HospitalAttendee> HospitalAttendees { get; set; }
        public DbSet<BoardAttendee> BoardAttendees { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<BoardAttachment> BoardAttachments { get; set; }
        public DbSet<BoardDecision> BoardDecisions { get; set; }

        // Company-related DbSets
        public DbSet<Company> Companies { get; set; }
        public DbSet<Shareholder> Shareholders { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BoardSetting> BoardSettings { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<BankingGroup> BankingGroups { get; set; }
        public DbSet<BankingGroupMember> BankingGroupMembers { get; set; }
        public DbSet<NonBankingAuthorization> NonBankingAuthorizations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ShareValue> ShareValues { get; set; }
        public DbSet<Association> Associations { get; set; }
        public DbSet<AssociationPlace> AssociationPlaces { get; set; }

        // Attachment DbSets
        public DbSet<CompanyAttachment> CompanyAttachments { get; set; }
        public DbSet<ShareHolder_Attaches> ShareHolderAttaches { get; set; }
        public DbSet<BranchAttachment> BranchAttachments { get; set; }
        public DbSet<BoardMemberAttach> BoardMemberAttaches { get; set; }
        public DbSet<BankingAttachment> BankingAttachments { get; set; }
        public DbSet<NonBankingAttach> NonBankingAttaches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== EXISTING RELATIONSHIPS (لم يتم التعديل عليها) ==========

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRole)
                .HasForeignKey(ur => ur.RoleID);

            modelBuilder.Entity<HospitalAttendee>()
                .HasOne(ha => ha.Hospital)
                .WithMany(h => h.HospitalAttendees)
                .HasForeignKey(ha => ha.Hospital_ID);

            modelBuilder.Entity<HospitalAttendee>()
                .HasOne(ha => ha.Attendee)
                .WithMany(a => a.HospitalAttendees)
                .HasForeignKey(ha => ha.Attendee_ID);

            modelBuilder.Entity<BoardAttendee>()
                .HasOne(ba => ba.Board)
                .WithMany(b => b.BoardAttendees)
                .HasForeignKey(ba => ba.BoardID);

            modelBuilder.Entity<BoardAttendee>()
                .HasOne(ba => ba.Attendee)
                .WithMany(a => a.BoardAttendees)
                .HasForeignKey(ba => ba.Attendee_ID);

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.Board)
                .WithMany(b => b.Approvals)
                .HasForeignKey(a => a.BoardApprovalID);

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.Attendee)
                .WithMany(at => at.Approvals)
                .HasForeignKey(a => a.Attendee_ID);

            modelBuilder.Entity<BoardAttachment>()
                .HasOne(ba => ba.Board)
                .WithMany(b => b.BoardAttachments)
                .HasForeignKey(ba => ba.Board_id);

            modelBuilder.Entity<BoardAttachment>()
                .HasOne(ba => ba.User)
                .WithMany(u => u.BoardAttachments)
                .HasForeignKey(ba => ba.user_id);

            modelBuilder.Entity<BoardDecision>()
                .HasOne(bd => bd.Board)
                .WithMany(b => b.BoardDecisions)
                .HasForeignKey(bd => bd.Board_ID);

            // ========== COMPANY RELATIONSHIPS ==========

            modelBuilder.Entity<Shareholder>()
                .HasOne(s => s.Company)
                .WithMany(c => c.Shareholders)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BoardSetting>()
                .HasOne(bs => bs.Company)
                .WithMany(c => c.BoardSettings)
                .HasForeignKey(bs => bs.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.BoardSetting)
                .WithMany(bs => bs.BoardMembers)
                .HasForeignKey(bm => bm.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BankingGroup>()
                .HasOne(bg => bg.Company)
                .WithMany(c => c.BankingGroups)
                .HasForeignKey(bg => bg.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BankingGroupMember>()
                .HasOne(bgm => bgm.BankingGroup)
                .WithMany(bg => bg.BankingGroupMembers)
                .HasForeignKey(bgm => bgm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NonBankingAuthorization>()
                .HasOne(nba => nba.Company)
                .WithMany(c => c.NonBankingAuthorizations)
                .HasForeignKey(nba => nba.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssociationPlace>()
                .HasOne(ap => ap.Association)
                .WithMany(a => a.AssociationPlaces)
                .HasForeignKey(ap => ap.associationID)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== ATTACHMENTS CONFIGURATION ==========

            // Company Attachments
            modelBuilder.Entity<CompanyAttachment>(entity =>
            {
                entity.ToTable("CompanyAttachments", "Company");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.CompanyAttachments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Shareholder Attachments (في Schema dbo)
            modelBuilder.Entity<ShareHolder_Attaches>(entity =>
            {
                entity.ToTable("ShareHolder_Attaches", "dbo");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.Shareholder)
                    .WithMany()
                    .HasForeignKey(e => e.ShareholderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Branch Attachments
            modelBuilder.Entity<BranchAttachment>(entity =>
            {
                entity.ToTable("BranchAttachments", "Company");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.Branch)
                    .WithMany(b => b.BranchAttachments)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.BranchAttachments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Board Member Attachments
            modelBuilder.Entity<BoardMemberAttach>(entity =>
            {
                entity.ToTable("BoardMemberAttaches", "Company");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.BoardMember)
                    .WithMany()
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Banking Attachments
            modelBuilder.Entity<BankingAttachment>(entity =>
            {
                entity.ToTable("BankingAttachments", "Company");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.BankingAttachments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Non-Banking Attachments
            modelBuilder.Entity<NonBankingAttach>(entity =>
            {
                entity.ToTable("NonBankingAttaches", "Company");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ID).UseIdentityColumn();

                entity.HasOne(e => e.NonBankingAuthorization)
                    .WithMany(n => n.NonBankingAttachments)
                    .HasForeignKey(e => e.AuthorizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.NonBankingAttachments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}