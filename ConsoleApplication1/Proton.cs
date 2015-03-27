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

namespace Elixir
{
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
            dls(command, 0, 0);
        }

        public async Task dls(string path, int depth = 0, int limit = 0)
        {
            if ((depth < limit) || (limit == 0))
            {
                try
                {
                    string[] array_file = Directory.GetFiles(path);

                    foreach (string file_path in array_file)
                    {
                        if (cts.Token.IsCancellationRequested) return;
                        await electron.Emit(file_path);
                    }

                    string[] array_directory = Directory.GetDirectories(path);
                    foreach (string directory_path in array_directory)
                    {
                        if (cts.Token.IsCancellationRequested) return;
                        await dls(directory_path, depth + 1, limit);
                    }
                }
                catch (System.UnauthorizedAccessException e)
                {
                    electron.Emit(e.Message).Wait();
                }
                catch (Exception e)
                {
                    electron.Emit(e.Message).Wait();
                }
                finally
                {

                }
            }
        }
    }
}
