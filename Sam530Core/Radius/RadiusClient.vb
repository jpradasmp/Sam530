Imports System.Collections.Concurrent
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks
Imports Flexinets.Radius.Core

''' <summary>
''' Create a radius client which sends and receives responses on localEndpoint
''' </summary>
Public Class RadiusClient
    Implements IDisposable

    Private ReadOnly _udpClient As UdpClient
    Private _receiveLoopTask As Task
    Private ReadOnly _cancellationTokenSource As New CancellationTokenSource()

    Private ReadOnly _pendingRequests As New ConcurrentDictionary(Of PendingRequest, TaskCompletionSource(Of UdpReceiveResult))()
    Private ReadOnly _radiusPacketParser As IRadiusPacketParser

    Private Structure PendingRequest
        Public ReadOnly Identifier As Byte
        Public ReadOnly RemoteEndpoint As IPEndPoint

        Public Sub New(identifier As Byte, remoteEndpoint As IPEndPoint)
            Me.Identifier = identifier
            Me.RemoteEndpoint = remoteEndpoint
        End Sub
    End Structure

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New(localEndpoint As IPEndPoint, radiusPacketParser As IRadiusPacketParser)
        _udpClient = New UdpClient(localEndpoint)
        _radiusPacketParser = radiusPacketParser
    End Sub

    ''' <summary>
    ''' Send a packet and wait for response with default timeout of 3 seconds
    ''' </summary>
    Public Async Function SendPacketAsync(packet As IRadiusPacket, sharedSecret As Byte(), remoteEndpoint As IPEndPoint) As Task(Of IRadiusPacket)
        Return Await SendPacketAsync(packet, sharedSecret, remoteEndpoint, TimeSpan.FromSeconds(10))
    End Function

    ''' <summary>
    ''' Send a packet and wait for response with specified timeout
    ''' </summary>
    Public Async Function SendPacketAsync(packet As IRadiusPacket, sharedSecret As Byte(), remoteEndpoint As IPEndPoint, timeout As TimeSpan) As Task(Of IRadiusPacket)
        ' Iniciar un bucle de recepción si aún no se ha iniciado
        If _receiveLoopTask Is Nothing Then
            _receiveLoopTask = Task.Factory.StartNew(AddressOf StartReceiveLoopAsync, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
        End If

        Dim completionSource As New TaskCompletionSource(Of UdpReceiveResult)()
        Dim pendingRequest As New PendingRequest(packet.Identifier, remoteEndpoint)

        If Not _pendingRequests.TryAdd(pendingRequest, completionSource) Then
            Throw New InvalidOperationException($"There is already a pending receive with id {packet.Identifier}")
        End If

        Await _udpClient.SendAsync(_radiusPacketParser.GetBytes(packet), remoteEndpoint).ConfigureAwait(False)

        If Await Task.WhenAny(completionSource.Task, Task.Delay(timeout)).ConfigureAwait(False) Is completionSource.Task Then
            Return _radiusPacketParser.Parse(completionSource.Task.Result.Buffer, sharedSecret, packet.Authenticator)
        End If

        If _pendingRequests.TryRemove(pendingRequest, Nothing) Then
            completionSource.SetCanceled()
        End If

        'Throw New TimeoutException($"Receive response for id {packet.Identifier} timed out after {timeout}")
        Return Nothing
    End Function

    ''' <summary>
    ''' Receive packets in a loop and complete tasks based on identifier
    ''' </summary>
    Private Async Function StartReceiveLoopAsync() As Task
        While Not _cancellationTokenSource.IsCancellationRequested
            Try
                Dim response As UdpReceiveResult = Await _udpClient.ReceiveAsync(_cancellationTokenSource.Token).ConfigureAwait(False)
                Dim pendingRequest As New PendingRequest(response.Buffer(1), response.RemoteEndPoint)

                Dim tcs As TaskCompletionSource(Of UdpReceiveResult) = Nothing
                If _pendingRequests.TryRemove(pendingRequest, tcs) Then
                    tcs.SetResult(response)
                End If
            Catch ex As ObjectDisposedException
                ' This is thrown when udpclient is disposed, can be safely ignored
            End Try
        End While
    End Function

    ''' <summary>
    ''' Dispose
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        GC.SuppressFinalize(Me)
        _cancellationTokenSource.Cancel()

        If _receiveLoopTask?.IsCompleted OrElse _receiveLoopTask?.IsCanceled OrElse _receiveLoopTask?.IsFaulted Then
            _receiveLoopTask.Dispose()
        End If

        _udpClient.Dispose()
    End Sub

End Class

