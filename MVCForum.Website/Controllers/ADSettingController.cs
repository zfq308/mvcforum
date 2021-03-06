﻿using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace MVCForum.Website.Controllers
{
    public class ADSettingController : BaseController
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IADSettingService _adSettingService;
        private readonly MVCForumContext _context;

        public ADSettingController(IMVCForumContext context, IADSettingService adSettingService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService) : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _adSettingService = adSettingService;
            _context = context as MVCForumContext;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Create()
        {
            return View(new ADSetting_CreateEdit_ViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ADSetting_CreateEdit_ViewModel adViewModel)
        {
            if (ModelState.IsValid)
            {
                var ad = new ADSetting();
                ad.ADType = (int)Enum_ADType.MainType;
                ad.Link = adViewModel.Link;
                ad.CreateTime = DateTime.Now;

                HttpPostedFileBase mUploadFile = Request.Files["files"];
                if (mUploadFile != null) adViewModel.UploadFile = mUploadFile;
                if (adViewModel.UploadFile != null)
                {
                    #region 准备上传路径
                    var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, "SysAD"));
                    //var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, LoggedOnReadOnlyUser.Id));
                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }
                    #endregion

                    var uploadResult = AppHelpers.UploadFile(adViewModel.UploadFile, uploadFolderPath, LocalizationService);
                    if (!uploadResult.UploadSuccessful)
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = uploadResult.ErrorMessage,
                            MessageType = GenericMessages.danger
                        };
                        return View(adViewModel);
                    }
                    ad.ImageName = adViewModel.UploadFileName;
                    ad.ImageSaveURL = uploadResult.UploadedFileUrl;

                    //// Add the filename to the database
                    //var uploadedFile = new UploadedFile
                    //{
                    //    Filename = uploadResult.UploadedFileName,
                    //    Post = topicPost,
                    //    MembershipUser = loggedOnUser
                    //};
                    //_uploadedFileService.Add(uploadedFile);

                    // Save the changes
                    _context.ADSetting.Add(ad);
                    _context.SaveChanges();
                }

                //try
                //{
                //    unitOfWork.Commit();
                //}
                //catch (Exception ex)
                //{
                //    unitOfWork.Rollback();
                //    LoggingService.Error(ex);
                //    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                //}
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "广告已创建。",
                    MessageType = GenericMessages.info
                };
                return RedirectToAction("ADSettingList", "ADSetting");

            }

            return View(adViewModel);
        }

        public ActionResult ADSettingList()
        {
            if (UserIsAdmin)
            {
                var ads = new ADSetting_List_ViewModel
                {
                    ADSettings = _adSettingService.GetAll()
                };
                return View(ads);
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }


        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAD(Guid Id)
        {
            var ad = _adSettingService.Get(Id);
            var adEdit = new ADSetting_CreateEdit_ViewModel
            {
                ADType = ad.ADType,
                Id = ad.Id,
                Link = ad.Link,
                UploadFileName = ad.ImageName,
            };
            return View(adEdit);
        }


        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult DeleteAD(Guid Id)
        {
            using (var profiler = MiniProfiler.Current.Step("删除广告窗（DeleteAD）操作"))
            {
                var ad = _adSettingService.Get(Id);
                _adSettingService.Delete(ad);
                _context.Entry<ADSetting>(ad).State = EntityState.Deleted;
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
            }
            return RedirectToAction("ADSettingList");
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAD(ADSetting_CreateEdit_ViewModel ad)
        {
            var existingAD = _adSettingService.Get(ad.Id);
            existingAD.ADType = ad.ADType;
            existingAD.Link = ad.Link;

            HttpPostedFileBase mUploadFile = Request.Files["files"];
            if (mUploadFile != null) ad.UploadFile = mUploadFile;
            if (ad.UploadFile != null)
            {
                var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, "SysAD"));
                //var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, LoggedOnReadOnlyUser.Id));
                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }
                var uploadResult = AppHelpers.UploadFile(ad.UploadFile, uploadFolderPath, LocalizationService);
                if (!uploadResult.UploadSuccessful)
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = uploadResult.ErrorMessage,
                        MessageType = GenericMessages.danger
                    };
                    return View(ad);
                }
                existingAD.ImageName = ad.UploadFileName;
                existingAD.ImageSaveURL = uploadResult.UploadedFileUrl;
            }

            _context.Entry<ADSetting>(existingAD).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
                throw new Exception("Error editing role");
            }

            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "AD 广告项保存完毕",
                MessageType = GenericMessages.success
            };

            return RedirectToAction("ADSettingList");
        }
    }
}