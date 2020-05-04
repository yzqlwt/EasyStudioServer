using log4net;
using PENet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EasyStudioService
{
    public class FileWatcher
    {

        private DelayAction _action = new DelayAction();
        readonly ILog _log = LogManager.GetLogger(
                                         typeof(FileWatcher));
        public FileWatcher()
        {
            AddWatcher();
        }

        public void AddWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = App.dirPath;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;
            //开始监视
            watcher.EnableRaisingEvents = true;
        }
        private  void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("File changed");
            _log.InfoFormat("File Changed，TIME：{0}", DateTime.Now);
            _action.Debounce(5000, null, () =>
            {
                _log.InfoFormat("Exported，TIME：{0}", DateTime.Now);
                Console.WriteLine("export");
                NetMsg msg = new NetMsg
                {
                    cmd = 1001,
                    desc = "flash导出了symbol",
                };
                MyService.SocketServer.GetSesstionLst().ForEach((session) =>
                {
                    _log.InfoFormat("Send MSG，TIME：{0}", DateTime.Now);
                    session.SendMsg(msg);
                });
                fla2csd.Fla2Csd.tocsd();
            });
        }

    }
}
