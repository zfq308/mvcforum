using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class PollMapping : EntityTypeConfiguration<Poll>
    {
        public PollMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.IsClosed).IsRequired();
            Property(x => x.DateCreated).IsRequired();
            Property(x => x.ClosePollAfterDays).IsOptional();

            HasMany(x => x.PollAnswers)
                .WithRequired(t => t.Poll)
                .Map(x => x.MapKey("Poll_Id"))
                .WillCascadeOnDelete(false);
        }
    }

    public class PollAnswerMapping : EntityTypeConfiguration<PollAnswer>
    {
        public PollAnswerMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Answer).IsRequired().HasMaxLength(600);
            HasMany(x => x.PollVotes)
                .WithRequired(t => t.PollAnswer)
                .Map(x => x.MapKey("PollAnswer_Id"))
                .WillCascadeOnDelete(false);
        }
    }

    public class PollVoteMapping : EntityTypeConfiguration<PollVote>
    {
        public PollVoteMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
        }
    }


}
