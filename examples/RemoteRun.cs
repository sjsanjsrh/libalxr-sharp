using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LibALXR.examples
{
    internal static class RemoteRun
    {
        public const ushort DefaultPortNo = LibALXR.TrackingServerDefaultPortNo;

        public static void Run(IPAddress clientAddress, CancellationToken cToken)
        {
            try
            {
                if (clientAddress == null)
                {
                    Console.Error.WriteLine($"[alxr-remote] client IP address is null or invalid.");
                    return;
                }

                Task.Run(async () =>
                {
                    while (!cToken.IsCancellationRequested)
                    {
                        try
                        {
                            using (var newTcpClient = await ConnectToServerAsync(clientAddress, DefaultPortNo, cToken))
                            {
                                if (newTcpClient == null || !newTcpClient.Connected)
                                    throw new Exception($"Error connecting to {clientAddress}:{DefaultPortNo}");

                                using (var stream = newTcpClient.GetStream())
                                {
                                    if (stream == null)
                                        throw new Exception($"Error connecting to {clientAddress}:{DefaultPortNo}");

                                    while (!cToken.IsCancellationRequested && stream.CanRead)
                                    {
                                        var newPacket = await ReadALXRFacialEyePacketAsync(stream, cToken);
                                        //
                                        // Your update/process function.
                                        // UpdateData(ref newPacket);
                                        //
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"[alxr-remote] {e.Message}");
                            Console.Error.WriteLine($"[alxr-remote] End of stream!");
                        }
                    }
                }, cToken).Wait();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[alxr-remote] {e.Message}");
            }
        }

        private static async Task<TcpClient> ConnectToServerAsync(IPAddress localAddr, int port, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = new TcpClient();
                    client.NoDelay = true;
                    Console.WriteLine($"[alxr-remote] Attempting to establish a connection at {localAddr}:{port}...");
                    await client.ConnectAsync(localAddr, port).WithCancellation(cancellationToken);
                    Console.WriteLine($"[alxr-remote] Successfully connected to ALXR client: {localAddr}:{port}");
                    return client;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException) && !(ex is ObjectDisposedException))
                {
                    Console.Error.WriteLine($"[alxr-remote] Error connecting to {localAddr}:{port}: {ex.Message}");
                }
                await Task.Delay(1000, cancellationToken);
            }
            return null;
        }

        private static byte[] rawExprBuffer = new byte[Marshal.SizeOf<ALXRFacialEyePacket>()];
        private static async Task<ALXRFacialEyePacket> ReadALXRFacialEyePacketAsync(NetworkStream stream, System.Threading.CancellationToken cancellationToken)
        {
            Debug.Assert(stream != null && stream.CanRead);

            int offset = 0;
            int readBytes = 0;
            do
            {
                readBytes = await stream.ReadAsync(rawExprBuffer, offset, rawExprBuffer.Length - offset, cancellationToken);
                offset += readBytes;
            }
            while (readBytes > 0 && offset < rawExprBuffer.Length &&
                    !cancellationToken.IsCancellationRequested);

            if (offset < rawExprBuffer.Length)
                throw new Exception("Failed read packet.");
            return ALXRFacialEyePacket.ReadPacket(rawExprBuffer);

        }

    }
}
