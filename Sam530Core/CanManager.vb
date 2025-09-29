
Imports System.Collections.Concurrent
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports SocketCANSharp


Public Class CanManager

    Private Shared ReadOnly concurrentQueue As BlockingCollection(Of CanFrame) = New BlockingCollection(Of CanFrame)()

    Function Start() As Boolean
        Using socketHandle As SafeFileDescriptorHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW)
            If socketHandle.IsInvalid Then
                'Error
                Console.WriteLine("Failed to create socket.")
                Return False
            End If

            Dim ifr = New Ifreq("vcan0")
            Dim ioctlResult As Integer = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr)
            If ioctlResult = -1 Then
                Console.WriteLine("Failed to look up interface by name.")
                Return False
            End If

            Dim addr = New SockAddrCan(ifr.IfIndex)
            Dim bindResult As Integer = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(GetType(SockAddrCan)))

            If bindResult = -1 Then
                Console.WriteLine("Failed to bind to address.")
                Return False
            End If

            Dim frameSize As Integer = Marshal.SizeOf(GetType(CanFrame))
            Console.WriteLine("Sniffing vcan0...")

            'Task.Run(() >= PrintLoop());
            '    While (True)
            '    {
            '        var readFrame = New CanFrame();
            '        Int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, frameSize); 
            '        If (nReadBytes > 0) Then
            '                            {
            '            concurrentQueue.Add(readFrame);
            '        }
            '    }


        End Using

        Return True

    End Function


    Private Shared Sub PrintLoop()
        While True
            Dim readFrame As CanFrame = ConcurrentQueue.Take()

            If (readFrame.CanId And CUInt(CanIdFlags.CAN_RTR_FLAG)) <> 0 Then
                Console.WriteLine($"{SocketCanConstants.CAN_ERR_MASK And readFrame.CanId}   [{readFrame.Length}]  RTR")
            Else
                Console.WriteLine($"{SocketCanConstants.CAN_ERR_MASK And readFrame.CanId}   [{readFrame.Length}]  {BitConverter.ToString(readFrame.Data.Take(readFrame.Length).ToArray()).Replace("-", " ")}")
            End If
        End While
    End Sub


End Class
