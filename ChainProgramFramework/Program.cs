using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChainProgramFramework.Program;

namespace ChainProgramFramework
{
    public class Program
    {
        static void Main(string[] args)
        {
            RequestConfigManager.Config.SetGlobalRequestSize(10)
                .SetGlobalRequestProtocol(new RequestProtocol(RequestProtocol.InternalSoa))
                .SetGlobalRequestFormatJson()
                .SetGlobalRequestFormatXml()
                .SetGlobalRequestFormatBinary()
                .SetGlobalRequestSafetyCheckLogic(requestcontext =>requestcontext.Size < RequestConfigContext.configContext.Size);
        }
        /// <summary>
        /// 请求上下文
        /// </summary>
        public class RequestContext
        {
            /// <summary>
            /// 请求内容格式
            /// </summary>
            public string Format { get; set; }
            /// <summary>
            /// 请求大小
            /// </summary>
            public int Size { get; set; }
            /// <summary>
            /// 协议类型
            /// </summary>
            public RequestProtocol Protocol { get; set; }
            /// <summary>
            /// 请求内容
            /// </summary>
            public string Content { get; set; }
            /// <summary>
            /// 请求上下文的安全检查
            /// </summary>
            public Func<RequestContext,bool> SafetyChecks { get; set; }
        }

        /// <summary>
        /// 配置请求上下文对象
        /// </summary>
        public class RequestConfigContext
        {
            public static RequestConfigContext configContext = new RequestConfigContext();
            /// <summary>
            /// 请求内容格式
            /// </summary>
            public string Format { get; set; }
            /// <summary>
            /// 请求大小
            /// </summary>
            public int Size { get; set; }
            /// <summary>
            /// 协议类型
            /// </summary>
            public RequestProtocol Protocol { get; set; }
            /// <summary>
            /// 请求内容
            /// </summary>
            public string Content { get; set; }
            /// <summary>
            /// 请求上下文的安全检查
            /// </summary>
            public Func<RequestContext, bool> SafetyChecks { get; set; }
        }

        /// <summary>
        /// 上下文协议类型
        /// </summary>
        public class RequestProtocol
        {
            /// <summary>
            /// webservice协议
            /// </summary>
            public const string Soap = "Soap";

            /// <summary>
            /// 内部Soa协议
            /// </summary>
            public const string InternalSoa = "Soa";
            /// <summary>
            /// 目前协议类型
            /// </summary>
            private string _protocol;
            /// <summary>
            /// 使用指定的协议初始化协议对象
            /// </summary>
            /// <param name="protocol"></param>
            public RequestProtocol(string protocol)
            {
                this._protocol = protocol;
            }
        }
        /// <summary>
        /// 请求上下文配置管理
        /// </summary>
        public class RequestConfigManager
        {
            /// <summary>
            /// 使用单例模式的请求上下文对象
            /// </summary>
            public static RequestConfigManager Config = new RequestConfigManager();
            /// <summary>
            /// 设置请求上下文内容格式json
            /// </summary>
            /// <returns></returns>
            public RequestConfigManager SetGlobalRequestFormatJson()
            {
                if (string.IsNullOrEmpty(RequestConfigContext.configContext.Format))
                    RequestConfigContext.configContext.Format = "Json";
                return this;
            }
            /// <summary>
            /// 设置请求上下文内容格式xml
            /// </summary>
            /// <returns></returns>
            public RequestConfigManager SetGlobalRequestFormatXml()
            {
                if (string.IsNullOrEmpty(RequestConfigContext.configContext.Format))
                    RequestConfigContext.configContext.Format = "Xml";
                return this;
            }
            /// <summary>
            /// 设置请求上下文的协议
            /// </summary>
            /// <param name="protocol"></param>
            /// <returns></returns>
            public RequestConfigManager SetGlobalRequestProtocol(RequestProtocol protocol)
            {
                RequestConfigContext.configContext.Protocol = protocol;
                return this;
            }
            /// <summary>
            /// 设置请求上下文大小
            /// </summary>
            /// <param name="size"></param>
            /// <returns></returns>
            public RequestConfigManager SetGlobalRequestSize(int size)
            {
                RequestConfigContext.configContext.Size = size;
                return this;
            }
        }

    }
    public static class HelpClass
    {
        public static RequestConfigManager SetGlobalRequestFormatBinary(this RequestConfigManager configManager)
        {
            if (string.IsNullOrEmpty(RequestConfigContext.configContext.Format))
                RequestConfigContext.configContext.Format = "Binary";
            return configManager;
        }
        public static RequestConfigManager SetGlobalRequestSafetyCheckLogic(this RequestConfigManager configManager,Func<RequestContext,bool> checkLogic)
        {
            RequestConfigContext.configContext.SafetyChecks = checkLogic;
            return configManager;
        }
    }
}
