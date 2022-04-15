using System;
namespace ITWebService.Core
{
    public interface IWebService : IDisposable
    {
        /// <summary>
        /// 启动服务
        /// </summary>
        protected void StartService();
        /// <summary>
        /// 暂停服务
        /// </summary>
        protected void StopService();
        /// <summary>
        /// 重启服务
        /// </summary>
        protected void ReStartService();
    }
    public abstract class WebService<T> where T : IWebService, new()
    {
        private static T Service { get; set; }
        public static T GetService()
        {
            if (null != Service)
                return Service;
            return Service = new T();
        }
    }
}

