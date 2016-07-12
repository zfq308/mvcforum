using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class MembershipUserPointsMapping : EntityTypeConfiguration<MembershipUserPoints>
    {
        public MembershipUserPointsMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Points).IsRequired();
            Property(x => x.DateAdded).IsRequired();
            Property(x => x.Notes).IsOptional().HasMaxLength(400);
            Property(x => x.PointsFor).HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipUserPoints_PointsFor", 1) { IsUnique = false }));
            Property(x => x.PointsForId).IsOptional().HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipUserPoints_PointsForId", 1) { IsUnique = false }));
        }
    }



    public class MembershipUserPictureMapping : EntityTypeConfiguration<MembershipUserPicture>
    {
        public MembershipUserPictureMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.UserId).IsRequired().HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipUserPicture_UserId", 1) { IsUnique = false }));
            Property(x => x.FileName).IsRequired().HasMaxLength(100);
            Property(x => x.OriginFileName).IsRequired().HasMaxLength(100);
            Property(x => x.Description).IsOptional().HasMaxLength(100);
            Property(x => x.UploadTime).IsRequired();
            Property(x => x.AuditStatus).IsRequired();
            Property(x => x.AuditComment).IsOptional().HasMaxLength(100);
            Property(x => x.AuditTime).IsOptional();
         
        }
    }
}
