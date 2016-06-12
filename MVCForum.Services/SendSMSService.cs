using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using System.Text;
using System.IO;
using System.Net;
using MVCForum.Domain.Interfaces;
using System.Security.Cryptography;

namespace MVCForum.Services
{
    /// <summary>
    /// ��֮Ѷƽ̨����������
    /// </summary>
    public class UCPaasConfig
    {
        /// <summary>
        /// Account sid
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Auth Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// ��Ӧ��Ӧ��id���ǲ���Ӧ��������ʹ��,
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// ����ģ��id����ͨ�����
        /// </summary>
        public string TemplatedId { set; get; }

        private UCPaasConfig() { }
        /// <summary>
        /// ��֮Ѷƽ̨�����������ʽ
        /// </summary>
        /// <param name="account">Account sid</param>
        /// <param name="token">Auth Token</param>
        /// <param name="appId">��Ӧ��Ӧ��id</param>
        /// <param name="templatedId">����ģ��id</param>
        public UCPaasConfig(string account, string token, string appId, string templatedId)
        {
            Account = account;
            Token = token;
            AppId = appId;
            TemplatedId = templatedId;
        }
    }

    /// <summary>
    /// ��֮Ѷ���ŷ�����
    /// </summary>
    public partial class SendSMSService_UCPaas : ISendSMSService
    {

        private UCPaasConfig Config { get; set; }

        /// <summary>
        /// �����ý���ʽ
        /// </summary>
        public SendSMSService_UCPaas()
        {
            // http://www.ucpaas.com
            // zfq3082002@163.com
            // zfq3082002
            Config = new UCPaasConfig("37f0484a57f4d028368f81a6163782dc", "4bd6e0c6ffd9d5d1ebcae39e6f3b2a9d", "e75da0fa959944c0ad4caf6871565a67", "24472");
        }

        /// <summary>
        /// �����ý���ʽ
        /// </summary>
        /// <param name="ucPaasConfig">��֮Ѷ��������ʵ��</param>
        public SendSMSService_UCPaas(UCPaasConfig ucPaasConfig)
        {
            Config = ucPaasConfig;
        }

        /// <summary>
        /// ͨ����֮Ѷƽ̨��www.ucpaas.com������SMS����
        /// </summary>
        /// <param name="Content">��������</param>
        /// <param name="MobileNumber">�ֻ����룬Ⱥ����������</param>
        /// <returns>
        /// The Errorcode, please view the URL link:http://docs.ucpaas.com/doku.php?id=rest_error
        /// <example>
        /// {"resp":{"respCode":"000000","templateSMS":{"createDate":"20160523150423","smsId":"8430bf35eb18643294efe881e0fbb50f"}}}
        /// </example>
        /// </returns>
        public string Send(string Content, string MobileNumber)
        {
            //The template content: This is demo, verifycode is {1}.
            string param = string.Format("{0}", Content);        //���Ų���
            UCSRestRequest api = new UCSRestRequest();
            api.init("api.ucpaas.com", "443");
            api.setAccount(Config.Account, Config.Token);
            api.enabeLog(true);
            api.setAppId(Config.AppId);
            api.enabeLog(true);

            #region Unuse Code
            string clientNum = "78446052175972";                    //Client �����˺�
            string clientpwd = "198c1667";                          //Client ��������
            string friendName = "";
            string clientType = "0";
            string charge = "0";
            string phone = "";
            string date = "day";
            uint start = 0;
            uint limit = 100;
            string fromSerNum = "4000000000";
            string toSerNum = "4000000000";
            string maxallowtime = "60";

            //��ѯ���˺�
            //string MainAccountInfo = api.QueryAccountInfo();

            //����client�˺�
            //api.CreateClient(friendName, clientType, charge, phone);

            //��ѯ�˺���Ϣ(�˺�)
            //var s = api.QueryClientNumber(clientNum);

            //��ѯ�˺���Ϣ(�绰����)
            //api.QueryClientMobile(phone);

            //��ѯ�˺��б�
            //api.GetClient(start, limit);

            //ɾ��һ���˺�
            //api.DropClient(clientNum);

            //��ѯӦ�û���
            //api.GetBillList(date);

            //��ѯ�˺Ż���
            //api.GetClientBillList(clientNum, date);

            //�˺ų�ֵ
            //api.ChargeClient(clientNum, clientType, charge);

            //�ز�
            //api.CallBack(clientNum, MobileNumber, fromSerNum, toSerNum, maxallowtime);

            //������֤��
            //api.VoiceCode(toPhone, "1234");
            #endregion

            var result = api.SendSMS(MobileNumber, Config.TemplatedId, param);
            return result;
        }
    }

