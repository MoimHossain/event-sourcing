
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SuperNova.Shared.Configs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Tests
{
    public class TestConfigStore : ConfigStore
    {
        private static Dictionary<string, string> _keyValues = new Dictionary<string, string>
            {
                { "StorageConfig:AccountName", "moimteststorage" },
                { "StorageConfig:AccountKey", "V+WEimdxossZaysmE0BeNNwJBXOPFGRBjebQCgHYvwg1PX5t0+74nd2BGWeiPKF0cV1HsYsZgDnROM5L2K38tg==" },
                { "StorageConfig:DocumentEndpoint", "https://moim.documents.azure.com:443/" },
                { "StorageConfig:DocumentKey", "EBVLVutQ35KhcFKJaDHZh0UhTAUp6ufwzNpz6BzsD1u7nFO2xh9Hi3UV9zzBLttBZx41Cu91lkTVeggG1Qrrbw==" },
            };

        public TestConfigStore(ILoggerFactory loggerFactory)
            : base(true, (key) => _keyValues[key], loggerFactory)
        {

        }
    }
}
