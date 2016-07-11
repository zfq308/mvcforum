using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Language : Entity
    {
        public Language()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LanguageCulture { get; set; }
        public string FlagImageFileName { get; set; }       
        public bool RightToLeft { get; set; }
        public virtual IList<LocaleStringResource> LocaleStringResources { get; set; }
    }

    public partial class LocaleStringResource : Entity
    {
        public LocaleStringResource()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual LocaleResourceKey LocaleResourceKey { get; set; }
        public string ResourceValue { get; set; }
        public virtual Language Language { get; set; }
    }

    public partial class LocaleResourceKey : Entity
    {
        public LocaleResourceKey()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public DateTime DateAdded { get; set; }
        public virtual IList<LocaleStringResource> LocaleStringResources { get; set; }
    }
}