    #region UCPaas ��֮Ѷƽ̨���

    enum EBodyType : uint
    {
        EType_XML = 0,
        EType_JSON
    };

    public class UCSRestRequest
    {
        private string m_restAddress = null;
        private string m_restPort = null;
        private string m_mainAccount = null;
        private string m_mainToken = null;
        private string m_appId = null;
        private bool m_isWriteLog = false;

        private EBodyType m_bodyType = EBodyType.EType_JSON;

        /// <summary>
        /// ������api�汾
        /// </summary>
        const string softVer = "2014-06-30";

        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="serverIP">��������ַ</param>
        /// <param name="serverPort">�������˿�</param>
        /// <returns></returns>
        public bool init(string restAddress, string restPort)
        {
            this.m_restAddress = restAddress;
            this.m_restPort = restPort;

            if (m_restAddress == null || m_restAddress.Length < 0 || m_restPort == null || m_restPort.Length < 0 || Convert.ToInt32(m_restPort) < 0)
                return false;

            return true;
        }

        /// <summary>
        /// �������ʺ���Ϣ
        /// </summary>
        /// <param name="accountSid">���ʺ�</param>
        /// <param name="accountToken">���ʺ�����</param>
        public void setAccount(string accountSid, string accountToken)
        {
            this.m_mainAccount = accountSid;
            this.m_mainToken = accountToken;
        }

        /// <summary>
        /// ����Ӧ��ID
        /// </summary>
        /// <param name="appId">Ӧ��ID</param>
        public void setAppId(string appId)
        {
            this.m_appId = appId;
        }

        /// <summary>
        /// ��־����
        /// </summary>
        /// <param name="enable">��־����</param>
        public void enabeLog(bool enable)
        {
            this.m_isWriteLog = enable;
        }

        /// <summary>
        /// ��ȡ��־·��
        /// </summary>
        /// <returns>��־·��</returns>
        public string GetLogPath()
        {
            string dllpath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            dllpath = dllpath.Substring(8, dllpath.Length - 8);    // 8�� file:// �ĳ���
            return System.IO.Path.GetDirectoryName(dllpath) + "\\log.txt";
        }

        /// <summary>
        /// ���ʺ���Ϣ��ѯ
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string QueryAccountInfo()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);
                Uri address = new Uri(uriStr);

