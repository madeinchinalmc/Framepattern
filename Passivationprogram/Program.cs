using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Passivationprogram
{
    /// <summary>
    /// 钝化
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Order order = new Order()
            {
                Items = new List<OrderItem>(),
                Customer = new CustomerInfo() { Email = "lmc@work.com", Name = "jack", Phone = "13671241939" }
            };
            order.Items.Add(new OrderItem() { Number = 10, Product = new Product() { Name = "自行车配件", Price = 500 } });
            order.Items.Add(new OrderItem() { Number = 5, Product = new Product() { Name = "自行车配件", Price = 300 } });


            OrderExamineApproveManager approveManager = OrderExamineApproveManager.CreateFlows();
            Stream stream = File.Open(@"D:\orderChecks.xml", FileMode.OpenOrCreate);
            BinaryFormatter format = new BinaryFormatter();
            format.Serialize(stream, approveManager);
            stream.Close();
            stream.Dispose();

            Stream stream1 = File.Open(@"D:\orderChecks.xml", FileMode.Open);
            BinaryFormatter forma1t = new BinaryFormatter();
            var approveFlows1 = forma1t.Deserialize(stream1) as OrderExamineApproveManager;
            approveFlows1.RunFlows(order);
            stream1.Close();
            stream1.Dispose();


            Stream stream2 = File.Open(@"D:\orderChecks.xml", FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream2, approveFlows1);
            stream2.Close();
            stream2.Dispose();
        }
    }
    /// <summary>
    /// 订单类
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 订单项
        /// </summary>
        public List<OrderItem> Items { get; set; }
        /// <summary>
        /// 客户信息
        /// </summary>
        public CustomerInfo Customer { get; set; }
    }
    /// <summary>
    /// 订单项
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 商品
        /// </summary>
        public Product Product { get; set; }
    }
    /// <summary>
    /// 商品
    /// </summary>
    public class Product
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
    }
    /// <summary>
    /// 客户信息
    /// </summary>
    public class CustomerInfo
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 客户电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
    }
    /// <summary>
    /// 订单审批过程索引
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="manager">过程管理器</param>
    /// <returns>该阶段是否审批成功</returns>
    public delegate bool OrderExamineApproveManagerHanlder(Order order, ref OrderExamineApproveManagerHanlder manager);

    /// <summary>
    /// 订单审批管理器
    /// </summary>
    [Serializable]
    public class OrderExamineApproveManager
    {
        /// <summary>
        /// 订单审批管理器
        /// </summary>
        /// <returns></returns>
        public static OrderExamineApproveManager CreateFlows()
        {
            OrderExamineApproveManager result = new OrderExamineApproveManager();
            Infomationer infomationer = new Infomationer();
            result.Flows += infomationer.CheckPrices;
            result.Flows += infomationer.CheckNumber;

            BusinessManager businessManager = new BusinessManager();
            result.Flows += businessManager.CallPhoneConfirm;
            result.Flows += businessManager.SendEmailNotice;


            GeneralManager generalManager = new GeneralManager();
            result.Flows += generalManager.FinalConfirm;
            result.Flows += generalManager.SignAndRecord;
            return result;
        }
        /// <summary>
        /// 流程索引
        /// </summary>
        public OrderExamineApproveManagerHanlder Flows;
        public void RunFlows(Order order)
        {
            this.Flows(order, ref this.Flows);
        }
    }
    /// <summary>
    ///信息员
    /// </summary>
    [Serializable]
    public class Infomationer
    {
        public bool CheckPrices(Order order,ref OrderExamineApproveManagerHanlder manager)
        {
            if(order.Items.Any(item=>item.Product.Price <= 0) ? false : true)
            {

                manager -= this.CheckPrices;//将自己从流程中处理移除
                return true;

            }
            return false;
        }
        public bool CheckNumber(Order order, ref OrderExamineApproveManagerHanlder manager)
        {
            if(order.Items.Any(item =>item.Number > 10) ? false : true)
            {
                manager -= this.CheckNumber;
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 业务经理
    /// </summary>
    [Serializable]
    public class BusinessManager
    {
        public bool CallPhoneConfirm(Order order ,ref OrderExamineApproveManagerHanlder manager)
        {
            manager -= this.CallPhoneConfirm;
            return true;
        }
        public bool SendEmailNotice(Order order,ref OrderExamineApproveManagerHanlder manager)
        {
            manager -= this.SendEmailNotice;
            return true;
        }
    }
    /// <summary>
    /// 总经理
    /// </summary>
    [Serializable]
    public class GeneralManager
    {
        public bool FinalConfirm(Order order,ref OrderExamineApproveManagerHanlder manager)
        {
            manager -= this.FinalConfirm;
            return true;
        }
        public bool SignAndRecord(Order order ,ref OrderExamineApproveManagerHanlder manager)
        {
            manager -= this.SignAndRecord;
            return true;
        }
    }
}
