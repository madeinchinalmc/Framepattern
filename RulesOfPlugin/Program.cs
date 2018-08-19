using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 规则外挂
/// </summary>
namespace RulesOfPluginFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = OrderSpecificationManagerFactory.CreateNewOrderSpecificationManager();
            var orderList = new List<Order>
            {
                new Order(){Customer =new Customer(){ CustomerType = CustomerType.Vip} },
                new Order(){Customer = new Customer(){CustomerType = CustomerType.Normal}} 
            };
            OrderBusiness business = new OrderBusiness(manager);
            using (manager)
            {
                orderList.ForEach(order => business.SubmitOrder(order));
            }
        }

    }
    /// <summary>
    /// 订单
    /// </summary>
    public class Order
    {
        public Customer Customer { get; set; }
    }
    /// <summary>
    /// 用户订单
    /// </summary>
    public class Customer
    {
        public CustomerType CustomerType;
    }
    /// <summary>
    /// 用户类型
    /// </summary>
    public enum CustomerType
    {
        Vip,
        Normal
    }
    /// <summary>
    /// 提交订单规则
    /// </summary>
    [Serializable]
    public class SubmitOrderSpecification
    {
        /// <summary>
        /// 是否是vip提交
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool CheckSubmitVipOrder(Order order)
        {
            if (order.Customer.CustomerType == CustomerType.Vip)
                return true;
            return false;
        }
        /// <summary>
        /// 是否是普通用户提交
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool CheckSubmitNormalOrder(Order order)
        {
            if (order.Customer.CustomerType == CustomerType.Normal)
                return true;
            return false;
        }
    }
    /// <summary>
    /// 订单规则索引
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public delegate bool OrderSpecifcationIndex(Order order);
    /// <summary>
    /// 订单规则管理器
    /// </summary>
    [Serializable]
    public class OrderSpecificationManager : IDisposable
    {
        /// <summary>
        /// 根据客户类型的订单提交规则
        /// </summary>
        public Dictionary<CustomerType,OrderSpecifcationIndex> Specification { get; set; }
        /// <summary>
        /// 根据指定客户类型获取一个订单提交业务规则
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public OrderSpecifcationIndex GetSpecifcationWithCustomerType(CustomerType type)
        {
            if (this.Specification.ContainsKey(type))
                return this.Specification[type];
            return null;
        }
        public void Dispose()
        {
            //冻结当前管理器对象
            OrderSpecificationManagerFactory.FreezeOrderSpecificationManagerObject(this);
        }
    }
    public class OrderSpecificationManagerFactory
    {
        /// <summary>
        /// 规则文件名称
        /// </summary>
        public const string SpecificationFileName = @"D:\orderSubmitSpec.xml";
        /// <summary>
        /// 创建一个用来检查提交订单相关的规则管理器
        /// </summary>
        /// <returns></returns>
        public static OrderSpecificationManager CreateNewOrderSpecificationManager()
        {
            OrderSpecificationManager result = new OrderSpecificationManager()
            {
                Specification = new Dictionary<CustomerType, OrderSpecifcationIndex>()
            };
            SubmitOrderSpecification submitOrderSpecification = new SubmitOrderSpecification();
            result.Specification.Add(CustomerType.Vip, submitOrderSpecification.CheckSubmitVipOrder);
            result.Specification.Add(CustomerType.Normal, submitOrderSpecification.CheckSubmitNormalOrder);
            return result;
        }
        /// <summary>
        /// 重建现有的规则管理对象
        /// </summary>
        /// <returns></returns>
        public static OrderSpecificationManager ReBuildOrderSpecificationManager()
        {
            using(Stream stream = File.Open(SpecificationFileName, FileMode.Open))
            {
                BinaryFormatter format = new BinaryFormatter();
                return format.Deserialize(stream) as OrderSpecificationManager;
            }
        }
        /// <summary>
        /// 冻结规则管理器对象
        /// </summary>
        /// <param name="manager"></param>
        public static void FreezeOrderSpecificationManagerObject(OrderSpecificationManager manager)
        {
            using (Stream stream = File.Open(SpecificationFileName, FileMode.Create))
            {
                BinaryFormatter format = new BinaryFormatter();
                format.Serialize(stream, manager);
            }
        }
    }
    /// <summary>
    /// 处理订单相关业务逻辑
    /// </summary>
    public class OrderBusiness
    {
        /// <summary>
        /// 订单规则管理器
        /// </summary>
        private OrderSpecificationManager OrderSpecManager;
        public OrderBusiness(OrderSpecificationManager orderSpecificationManager)
        {
            OrderSpecManager = orderSpecificationManager;
        }
        public void SubmitOrder(Order order)
        {
            var spec = OrderSpecManager.GetSpecifcationWithCustomerType(order.Customer.CustomerType);
            if(order.Customer.CustomerType == CustomerType.Vip && spec(order))
            {
                //处理Vip订单逻辑
            }
            else if(order.Customer.CustomerType == CustomerType.Normal && spec(order))
            {
                //处理普通用户订单
            }
        }
    }
}
