using System;
using System.Collections;
using System.Collections.Generic;
using ITWebService.Core;
namespace ITWebService.Core
{
    public  class ServiceManage
    {
        private static ServiceManage serviceManage;
        public static ServiceManage SM { get; }
        public static string Start()
        {
            if ( serviceManage== null)
            {
                serviceManage= new ServiceManage();
                return $"{typeof(ServiceManage).Name}启动成功";
            }
            return $"{typeof(ServiceManage).Name} 已经启动";

        }
        public ServiceManage()
        {
            Console.WriteLine($"{typeof(ServiceManage).Name} 启动");
        }
        public static void RegisterService<T>()where T:WebService<T>,IWebService,new()
        {
            /*注册新的Service,返回Serviceid,
             * 写入Service记录库 
             * 
             */
        }

     
       
    }

}
