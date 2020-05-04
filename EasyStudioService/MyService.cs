using log4net;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PENet;

namespace EasyStudioService
{
    public class MyService
    {
        public static PESocket<ServerSession, NetMsg> SocketServer = new PESocket<ServerSession, NetMsg>();
        private Timer _timer = null;
        readonly ILog _log = LogManager.GetLogger(
                                         typeof(MyService));
        public MyService()
        {
            _timer = new Timer(1000 * 5) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) =>
            {
                _log.InfoFormat("服务正在运行，TIME：{0}", DateTime.Now);
            };
            SocketServer.StartAsServer("127.0.0.1", 12139);
        }
        public void Start() {
            _timer.Start();
            var fileWatcher = new FileWatcher();
        }
        public void Stop() { _timer.Stop(); }
    }
}
