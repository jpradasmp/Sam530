Imports System.Net
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Security.Claims
Imports System.Text
Imports System.Threading
Imports Flexinets.Radius.Core
Imports Microsoft.Extensions.Logging
Imports Serilog
Imports Serilog.Formatting.Display
Imports Serilog.Sinks.Syslog
Imports DataStore

Public Class RSyslog
    ''' <summary>
    ''' Logger expuesto para uso externo (solo lectura)
    ''' </summary>
    Public ReadOnly Property Logger As Serilog.ILogger
        Get
            Return _logger
        End Get
    End Property

    Private _logger As Serilog.ILogger = Serilog.Log.Logger ' Logger neutro por defecto

    Public Enum ProtocolType
        UDP = 0
        TCP
        LOCAL
    End Enum

    Private Property EndPoint As String
    Private Property Protocol As ProtocolType = ProtocolType.UDP

    Private ReadOnly Store As New DataStore.DbStore()
    Private syslogSettings As DataStore.Models.Syslog

    Public Async Function Start() As Task
        Try
            syslogSettings = Await Store.GetSyslogAsync(1)
            If syslogSettings.Active = Models.Active.ACTIVATED Then
                EndPoint = syslogSettings.IPAddress
                Protocol = syslogSettings.ProtocolType

                Dim config = New LoggerConfiguration()

                Select Case Protocol
                    Case ProtocolType.UDP
                        _logger = config.WriteTo.UdpSyslog(EndPoint).CreateLogger()
                    Case ProtocolType.TCP
                        Dim tcpConfig As New SyslogTcpConfig With {
                            .Host = EndPoint,
                            .Port = 6514,
                            .Formatter = New Rfc5424Formatter(facility:=Facility.Local0, templateFormatter:=New MessageTemplateTextFormatter("TCP: {Message}")),
                            .Framer = New MessageFramer(FramingType.OCTET_COUNTING),
                            .UseTls = False
                        }
                        _logger = config.WriteTo.TcpSyslog(tcpConfig).CreateLogger()
                    Case ProtocolType.LOCAL
                        If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then
                            _logger = config.WriteTo.LocalSyslog(outputTemplate:="Local: {Message}").CreateLogger()
                        Else
                            Console.WriteLine("LocalSyslog solo está soportado en Linux.")
                            _logger = config.CreateLogger() ' Fallback
                        End If
                End Select
            Else
                _logger = New LoggerConfiguration().CreateLogger() ' Logger vacío
            End If
        Catch ex As Exception
            Console.WriteLine("Error inicializando RSyslog: " & ex.Message)
            Throw
        End Try
    End Function
End Class


