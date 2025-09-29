Imports System.Data
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Threading
Imports Microsoft.Extensions.Logging


Public Class TelnetServer

    ''' <summary>
    ''' The logger
    ''' </summary>
    Public Shared logger As ILogger

    ''' <summary>
    ''' Server Name. Must be AccessPoint Instance Name from WorkerRole
    ''' </summary>
    ''' <returns></returns>
    Public Property EndPoint As String

    ''' <summary>
    ''' Listen Port
    ''' </summary>
    ''' <returns></returns>
    Public Property Port As Integer

    ''' <summary>
    ''' Serial COM Port
    ''' </summary>
    ''' <returns></returns>
    Public Property COMPort As String

    ''' <summary>
    ''' Listener
    ''' </summary>
    Private Listener As TcpListener

    ''' <summary>
    ''' Thread signal to accept new clients 
    ''' </summary>
    Private Shared EvConnected As New ManualResetEvent(False)

    ''' <summary>
    ''' Controls End of Service
    ''' </summary>
    Private EndService As Boolean = False

    ''' <summary>
    ''' The list of connected clients
    ''' </summary>
    Public Clients As New List(Of TelnetClient)

    ''' <summary>
    ''' The Serial Device
    ''' </summary>
    Public WithEvents Serial As SerialDevice
    ''' <summary>
    ''' Manages State between STM and Linux
    ''' </summary>
    Public StateMgr As StateManager


#Region "Public Methods"


    Public Sub Start()
        Try
            'Prepare Serial Channel
            Serial = New SerialDevice()
            Serial.Bauds = 9600
            Serial.Comm = COMPort
            Serial.Is485 = False
            Serial.Connect()

            AddHandler Serial.OnPacketReceived, AddressOf OnSerialPacketReceived
            AddHandler Serial.OnPayloadReceived, AddressOf OnSerialPayloadReceived

            'Run main task to receive clients
            Dim t = New Task(Sub()
                                 Dim localAddr As IPAddress = IPAddress.Parse(EndPoint)
                                 Listener = New TcpListener(localAddr, Port)
                                 Listener.Start()
                                 logger.LogInformation($"Telnet Server is Running at {localAddr.ToString}:{Port}...")

                                 'Prepara el control d'estat
                                 StateMgr = New StateManager(5000)
                                 AddHandler StateMgr.OnSendCommand, AddressOf OnStateMgrSendCommand
                                 StateMgr.Start()


                                 Do While Not EndService
                                     EvConnected.Reset()
                                     Listener.BeginAcceptSocket(New AsyncCallback(AddressOf OnAcceptSocket), Listener)
                                     EvConnected.WaitOne()
                                 Loop
                             End Sub)
            t.Start()

        Catch ex As Exception

        End Try

    End Sub


    Public Sub [Stop]()
        For Each c In Clients
            c.Stop()
        Next
        Serial.Close()
    End Sub

#End Region
    Private Sub OnAcceptSocket(ByVal ar As IAsyncResult)

        ' Get the listener that handles the client request.
        Dim listener As TcpListener = CType(ar.AsyncState, TcpListener)
        ' End the operation and display the received data on the
        'console.
        Dim ClientSocket As Socket = listener.EndAcceptSocket(ar)

        Dim Client As New TelnetClient()
        Client.Socket = ClientSocket
        AddHandler Client.OnSocketClosed, AddressOf OnClientClosed
        AddHandler Client.OnPacketReceived, AddressOf OnClientTCPPacketReceived

        Clients.Add(Client)
        StateMgr.Enabled = False 'Client Connected StateMgr disabled

        logger.LogInformation($"New Client Connected")
        logger.LogInformation($"{EndPoint} Connected Clients {Clients.Count}")
        logger.LogInformation("Disable State Manager")

        Client.Start()

        EvConnected.Set()
    End Sub

#Region "TCP Events"

    Shared Sub OnClientConnected(Sender As TelnetServer, Client As TelnetClient)
        logger.LogInformation($"New Client Connected")
    End Sub

    Shared Sub OnClientDisconnected(Sender As TelnetServer, Client As TelnetClient)
        logger.LogInformation($"Client Disconnected")
    End Sub

    Private Sub OnClientTCPPacketReceived(Sender As Object, Packet As List(Of Byte))
        Serial.Write(Packet.ToArray)
    End Sub

    ''' <summary>
    ''' Removes client from clients list
    ''' </summary>
    ''' <param name="Sender"></param>
    Private Sub OnClientClosed(Sender As TelnetClient)
        Clients.Remove(Sender)
        logger.LogInformation($"Client Disconnected")
        logger.LogInformation($"{EndPoint} Connected Clients {Clients.Count}")
        If Clients.Count = 0 Then
            logger.LogInformation("Enable State Manager")
            StateMgr.Enabled = True 'Not connected clients => SateMgr enabled.
        End If
    End Sub
#End Region
#Region "State Manager Events"
    Private Sub OnStateMgrSendCommand(Sender As Object, Packet As List(Of Byte))
        Serial.EncodeAndWrite(Packet)
    End Sub
#End Region

#Region "Serial Management Events"
    ''' <summary>
    ''' On Serial Packet Received. Packet contains whole packet
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="Packet"></param>
    Private Sub OnSerialPacketReceived(Sender As Object, Packet As List(Of Byte))
        If Clients.Count Then
            For Each c In Clients
                If c.IsConnected Then c.Send(Packet)
            Next
        End If
    End Sub
    ''' <summary>
    ''' On Serial Packet Received. Packet contains just payload
    ''' </summary>
    ''' <param name="Sender"></param>
    ''' <param name="Packet"></param>
    Private Sub OnSerialPayloadReceived(Sender As Object, Packet As List(Of Byte))
        If Clients.Count = 0 Then
            StateMgr.OnSerialPacketReceived(Packet)
        End If
    End Sub

#End Region


End Class