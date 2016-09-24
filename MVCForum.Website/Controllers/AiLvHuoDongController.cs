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

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController : BaseController
    {
        #region 成员变量
        JsApiPay jsApiPay = new JsApiPay();

        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        #region 微信支付

        /**
 * 
 * 网页授权获取用户基本信息的全部过程
 * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
 * 第一步：利用url跳转获取code
 * 第二步：利用code去获取openid和access_token
 * 
 */
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

                //string redirect_uri = HttpUtility.UrlEncode("http://gzh.lmx.ren");
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);

                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
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

        /// <summary>
        /// 支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MeterRecharge()
        {
            object objResult = "";
            string strTotal_fee = Request.Form["totalfee"];
            string DetailsIdstr = Request.Form["DetailsId"];
            Guid detailsId = Guid.Parse(DetailsIdstr);

            ActivityRegister ar = _ActivityRegisterService.Get(detailsId);
            if (ar != null)
            {
                string strFee = (double.Parse(strTotal_fee) * 100).ToString();
                jsApiPay.total_fee = int.Parse(strFee);

                if (jsApiPay.total_fee != ar.FeeNumber)
                {
                    base.LoggingService.Error("钱不相等。" + jsApiPay.total_fee.ToString() + ":" + ar.FeeNumber.ToString());
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
                    return Json(objResult);
                }

                //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
                jsApiPay.openid = Session["openid"].ToString();

                try
                {
                    #region JSAPI支付预处理

                    string strBody = "爱驴网微信支付:" + _aiLvHuoDongService.Get(ar.Id).MingCheng + ":" +
                        _MembershipService.GetUser(ar.UserId).UserName;//商品描述

                    WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(strBody);
                    WxPayData wxJsApiParam = jsApiPay.GetJsApiParameters();//获取H5调起JS API参数
                    var aOrder = new ActivityRegisterForOrder()
                    {
                        appId = wxJsApiParam.GetValue("appId").ToString(),
                        nonceStr = wxJsApiParam.GetValue("nonceStr").ToString(),
                        packageValue = wxJsApiParam.GetValue("package").ToString(),
                        paySign = wxJsApiParam.GetValue("paySign").ToString(),
                        timeStamp = wxJsApiParam.GetValue("timeStamp").ToString(),
                        msg = "成功下单,正在接入微信支付."
                    };
                    objResult = aOrder;
             
                    #endregion
                }
                catch (Exception ex)
                {
                    var aOrder = new ActivityRegisterForOrder()
                    {
                        appId = "",
                        nonceStr = "",
                        packageValue = "",
                        paySign = "",
                        timeStamp = "",
                        msg = "下单失败，请重试,多次失败,请联系管理员.具体错误信息：" + ex.Message
                    };
                    objResult = aOrder;
                }
               
                return Json(objResult); 
            }
            else
            {
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
                return Json(objResult);
            }
        }

        #endregion

        public ActionResult Index()
        {
            return View();
        }

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
                        item.ZhuangTai = ViewModel.ZhuangTai;
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
                        //_ActivityRegisterService.Add(ar);
                        EntityOperationUtils.InsertObject(ar);
                        break;
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredTheActivity:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您已提交了这个活动，请勿重复报名。",
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

                    if (ar.FeeNumber > 0)
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = ar.FeeNumber > 0 ? "活动已报名。请支付报名费用" : "活动已报名。",
                            MessageType = GenericMessages.info
                        };

                        //因微信校验目录的原因，此处不能在URL中传参，改用Session传值
                        Session["WechatPayDetailsId"] = ar.DetailsId;
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

        [Authorize]
        [HttpPost]
        public ActionResult ExportHuoDongUsers(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                List<MembershipUser> customers = null;
                var collection = _ActivityRegisterService.GetActivityRegisterListByHongDongId(id).ToList();
                if (collection != null && collection.Count > 0)
                {
                    customers = new List<MembershipUser>();
                    foreach (var item in collection)
                    {
                        MembershipUser user = _MembershipService.GetUser(item.UserId);
                        if (!customers.Contains(user))
                        {
                            customers.Add(user);
                        }
                    }
                }

                if (this.HttpContext.IsMobileDevice())
                {
                    var csvcontent = MembershipService.ToCsv(customers, UserIsAdmin);
                    return Content(csvcontent);
                }
                else
                {
                    var csvcontent = MembershipService.ToCsv(customers, UserIsAdmin);
                    byte[] fileContents = Encoding.UTF8.GetBytes(MembershipService.RemoveHTMLToCSV(csvcontent));
                    return File(fileContents, "application/vnd.ms-excel", "MVCForumUsers.csv");
                }
            }
        }

        [HttpGet]
        public ActionResult WechatPay()
        {
            if (Session["WechatPayDetailsId"]==null)
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
                    #region 调用微信接口

                    if (Session["openid"] == null)
                    {
                        try
                        {
                            //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                            GetOpenidAndAccessToken();
                        }
                        catch (Exception ex)
                        {
                            Response.Write(ex.ToString());
                            throw;
                        }
                    }

                    #endregion
                }

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
                base.LoggingService.Error("Enter  WechatPay5");
                return View();
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
            return View(JiluList);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateAiLvHuodongJilu(Guid Id)
        {
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

            var checkexistHuodongJilu = _topicService.GetAllTopicsByCategory(category.Id).Where(x => x.Name == model.Name).Count();
            if (checkexistHuodongJilu == 0)
            {
                //Benjamin, 以POST方式主动提交Action
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
            return View(ziXunList);
        }

        #endregion

        #region 爱驴服务模块

        public ActionResult ZuiXinFuWu()
        {
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
            return View(fuwuList);
        }

        #endregion

        /// <summary>
        /// 每日之星
        /// </summary>
        /// <returns></returns>
        public ActionResult MeiRiZhiXing()
        {
            MeiRiZhiXing_ListViewModel model = new MeiRiZhiXing_ListViewModel();
            model.MeiRiZhiXingUserList = _membershipTodayStarService.LoadAllAvailidUsers();
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
        /// 我的
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult AiLvZhangHu()
        {
            AiLvZhangHu_ViewModel model = new AiLvZhangHu_ViewModel();
            model.User = LoggedOnReadOnlyUser;
            model.UnReadPrivateMessageGroupCount = _PrivatemessageService.NewPrivateMessageCount(LoggedOnReadOnlyUser.Id);

            return View(model);
        }


    }
}