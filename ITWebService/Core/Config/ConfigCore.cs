using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace ITWebService.Core.Config
{
    public abstract class IConfig { }
    public static class ConfigCore
    {
        private static bool IsInit = false;
        public static Task InitTask;
        /// <summary>
        /// wwwroot文件夹目录
        /// </summary>
        public static string WebRootPath { get; set; } = "/app/";
        public static string TempFilePath { get { return Path.Combine(WebRootPath, "TempFile"); } }
        /// <summary>
        /// 程序工作目录
        /// </summary>
        public static string ITWebServicePath { get; set; } = System.IO.Directory.GetCurrentDirectory();
        public static readonly string ConfigPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Configs");
        private static List<Type> ServiceConfig { get; set; } = new List<Type>();
        public static Dictionary<string, IConfig> GetConfig { get; set; } = new();
        /// <summary>
        /// 配置文件初始化
        /// </summary>
        public static async Task ConfigInit()
        {
            await LoadingConfigAsync();
            IsInit = true;
        }
        public static async Task LoadingConfigAsync()
        {

            foreach (var config in ServiceConfig)
            {
                Console.WriteLine($"ServiceName:{config.Name}");
                var filepath = Path.Combine(ConfigPath, string.Format($"{config.Name}.json"));
                Console.WriteLine(filepath);
                try
                {
                    IConfig con = (IConfig)JsonSerializer.Deserialize(await LoadingJsonfile(filepath), config);
                    GetConfig.Add(config.Name, con);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        public static async Task<string> LoadingJsonfile(string path)
        {
            if (File.Exists(path))
            {
                return await File.ReadAllTextAsync(path);
            }
            throw new Exception($"Not find file:{path}");

        }
        public static void AddConfig<T>() where T : IConfig
        {
            ServiceConfig.Add(typeof(T));
        }
        public static T GetConfigItem<T>(string ConfigName) where T : IConfig
        {
            try
            {
                if (GetConfig.ContainsKey(ConfigName))
                {
                    return GetConfig[ConfigName] as T;
                }
                return null;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return null;
            }

        }
        public static T GetConfigItem<T>() where T : IConfig
        {
            try
            {
                string ConfigName = typeof(T).Name;
                if (GetConfig.ContainsKey(ConfigName))
                {
                    return GetConfig[ConfigName] as T;
                }
                return null;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return null;
            }

        }
        /// <summary>
		/// 刷新配置文件
		/// </summary>
        public static void Refresh()
        {
            ConfigInit().Wait();
        }

    }
}


