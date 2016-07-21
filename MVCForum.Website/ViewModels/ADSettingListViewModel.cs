using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Website.Application;
using MVCForum.Domain.Constants;

namespace MVCForum.Website.ViewModels
{
    public class ADSetting_List_ViewModel
    {
        public IList<ADSetting> ADSettings { get; set; }
    }

    public class ADSetting_CreateEdit_ViewModel
    {
        //[UIHint(AppConstants.EditorType), AllowHtml]
        //[StringLength(6000)]
        //public string Content { get; set; }

        [Display(Name = "广告类型")]
        public int ADType { get; set; }

        [Display(Name = "上传文件名")]
        public HttpPostedFileBase UploadFile { get; set; }

        [Required]
        [Display(Name = "上传图片文件")]
        public string UploadFileName { get; set; }

        [Required(ErrorMessage = "你是不是忘记填写链接地址了？")]
        [Display(Name = "广告链接地址")]
        [StringLength(500)]
        public string Link { get; set; }

        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }


    }


    public class PrivatePicture_CreateEdit_ViewModel
    {
        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }
      
        [HiddenInput]
        public Guid UserId { get; set; }

        [Display(Name = "上传文件名")]
        public HttpPostedFileBase UploadFile { get; set; }

        [Required]
        [Display(Name = "上传图片文件")]
        public string UploadFileName { get; set; }

        [Required]
        [Display(Name = "上传图片描述")]
        public string Description { get; set; }

    }

}