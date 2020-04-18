using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ISNAP_4.Models.Entities;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace ISNAP_4.Models.PythonSolver
{
    public class PythonSimplexSolver : ISimplexSolver
    {
        
        private readonly string _pythonPath = "C:\\Users\\1\\AppData\\Local\\Programs\\Python\\Python38-32\\python.exe";
        private readonly string _pythonSolverPath = "C:\\Users\\1\\RiderProjects\\ISNAP_4\\ISNAP_4\\Models\\PythonSolver\\optimizator.py";

        private Socket _socket;

        public PythonSimplexSolver()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = _pythonSolverPath
            };
            //Process.Start(processStartInfo);
            InitConnectionAsync();
        }
        
        public Vector<double> Solve(SimplexTable table)
        {
            var request = new Request
            {
                A = table.Coefficients.ToRowArrays(),
                B = table.Limitations.ToArray(),
                C = table.Target.ToArray()
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            
            while (!_socket.Connected) Thread.Sleep(100);

            var encoding = new UTF8Encoding();
            
            var buffer = encoding.GetBytes(jsonRequest);
            _socket.Send(buffer);
            
            buffer = new byte[1024];
            _socket.Receive(buffer);

            var jsonResponse = encoding.GetString(buffer.Where(it => it != 0).ToArray());

            var response = JsonSerializer.Deserialize<Response>(jsonResponse);

            return Vector<double>.Build.DenseOfArray(response.X);
        }

        private void InitConnectionAsync()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5265);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var task = new Task(() =>
            {
                while (!_socket.Connected)
                {
                    try
                    {
                        _socket.Connect(endPoint);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
            task.Start();
        }
    }
}