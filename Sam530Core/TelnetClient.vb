Imports System.Threading
Imports System.Net.Sockets
Imports Microsoft.Extensions.Logging

Public Class TelnetClient
    ''' <summary>
    ''' Logger component
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property logger As ILogger

    ''' <summary>
    ''' Size of receive buffer
    ''' </summary>
    Private Const BUFFER_SIZE As Integer = 1024

    ''' <summary>
    ''' Is client connected
    ''' </summary>
    Dim _IsConnected As Boolean
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _IsConnected
        End Get
    End Property

    ''' <summary>
    ''' The low level socket
    ''' </summary>
    Public Property Socket As Socket = Nothing
    ''' <summary>
    ''' Net Protocol
    ''' </summary>
    Private Property Protocol As IProtocol


    ''' <summary>
    ''' Receive Buffer
    ''' </summary>
    Private Buffer(BUFFER_SIZE) As Byte
    ''' <summary>
    ''' Controls Socket Activity
    ''' </summary>
    Private tmrPollSocket As Timer

#Region "Events"
    ''' <summary>
    ''' Fires when disconnection is detected on client side
    ''' </summary>
    ''' <param name="Sender"></param>
    Public Event OnSocketClosed(Sender As Object)
    ''' <summary>
    ''' Fires on Client Disconnected
    ''' </summary>
    ''' <param name="Sender"></param>
    Public Event OnDisconnected(Sender As Object)
    ''' <summary>
    ''' Fire on complete packet received
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="Packet"></param>
    Public Event OnPacketReceived(Sender As Object, Packet As List(Of Byte))
#End Region


    Public Sub New()
        Protocol = New ProtocolSelba
        Protocol.SetAddress(1)
    End Sub


    Sub [Stop]()
        Try
            tmrPollSocket.Change(Timeout.Infinite, Timeout.Infinite)
            Socket.Shutdown(SocketShutdown.Both)
            Socket.Close()
        Catch ex As Exception

        Finally
            _IsConnected = False
            RaiseEvent OnSocketClosed(Me)
            RaiseEvent OnDisconnected(Me)
        End Try


    End Sub

    Sub Start()
        _IsConnected = True
        tmrPollSocket = New Timer(New TimerCallback(AddressOf PollSocket), Socket, 5000, 5000)
        Socket.BeginReceive(Buffer, 0, BUFFER_SIZE, 0, New AsyncCallback(AddressOf OnChannelRead), Me)
    End Sub



    Public Sub OnChannelRead(ByVal ar As IAsyncResult)
        'Dim content As String = String.Empty

        ' Retrieve the state object and the handler socket  
        ' from the asynchronous state object.  
        'Dim Client As SocketClient = CType(ar.AsyncState, SocketClient)

        ' Read data from the client socket.   
        Dim bytesRead As Integer = Socket.EndReceive(ar)

        If bytesRead > 0 Then
            ' There  might be more data, so store the data received so far.  

            Try
                Dim Data = System.Text.Encoding.Default.GetString(Buffer)
                If Protocol.Decode(Buffer.ToList) Then
                    RaiseEvent OnPacketReceived(Me, Protocol.RxPacket)
                End If

                'wait for next data
                Array.Clear(Buffer, 0, BUFFER_SIZE)
                Socket.BeginReceive(Buffer, 0, BUFFER_SIZE, 0, New AsyncCallback(AddressOf OnChannelRead), Me)
            Catch ex As Exception
                _logger.LogError("Bad Data Received. Decoding aborted")
            End Try
        Else
            'Log.Info("Client Disconnected")
            [Stop]()
        End If
    End Sub

    Sub PollSocket(Sck As Socket)
        Try
            If Not (Sck.Poll(1, SelectMode.SelectRead) And Sck.Available = 0) Then
                ' Trace.TraceInformation("Client is Connected")
            Else
                'Trace.TraceWarning("Client Disconnected")
                _logger.LogInformation("Client Disconnected")
                [Stop]()
            End If
        Catch ex As Exception
            'Trace.TraceWarning("Client Disconnected")
            _logger.LogInformation("Client Disconnected")
            [Stop]()
        End Try
    End Sub

    Public Sub Send(ByVal data As List(Of Byte))

        ' Convert the string data to byte data u4sing ASCII encoding.  
        Dim byteData As Byte() = data.ToArray

        ' Begin sending the data to the remote device.  
        Socket.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf OnChannelSent), Me)

    End Sub

    Public Sub Send(Data As String)


        Dim byteData As Byte() = System.Text.Encoding.Default.GetBytes(" " & Data)

        ' Begin sending the data to the remote device.  
        Socket.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf OnChannelSent), Me)

    End Sub

    Private Sub OnChannelSent(ByVal ar As IAsyncResult)
        ' Retrieve the socket from the state object.  
        'Dim handler As Socket = CType(ar.AsyncState, Socket)

        ' Complete sending the data to the remote device.  
        Dim bytesSent As Integer = Socket.EndSend(ar)
        'Trace.TraceInformation("Sent {0} bytes to client.", bytesSent)


    End Sub 'SendCallback 

End Class