                WriteLog("QueryAccountInfo url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "GET";
                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("QueryAccountInfo responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
                // ��ȡ����
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// ����client�ʺ�
        /// </summary>
        /// <param name="friendlyName">client�ʺ����ơ�</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string CreateClient(string friendlyName, string clientType, string charge, string mobile)
        {

            if (friendlyName == null)
                throw new ArgumentNullException("friendlyName");

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Clients{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("CreateClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><client>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<friendlyName>").Append(friendlyName).Append("</friendlyName>");
                    data.Append("<clientType>").Append(clientType).Append("</clientType>");
                    data.Append("<charge>").Append(charge).Append("</charge>");
                    data.Append("<mobile>").Append(mobile).Append("</mobile>");
                    data.Append("</client>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"client\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"friendlyName\":\"").Append(friendlyName).Append("\"");
                    data.Append(",\"clientType\":\"").Append(clientType).Append("\"");
                    data.Append(",\"charge\":\"").Append(charge).Append("\"");
                    data.Append(",\"mobile\":\"").Append(mobile).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateClient requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateClient responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// �ͷ�client�ʺ�
        /// </summary>
        /// <param name="clientNum">client�ʺ�</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string DropClient(string clientNum)
        {

            if (clientNum == null)
                throw new ArgumentNullException("clientNum");

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/dropClient{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("DropClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><client>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<clientNumber>").Append(clientNum).Append("</clientNumber>");
                    data.Append("</client>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"client\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"clientNumber\":\"").Append(clientNum).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("DropClient requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("DropClient responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ��ȡӦ����client�ʺ�
        /// </summary>
        /// <param name="startNo">��ʼ����ţ�Ĭ�ϴ�0��ʼ</param>
        /// <param name="offset">һ�β�ѯ�������������С��1���������100��</param>
        /// <exception cref="ArgumentOutOfRangeException">����������Χ</exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public string GetClient(uint startNo, uint offset)
        {

            if (offset < 1 || offset > 100)
            {
                throw new ArgumentOutOfRangeException("offset������Χ");
            }
            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/clientList{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("GetClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><client>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<start>").Append(startNo).Append("</start>");
                    data.Append("<limit>").Append(offset).Append("</limit>");
                    data.Append("</client>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"client\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"start\":\"").Append(startNo).Append("\"");
                    data.Append(",\"clientNumber\":\"").Append(offset).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("GetClient requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("GetClient responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ��ѯclient�ʺ���Ϣ
        /// </summary>
        /// <param name="clientNum">client�ʺ�</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string QueryClientNumber(string clientNum)
        {
            if (clientNum == null)
                throw new ArgumentNullException("clientNum");

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Clients{4}?sig={5}&clientNumber={6}&appId={7}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr, clientNum, m_appId);

                Uri address = new Uri(uriStr);

                WriteLog("QueryClientNumber url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "GET";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("QueryClientNumber responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ��ѯclient��Ϣ(�����ֻ���)
        /// </summary>
        /// <param name="clientMobile">client�ʺŶ�Ӧ���ֻ���</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string QueryClientMobile(string clientMobile)
        {
            if (clientMobile == null)
                throw new ArgumentNullException("clientMobile");

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/ClientsByMobile{4}?sig={5}&mobile={6}&appId={7}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr, clientMobile, m_appId);

                Uri address = new Uri(uriStr);

                WriteLog("QueryClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "GET";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);

                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("QueryClientNumber responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Ӧ�û�������
        /// </summary>
        /// <param name="range">day ����ǰһ������ݣ���00:00 �C 23:59��;week����ǰһ�ܵ�����(��һ ������)��month��ʾ��һ���µ����ݣ��ϸ��±�ʾ��ǰ�¼�1�����������4��10�ţ����ѯ�����3�·ݵ����ݣ�</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public string GetBillList(string range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/billList{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("GetBillList url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><appBill>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<date>").Append(range).Append("</date>");
                    data.Append("</appBill>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"appBill\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"date\":\"").Append(range).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="clientNum"
        /// <param name="range">day ����ǰһ������ݣ���00:00 �C 23:59��;week����ǰһ�ܵ�����(��һ ������)��month��ʾ��һ���µ����ݣ��ϸ��±�ʾ��ǰ�¼�1�����������4��10�ţ����ѯ�����3�·ݵ����ݣ�</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public string GetClientBillList(string clientNum, string range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }
            if (clientNum == null)
            {
                throw new ArgumentNullException("clientNum");
            }
            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Clients/billList{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("CreateClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><clientBill>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<date>").Append(range).Append("</date>");
                    data.Append("<clientNumber>").Append(clientNum).Append("</clientNumber>");
                    data.Append("</clientBill>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"clientBill\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"date\":\"").Append(range).Append("\"");
                    data.Append(",\"clientNumber\":\"").Append(clientNum).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// Client��ֵ
        /// </summary>
        /// <param name="clientMobile">client�ʺ�</param>
        /// <param name="chargeType">0 ��ֵ��1 ����</param>
        /// <param name="charge">��ֵ����յĽ��</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string ChargeClient(string clientNum, string chargeType, string charge)
        {

            if (clientNum == null)
                throw new ArgumentNullException("clientNum");
            if (chargeType == null)
                throw new ArgumentNullException("chargeType");
            if (charge == null)
                throw new ArgumentNullException("charge");
            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/chargeClient{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("ChargeClient url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><client>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<clientNumber>").Append(clientNum).Append("</clientNumber>");
                    data.Append("<chargeType>").Append(chargeType).Append("</chargeType>");
                    data.Append("<charge>").Append(charge).Append("</charge>");
                    data.Append("</client>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"client\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"clientNumber\":\"").Append(clientNum).Append("\"");
                    data.Append(",\"chargeType\":\"").Append(chargeType).Append("\"");
                    data.Append(",\"charge\":\"").Append(charge).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("ChargeClient requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("ChargeClient responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ���Ͷ���
        /// </summary>
        /// <param name="to">���Ž��ն��ֻ�����</param>
        /// <param name="templateId">����ģ��ID</param>
        /// <param name="param">�������ݣ������滻ģ����{����}</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string SendSMS(string to, string templateId, string param)
        {

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            if (templateId == null)
            {
                throw new ArgumentNullException("templateId");
            }

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Messages/templateSMS{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("SendSMS url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><templateSMS>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<templateId>").Append(templateId).Append("</templateId>");
                    data.Append("<to>").Append(to).Append("</to>");
                    data.Append("<param>").Append(param).Append("</param>");
                    data.Append("</templateSMS>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"templateSMS\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"templateId\":\"").Append(templateId).Append("\"");
                    data.Append(",\"to\":\"").Append(to).Append("\"");
                    data.Append(",\"param\":\"").Append(param).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        /// <summary>
        /// ˫��غ�
        /// </summary>
        /// <param name="fromClient">���е绰</param>
        /// <param name="toPhone">���е绰</param>
        /// <param name="fromSerNum">���в���ʾ�ĺ��룬ֻ����ʾ400�����̻���</param>
        /// <param name="toSerNum">���в���ʾ�ĺ��롣����ʾ�ֻ����롢400�����̻���</param>
        /// <param name="maxallowtime">���в���ʾ�ĺ��롣����ʾ�ֻ����롢400�����̻���</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns>��������</returns>
        public string CallBack(string fromClient, string toPhone, string fromSerNum, string toSerNum, string maxallowtime)
        {

            if (fromClient == null)
            {
                throw new ArgumentNullException("fromClient");
            }

            if (toPhone == null)
            {
                throw new ArgumentNullException("toPhone");
            }

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Calls/callBack{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("CallBack url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><callback>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<fromClient>").Append(fromClient).Append("</fromClient>");
                    data.Append("<to>").Append(toPhone).Append("</to>");
                    data.Append("<fromSerNum>").Append(fromSerNum).Append("</fromSerNum>");
                    data.Append("<toSerNum>").Append(toSerNum).Append("</toSerNum>");
                    data.Append("<maxallowtime>").Append(maxallowtime).Append("</maxallowtime>");
                    data.Append("</callback>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"callback\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"fromClient\":\"").Append(fromClient).Append("\"");
                    data.Append(",\"to\":\"").Append(toPhone).Append("\"");
                    data.Append(",\"fromSerNum\":\"").Append(fromSerNum).Append("\"");
                    data.Append(",\"toSerNum\":\"").Append(toSerNum).Append("\"");
                    data.Append(",\"maxallowtime\":\"").Append(maxallowtime).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ������֤��
        /// </summary>
        /// <param name="to">���պ���</param>
        /// <param name="verifyCode">��֤�����ݣ�Ϊ����0~9������4-8λ</param>
        /// <exception cref="ArgumentNullException">��������Ϊ��</exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public string VoiceCode(string toPhone, string verifyCode)
        {

            if (toPhone == null)
            {
                throw new ArgumentNullException("toPhone");
            }

            if (verifyCode == null)
            {
                throw new ArgumentNullException("verifyCode");
            }

            try
            {
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ����URL����
                string sigstr = MD5Encrypt(m_mainAccount + m_mainToken + date);
                string uriStr;
                string xml = (m_bodyType == EBodyType.EType_XML ? ".xml" : "");
                uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/Calls/voiceCode{4}?sig={5}", m_restAddress, m_restPort, softVer, m_mainAccount, xml, sigstr);

                Uri address = new Uri(uriStr);

                WriteLog("VoiceCode url = " + uriStr);

                // ������������  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();

                // ����Head
                request.Method = "POST";

                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(m_mainAccount + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);


                // ����Body
                StringBuilder data = new StringBuilder();

                if (m_bodyType == EBodyType.EType_XML)
                {
                    request.Accept = "application/xml";
                    request.ContentType = "application/xml;charset=utf-8";

                    data.Append("<?xml version='1.0' encoding='utf-8'?><voiceCode>");
                    data.Append("<appId>").Append(m_appId).Append("</appId>");
                    data.Append("<verifyCode>").Append(verifyCode).Append("</verifyCode>");
                    data.Append("<to>").Append(toPhone).Append("</to>");
                    data.Append("</voiceCode>");
                }
                else
                {
                    request.Accept = "application/json";
                    request.ContentType = "application/json;charset=utf-8";

                    data.Append("{");
                    data.Append("\"voiceCode\":{");
                    data.Append("\"appId\":\"").Append(m_appId).Append("\"");
                    data.Append(",\"verifyCode\":\"").Append(verifyCode).Append("\"");
                    data.Append(",\"to\":\"").Append(toPhone).Append("\"");
                    data.Append("}}");
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                WriteLog("CreateSubAccount requestBody = " + data.ToString());

                // ��ʼ����
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // ��ȡ����
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseStr = reader.ReadToEnd();

                    WriteLog("CreateSubAccount responseBody = " + responseStr);

                    if (responseStr != null && responseStr.Length > 0)
                    {
                        return responseStr;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region MD5 �� https������������
        private void WriteLog(string log)
        {
            if (m_isWriteLog)
            {
                string strFilePath = GetLogPath();
                System.IO.FileStream fs = new System.IO.FileStream(strFilePath, System.IO.FileMode.Append);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.Default);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + log);
                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// MD5����
        /// </summary>
        /// <param name="source">ԭ����</param>
        /// <returns>���ܺ�����</returns>
        public static string MD5Encrypt(string source)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(source));

            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// ���÷�����֤����֤�ص�
        /// </summary>
        public void setCertificateValidationCallBack()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidationResult;
        }

        /// <summary>
        ///  ֤����֤�ص�����  
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cer"></param>
        /// <param name="chain"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool CertificateValidationResult(object obj, System.Security.Cryptography.X509Certificates.X509Certificate cer, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
        #endregion
    }

    class CreateToken
    {
        public string createToken(string mainAccount, string mianToken, string client, string clientPwd, string expireTime = null)
        {
            if (expireTime == null)
            {
                expireTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            string head;
            string body;

            StringBuilder headData = new StringBuilder();
            StringBuilder bodyData = new StringBuilder();

            headData.Append("{");
            headData.Append("\"Alg\":\"HS256\",\"Accid\":\"").Append(mainAccount).Append("\"");
            headData.Append(",\"Cnumber\":\"").Append(client).Append("\"");
            headData.Append(",\"Expiretime\":\"").Append(expireTime).Append("\"");
            headData.Append("}");

            bodyData.Append("{");
            bodyData.Append("\"Accid\":\"").Append(mainAccount).Append("\"");
            bodyData.Append(",\"AccToken\":\"").Append(mianToken).Append("\"");
            bodyData.Append(",\"Cnumber\":\"").Append(client).Append("\"");
            bodyData.Append(",\"Cpwd\":\"").Append(clientPwd).Append("\"");
            bodyData.Append(",\"Expiretime\":\"").Append(expireTime).Append("\"");
            bodyData.Append("}");

            head = headData.ToString();
            body = bodyData.ToString();

            Encoding myEncoding = Encoding.GetEncoding("utf-8");

            byte[] SHA256Byte = myEncoding.GetBytes(body);
            HMACSHA256 sha256 = new HMACSHA256(myEncoding.GetBytes(mianToken));
            byte[] bodyByte = sha256.ComputeHash(SHA256Byte);
            body = Convert.ToBase64String(bodyByte);

            byte[] headByte = myEncoding.GetBytes(head);
            head = Convert.ToBase64String(headByte);

            string token = head + "." + body;
            return token;
        }
    }

    #endregion

}