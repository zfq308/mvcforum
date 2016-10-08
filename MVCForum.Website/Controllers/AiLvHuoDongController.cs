using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Website.Application.ActionFilterAttributes;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Website.ViewModels.Mapping;
using MVCForum.Website.Application;
using System.Text;
using WxPayAPI;
using System.Web.Hosting;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using MVCForum.Domain.DomainModel.General;

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController : BaseController
    {
        #region 成员变量
        JsApiPay jsApiPay = new JsApiPay();

        log4net.ILog loggerForCoreActionLog = log4net.LogManager.GetLogger("CoreActionLog");
        log4net.ILog loggerForPerformanceLog = log4net.LogManager.GetLogger("PerformanceLog");

        private readonly IAiLvHuoDongService _aiLvHuoDongService;
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryservice;
        private readonly MVCForumContext _context;
        private readonly IActivityRegisterService _ActivityRegisterService;
        private readonly IMembershipService _MembershipService;
        private readonly IMembershipTodayStarService _membershipTodayStarService;
        private readonly IPrivateMessageService _PrivatemessageService;

        #endregion

        #region 建构式  
        public AiLvHuoDongController(
            IMVCForumContext context,
            ICategoryService Categoryservice,
            ITopicService TopicService,
            IAiLvHuoDongService aiLvHuoDongService,
            IActivityRegisterService ActivityRegisterService,
            IPrivateMessageService PrivateMessageService,

            ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            IMembershipTodayStarService membershipTodayStarService,
            ISettingsService settingsService)
             : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _aiLvHuoDongService = aiLvHuoDongService;
            _ActivityRegisterService = ActivityRegisterService;
            _topicService = TopicService;
            _categoryservice = Categoryservice;
            _MembershipService = membershipService;
            _membershipTodayStarService = membershipTodayStarService;
            _PrivatemessageService = PrivateMessageService;
            _context = context as MVCForumContext;
        }

        #endregion

        #region 爱驴活动模块

        #region 创建活动

        [Authorize]
        public ActionResult CreateAiLvHuoDong()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = LoggedOnReadOnlyUser?.Id ?? Guid.Empty;
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (UserIsAdmin || User.IsInRole(AppConstants.SupplierRoleName))
                {
                    //绑定页面控件
                    BindControlData();
                    //给出部分默认值
                    var model = new AiLvHuoDong_CreateEdit_ViewModel();
                    model.StartTime = DateTime.Today.AddDays(14).AddHours(8);
                    model.StopTime = DateTime.Today.AddDays(15);
                    model.BaoMingJieZhiTime = DateTime.Today.AddDays(10).AddHours(20);
                    model.LeiBie = Enum_HuoDongLeiBie.FreeRegister;

                    return View(model);
                }
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAiLvHuoDong(AiLvHuoDong_CreateEdit_ViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {

                    var item = new AiLvHuoDong();

                    #region 属性赋值
                    item.MingCheng = ViewModel.MingCheng;
                    item.LeiBie = ViewModel.LeiBie;
                    item.YaoQiu = ViewModel.YaoQiu;
                    item.StartTime = ViewModel.StartTime;
                    item.StopTime = ViewModel.StopTime;
                    item.BaoMingJieZhiTime = ViewModel.BaoMingJieZhiTime;
                    item.DiDian = ViewModel.DiDian;
                    item.LiuCheng = ViewModel.LiuCheng;
                    item.Feiyong = ViewModel.Feiyong;
                    item.FeiyongShuoMing = ViewModel.FeiyongShuoMing;
                    item.ZhuYiShiXiang = ViewModel.ZhuYiShiXiang;
                    item.YuGuRenShu = ViewModel.YuGuRenShu;
                    item.XingBieBiLi = ViewModel.XingBieBiLi;
                    item.YaoQingMa = string.IsNullOrEmpty(ViewModel.YaoQingMa) ? "" : ViewModel.YaoQingMa;
                    item.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                    item.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                    if (UserIsAdmin)
                    {
                        item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                        item.AuditComments = "";
                    }
                    else
                    {
                        item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                        item.AuditComments = "";
                    }
                    item.CreatedTime = DateTime.Now;

                    if (ViewModel.Files != null)
                    {
                        // Before we save anything, check the user already has an upload folder and if not create one
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, "Huodong"));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        // Loop through each file and get the file info and save to the users folder and Db
                        var file = ViewModel.Files[0];
                        if (file != null)
                        {
                            // If successful then upload the file
                            var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                            if (!uploadResult.UploadSuccessful)
                            {
                                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                {
                                    Message = uploadResult.ErrorMessage,
                                    MessageType = GenericMessages.danger
                                };
                                return View(ViewModel);
                            }

                            // Save avatar to user
                            item.Avatar = uploadResult.UploadedFileName;
                        }
                    }

                    // Set the users Avatar for the confirmation page
                    ViewModel.Avatar = item.Avatar;

                    #endregion

                    try
                    {
                        EntityOperationUtils.InsertObject(item);
                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "活动已创建。",
                            MessageType = GenericMessages.info
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                    return View(ViewModel);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(ViewModel);
            }
        }

        #endregion

        #region 编辑活动

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        [ValidateAntiForgeryToken]
        public ActionResult EditAiLvHuoDong(AiLvHuoDong_CreateEdit_ViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var item = _aiLvHuoDongService.Get(ViewModel.Id);
                    if (item != null)
                    {
                        item.MingCheng = ViewModel.MingCheng;
                        item.LeiBie = ViewModel.LeiBie;
                        item.YaoQiu = ViewModel.YaoQiu;
                        item.StartTime = ViewModel.StartTime;
                        item.StopTime = ViewModel.StopTime;
                        item.BaoMingJieZhiTime = ViewModel.BaoMingJieZhiTime;
                        item.DiDian = ViewModel.DiDian;
                        item.LiuCheng = ViewModel.LiuCheng;
                        item.Feiyong = ViewModel.Feiyong;
                        item.FeiyongShuoMing = ViewModel.FeiyongShuoMing;
                        item.ZhuYiShiXiang = ViewModel.ZhuYiShiXiang;
                        item.YuGuRenShu = ViewModel.YuGuRenShu;
                        item.XingBieBiLi = ViewModel.XingBieBiLi;
                        item.YaoQingMa = (ViewModel.YaoQingMa == null ? "" : ViewModel.YaoQingMa);
                        //item.ZhuangTai = ViewModel.ZhuangTai;
                        item.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                        if (UserIsAdmin)
                        {
                            item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                            item.AuditComments = "";
                        }
                        else
                        {
                            item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                            item.AuditComments = "";
                        }
                        //EntityOperationUtils.ModifyObject(item);

                        _context.Entry(item).State = EntityState.Modified;
                    }



                    if (ViewModel.Files != null)
                    {
                        // Before we save anything, check the user already has an upload folder and if not create one
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, "Huodong"));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        // Loop through each file and get the file info and save to the users folder and Db
                        var file = ViewModel.Files[0];
                        if (file != null)
                        {
                            // If successful then upload the file
                            var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                            if (!uploadResult.UploadSuccessful)
                            {
                                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                {
                                    Message = uploadResult.ErrorMessage,
                                    MessageType = GenericMessages.danger
                                };
                                return View(ViewModel);
                            }

                            // Save avatar to user
                            item.Avatar = uploadResult.UploadedFileName;
                        }
                    }

                    // Set the users Avatar for the confirmation page
                    ViewModel.Avatar = item.Avatar;


                    try
                    {

                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "活动已更新。",
                            MessageType = GenericMessages.info
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                    return View(ViewModel);

                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(ViewModel);
            }
        }

        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAiLvHuoDong(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            var EditModel = new AiLvHuoDong_CreateEdit_ViewModel
            {
                Id = item.Id,
                MingCheng = item.MingCheng,
                LeiBie = item.LeiBie,
                YaoQiu = item.YaoQiu,
                StartTime = item.StartTime,
                StopTime = item.StopTime,
                BaoMingJieZhiTime = item.BaoMingJieZhiTime,
                DiDian = item.DiDian,
                LiuCheng = item.LiuCheng,
                Feiyong = item.Feiyong,
                FeiyongShuoMing = item.FeiyongShuoMing,
                ZhuYiShiXiang = item.ZhuYiShiXiang,
                YuGuRenShu = item.YuGuRenShu,
                XingBieBiLi = item.XingBieBiLi,
                YaoQingMa = item.YaoQingMa,
                ZhuangTai = item.ZhuangTai,
                ShenHeBiaoZhi = item.ShenHeBiaoZhi,
            };
            BindControlData();
            return View(EditModel);
        }

        #endregion

        #region 删除活动

        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult DeleteAiLvHuoDong(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.Delete(item);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Deleted;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }

        #endregion

        #region 活动审核

        [Authorize(Roles = "Admin")]
        [BasicMultiButton("btn_AuditSuccess")]
        public ActionResult AuditAiLvHuodong_Success(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.AuditAiLvHuodong(item, true);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }

        [Authorize(Roles = "Admin")]
        [BasicMultiButton("btn_AuditFail")]
        public ActionResult AuditAiLvHuodong_Fail(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.AuditAiLvHuodong(item, false);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }
        #endregion

        #region 查看/载入活动
        /// <summary>
        /// 活动List清单
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuoDong()
        {
            var HuoDongList = new AiLvHuoDong_ListViewModel
            {
                AiLvHuoDongList = _aiLvHuoDongService.GetAll().OrderByDescending(x => x.CreatedTime).ToList()
            };
            return View(HuoDongList);
        }

        public ActionResult ViewActivity(Guid Id)
        {
            if (Id == null || Id == Guid.Empty)
            {
                return View();
            }
            var item = _aiLvHuoDongService.Get(Id);

            if (item != null)
            {
                var EditModel = new AiLvHuoDong_CreateEdit_ViewModel
                {
                    Id = item.Id,
                    MingCheng = item.MingCheng,
                    LeiBie = item.LeiBie,
                    YaoQiu = item.YaoQiu,
                    StartTime = item.StartTime,
                    StopTime = item.StopTime,
                    BaoMingJieZhiTime = item.BaoMingJieZhiTime,
                    DiDian = item.DiDian,
                    LiuCheng = item.LiuCheng,
                    Feiyong = item.Feiyong,
                    FeiyongShuoMing = item.FeiyongShuoMing,
                    ZhuYiShiXiang = item.ZhuYiShiXiang,
                    YuGuRenShu = item.YuGuRenShu,
                    XingBieBiLi = item.XingBieBiLi,
                    YaoQingMa = "", //item.YaoQingMa,
                    ZhuangTai = item.ZhuangTai,
                    ShenHeBiaoZhi = item.ShenHeBiaoZhi,
                    GongYingShangUserId = item.GongYingShangUserId,
                };
                var userlist = _ActivityRegisterService.GetActivityRegisterListByHongDong(item);
                if (userlist != null && userlist.Count > 0)
                {
                    List<Guid> BoyUserId = new List<Guid>();
                    List<Guid> GirlUserId = new List<Guid>();
                    EditModel.BoyJoinner = new List<MembershipUser>();
                    EditModel.GirlJoiner = new List<MembershipUser>();
                    foreach (ActivityRegister ar in userlist)
                    {
                        if (ar.UserGender == Enum_Gender.boy)
                        {
                            if (!BoyUserId.Contains(ar.UserId))
                            {
                                BoyUserId.Add(ar.UserId);
                                MembershipUser user1 = _MembershipService.GetUser(ar.UserId);
                                if (user1 != null)
                                {
                                    EditModel.BoyJoinner.Add(user1);
                                }
                            }
                        }
                        else
                        {
                            if (!GirlUserId.Contains(ar.UserId))
                            {
                                GirlUserId.Add(ar.UserId);
                                MembershipUser user1 = _MembershipService.GetUser(ar.UserId);
                                if (user1 != null)
                                {
                                    EditModel.GirlJoiner.Add(user1);
                                }
                            }

                        }
                    }
                }
                return View(EditModel);
            }
            else
            {
                return View();
            }
        }

        #endregion

        private void BindControlData()
        {
            var Items_LeiBieList = new List<SelectListItem>();
            Items_LeiBieList.Add(new SelectListItem { Text = "自由报名", Value = "1" });
            Items_LeiBieList.Add(new SelectListItem { Text = "特殊邀请", Value = "2" });
            ViewData["LeiBieList"] = Items_LeiBieList;


            var Items_YaoQiuList = new List<SelectListItem>();
            Items_YaoQiuList.Add(new SelectListItem { Text = "单身人士", Value = "1" });
            Items_YaoQiuList.Add(new SelectListItem { Text = "邀请人员", Value = "2" });
            Items_YaoQiuList.Add(new SelectListItem { Text = "无要求", Value = "3" });
            ViewData["YaoQiuList"] = Items_YaoQiuList;
        }

        #endregion

        #region 活动报名
        /// <summary>
        /// 参与活动报名
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize()]
        [HttpPost]
        public ActionResult CreateActivityRegister(AiLvHuoDong_CreateEdit_ViewModel model)
        {
            string YaoQingMa = Request.Form["YaoQingMa"];
            string Idstr = Request.Form["Id"];

            #region 输入参数校验

            Guid Id = Guid.Parse(Idstr);
            var huodong = _aiLvHuoDongService.Get(Id);

            if (huodong == null)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "参数错误，请联系管理员。",
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
            }

            if (huodong.LeiBie == Enum_HuoDongLeiBie.SpecicalRegister)
            {
                if (huodong.YaoQingMa != YaoQingMa)
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "邀请码不正确。",
                        MessageType = GenericMessages.danger
                    };
                    return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                }
            }

            #endregion

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var ar = new ActivityRegister(Id, LoggedOnReadOnlyUser);
                ar.UserTelphone = LoggedOnReadOnlyUser.MobilePhone;
                ar.FeeSource = huodong.Id.ToString();
                ar.FeeId = "000000";
                ar.FeeNumber = huodong.Feiyong;

                #region 校验报名条件

                switch (_ActivityRegisterService.CheckRegisterStatus(huodong, LoggedOnReadOnlyUser))
                {
                    case Enum_VerifyActivityRegisterStatus.Success:
                        EntityOperationUtils.InsertObject(ar);
                        break;

                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredTheActivity:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您已报名了这个活动，请勿重复报名。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");

                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredNoPay:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您已报名了这个活动，请完成费用支付。",
                            MessageType = GenericMessages.danger
                        };
                        var arrecord = _ActivityRegisterService.Get(huodong, LoggedOnReadOnlyUser);
                        if (arrecord != null)
                        {
                            Session["WechatPayDetailsId"] = arrecord.DetailsId;
                            return RedirectToAction("WechatPay", "AiLvHuoDong");
                        }
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");  // 获取数据错误，跳至活动清单页
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredPaied:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您已报名并支付了这个活动的费用，请勿重复报名。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    case Enum_VerifyActivityRegisterStatus.Fail_BeyondDeadlineTime:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "已超过报名时限。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyMarriedStatus:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "此活动限未婚人士参加。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyUserGender:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "此活动同性别名额已满员。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");

                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyUserApproveStatus:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您的账号还未通过管理员审核，请等待管理员审核后再参加精彩的爱驴活动。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    default:
                        break;
                }

                #endregion

                try
                {
                    unitOfWork.Commit();

                    string log = LoggedOnReadOnlyUser.UserName + "已报名活动:" + huodong.MingCheng + ",活动的主键Id为：" + huodong.Id.ToString() + ".";
                    loggerForCoreActionLog.Info(log);

                    if (ar.FeeNumber > 0)
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = ar.FeeNumber > 0 ? "活动已报名。请支付报名费用" : "活动已报名。",
                            MessageType = GenericMessages.info
                        };

                        //因微信校验目录的原因，此处不能在URL中传参，改用Session传值
                        Session["WechatPayDetailsId"] = ar.DetailsId;
                        // 跳至微信支付页面进行支付
                        return RedirectToAction("WechatPay", "AiLvHuoDong");
                    }
                    else
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "活动已报名。",
                            MessageType = GenericMessages.info
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                return View();
            }
        }


        [HttpGet]
        public ActionResult WechatPay()
        {
            if (Session["WechatPayDetailsId"] == null)
            {
                return View();
            }

            var Idstr = Session["WechatPayDetailsId"].ToString();
            Guid Id = Guid.Parse(Idstr);

            var item = _ActivityRegisterService.Get(Id);
            if (item != null)
            {
                if (item.FeeNumber > 0)
                {
                    #region 调用微信"网页授权获取用户信息"接口

                    if (Session["openid"] == null)
                    {
                        try
                        {
                            //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                            GetOpenidAndAccessToken();
                        }
                        catch (Exception ex)
                        {
                            base.LoggingService.Error("调用【网页授权获取用户信息】接口失败，Details:" + ex.Message);
                            Response.Write(ex.ToString());
                            throw;
                        }
                    }

                    #endregion
                }

                // 授权成功，显示支付详细信息
                var Model = new WechatPay_Model
                {
                    activityregister = item,
                    User = _MembershipService.GetUser(item.UserId),
                    PayNumber = item.FeeNumber,
                    Huodong = _aiLvHuoDongService.Get(item.Id)
                };
                return View(Model);
            }
            else
            {
                base.LoggingService.Error("取得ActivityRegister实例失败，Id:" + Idstr);
                return View();
            }
        }

        /// <summary>
        /// 接收微信下单的返回结果
        /// </summary>
        /// <returns></returns>
        public ActionResult ResultNotifyPage()
        {
            var data = GetNotifyData();

            if (data.GetValue("result_code").ToString() != "SUCCESS")
            {
                LoggingService.Error("微信支付result_code返回不为SUCCESS.");
                return View();
            }

            string mId = data.GetValue("attach").ToString();
            string mFeeId = data.GetValue("transaction_id").ToString();
            string PayCompletedTimeStr = data.GetValue("time_end").ToString();
            //LoggingService.Error("attach=" + mId);
            //LoggingService.Error("transaction_id=" + mFeeId);

            DateTime PayCompletedTime = DateTime.ParseExact(PayCompletedTimeStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            //LoggingService.Error("time_end=" + PayCompletedTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //string mId = "dbac362e-97ce-4c5c-994d-a6980042d5af";
            //string mFeeId = "4010212001201610076009743132";
            //DateTime PayCompletedTime = DateTime.ParseExact("20161007040338", "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            Guid DetailsId = Guid.Empty;
            if (Guid.TryParse(mId, out DetailsId))
            {
                if (DetailsId != Guid.Empty && !string.IsNullOrEmpty(mFeeId))
                {
                    using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                    {
                        #region 更新报名记录

                        try
                        {
                            _ActivityRegisterService.ConfirmPay(DetailsId, mFeeId, PayCompletedTime);
                            unitOfWork.Commit();
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = "已完成活动费用的支付。",
                                MessageType = GenericMessages.info
                            };
                            loggerForCoreActionLog.Info("已完成活动费用的支付.DetailsId=" + DetailsId.ToString() + ",微信单号为：" + mFeeId);
                            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));

                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = ex.Message,
                                MessageType = GenericMessages.danger
                            };
                            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                        }

                        #endregion
                    }
                }
            }
            //出错时处理
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "更新订单信息时返回的参数不符合要求，报名DetailsId=" + mId + ", 微信支付单号:" + mFeeId,
                MessageType = GenericMessages.danger
            };
            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
        }

        /// <summary>
        /// 解析微信JSAPI支付后的返回信息，返回的地址在微信Config类中定义
        /// </summary>
        /// <returns></returns>
        public WxPayData GetNotifyData()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            Log.Info(this.GetType().ToString(), "Receive data from WeChat B : " + builder.ToString());

            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }
            return data;
        }


        /// <summary>
        /// 导出参加活动的用户清单
        /// </summary>
        /// <param name="id">爱驴活动实例的Guid编号</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult ExportHuoDongUsers(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                List<ExportAiLvHuodongUser_Model> customers = null;


                var collection = _ActivityRegisterService.GetActivityRegisterListByHongDongId(id).ToList();
                if (collection != null && collection.Count > 0)
                {
                    customers = new List<ExportAiLvHuodongUser_Model>();
                    foreach (ActivityRegister item in collection)
                    {
                        MembershipUser user = _MembershipService.GetUser(item.UserId);

                        ExportAiLvHuodongUser_Model model = new ExportAiLvHuodongUser_Model();
                        model.activityregister = item;
                        model.User = user;


                        if (!customers.Contains(model))
                        {
                            customers.Add(model);
                        }
                    }
                }

                if (this.HttpContext.IsMobileDevice())
                {
                    var csvcontent = ToCsv(customers);
                    return Content(csvcontent);
                }
                else
                {
                    var csvcontent = ToCsv(customers);
                    byte[] fileContents = Encoding.UTF8.GetBytes(MembershipService.RemoveHTMLToCSV(csvcontent));
                    return File(fileContents, "application/vnd.ms-excel", "MVCForumUsers.csv");
                }
            }
        }

        private string ToCsv(List<ExportAiLvHuodongUser_Model> userlist)
        {

            var csv = new StringBuilder("");
            if (userlist != null && userlist.Count > 0)
            {
                csv.Append("<table border='1' bordercolor='#a0c6e5' style='border-collapse:collapse;'>");
                csv.Append("<tr>");
                csv.AppendLine("<td>账号</td><td>昵称</td><td>真实姓名</td><td>联系方式</td><td>性别</td><td>年龄</td><td>婚否</td><td>身高</td><td>体重</td><td>现居地</td><td>最后登录时间</td><td>审核标志位</td><td>会员状态</td><td>会员类别</td><td>缴费情况</td><td>微信单号</td>");
                csv.Append("</tr>");
                foreach (var ExportAiLvHuodongUser in userlist)
                {
                    if (ExportAiLvHuodongUser == null) continue;

                    var user = ExportAiLvHuodongUser.User;
                    var ar = ExportAiLvHuodongUser.activityregister;

                    var UserName = string.Empty;
                    var AliasName = string.Empty;
                    var RealName = string.Empty;
                    var MobilePhone = string.Empty;
                    var Gender = string.Empty;
                    var Age = string.Empty;
                    var IsMarried = string.Empty;
                    var Height = string.Empty;
                    var Weight = string.Empty;
                    var Location = string.Empty;
                    var IsApproved = string.Empty;
                    var IsBanned = string.Empty;
                    var UserType = string.Empty;
                    var LastLoginDate = "";
                    var PaidFlag = "";
                    var tran_id = ar.FeeId;

                    try
                    {
                        UserName = user.UserName;
                        AliasName = user.AliasName;
                        RealName = user.RealName;
                        MobilePhone = user.MobilePhone;
                        Gender = user.Gender == Enum_Gender.boy ? "男" : "女";
                        Age = user.Age.ToString();
                        IsMarried = user.IsMarried == Enum_MarriedStatus.Married ? "已婚" : "单身";
                        Height = user.Height.ToString();
                        Weight = user.Weight.ToString();
                        Location = string.Concat(TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName,
                                      TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName,
                                      TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName);
                        LastLoginDate = user.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss");
                        IsApproved = user.IsApproved ? "已审核" : "待审核";
                        IsBanned = user.IsBanned ? "用户已隐藏" : "正常状态";
                        UserType = user.UserType.ToString();

                        if(ar.FeeNumber>0)
                        {
                            PaidFlag = ar.FeeStatus == Enum_FeeStatus.PayedFee ? "已支付" : "待支付";
                        }
                        else
                        {
                            PaidFlag = "免费项目";
                        }
                       
                        csv.Append("<tr>");
                        csv.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td><td>{10}</td><td>{11}</td><td>{12}</td><td>{13}</td><td>{14}</td><td>{15}</td>",
                            UserName,
                            AliasName,
                            RealName,
                            MobilePhone,
                            Gender,
                            Age,
                            IsMarried,
                            Height,
                            Weight,
                            Location,
                            LastLoginDate,
                            IsApproved,
                            IsBanned,
                            UserType,
                            PaidFlag,
                            tran_id
                            );
                        csv.Append("</tr>");
                        csv.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        loggerForCoreActionLog.Error("导出活动用户清单失败。详细信息为：" + ex.Message);
                    }
                }
                csv.Append("</table>");
            }
            return csv.ToString();
        }


        #endregion

        #region 微信身份验证

        /// <summary>
        /// 网页授权获取用户基本信息的全部过程,详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        /// 第一步：利用url跳转获取code
        /// 第二步：利用code去获取openid和access_token
        /// </summary>
        public void GetOpenidAndAccessToken()
        {
            if (Session["code"] != null)
            {
                //获取code码，以获取openid和access_token
                string code = Session["code"].ToString();
                jsApiPay.GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                //构造网页授权获取code的URL
                string host = Request.Url.Host;
                string path = Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);

                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);

                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                //Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
                Session["url"] = url;
            }
        }

        /// <summary>
        /// 获取code
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getCode()
        {
            object objResult = "";
            if (Session["url"] != null)
            {
                objResult = Session["url"].ToString();
            }
            else
            {
                objResult = "url为空。";
            }
            return Json(objResult);
        }

        /// <summary>
        /// 通过code换取网页授权access_token和openid的返回数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getWxInfo()
        {
            object objResult = "";
            string strCode = Request.Form["code"];

            if (Session["access_token"] == null || Session["openid"] == null)
            {
                jsApiPay.GetOpenidAndAccessTokenFromCode(strCode);
            }

            string strAccess_Token = Session["access_token"].ToString();
            string strOpenid = Session["openid"].ToString();
            objResult = new { openid = strOpenid, access_token = strAccess_Token };
            return Json(objResult);
        }

        #endregion

        #region 微信支付

        /// <summary>
        /// 调用JSAPI支付预处理，完成微信支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MeterRecharge()
        {
            object objResult = "";
            string strTotal_fee = Request.Form["totalfee"];
            string DetailsIdstr = Request.Form["DetailsId"];

            Guid detailsId = Guid.Empty;
            if (Guid.TryParse(DetailsIdstr, out detailsId))
            {
                ActivityRegister ar = _ActivityRegisterService.Get(detailsId);
                if (ar != null)
                {
                    #region 支付金额校验

                    string strFee = (double.Parse(strTotal_fee) * 100).ToString(); // *100，转化金额从“分”到“元”
                    jsApiPay.total_fee = int.Parse(strFee);

                    if (jsApiPay.total_fee != ar.FeeNumber * 100)
                    {
                        #region 出错，钱不相等

                        var aOrder = new ActivityRegisterForOrder()
                        {
                            appId = "",
                            nonceStr = "",
                            packageValue = "",
                            paySign = "",
                            timeStamp = "",
                            msg = "支付信息不匹配,请联系管理员."
                        };
                        objResult = aOrder;
                        base.LoggingService.Error("钱不相等。" + jsApiPay.total_fee.ToString() + ":" + ar.FeeNumber.ToString());

                        return Json(objResult);

                        #endregion
                    }

                    #endregion

                    //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
                    jsApiPay.openid = Session["openid"].ToString();

                    try
                    {
                        #region JSAPI支付预处理

                        string strBody = "爱驴网微信支付:" + _aiLvHuoDongService.Get(ar.Id).MingCheng + ":" +
                            _MembershipService.GetUser(ar.UserId).UserName;//商品描述
                        string attachInfo = ar.DetailsId.ToString(); // 传入ActivityRegister的主键字段DetailsId
                        WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(strBody, attachInfo);
                        WxPayData wxJsApiParam = jsApiPay.GetJsApiParameters();//获取H5调起JS API参数
                        var aOrder = new ActivityRegisterForOrder()
                        {
                            appId = wxJsApiParam.GetValue("appId").ToString(),
                            nonceStr = wxJsApiParam.GetValue("nonceStr").ToString(),
                            packageValue = wxJsApiParam.GetValue("package").ToString(),
                            paySign = wxJsApiParam.GetValue("paySign").ToString(),
                            timeStamp = wxJsApiParam.GetValue("timeStamp").ToString(),
                            msg = "成功下单,正在接入微信支付.",
                            detailsId = ar.DetailsId.ToString(),
                        };
                        objResult = aOrder;


                        loggerForCoreActionLog.Info("微信支付下单完成。Body信息为：" + strBody + ",对应的ActivityRegister主键字段值为：" + attachInfo);

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        #region 下单失败

                        var aOrder = new ActivityRegisterForOrder()
                        {
                            appId = "",
                            nonceStr = "",
                            packageValue = "",
                            paySign = "",
                            timeStamp = "",
                            msg = "下单失败，请重试。若多次失败,请联系管理员.具体错误信息：" + ex.Message,
                            detailsId = ""
                        };
                        base.LoggingService.Error("微信支付下单失败，具体错误信息：" + ex.Message);
                        loggerForCoreActionLog.Error("微信支付下单失败，具体错误信息：" + ex.Message);
                        objResult = aOrder;

                        #endregion
                    }
                    return Json(objResult);
                }
                else
                {
                    #region 无法获取报名实例

                    var aOrder = new ActivityRegisterForOrder()
                    {
                        appId = "",
                        nonceStr = "",
                        packageValue = "",
                        paySign = "",
                        timeStamp = "",
                        msg = "未取得用户注册活动信息,请联系管理员.",
                        detailsId = ""
                    };
                    objResult = aOrder;
                    base.LoggingService.Error("未取得用户注册活动实例信息，detailsId：" + detailsId);
                    loggerForCoreActionLog.Error("未取得用户注册活动实例信息，detailsId：" + detailsId);

                    #endregion
                    return Json(objResult);
                }
            }
            else
            {
                #region 转换DetailsId参数错误
                var aOrder = new ActivityRegisterForOrder()
                {
                    appId = "",
                    nonceStr = "",
                    packageValue = "",
                    paySign = "",
                    timeStamp = "",
                    msg = "未取得用户注册活动信息,请联系管理员.",
                    detailsId = ""
                };
                objResult = aOrder;
                base.LoggingService.Error("转换DetailsId参数错误，detailsId：" + detailsId);
                loggerForCoreActionLog.Error("转换DetailsId参数错误，detailsId：" + detailsId);
                #endregion
                return Json(objResult);
            }
        }

        #endregion

        #region 爱驴记录模块

        /// <summary>
        /// 最新记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinJilu()
        {
            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            var JiluList = new AiLvJiLu_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvJiLu).OrderByDescending(x => x.CreateDate).ToList();
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvJiLu));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                JiluList.Topics.AddRange(topicViewModels);
            }

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            loggerForPerformanceLog.Info("Load ZuiXinJilu list, cost time:" + (t / 1000).ToString() + "second.");

            return View(JiluList);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateAiLvHuodongJilu(Guid Id)
        {
            #region 生成爱驴活动记录的实例

            var item = _aiLvHuoDongService.Get(Id);

            var model = new CreateEditTopicViewModel();

            var category = _categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvJiLu);
            model.CategoryId = category.Id;
            var allowedCategories = new List<Category>();
            allowedCategories.Add(category);
            model.Categories = _categoryservice.GetBaseSelectListCategories(allowedCategories);

            var permissions = RoleService.GetPermissions(null, UsersRole);
            var canInsertImages = UserIsAdmin;
            if (!canInsertImages)
            {
                canInsertImages = permissions[SiteConstants.Instance.PermissionInsertEditorImages].IsTicked;
            }

            model.OptionalPermissions = new CheckCreateTopicPermissions
            {
                CanLockTopic = UserIsAdmin,
                CanStickyTopic = UserIsAdmin,
                CanUploadFiles = UserIsAdmin,
                CanCreatePolls = UserIsAdmin,
                CanInsertImages = canInsertImages
            };
            model.PollAnswers = new List<PollAnswer>();
            model.IsTopicStarter = true;
            model.PollCloseAfterDays = 0;

            model.Name = "【" + item.MingCheng.Trim() + "】的活动记录";
            model.Content = "请更新" + model.Name;

            #endregion

            var checkexistHuodongJilu = _topicService.GetAllTopicsByCategory(category.Id).Where(x => x.Name == model.Name).Count();
            if (checkexistHuodongJilu == 0)
            {
                //以POST方式主动提交Action，来创建/保存活动记录
                TempData["model"] = model;
                return RedirectToAction("CreateNewTopicRecord", "Topic", new { model });
            }
            else
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "活动记录已存在。",
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
            }
        }

        #endregion

        #region 每日心情

        [Authorize]
        public ActionResult DailyRecord()
        {
            var JiluList = new MeiRiXinQing_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCondition(EnumCategoryType.MeiRiXinqing, LoggedOnReadOnlyUser);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.MeiRiXinqing));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                JiluList.Topics.AddRange(topicViewModels);
            }
            return View(JiluList);
        }

        #endregion

        #region 爱驴资讯模块

        public ActionResult ZuiXinZiXun()
        {
            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            var ziXunList = new AiLvZiXun_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvZiXun);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvZiXun));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                ziXunList.Topics.AddRange(topicViewModels);
            }

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            loggerForPerformanceLog.Info("Load ZuiXinZiXun list cost time:" + (t / 1000).ToString() + "second.");

            return View(ziXunList);
        }

        #endregion

        #region 爱驴服务模块

        public ActionResult ZuiXinFuWu()
        {
            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            var fuwuList = new AiLvFuWu_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvFuWu);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvFuWu));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                fuwuList.Topics.AddRange(topicViewModels);
            }

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            loggerForPerformanceLog.Info("Load ZuiXinFuWu List, cost time:" + (t / 1000).ToString() + "second.");

            return View(fuwuList);
        }

        #endregion

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 每日之星
        /// </summary>
        /// <returns></returns>
        public ActionResult MeiRiZhiXing()
        {
            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            MeiRiZhiXing_ListViewModel model = new MeiRiZhiXing_ListViewModel();
            model.MeiRiZhiXingUserList = _membershipTodayStarService.LoadAllAvailidUsers();

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            loggerForPerformanceLog.Info("Load MeiRiZhiXing list cost time:" + (t / 1000).ToString() + "second.");

            return View(model);

        }

        /// <summary>
        /// 最新会员
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuiYuan()
        {
            return View();
        }

        /// <summary>
        /// 我的账户
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult AiLvZhangHu()
        {
            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            AiLvZhangHu_ViewModel model = new AiLvZhangHu_ViewModel();
            model.User = LoggedOnReadOnlyUser;
            model.UnReadPrivateMessageGroupCount = _PrivatemessageService.NewPrivateMessageCount(LoggedOnReadOnlyUser.Id);
            MyStopWatch.Stop();

            decimal t = MyStopWatch.ElapsedMilliseconds;
            loggerForPerformanceLog.Info("Load AiLvZhangHu For " + LoggedOnReadOnlyUser.UserName + ", cost time:" + (t / 1000).ToString() + "second.");

            return View(model);
        }

    }
}