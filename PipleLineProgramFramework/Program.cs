using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipleLineProgramFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            //启动客户端管道处理
            Request request = new Request()
            {
                Content = new RequestContent { Content = "Query order method" },
                Head = new StringBuilder(),
                ClientType = RequestClientTypeFactory.CreateRequestClientTypeForNet2()
            };
            ClientPipelineObject pipe = new ClientPipelineObject();
            pipe.AddModule(ClientPipelineModules.CheckRequestContent);
            pipe.AddModule(ClientPipelineModules.AddRequestHead);
            pipe.AddModule(ClientPipelineModules.TransFerRequestFormat);
            pipe.AddModule(ClientPipelineModules.ReduceRequest);
            pipe.RunPipeline(request);
            //执行特性类型客户端的管道处理
            IBuildOperationLogicPipelineObject clientType = OperationLogicPipelineObjectFactory.Create(request.ClientType);
            OperationLogicPelineObject pipleline = clientType.BuildOperationPipeline(request);
            pipleline.RunPipeLine(request);
        }
    }
    /// <summary>
    /// SOA请求
    /// </summary>
    public class Request
    {
        /// <summary>
        /// 请求头
        /// </summary>
        public StringBuilder Head { get; set; }
        /// <summary>
        /// 请求客户端类型
        /// </summary>
        public RequestClientType ClientType { get; set; }
        /// <summary>
        /// 请求正文
        /// </summary>
        public RequestContent Content { get; set; }
    }
    /// <summary>
    /// 请求客户端类型
    /// </summary>
    public class RequestClientType
    {
        /// <summary>
        /// 移动app
        /// </summary>
        public const string App = "App";
        /// <summary>
        /// net 客户端2.0
        /// </summary>
        public const string NetClient = ".Net Client 2.0";
        /// <summary>
        /// 目前请求类型
        /// </summary>
        internal string type;
    }
    /// <summary>
    /// ClientType工厂
    /// </summary>
    public class RequestClientTypeFactory
    {
        /// <summary>
        /// 创建一个APP客户端请求类型
        /// </summary>
        /// <returns></returns>
        public static RequestClientType CreateRequestTypeForApp()
        {
            return new RequestClientType() { type = RequestClientType.App };
        }
        /// <summary>
        /// 创建一个Net2.0客户端请求类型
        /// </summary>
        /// <returns></returns>
        public static RequestClientType CreateRequestClientTypeForNet2()
        {
            return new RequestClientType() { type = RequestClientType.NetClient };
        }
    }
    /// <summary>
    /// 请求正文
    /// </summary>
    public class RequestContent
    {
        /// <summary>
        /// 请求字符串
        /// </summary>
        public string Content { get; set; }
    }
    /// <summary>
    /// 客户端Modules
    /// </summary>
    public class ClientPipelineModules
    {
        /// <summary>
        /// 验证请求正文
        /// </summary>
        /// <param name="request"></param>
        public static void CheckRequestContent(Request request)
        {
            if(request==null || request.Content ==null || string.IsNullOrEmpty(request.Content.Content))
            {
                throw new InvalidOperationException("无效请求");
            }
        }
        /// <summary>
        /// 添加请求头
        /// </summary>
        /// <param name="request"></param>
        public static void AddRequestHead(Request request)
        {
            request.Head.Append("Request source:SOA Client");
        }
        /// <summary>
        /// 转换请求格式
        /// </summary>
        /// <param name="request"></param>
        public static void TransFerRequestFormat(Request request)
        {
            request.Content.Content = request.Content.Content;
        }
        /// <summary>
        /// 压缩请求
        /// </summary>
        /// <param name="request"></param>
        public static void ReduceRequest(Request request)
        {
            //ReduceRequestBody.Reduce(request);
        }
    }
    /// <summary>
    /// 客户端管道模型中模块实例委托
    /// </summary>
    /// <param name="request"></param>
    public delegate void ClientPipelingObjectModules(Request request);
    /// <summary>
    /// 客户端管道对象
    /// </summary>
    public class ClientPipelineObject
    {
        /// <summary>
        /// 管道模块引用
        /// </summary>
        private ClientPipelingObjectModules modules;
        /// <summary>
        /// 添加管道中的模块
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(ClientPipelingObjectModules module)
        {
            modules += module;
        }
        /// <summary>
        /// 开始管道处理
        /// </summary>
        /// <param name="request"></param>
        public void RunPipeline(Request request)
        {
            modules(request);
        }
    }
    /// <summary>
    /// 服务端模型实例委托
    /// </summary>
    /// <param name="request"></param>
    public delegate void OperationLogicPipelineObjectModules(Request request);
    public class OperationLogicPelineObject
    {
        /// <summary>
        /// 管道模块
        /// </summary>
        public OperationLogicPipelineObjectModules modules;
        /// <summary>
        /// 添加模块到管道中
        /// </summary>
        /// <param name="module"></param>
        internal void Add(OperationLogicPipelineObjectModules module)
        {
            this.modules += module;
        }
        /// <summary>
        /// 执行管道
        /// </summary>
        /// <param name="request"></param>
        public void RunPipeLine(Request request)
        {
            this.modules(request);
        }
    }
    /// <summary>
    /// 生成逻辑处理管道接口
    /// </summary>
    public interface IBuildOperationLogicPipelineObject
    {
        /// <summary>
        /// 生成一个符合当前客户端类型的处理管道
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        OperationLogicPelineObject BuildOperationPipeline(Request request);
    }
    /// <summary>
    /// App客户端创建者
    /// </summary>
    public class ClientForAppType : IBuildOperationLogicPipelineObject
    {
        public OperationLogicPelineObject BuildOperationPipeline(Request request)
        {
            var result = new OperationLogicPelineObject();
            result.Add(requestObject => {
                //记录app请求Log
            });
            result.Add(requestObjcet =>
            {
                //执行APP请求
            });
            return result;
        }
    }
    /// <summary>
    /// Client客户端创建者
    /// </summary>
    public class ClientForNet2Type : IBuildOperationLogicPipelineObject
    {
        public OperationLogicPelineObject BuildOperationPipeline(Request request)
        {
            var result = new OperationLogicPelineObject();
            result.Add(requestObject => {
                //记录.net 客户端请求Log
            });
            result.Add(requestObjcet =>
            {
                //执行.net请求
            });
            return result;
        }
    }
    /// <summary>
    /// 管道创建工厂
    /// </summary>
    public class OperationLogicPipelineObjectFactory
    {
        public static IBuildOperationLogicPipelineObject Create(RequestClientType clientType)
        {
            if(clientType.type == RequestClientType.App)
            {
                return new ClientForAppType();
            }
            else if(clientType.type == RequestClientType.NetClient)
            {
                return new ClientForNet2Type();
            }
            return null;
        }
    }
}
