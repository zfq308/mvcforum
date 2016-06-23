using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace MVCForum.Website.Controllers
{
    public class ADSettingController : BaseController
    {
        private readonly IADSettingService _adSettingService;

        public ADSettingController(IADSettingService adSettingService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService) : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _adSettingService = adSettingService;
        }

        [Authorize]
        public ActionResult Create()
        {
            return View(new CreateEditADViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEditADViewModel adViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var ad = new ADSetting();
                    ad.ADType = (int)Enum_ADType.MainType;
                    ad.Link = adViewModel.Link;
                    ad.CreateTime = DateTime.Now;

                    if (adViewModel.ImageName != null)
                    {
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, LoggedOnReadOnlyUser.Id));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        var uploadResult = AppHelpers.UploadFile(adViewModel.ImageName, uploadFolderPath, LocalizationService);
                        if (!uploadResult.UploadSuccessful)
                        {
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = uploadResult.ErrorMessage,
                                MessageType = GenericMessages.danger
                            };
                            unitOfWork.Rollback();
                            return View(adViewModel);
                        }
                        ad.ImageOriginName = uploadResult.UploadedFileUrl;

                        //// Add the filename to the database
                        //var uploadedFile = new UploadedFile
                        //{
                        //    Filename = uploadResult.UploadedFileName,
                        //    Post = topicPost,
                        //    MembershipUser = loggedOnUser
                        //};
                        //_uploadedFileService.Add(uploadedFile);

                        // Save the changes
                        _adSettingService.Add(ad);
                        unitOfWork.SaveChanges();

                    }

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "广告已创建。",
                        MessageType = GenericMessages.info
                    };
                    return RedirectToAction("ADSettingList", "ADSetting");
                }
            }

            return View(adViewModel);
        }

        public ActionResult ADSettingList()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var roles = new ADSettingListViewModel
                {
                    ADSettings = _adSettingService.GetAll()
                };
                return View(roles);
            }
        }


        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAD(Guid Id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var ad = _adSettingService.Get(Id);
                return View(ad);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAD(ADSetting ad)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var existingAD = _adSettingService.Get(ad.Id);
                existingAD.ADType = ad.ADType;
                existingAD.Link = ad.Link;
                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception("Error editing role");
                }
            }

            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "AD 广告项保存完毕",
                MessageType = GenericMessages.success
            };

            return RedirectToAction("List");
        }




    }
}