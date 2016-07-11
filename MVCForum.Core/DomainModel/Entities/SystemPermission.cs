using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Permission : Entity
    {
        public Permission()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsGlobal { get; set; }
        public virtual IList<CategoryPermissionForRole> CategoryPermissionForRoles { get; set; }
        public virtual IList<GlobalPermissionForRole> GlobalPermissionForRoles { get; set; }
    }

    public partial class CategoryPermissionForRole : Entity
    {
        public CategoryPermissionForRole()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Permission Permission { get; set; }
        public virtual MembershipRole MembershipRole { get; set; }
        public virtual Category Category { get; set; }
        public bool IsTicked { get; set; }
    }

    public partial class GlobalPermissionForRole : Entity
    {
        public GlobalPermissionForRole()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Permission Permission { get; set; }
        public virtual MembershipRole MembershipRole { get; set; }
        public bool IsTicked { get; set; }
    }

    /// <summary>
    /// 用户角色实体定义类
    /// </summary>
    public partial class MembershipRole : Entity
    {
        public MembershipRole()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// 用户角色实体定义流水Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 特定用户角色实例对应的用户集合
        /// </summary>
        public virtual IList<MembershipUser> Users { get; set; }

        /// <summary>
        /// 系统设定类的实例
        /// </summary>
        public virtual Settings Settings { get; set; }

        // Category Permissions
        public virtual IList<CategoryPermissionForRole> CategoryPermissionForRoles { get; set; }

        // Global Permissions
        public virtual IList<GlobalPermissionForRole> GlobalPermissionForRole { get; set; }

        public virtual Dictionary<Guid, Dictionary<Guid, bool>> GetCategoryPermissionTable()
        {
            var permissionRows = new Dictionary<Guid, Dictionary<Guid, bool>>();

            foreach (var catPermissionForRole in CategoryPermissionForRoles)
            {
                if (!permissionRows.ContainsKey(catPermissionForRole.Category.Id))
                {
                    var permissionList = new Dictionary<Guid, bool>();

                    permissionRows.Add(catPermissionForRole.Category.Id, permissionList);
                }

                if (!permissionRows[catPermissionForRole.Category.Id].ContainsKey(catPermissionForRole.Permission.Id))
                {
                    permissionRows[catPermissionForRole.Category.Id].Add(catPermissionForRole.Permission.Id, catPermissionForRole.IsTicked);
                }
                else
                {
                    permissionRows[catPermissionForRole.Category.Id][catPermissionForRole.Permission.Id] = catPermissionForRole.IsTicked;
                }


            }
            return permissionRows;
        }

    }

    public class PermissionForRole
    {
        public Permission Permission { get; set; }
        public MembershipRole MembershipRole { get; set; }
        public Category Category { get; set; }
        public bool IsTicked { get; set; }
    }

}
