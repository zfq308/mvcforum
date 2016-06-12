﻿using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 分类定义实体
    /// </summary>
    public partial class Category : Entity
    {
        /// <summary>
        /// 分类定义建构式
        /// </summary>
        public Category()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// 分类定义的流水Id 
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 分类定义的名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 分类的类别
        /// </summary>
        public string CategoryType { get; set; }

        /// <summary>
        /// 此类别实例是否是系统使用的类别定义
        /// </summary>
        public bool IsSystemCategory { get; set; }
        /// <summary>
        /// 要显示此类型的条件阈值（暂未使用）
        /// </summary>
        public int ShowTheCategoryCondition { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否锁定此分类
        /// </summary>
        public bool IsLocked { get; set; }
        /// <summary>
        /// 排序顺序
        /// </summary>
        public int SortOrder { get; set; }
        /// <summary>
        /// 记录生成时间
        /// </summary>
        public DateTime DateCreated { get; set; }
        public string Slug { get; set; }
        public string PageTitle { get; set; }
        public string Path { get; set; }
        /// <summary>
        /// 元数据描述
        /// </summary>
        public string MetaDescription { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Colour { get; set; }
        public string Image { get; set; }
        /// <summary>
        /// 当前类别定义在类别节点中的层次数
        /// </summary>
        public int Level { get; set; }

        public bool? ModerateTopics { get; set; }
        public bool? ModeratePosts { get; set; }

        /// <summary>
        /// 所属的父类别
        /// </summary>
        public virtual Category ParentCategory { get; set; }

        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<CategoryNotification> CategoryNotifications { get; set; }
        public virtual IList<CategoryPermissionForRole> CategoryPermissionForRoles { get; set; }

        public string NiceUrl => UrlTypes.GenerateUrl(UrlType.Category, Slug);
    }

    /// <summary>
    /// Category的类型，此枚举用于扩展Category的功能
    /// </summary>
    public enum EnumCategoryType
    {
        PublicCategory = 1,
        PrivateCategory = 2,
    }

}
