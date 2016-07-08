using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 验证码定义类
    /// </summary>
    public partial class VerifyCode : Entity
    {
        /// <summary>
        /// 建构式
        /// </summary>
        public VerifyCode()
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.Now;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// 建构式
        /// </summary>
        /// <param name="mobileNumber">手机号码</param>
        /// <param name="verifyCodeStatus">验证状态</param>
        /// <param name="verifyNumber">验证码内容</param>
        public VerifyCode(string mobileNumber, VerifyCodeStatus verifyCodeStatus, string verifyNumber)
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.Now;
            LastUpdate = DateTime.Now;
            MobileNumber = mobileNumber;
            Status = verifyCodeStatus;
            VerifyNumber = verifyNumber;
            FailNumber = 0;
        }

        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 验证码生成时间
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// 所属手机号码
        /// </summary>
        public string MobileNumber { get; set; }
        /// <summary>
        /// 验证码内容
        /// </summary>
        public string VerifyNumber { get; set; }
        /// <summary>
        /// 验证状态
        /// </summary>
        public VerifyCodeStatus Status { get; set; }
        /// <summary>
        /// 验证失败次数（待扩展）
        /// </summary>
        public int FailNumber { get; set; }
        /// <summary>
        /// 记录更新时间
        /// </summary>
        public DateTime LastUpdate { get; set; }
        /// <summary>
        /// 发送返回数据
        /// </summary>
        public string ReturnMessage { get; set; }
    }

    /// <summary>
    /// 验证码状态枚举
    /// </summary>
    public enum VerifyCodeStatus
    {
        /// <summary>
        /// 等待验证
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// 验证成功
        /// </summary>
        VerifySuccess = 1,
        /// <summary>
        /// 验证时间窗口已过，未验证
        /// </summary>
        Invalid = 2,
    }
}