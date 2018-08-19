using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StatementComponentFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "";
            Order order = new Order { customer = new Customer() { Type = CustomerType.VIP } };
            order.Add(new OrderItem()
            {
                product = new Product() { PId = "31423145", PName = "IPhone6", Price = 8000 },
                Number = 10
            });
            order.Add(new OrderItem()
            {
                product = new Product() { PId = "31423146", PName = "IPhone7", Price = 9000 },
                Number = 10
            });
            //LanguageComponentManager manager = new LanguageComponentManager();
            //manager.Run();
            var manager = LanguageComponentManagerFactory.CreateNewOrderLanguageComponent(order);
            manager.Run();
            LanguageComponentManagerFactory.FreeLanguageCompoentManagerObject(manager);
            using (var language = LanguageComponentManagerFactory.RebuildLanguageComponent())
            {
                language.Resume();
            }
            Console.Read();
        }
    }
    /// <summary>
    /// 订单实体
    /// </summary>
    [Serializable]
    public class Order:List<OrderItem>,IEnumerable<OrderItem>
    {
        /// <summary>
        /// 客户信息
        /// </summary>
        public Customer customer { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string ONo { get; set; }
    }
    /// <summary>
    /// 客户信息实体
    /// </summary>
    [Serializable]
    public class Customer
    {
        /// <summary>
        /// 客户类型
        /// </summary>
        public CustomerType Type { get; set; }
    }
    /// <summary>
    /// 客户类型
    /// </summary>
    [Serializable]
    public enum CustomerType
    {
        VIP,
        Normal
    }
    /// <summary>
    /// 订单项
    /// </summary>
    [Serializable]
    public class OrderItem
    {
        /// <summary>
        /// 商品
        /// </summary>
        public Product product { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public int Number { get; set; }
    }
    /// <summary>
    /// 商品实体
    /// </summary>
    [Serializable]
    public class Product
    {
        /// <summary>
        /// Id
        /// </summary>
        public string PId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string PName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
    }

    //语句组建对象模型！！！！
    /// <summary>
    /// 语句组件基类
    /// </summary>
    [Serializable]
    public abstract class LanguageComponent
    {
        /// <summary>
        /// 运行语句组建
        /// </summary>
        /// <param name="trackMark"></param>
        public virtual void Run(LanguageComponentManager trackMark) { }
        /// <summary>
        /// 运行语句组建
        /// </summary>
        /// <param name="trackMark"></param>
        public virtual void Run(object parameter,LanguageComponentManager trackMark) { }
    }
    /// <summary>
    /// 语句组建跟踪标签
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="track"></param>
    public delegate void LanguageComponentTrackMart(object parameter, LanguageComponentManager track);
    /// <summary>
    /// 语句组建管理器
    /// </summary>
    [Serializable]
    public class LanguageComponentManager : IDisposable
    {
        /// <summary>
        /// 语句组建跟踪标签
        /// </summary>
        public LanguageComponentTrackMart TrackMark;
        /// <summary>
        /// 参数
        /// </summary>
        public object Parameter;
        /// <summary>
        /// 索引
        /// </summary>
        public int Index;
        /// <summary>
        /// 第一个语句组件
        /// </summary>
        public LanguageComponent FirstLanguage;
        /// <summary>
        /// 首次运行语句组建
        /// </summary>
        public void Run()
        {
            FirstLanguage.Run(this);
        }
        /// <summary>
        /// 重建后继续运行语句组件
        /// </summary>
        public void Resume()
        {
            if(TrackMark !=null)
            {
                (TrackMark.GetInvocationList()[Index] as LanguageComponentTrackMart)(Parameter, this);
            }
        }
        /// <summary>
        /// 每次结束时钝化当前管理器
        /// </summary>
        public void Dispose()
        {
            if(this.TrackMark == null)
            {
                this.FirstLanguage = null;
                this.Parameter = null;
            }
            
        }
    }
    /// <summary>
    /// 语句组建管理器工厂
    /// </summary>
    public class LanguageComponentManagerFactory
    {
        /// <summary>
        /// 语句组件持久化文件
        /// </summary>
        public const string LanguageFileName = @"D:\language.xml";
        /// <summary>
        /// 创建一个有关订单审批流程的语句组件管理器
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static LanguageComponentManager CreateNewOrderLanguageComponent(Order order)
        {
            LanguageComponentManager result = new LanguageComponentManager() { Parameter = order };
            var sendEmail = new SenEmailOrderItemConfirmComponent();
            var forlanguage = new ForLanguageComponent<Order>(sendEmail, null);
            var ifelselanguage = new IfLanguageComponent<Order>(forlanguage, null);
            ifelselanguage.SetIfExpression(ord => ord.customer.Type == CustomerType.VIP, order);
            result.TrackMark += ifelselanguage.Run;
            result.TrackMark += forlanguage.Run;
            result.TrackMark += sendEmail.Run;
            result.FirstLanguage = ifelselanguage;
            return result;
        }
        /// <summary>
        /// 重建LanguageComponentManager语句组建模型
        /// </summary>
        /// <returns></returns>
        public static LanguageComponentManager RebuildLanguageComponent()
        {
            using(Stream stream = File.Open(LanguageFileName, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream) as LanguageComponentManager;
            }
        }
        /// <summary>
        /// 钝化语句组建
        /// </summary>
        /// <param name="manager"></param>
        public static void FreeLanguageCompoentManagerObject( LanguageComponentManager manager)
        {
            using(Stream stream = File.Open(LanguageFileName, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, manager);
            }
        }
    }
    /// <summary>
    /// If else 语句组件
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    [Serializable]
    public class IfLanguageComponent<TParameter>:LanguageComponent where TParameter : class, new()
    {
        /// <summary>
        /// If分支语句组件
        /// </summary>
        public LanguageComponent If { get; set; }
        /// <summary>
        /// Else分支语句组件
        /// </summary>
        public LanguageComponent Else { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public TParameter Parameter { get; set; }
        /// <summary>
        /// 条件表达式
        /// </summary>
        public Func<TParameter,bool> Expression { get; private set; }
        public IfLanguageComponent(LanguageComponent ifLanguage,LanguageComponent elseLanguage)
        {
            this.If = ifLanguage;
            this.Else = elseLanguage;
        }
        /// <summary>
        /// 设置条件表达式和参数
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter"></param>
        public void SetIfExpression(Func<TParameter,bool> expression,TParameter parameter)
        {
            this.Expression = expression;
            this.Parameter = parameter;
        }
        /// <summary>
        /// 有参数运行语句组建
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="trackMark"></param>
        public override void Run(object parameter, LanguageComponentManager trackMark)
        {
            trackMark.Index = 0;
            if(this.Expression(parameter as TParameter))
            {
                this.If.Run(parameter, trackMark);
            }
            else if(Else != null)
            {
                this.Else.Run(parameter, trackMark);
            }
        }
        public override void Run(LanguageComponentManager trackMark)
        {
            this.Run(Parameter, trackMark);
        }
    }
    /// <summary>
    /// for 语句组件
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    [Serializable]
    public class ForLanguageComponent<TParameter>:LanguageComponent where TParameter:class, IList
    {
        /// <summary>
        /// 循环中每一项所调用的语句组件
        /// </summary>
        public LanguageComponent ItemExpression { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        private TParameter Parameter { get; set; }
        /// <summary>
        /// 当前循环体的索引，记录当前循环的位置
        /// </summary>
        private int CurrentIndex { get; set; }
        public ForLanguageComponent(LanguageComponent itemExpression ,TParameter parameter)
        {
            this.ItemExpression = itemExpression;
            Parameter = parameter;
        }
        /// <summary>
        /// 带参运行语句组件
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="trackMark"></param>
        public override void Run(object parameter, LanguageComponentManager trackMark)
        {
            trackMark.Index = 1;
            var PList = (parameter as TParameter);
            for(int index = CurrentIndex;index < PList.Count; index++)
            {
                this.CurrentIndex = index;//每次执行完了都记录下当前执行位置
                if (ItemExpression != null)
                    ItemExpression.Run(PList[index], trackMark);
            }
        }
        /// <summary>
        /// 不带参数运行
        /// </summary>
        /// <param name="trackMark"></param>
        public override void Run(LanguageComponentManager trackMark)
        {
            this.Run(Parameter, trackMark);
        }
    }
    /// <summary>
    ///发送订单项确认邮件，让其参与到语句流程中
    /// </summary>
    [Serializable]
    public class SenEmailOrderItemConfirmComponent : LanguageComponent
    {
        public override void Run(object parameter, LanguageComponentManager trackMark)
        {
            if(parameter != null)
            {
                var orderItem = parameter as OrderItem;
                Console.WriteLine($"发送采购商品确定邮件，商品名称：{orderItem.product.PName},数量{orderItem.Number}");
            }
        }
    }
}
