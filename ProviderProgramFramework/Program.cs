using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviderProgramFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            LogEntity logEntity = new LogEntity();
            logEntity.Content.LogTrackInfo = "Program.Main";
            ILogSaveProvider saveProvider = new LogSaveLocalhostProvider();
            saveProvider.SaveLog(logEntity);
        }
        /// <summary>
        /// 日志实体
        /// </summary>
        public class LogEntity
        {
            /// <summary>
            /// 日志类别
            /// </summary>
            public LogType Type { get; set; }
            /// <summary>
            /// 日志等级
            /// </summary>
            public LogLevel Level { get; set; }
            /// <summary>
            /// 日志内容
            /// </summary>
            public LogContent Content { get; set; }
        }
        public class LogType
        {
            /// <summary>
            /// 应用程序异常
            /// </summary>
            public const string Exception = "Error";
            /// <summary>
            /// 应用程序跟踪
            /// </summary>
            public const string ApplicationTask = "Track";
        }
        public class LogLevel
        {
            /// <summary>
            /// 警告
            /// </summary>
            public const string Warning = "Warning";
            /// <summary>
            /// 严重，需要立即处理
            /// </summary>
            public const string Graveness = "Graveness";
        }
        public class LogContent
        {
            /// <summary>
            /// 跟踪信息，记录此Log的当前程序位置
            /// </summary>
            public string LogTrackInfo { get; set; }
            /// <summary>
            /// Log文本信息
            /// </summary>
            public string Message { get; set; }
        }
        /// <summary>
        /// 保存Log提供程序接口，仅供内部使用
        /// </summary>
        public interface ILogSaveProvider
        {
            /// <summary>
            /// 保存Log
            /// </summary>
            /// <param name="logEntity"></param>
            /// <returns></returns>
            bool SaveLog(LogEntity logEntity);
        }
        /// <summary>
        /// 提供程序基类
        /// </summary>
        public abstract class LogSaveBaseProvider : ILogSaveProvider
        {
            /// <summary>
            /// 实现保存日志方法
            /// </summary>
            /// <param name="logEntity"></param>
            /// <returns></returns>
            public bool SaveLog(LogEntity logEntity)
            {
                if (!this.IsSaveLogWithConfiguration(logEntity))
                    return false;
                if (!this.ValidatorLogEntity(logEntity))
                    return false;
                this.FormatLogContent(logEntity);
                return this.DoSaveLog(logEntity);
            }
            /// <summary>
            /// 判断Log是否是配置文件中配置需要保存的类型
            /// </summary>
            /// <param name="logEntity"></param>
            /// <returns></returns>
            protected virtual bool IsSaveLogWithConfiguration(LogEntity logEntity)
            {
                string LogType = ConfigurationManager.AppSettings["LogType"];
                if (logEntity.Type.Equals(LogType))
                    return true;
                return false;
            }
            /// <summary>
            /// 验证Log实体是否有效
            /// </summary>
            /// <param name="logEntity"></param>
            /// <returns></returns>
            protected virtual bool ValidatorLogEntity(LogEntity logEntity)
            {
                if (logEntity == null || logEntity.Content == null)
                    return false;
                return true;
            }
            /// <summary>
            /// 格式化Log实体中信息内容
            /// </summary>
            /// <param name="logEntity"></param>
            protected virtual void FormatLogContent(LogEntity logEntity)
            {
                //提供程序根据自己需要格式化内容
            }
            /// <summary>
            /// 开始最终的保存动作
            /// </summary>
            /// <param name="logEntity"></param>
            /// <returns></returns>
            protected abstract bool DoSaveLog(LogEntity logEntity);
        }
        public class LogSaveLocalhostProvider : LogSaveBaseProvider
        {
            protected override bool ValidatorLogEntity(LogEntity logEntity)
            {
                //执行默认基础检查
                if (base.ValidatorLogEntity(logEntity))
                {
                    if (string.IsNullOrEmpty(logEntity.Content.LogTrackInfo))
                        return false;
                }
                return true;
            }
            protected override void FormatLogContent(LogEntity logEntity)
            {
                //执行简单的字符串替换工作
                logEntity.Content.Message = logEntity.Content.Message.Replace("\\","--");

            }
            protected override bool DoSaveLog(LogEntity logEntity)
            {
                //保存动作
                return true;
            }
        }
    }
}
