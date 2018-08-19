using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDataPoolFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            new Order_RepositoryDesign().BeginAddItem();

            new Item_RepositoryDesign().BeginAddItem();
            var s = MemoryMetadataPool.GlobalMemoryPool[$"{typeof(Order).ToString()}"];
            var key = s.Key;
            var value = s.Item as MemoryTableMetadata;
            Console.WriteLine(value.Columns[0].ColumnName);
            Console.ReadLine();
        }
    }
    /// <summary>
    /// 领域实体
    /// </summary>
    public class Order
    {
        public string OId { get; set; }
        public string CustomerId { get; set; }
        public string Address { get; set; }
        public Dictionary<string, Item> items = new Dictionary<string, Item>();
        /// <summary>
        /// 添加订单项
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(Item item)
        {
            if (item != null && item.PId.StartsWith("P") && item.Number > 0)
                this.items.Add(item.PId, item);
        }
        /// <summary>
        /// 删除订单项
        /// </summary>
        /// <param name="ItemId"></param>
        public void DelteItem(string ItemId)
        {
            if (this.items.ContainsKey(ItemId))
                this.items.Remove(ItemId);
        }
    }
    /// <summary>
    /// 订单项
    /// </summary>
    public class Item
    {
        public Item()
        {
            this.ItemId = Guid.NewGuid().ToString();
        }
        public string PId { get; set; }
        public int Number { get; set; }
        public string ItemId { get; private set; }
    }
    //元数据对象
    /// <summary>
    /// 领域实体持久化表元数据
    /// </summary>
    public class MemoryTableMetadata
    {
        /// <summary>
        /// 领域实体所需要存储的表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 领域实体存储的字段集合
        /// </summary>
        public List<MemoryColumnMetadata> Columns { get; set; }
    }
    /// <summary>
    /// 领域实体持久化列元数据
    /// </summary>
    public class MemoryColumnMetadata
    {
        public string ColumnName { get; set; }
    }
    /// <summary>
    /// 存储方面的元数据缓存池
    /// </summary>
    public class MemoryMetadataPool : Dictionary<string, MetadataPoolItem>
    {
        /// <summary>
        ///全局缓存池
        /// </summary>
        public static MemoryMetadataPool GlobalMemoryPool = new MemoryMetadataPool();

        public void AddPoolItem(MetadataPoolItem item)
        {
            this.Add(item.Key, item);
        }
    }
    /// <summary>
    /// 元数据缓存池中项
    /// </summary>
    public class MetadataPoolItem
    {
        /// <summary>
        /// 项唯一Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 项对象引用
        /// </summary>
        public Object Item { get; set; }
    }
    //元数据构造代码
    public abstract class RepositoryDesign
    {
        public void BeginAddItem()
        {

            this.AddItem(MemoryMetadataPool.GlobalMemoryPool);
        }
        protected abstract void AddItem(MemoryMetadataPool metadataPool);
    }
    //订单类型元数据构造代码
    public partial class Order_RepositoryDesign : RepositoryDesign
    {
        protected override void AddItem(MemoryMetadataPool metadataPool)
        {
            var orderTable = new MemoryTableMetadata() { TableName = "Table_Order" };
            orderTable.Columns = new List<MemoryColumnMetadata>();
            orderTable.Columns.Add(new MemoryColumnMetadata { ColumnName = "OId" });
            orderTable.Columns.Add(new MemoryColumnMetadata { ColumnName = "CustomerId" });
            orderTable.Columns.Add(new MemoryColumnMetadata { ColumnName = "Address" });

            metadataPool.AddPoolItem(new MetadataPoolItem { Item = orderTable, Key = typeof(Order).ToString() });
        }
    }
    public partial class Item_RepositoryDesign : RepositoryDesign
    {
        protected override void AddItem(MemoryMetadataPool metadataPool)
        {
            var itemTable = new MemoryTableMetadata() { TableName = "Table_OrderItems" };
            itemTable.Columns = new List<MemoryColumnMetadata>();
            itemTable.Columns.Add(new MemoryColumnMetadata() { ColumnName = "ItemId" });
            itemTable.Columns.Add(new MemoryColumnMetadata() { ColumnName = "Pid" });
            itemTable.Columns.Add(new MemoryColumnMetadata() { ColumnName = "Number" });
            metadataPool.AddPoolItem(new MetadataPoolItem { Item = itemTable, Key = typeof(Item).ToString() });
        }
    }
}
