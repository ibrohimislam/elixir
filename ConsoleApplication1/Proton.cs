using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Elixir
{
    struct worklist
    {
        public List<string> filepaths;
        public long size;
    };

    class Proton : IDisposable
    {
        private CancellationTokenSource cts;

        Electron electron;

        public void Dispose()
        {
            cts.Cancel();
            GC.SuppressFinalize(this);
        }

        public Proton(Electron _electron)
        {
            electron = _electron;
            cts = new CancellationTokenSource();
        }

        public async Task Do(string command)
        {
            worklist result = await dls(command, 0, 0);

            double progress = 0.0;

            //electron.Emit(result.filepaths.Count().ToString());

            foreach (string file_path in result.filepaths)
            {
                FileInfo f = new FileInfo(file_path);
                progress += (double)f.Length / (double)result.size;
                System.Threading.Thread.Sleep((int)((double)f.Length / (double)result.size * 10000.0));

                electron.Emit("1 " + progress.ToString("0.000000", CultureInfo.InvariantCulture));
            }
        }

        public async Task<worklist> dls(string path, int depth = 0, int limit = 0)
        {
            worklist result;
            result.size = 0;
            result.filepaths = new List<string>();

            if ((depth < limit) || (limit == 0))
            {
                try
                {
                    string[] array_file = Directory.GetFiles(path);

                    foreach (string file_path in array_file)
                    {
                        if (cts.Token.IsCancellationRequested) return result;

                        FileInfo f = new FileInfo(file_path);
                        result.size += f.Length;
                        result.filepaths.Add(file_path);
                    }

                    string[] array_directory = Directory.GetDirectories(path);
                    foreach (string directory_path in array_directory)
                    {
                        if (cts.Token.IsCancellationRequested) return result;

                        worklist child_result = await dls(directory_path, depth+1, limit);
                        result.filepaths = result.filepaths.Concat(child_result.filepaths).ToList();
                        result.size += child_result.size;
                    }
                }
                catch (Exception e)
                {
                    electron.Emit("error " + e.Message).Wait();
                }
            }

            return result;
        }
    }
}
