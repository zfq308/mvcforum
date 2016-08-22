using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using System.Data.Entity;

namespace MVCForum.Services
{

    public partial class MembershipUserPictureService : IMembershipUserPictureService
    {
        #region 建构式

        private readonly MVCForumContext _context;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly ILoggingService _loggingService;

        public MembershipUserPictureService(IMVCForumContext context, ILoggingService loggingService, IUploadedFileService uploadedFileService)
        {
            _context = context as MVCForumContext;
            _uploadedFileService = uploadedFileService;
            _loggingService = loggingService;
        }

        #endregion

        public MembershipUserPicture Add(MembershipUserPicture newpic)
        {
            return _context.MembershipUserPicture.Add(newpic);
        }

        public void AuditMembershipUserPicture(Guid PictureOwnerId, string AuditComment, Enum_UploadPictureAuditStatus Status)
        {
            var collection = _context.MembershipUserPicture.Where(x => x.UserId == PictureOwnerId).ToList();
            foreach (var item in collection)
            {
                item.AuditComment = AuditComment;
                item.AuditTime = DateTime.Now;
                item.AuditStatus = Status;
            }
            _context.SaveChanges();

        }

        public void AuditMembershipUserPicture(MembershipUser PictureOwner, string AuditComment, Enum_UploadPictureAuditStatus Status)
        {
            if (PictureOwner != null)
            {
                AuditMembershipUserPicture(PictureOwner.Id, AuditComment, Status);
            }
        }

        public void AuditMembershipUserPicture(MembershipUserPicture PictureInstance, string AuditComment, Enum_UploadPictureAuditStatus Status)
        {
            if (PictureInstance != null)
            {
                PictureInstance.AuditComment = AuditComment;
                PictureInstance.AuditTime = DateTime.Now;
                PictureInstance.AuditStatus = Status;
            }
        }

        public void Delete(MembershipUserPicture picture)
        {
            _context.MembershipUserPicture.Remove(picture);
        }

        public MembershipUserPicture GetMembershipUserPicture(Guid id)
        {
            return _context.MembershipUserPicture.FirstOrDefault(x => x.Id == id);
        }

        public IList<MembershipUserPicture> GetMembershipUserPictureListByUserId(Guid Id)
        {
            return _context.MembershipUserPicture.Where(x => x.UserId == Id).ToList();
        }
    }

}