using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace Synchronization
{
    class Cpublic
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(Program));//log4net日志
    }
}
