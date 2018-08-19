using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace AsyncMessageFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化远程服务
            OrderService orderService = new OrderService();
            //绑定到队列通知中
            MessageQueue.GlobalQueue.MessageNotfiyEvent += orderService.SubmitOrder;
            var orders = new List<Order> {
                new Order{OrderCode = "P001"},
                new Order{OrderCode = "P002"},
                new Order{OrderCode = "B003"}
            };
            OrderServiceProxy proxy = new OrderServiceProxy();
            orders.ForEach(order => proxy.SubmitOrder(new Message() { Body = order }));
            Console.ReadLine();//处理完订单后 检查订单处理状态信息
            foreach (var key in MessageOperationStateDictionary.GlobalDictionary.Keys)
            {
                Console.WriteLine($"消息号:{MessageOperationStateDictionary.GlobalDictionary[key].MesageKey}");
                var state  = MessageOperationStateDictionary.GlobalDictionary.SearchState(MessageOperationStateDictionary.GlobalDictionary[key].MesageKey);
                if (MessageOperationStateDictionary.GlobalDictionary[key].Exception != null)
                    Console.WriteLine($"异常信息{MessageOperationStateDictionary.GlobalDictionary[key].Exception.Message}");
                Console.WriteLine($"消息状态{MessageOperationStateDictionary.GlobalDictionary[key].State}");
            }
        }
    }
    [Serializable]
    public class Order
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }
    }
    /// <summary>
    /// 订单服务接口
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderMessage"></param>
        void SubmitOrder(Message orderMessage);
    }
    public class OrderService : IOrderService
    {
        /// <summary>
        /// 订单编号前缀
        /// </summary>
        public const string OrderCodePrefix = "P";
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderMessage"></param>
        public void SubmitOrder(Message orderMessage)
        {
            Order order = orderMessage.Body as Order;
            if (!order.OrderCode.StartsWith(OrderCodePrefix))
            {
                MessageOperationStateDictionary.GlobalDictionary.Add(orderMessage.MessageKey, new MessageOperationState()
                {
                    Exception = new Exception($"订单处理错误,订单编号{order.OrderCode}格式不对"),
                    State = OperationState.Exception,
                    MesageKey = orderMessage.MessageKey
                });
            }
            else
            {
                //订单处理逻辑
                MessageOperationStateDictionary.GlobalDictionary.Add(orderMessage.MessageKey, new MessageOperationState()
                {
                    State = OperationState.Finished,
                    MesageKey = orderMessage.MessageKey
                });
            }
        }
    }
    //中间层框架对象模型
    /// <summary>
    /// 消息对象
    /// </summary>
    [Serializable]
    public class Message
    {
        /// <summary>
        /// 构造一个新的消息
        /// </summary>
        public Message()
        {
            this.MessageKey = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 消息Key，这个Key将用来作为查询状态的唯一键值
        /// </summary>
        public string MessageKey { get; private set; }
        /// <summary>
        /// 消息正文
        /// </summary>
        public object Body { get; set; }
    }
    /// <summary>
    /// 消息队列事件通知
    /// </summary>
    /// <param name="message"></param>
    public delegate void MessageQueueEventNotifyHandler(Message message);
    /// <summary>
    /// 异步消息队列
    /// </summary>
    public class MessageQueue : Queue<Message>
    {
        /// <summary>
        /// 当前上下文全局消息队列
        /// </summary>
        public static MessageQueue GlobalQueue = new MessageQueue();
        /// <summary>
        /// 队列事件通知定时器
        /// </summary>
        private Timer timer = new Timer();
        /// <summary>
        /// 构造一个新的消息队列
        /// </summary>
        public MessageQueue()
        {
            this.timer.Interval = 3000;
            this.timer.Elapsed += Notfiy;
            this.timer.Enabled = true;
        }
        /// <summary>
        /// 通知队列监听者，并且传递消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Notfiy(object sender,ElapsedEventArgs e)
        {
            lock(this)
            {
                if(this.Count > 0)
                {
                    this.messageNotfiyEvent.GetInvocationList()[0].DynamicInvoke(this.Dequeue());
                }
            }
        }
        /// <summary>
        /// 队列通知索引器
        /// </summary>
        public MessageQueueEventNotifyHandler messageNotfiyEvent;
        /// <summary>
        /// 队列通知事件
        /// </summary>
        public event MessageQueueEventNotifyHandler MessageNotfiyEvent
        {
            add
            {
                this.messageNotfiyEvent += value;
            }
            remove
            {
                if (this.messageNotfiyEvent != null)
                    this.messageNotfiyEvent -= value;
            }
        }
    }
    //消息处理状态
    [Serializable]
    public struct OperationState
    {
        /// <summary>
        /// 处理成功结束
        /// </summary>
        public const string Finished = "OK";
        /// <summary>
        /// 处理发生异常
        /// </summary>
        public const string Exception = "Error";
    }
    /// <summary>
    /// 消息处理最终状态
    /// </summary>
    [Serializable]
    public class MessageOperationState
    {
        /// <summary>
        /// 消息Key
        /// </summary>
        public string MesageKey { get; set; }
        /// <summary>
        /// 消息处理状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 是否存在异常信息
        /// </summary>
        public Exception Exception { get; set; }
    }
    //消息处理状态字典
    /// <summary>
    /// 消息处理状态表
    /// </summary>
    public class MessageOperationStateDictionary : Dictionary<string, MessageOperationState>
    {
        /// <summary>
        /// 当前上下文全局状态表
        /// </summary>
        public static MessageOperationStateDictionary GlobalDictionary = new MessageOperationStateDictionary();
        /// <summary>
        /// 查询消息处理状态
        /// </summary>
        /// <param name="messageKey"></param>
        /// <returns></returns>
        public MessageOperationState SearchState(string messageKey)
        {
            if (this.ContainsKey(messageKey))
                return this[messageKey];
            return null;
        }
    }
    /// <summary>
    /// 订单服务代理
    /// </summary>
    public class OrderServiceProxy : IOrderService
    {
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderMessage"></param>
        public void SubmitOrder(Message orderMessage)
        {
            //消息入队
            MessageQueue.GlobalQueue.Enqueue(orderMessage);
        }
    }
}
