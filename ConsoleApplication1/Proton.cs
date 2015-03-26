using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Elixir
{
    class Proton
    {
        Electron electron;

        public Proton(Electron _electron)
        {
            electron = _electron;
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
                        electron.Emit(file_path);
                    }

                    string[] array_directory = Directory.GetDirectories(path);
                    foreach (string directory_path in array_directory)
                    {
                        await dls(directory_path, depth + 1, limit);
                    }
                }
                catch (System.UnauthorizedAccessException e)
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
