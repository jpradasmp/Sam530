Imports System.Net
Imports System.Text
Imports System.Threading.Channels
Imports System.Threading.Tasks
Imports DataStore
Imports Flexinets.Radius
Imports Flexinets.Radius.Core
Imports Microsoft.Extensions.Logging
Imports Serilog

Public Class Radius

    Private ReadOnly _channel As Channel(Of RadiusRequestItem) = Channel.CreateUnbounded(Of RadiusRequestItem)()
    Private ReadOnly _logger As Serilog.ILogger
    Private ReadOnly _store As New DbStore()

    Private _settings As DataStore.Models.Radius

    Public Sub New(logger As Serilog.ILogger)
        _logger = logger
        StartProcessing()
    End Sub

    ''' <summary>
    ''' Encola una petición de autenticación y espera el resultado
    ''' </summary>
    Public Async Function AuthenticateAsync(username As String, password As String) As Task(Of Boolean)
        Dim request = New RadiusRequestItem With {
            .Username = username,
            .Password = password,
            .CompletionSource = New TaskCompletionSource(Of Boolean)()
        }

        Await _channel.Writer.WriteAsync(request)
        Return Await request.CompletionSource.Task
    End Function

    ''' <summary>
    ''' Hilo principal que consume las peticiones del canal
    ''' </summary>
    Private Sub StartProcessing()
        Task.Run(Async Function()
                     Try
                         _settings = Await _store.GetRadiusAsync(1)
                         If _settings.Active = Models.ActiveRadius.DEACTIVATED Then
                             _logger?.Warning("El servidor RADIUS no está activo.")
                             Return
                         End If

                         Dim radiusEndpoint = New IPEndPoint(IPAddress.Parse(_settings.IPAddress), 1812)
                         Dim sharedSecretBytes = Encoding.UTF8.GetBytes(_settings.SharedSecret)

                         Dim radiusLoggerFactory As New LoggerFactory()
                         Dim radiusLogger = radiusLoggerFactory.CreateLogger(Of RadiusPacketParser)()

                         While Await _channel.Reader.WaitToReadAsync()
                             Dim item = Await _channel.Reader.ReadAsync()

                             Try
                                 Using client As New RadiusClient(New IPEndPoint(IPAddress.Any, 0),
                                         New RadiusPacketParser(radiusLogger,
                                         RadiusDictionary.Parse(DefaultDictionary.RadiusDictionary)))

                                     Dim packet = New RadiusPacket(PacketCode.AccessRequest, 1, _settings.SharedSecret)
                                     packet.AddMessageAuthenticator()
                                     packet.AddAttribute("User-Name", item.Username)
                                     packet.AddAttribute("User-Password", item.Password)

                                     _logger?.Information("Radius: Enviando petición para usuario {User}", item.Username)

                                     Dim response = Await client.SendPacketAsync(packet, sharedSecretBytes, radiusEndpoint)

                                     If response?.Code = PacketCode.AccessAccept Then
                                         _logger?.Information("Radius: Acceso concedido a {User}", item.Username)
                                         item.CompletionSource.TrySetResult(True)
                                     Else
                                         _logger?.Information("Radius: Acceso denegado a {User}", item.Username)
                                         item.CompletionSource.TrySetResult(False)
                                     End If

                                 End Using

                             Catch ex As Exception
                                 _logger?.Error(ex, "Error al procesar petición RADIUS para {User}", item.Username)
                                 item.CompletionSource.TrySetException(ex)
                             End Try
                         End While

                     Catch ex As Exception
                         _logger?.Error(ex, "Error crítico en el bucle principal de Radius")
                     End Try
                 End Function)
    End Sub

    ''' <summary>
    ''' Estructura de datos para encapsular una petición RADIUS
    ''' </summary>
    Private Class RadiusRequestItem
        Public Property Username As String
        Public Property Password As String
        Public Property CompletionSource As TaskCompletionSource(Of Boolean)
    End Class

End Class

