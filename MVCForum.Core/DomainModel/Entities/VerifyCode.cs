using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// ��֤�붨����
    /// </summary>
    public partial class VerifyCode : Entity
    {
        /// <summary>
        /// ����ʽ
        /// </summary>
        public VerifyCode()
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.Now;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// ����ʽ
        /// </summary>
        /// <param name="mobileNumber">�ֻ�����</param>
        /// <param name="verifyCodeStatus">��֤״̬</param>
        /// <param name="verifyNumber">��֤������</param>
        public VerifyCode(string mobileNumber, VerifyCodeStatus verifyCodeStatus, string verifyNumber)
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.Now;
            LastUpdate = DateTime.Now;
            MobileNumber = mobileNumber;
            Status = (int)verifyCodeStatus;
            VerifyNumber = verifyNumber;
            FailNumber = 0;
        }

        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// ��֤������ʱ��
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// �����ֻ�����
        /// </summary>
        public string MobileNumber { get; set; }
        /// <summary>
        /// ��֤������
        /// </summary>
        public string VerifyNumber { get; set; }
        /// <summary>
        /// ��֤״̬
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// ��֤ʧ�ܴ���������չ��
        /// </summary>
        public int FailNumber { get; set; }
        /// <summary>
        /// ��¼����ʱ��
        /// </summary>
        public DateTime LastUpdate { get; set; }
        /// <summary>
        /// ���ͷ�������
        /// </summary>
        public string ReturnMessage { get; set; }
    }

    /// <summary>
    /// ��֤��״̬ö��
    /// </summary>
    public enum VerifyCodeStatus
    {
        /// <summary>
        /// �ȴ���֤
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// ��֤�ɹ�
        /// </summary>
        VerifySuccess = 1,
        /// <summary>
        /// ��֤ʱ�䴰���ѹ���δ��֤
        /// </summary>
        Invalid = 2,
    }
}