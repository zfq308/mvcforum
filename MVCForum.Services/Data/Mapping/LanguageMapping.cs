using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class LanguageMapping : EntityTypeConfiguration<Language>
    {
        public LanguageMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(100);
            Property(x => x.LanguageCulture).IsRequired().HasMaxLength(20);
            Property(x => x.FlagImageFileName).IsOptional().HasMaxLength(50);
            Property(x => x.RightToLeft).IsRequired();

            HasMany(x => x.LocaleStringResources)
                .WithRequired(x => x.Language).Map(x => x.MapKey("Language_Id"))
                .WillCascadeOnDelete(false);
        }
    }

    public class LocaleStringResourceMapping : EntityTypeConfiguration<LocaleStringResource>
    {
        public LocaleStringResourceMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.ResourceValue).IsRequired().HasMaxLength(1000);
        }
    }

    public class LocaleResourceKeyMapping : EntityTypeConfiguration<LocaleResourceKey>
    {
        public LocaleResourceKeyMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Notes).IsOptional();
            Property(x => x.DateAdded).IsRequired();

            HasMany(x => x.LocaleStringResources).WithRequired(x => x.LocaleResourceKey)
                .Map(x => x.MapKey("LocaleResourceKey_Id"))
                .WillCascadeOnDelete(false);
        }
    }
}
