using System;
using System.Collections.Generic;

namespace BusModelFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBase orderService = new OrderService();
            orderService.Register();

            //订单测试数据
            Order order = new Order { OId = "P001", SubmitTime = DateTime.Now };
            Message message = new Message()
            {
                Body = order,
                Header = new MessageHeader
                {
                    MessageKey = Guid.NewGuid().ToString(),
                    RequestKey = "OrderService.SubmitOrder"
                }
            };
            bool isOk = MessageBus.SendBusAndAction<bool>(message);
            if(isOk)
            {
                Console.WriteLine("订单提交成功");
            }
            Console.ReadLine();
        }
    }
    /// <summary>
    /// 订单实体
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public string OId { get; set; }
        /// <summary>
        /// 提交订单时间
        /// </summary>
        public DateTime SubmitTime { get; set; }
    }
    /// <summary>
    /// 总线消息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 消息头
        /// </summary>
        public MessageHeader Header { get; set; }
        /// <summary>
        /// 消息体
        /// </summary>
        public object Body { get; set; }
    }
    /// <summary>
    /// 消息头
    /// </summary>
    public class MessageHeader
    {
        /// <summary>
        /// 消息Key
        /// </summary>
        public string MessageKey { get; set; }
        /// <summary>
        /// 请求Key 也可以称为请求服务的方法名
        /// </summary>
        public string RequestKey { get; set; }
    }
    /// <summary>
    /// 服务处理绑定
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="result"></param>
    public delegate void ServiceOperationHandler(object parameter, ref object result);
    public class ServiceRegister : Dictionary<string, ServiceOperationHandler>
    {
        /// <summary>
        /// 全局服务注册器
        /// </summary>
        public static ServiceRegister GlobalRegister = new ServiceRegister();
        public void Register(string requestKey,ServiceOperationHandler requestOperation)
        {
            if (this.ContainsKey(requestKey))
            {
                this[requestKey] = requestOperation;
            }
            else
            {
                this.Add(requestKey, requestOperation);
            }
        }
        public ServiceOperationHandler GetOperationHandler(string requestKey)
        {
            if (this.ContainsKey(requestKey))
                return this[requestKey];
            return null;
        }
    }
    /// <summary>
    /// 服务路由
    /// </summary>
    public class ServiceRouting
    {
        /// <summary>
        /// 全局路由
        /// </summary>
        public static ServiceRouting GlobalRouting = new ServiceRouting();
        /// <summary>
        /// 获取请求处理句柄，也就是对应的请求处理服务
        /// </summary>
        /// <param name="requestKey"></param>
        /// <returns></returns>
        public ServiceOperationHandler GetRequestOperationHandler(string requestKey)
        {
            return ServiceRegister.GlobalRegister.GetOperationHandler(requestKey);
        }
    }
    /// <summary>
    /// 服务基类。完成服务在Bus中的动态注册功能 所有服务进行统一注册约定
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        /// 开始注册服务
        /// </summary>
        public void Register()
        {
            this.RegisterServiceRequest(ServiceRegister.GlobalRegister);
        }
        /// <summary>
        /// 所有服务实现此方法用来添加服务中所能处理的请求
        /// </summary>
        /// <param name="Register"></param>
        protected abstract void RegisterServiceRequest(ServiceRegister Register);
    }
    /// <summary>
    /// 消息总线对象
    /// </summary>
    public class MessageBus
    {
        /// <summary>
        /// 发送到Bus并且同步执行该请求
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static TResult SendBusAndAction<TResult>(Message parameter)
        {
            //根据请求Key获取该请求所对应的服务处理句柄
            ServiceOperationHandler handler = ServiceRouting.GlobalRouting.GetRequestOperationHandler(parameter.Header.RequestKey);
            //使用参数带出返回值
            object result = null;
            //执行远程服务
            handler(parameter.Body, ref result);
            if (result == null)
                return default(TResult);
            return (TResult)result;
        }
    }
    /// <summary>
    /// 订单处理服务
    /// </summary>
    public class OrderService : ServiceBase
    {
        /// <summary>
        /// 注册OrderService服务中所有服务于请求的对应项
        /// </summary>
        /// <param name="Register"></param>
        protected override void RegisterServiceRequest(ServiceRegister Register)
        {
            Register.Register("OrderService.SubmitOrder", (object parameter, ref object result) =>
            {
                result = this.SubmitOrder(parameter as Order);
            });
            Register.Register("OrderService.DeleteOrder", (object parameter, ref object result) =>
            {
                result = this.DeleteOrder(parameter as string);
            });
        }
        public bool SubmitOrder(Order order)
        {
            Console.WriteLine($"提交订单{order.OId}");
            return true;
        }
        public bool DeleteOrder(String Old)
        {
            Console.WriteLine($"删除订单{Old}");
            return true;
        }
    }
}
