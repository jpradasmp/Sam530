Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Diagnostics
Imports System.Net.Sockets
Imports System.IO
Imports Microsoft.Extensions.Logging
Imports Microsoft.EntityFrameworkCore
Imports System.Runtime
Imports System.Runtime.InteropServices
Imports DataStore.Models


''' <summary>
''' Implements linux commands
''' Current user must be a sudoer without protocol (Refer to web on 'How to...') or document de selba a teams Netpoint/debian
''' Command 'route' must be enabled, so net-tools must be installed in linux system. Reference in document de selba a teams Netpoint/debian
''' </summary>
Public Class LinuxManager

    ''' <summary>
    ''' Logger component
    ''' </summary>
    ''' <returns></returns>
    Private Shared Property _logger As ILogger

    Public Const FACTORY_IP As String = "192.168.1.117"
    Public Const FACTORY_NETMASK As String = "255.255.255.1"
    Public Const FACTORY_GATEWAY As String = "192.168.1.1"

    Public Shared Function SetLogger(Log As ILogger)
        _logger = Log
        Return True
    End Function


    ''' <summary>
    ''' Set Ip, Neworkmask and Gateway
    ''' </summary>
    ''' <param name="Ip"></param>
    ''' <param name="NetMask"></param>
    ''' <param name="Gatewaty"></param>
    ''' <returns></returns>
    Public Shared Function SetIp(Ip As String, NetMask As String) As Boolean
        Dim Cidr = NetMaskToCIDR(NetMask)

        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c ""sudo ip addr add {Ip}/{Cidr} dev eth0"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function

    ''' <summary>
    ''' Removes Ip 
    ''' </summary>
    ''' <param name="Ip"></param>
    ''' <returns></returns>
    Public Shared Function RemoveIp(Ip As String, NetMask As String)

        Dim Cidr = NetMaskToCIDR(NetMask)

        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c ""sudo ip addr delete {Ip}/{Cidr} dev eth0"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function

    ''' <summary>
    ''' Set Gateway, Neworkmask and Gateway
    ''' </summary>
    ''' <param name="Ip"></param>
    ''' <param name="NetMask"></param>
    ''' <param name="Gatewaty"></param>
    ''' <returns></returns>
    Public Shared Function SetGateway(Ip As String) As Boolean

        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c ""sudo route add default gw {Ip} eth0"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function

    ''' <summary>
    ''' Set Gateway, Neworkmask and Gateway
    ''' </summary>
    ''' <param name="Ip"></param>
    ''' <param name="NetMask"></param>
    ''' <param name="Gatewaty"></param>
    ''' <returns></returns>
    Public Shared Function RemoveGateway(Ip As String) As Boolean

        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c ""sudo route delete default gw {Ip} eth0"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function

    Public Shared Function NetMaskToCIDR(NetMask As String) As Integer
        Dim Data = NetMask.Split("."c)
        Dim CIDR = 0

        For i = 0 To 3
            Dim k = &H80
            For j = 0 To 7
                If (Data(i) And k) Then CIDR += 1
                k >>= 1
            Next
        Next

        Return CIDR
    End Function


    ''' <summary>
    ''' Get the network IpAddress and return de LSB to use as identifier
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetIPAddress() As List(Of IPAddress)
        Dim IpAddresList As New List(Of IPAddress)
        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        For Each adapter As NetworkInterface In adapters
            _logger.LogInformation($"Adapter: {adapter.Id}")
            If adapter.Id = "eth1" Then
                Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()
                For Each ip In adapterProperties.UnicastAddresses
                    If ip.Address.AddressFamily = AddressFamily.InterNetwork And Not IPAddress.IsLoopback(ip.Address) Then
                        IpAddresList.Add(ip.Address)
                    End If
                Next
            End If
        Next

        If IpAddresList.Count = 0 Then
            IpAddresList.Add(New IPAddress("127.0.0.1"))
        End If



        Return IpAddresList
    End Function

    ''' <summary>
    ''' Get the network IpAddress and return de LSB to use as identifier
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetGateways() As List(Of IPAddress)
        Dim IpAddresList As New List(Of IPAddress)
        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        For Each adapter As NetworkInterface In adapters
            Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()
            For Each ip In adapterProperties.GatewayAddresses
                If ip.Address.AddressFamily = AddressFamily.InterNetwork And Not IPAddress.IsLoopback(ip.Address) Then
                    IpAddresList.Add(ip.Address)
                End If
            Next
        Next

        If IpAddresList.Count = 0 Then
            IpAddresList.Add(New IPAddress("127.0.0.1"))
        End If

        Return IpAddresList
    End Function

    Public Shared Function Set_eth0(NetworkSetup As Setups) As Boolean
        'Adjust Ip and Gateway (Only Linux System)

        If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then
            _logger.LogInformation($"Linux: Updating IP Address. Firmware Path is {Settings.FirmwarePath}")
            Dim Template = Path.Combine(Settings.FirmwarePath, "eth1.static.template")
            Dim iface = Path.Combine(Settings.FirmwarePath, "eth1")
            Dim Fr As New FileStream(Template, FileMode.Open)
            Dim Fw As New FileStream(iface, FileMode.Create)
            Dim Sr As New StreamReader(Fr)
            Dim Sw As New StreamWriter(Fw)

            Dim Data = Sr.ReadToEnd
            Data = Data.Replace("[ADDRESS]", NetworkSetup.IpAddress)
            Data = Data.Replace("[SUBNET]", NetworkSetup.SubnetMask)
            Data = Data.Replace("[GATEWAY]", NetworkSetup.Gateway)

            Sw.Write(Data)

            Sw.Close()
            Sr.Close()

            LinuxManager.CopyInterface(iface)
            Return True
        Else
            _logger.LogError($"In windows IP Address can't be set")
            Return False
        End If

    End Function



    Public Shared Function CopyInterface(File As String) As Boolean


        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c ""sudo cp {File} /etc/network/interfaces.d"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function

    Public Shared Function Reboot() As Boolean


        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        With startInfo
            .FileName = "/bin/bash"
            .Arguments = $"-c "" sleep 5 &&  sudo reboot &"""
            .UseShellExecute = False
            .RedirectStandardOutput = True
        End With
        Dim Proc As New Process
        Proc.StartInfo = startInfo
        Proc.Start()
        Do While Not Proc.StandardOutput.EndOfStream
            Console.WriteLine(Proc.StandardOutput.ReadLine())
        Loop

        Return True

    End Function


    Public Shared Function UpdateTime(Time As String)
        Try
            Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
            Dim Start As Integer = Time.IndexOf("[") + 1
            Dim Year = Time.Substring(Start, 4) : Start += 4
            Dim Month = Time.Substring(Start, 2) : Start += 2
            Dim Day = Time.Substring(Start, 2) : Start += 2
            Dim Hour = Time.Substring(Start, 2) : Start += 2
            Dim Minute = Time.Substring(Start, 2) : Start += 2
            Dim Second = Time.Substring(Start, 2) : Start += 2

            Dim Now = New DateTime(Year, Month, Day, Hour, Minute, Second)
            If Math.Abs(DateDiff(DateInterval.Second, Now, Date.Now)) < 5 Then
                Return True
            End If

            _logger.LogWarning($"Linux Time Updated To {Year}-{Month}-{Day} {Hour}:{Minute}:{Second}")


            With startInfo
                .FileName = "/bin/bash"
                .Arguments = $"-c ""sudo timedatectl set-time '{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}'"""
                .UseShellExecute = False
                .RedirectStandardOutput = True
            End With
            Dim Proc As New Process
            Proc.StartInfo = startInfo
            Proc.Start()
            Do While Not Proc.StandardOutput.EndOfStream
                Console.WriteLine(Proc.StandardOutput.ReadLine())
            Loop

            Return True

        Catch ex As Exception
            _logger.LogError($"Can't set Date and Time on Linux. Cause {ex.Message}")
            Return False
        End Try
    End Function


    ''' <summary>
    ''' Validate if firmware is Valid
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function ValidateFirmware() As Boolean
        'Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        'Try

        '    With startInfo
        '        .FileName = "/bin/bash"
        '        .Arguments = $"-c ""sudo sh {NetPointCore.Settings.FirmwarePath}emupdatefirmware.sh"""
        '        .UseShellExecute = False
        '        .RedirectStandardOutput = True
        '    End With

        '    Dim Proc As New Process
        '    Proc.StartInfo = startInfo
        '    Proc.Start()
        '    Proc.WaitForExit()

        '    Dim MainFiles As New List(Of String) From {$"NetPointCore.dll", "NetpointWeb.dll", "appsettings.json"}

        '    'Look for main files presence
        '    For Each f In MainFiles
        '        If Not IO.File.Exists($"{NetPointCore.Settings.FirmwarePath}publish/{f}") Then
        '            _logger.LogError($"Error on Updating Firmware. File  is missing")
        '            Return False
        '        End If
        '    Next

        '    Return True
        'Catch ex As Exception
        '    _logger.LogError(ex, $"Error on validating firmware. Cause: {ex.Message}")
        '    Return False
        'End Try

        Return False

    End Function

    ''' <summary>
    ''' Validate if firmware is Valid
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function ApplyFirmware() As Boolean
        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        Try

            ''Copy new version
            'With startInfo
            '    .FileName = "/bin/bash"
            '    .Arguments = $"-c ""sudo sh {NetPointCore.Settings.FirmwarePath}emapplyfirmware.sh"""
            '    .UseShellExecute = False
            '    .RedirectStandardOutput = True
            'End With

            'Dim Proc As New Process
            'Proc.StartInfo = startInfo
            'Proc.Start()
            'Proc.WaitForExit()

            'Copy Resources Files (if exists)
            'If Not IO.Directory.Exists($"{NetPointCore.Settings.FirmwarePath}publish/selba") Then
            '    Dim Files = IO.Directory.GetFiles($"{NetPointCore.Settings.FirmwarePath}publish/selba")
            '    For Each f In Files
            '        File.Copy(f, $"{NetPointCore.Settings.FirmwarePath}{Path.GetFileName(f)}", True)
            '    Next
            'End If


            Return True
        Catch ex As Exception
            _logger.LogError(ex, $"Error on Applying firmware. Cause: {ex.Message}")

            Return False
        End Try


    End Function


End Class


