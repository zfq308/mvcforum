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

    public class ADSettingListViewModel
    {
        public IList<ADSetting> ADSettings { get; set; }
    }

    public class CreateEditADViewModel
    {
        //[UIHint(AppConstants.EditorType), AllowHtml]
        //[StringLength(6000)]
        //public string Content { get; set; }

        [Display(Name = "�������")]
        public int ADType { get; set; }

        [Required]
        [Display(Name = "�ϴ��ļ���")]
        public HttpPostedFileBase ImageName { get; set; }

        [Required]
        [Display(Name = "���ӵ�ַ")]
        [StringLength(500)]
        public string Link { get; set; }

        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }

      
    }

}