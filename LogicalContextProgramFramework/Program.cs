using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalContextProgramFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceProxy proxy = new ServiceProxy();
            using(SoaServiceCallContext soaContext= new SoaServiceCallContext(true, true))
            {
                soaContext.BeginRecordLogTrackEvent += SoaContext_BeginRecordLogTrackEvent;
                soaContext.TransactionEndEvent += SoaContext_TransactionEndEvent;
                proxy.SetTicketPrice("29339", 300);
                proxy.UpdateTicketCache("29339", 350);
            }
        }

        private static void SoaContext_TransactionEndEvent(TransactionActionInfo arg)
        {
            throw new NotImplementedException();
        }

        private static void SoaContext_BeginRecordLogTrackEvent(LogTrackLocation arg)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// soaservicecallcontext服务调用上下文事件委托
    /// </summary>
    /// <typeparam name="T">传递对象类型</typeparam>
    /// <param name="arg">传递对象实例</param>
    public delegate void SoaServiceCallContextHander<T>(T arg);
    /// <summary>
    /// SOA调用上下文
    /// </summary>
    public class SoaServiceCallContext : IDisposable
    {
        internal static SoaServiceCallContext context;
        /// <summary>
        /// 是否开启服务调用事务
        /// </summary>
        public bool Transaction { get; private set; }
        /// <summary>
        /// 是否开启日志跟踪
        /// </summary>
        public bool LogTrack { get; private set; }
        /// <summary>
        /// 构造一个服务调用上下文
        /// </summary>
        /// <param name="transaction">是否开启事务</param>
        /// <param name="logtrack">是否开启日志跟踪</param>
        public SoaServiceCallContext(bool transaction,bool logtrack)
        {
            this.Transaction = transaction;
            this.LogTrack = logtrack;
            //设置一个服务调用上下文的访问点，便于被感知
            SoaServiceCallContext.context = this;
        }
        private SoaServiceCallContextHander<LogTrackLocation> beginRecordLogTackHander;
        /// <summary>
        /// SOA服务调用上下文已经开始纪录跟踪日志
        /// </summary>
        public event SoaServiceCallContextHander<LogTrackLocation> BeginRecordLogTrackEvent
        {
            add
            {
                this.BeginRecordLogTrackEvent += value;
            }
            remove
            {
                if (this.beginRecordLogTackHander != null)
                    this.beginRecordLogTackHander -= value;
            }
        }
        private SoaServiceCallContextHander<TransactionActionInfo> transactionEndHandler;
        /// <summary>
        /// 事务执行结束后的动作信息
        /// </summary>
        public event SoaServiceCallContextHander<TransactionActionInfo> TransactionEndEvent
        {
            add
            {
                this.transactionEndHandler += value;
            }
            remove
            {
                if (this.transactionEndHandler != null)
                    this.transactionEndHandler -= value;
            }
        }
        public void Dispose()
        {
            this.transactionEndHandler = null;
            this.beginRecordLogTackHander = null;
        }
    }
    /// <summary>
    /// 服务上下文管理，该对象负责自动感知SOA服务上下文
    /// </summary>
    public class ServiceContextManager
    {
        /// <summary>
        /// 当前上下文属性
        /// </summary>
        public SoaServiceCallContext CurrentContext
        {
            get { return SoaServiceCallContext.context; }
        }
        /// <summary>
        /// 感知服务调用上下文
        /// </summary>
        /// <param name="request"></param>
        protected void ApperceivceContext(Request request)
        {
            if(SoaServiceCallContext.context != null)
            {
                //感知日志跟踪
                request.LogTrackId = CurrentContext.LogTrack ? Guid.NewGuid() : new Guid();
                //感知事务处理
                request.TransactionId = CurrentContext.Transaction ? Guid.NewGuid() : new Guid();
            }
        }
    }
    /// <summary>
    /// 服务调用请求对象
    /// </summary>
    public class ServiceProxyRequest : Request
    {
        /// <summary>
        /// 门票ID
        /// </summary>
        public string TicketId { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
    }
    /// <summary>
    /// 远程服务代理
    /// </summary>
    public class ServiceProxy : ServiceContextManager
    {
        /// <summary>
        /// 设置门票价格
        /// </summary>
        /// <param name="ticketId">门票ID</param>
        /// <param name="price">价格</param>
        public void SetTicketPrice(string ticketId,int price)
        {
            base.ApperceivceContext(new ServiceProxyRequest() { TicketId=ticketId,Price=price});
        }
        /// <summary>
        /// 更新门票缓存
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="price"></param>
        public void UpdateTicketCache(string ticketId,int price)
        {
            base.ApperceivceContext(new ServiceProxyRequest() { TicketId = ticketId, Price = price });
        }
    }
    /// <summary>
    /// SOA服务请求
    /// </summary>
    public class Request
    {
        /// <summary>
        /// 分布式事务ID
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// 全局跟踪LogId
        /// </summary>
        public Guid LogTrackId { get; set; }
    }
    /// <summary>
    /// 本地日志
    /// </summary>
    public class LogTrackLocation { }
    public class TransactionActionInfo { }

}
