Imports System.ComponentModel
Imports System.IO.Ports
Imports Microsoft.Extensions.Logging



Public Class SerialDevice
    Inherits DeviceBase
    Implements INotifyPropertyChanged, IDevice


    Public WithEvents oSerialPort As SerialPort
    Dim LocalClose As Boolean = False

    Protected BufferIn As String



#Region "Events"
    Public Event OnConnectionStablished(Sender As Object)
    Public Event OnConnectionFail(Sender As Object)
    Public Event OnConnectionClosed(Sender As Object, Origin As CLOSE_ORIGIN)
    Public Event PropertyChanged(Sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Public Event OnPacketReceived(Sender As Object, Packet As List(Of Byte))
    Public Event OnPayloadReceived(Sender As Object, Packet As List(Of Byte))

#End Region
#Region "Protected Events"
    Protected Event OnIncommingData(Data As List(Of Byte)) Implements IDevice.OnIncommingData
    Protected Event OnConnectionError(Sender As Object) Implements IDevice.OnConnectionError
#End Region

#Region "Properties"
    Public Shared Property logger As ILogger

    Public Property Comm As String = "COM1"
    Public Property Bauds As UInt32 = 9600
    Public Property Is485 As Boolean = False

    Public ReadOnly Property IsConnected As Boolean Implements IDevice.IsConnected
        Get
            Return (oSerialPort IsNot Nothing AndAlso oSerialPort.IsOpen)
        End Get
    End Property
    Public ReadOnly Property CanWrite
        Get
            Return oSerialPort.IsOpen
        End Get
    End Property

    Dim _Name As String
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property


    Dim _State As STATES = STATES.IDLE
    Public Property State As STATES
        Get
            Return _State
        End Get
        Set(value As STATES)
            _State = value
        End Set
    End Property

    ''' <summary>
    ''' Used Protocol to decode frames
    ''' </summary>
    ''' <returns></returns>
    Public Property Protocol As ProtocolSelba

#End Region
#Region "Public Methods"

    Public Sub New()
        Protocol = New ProtocolSelba
        Protocol.SetAddress(1)
    End Sub

    ''' <summary>
    ''' Establishes Communication
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect() As Boolean Implements IDevice.Connect
        Return Connect(Comm, Bauds)
    End Function
    ''' <summary>
    ''' Stablishes Communication
    ''' </summary>
    ''' <param name="ComPort"></param>
    ''' <param name="Bauds"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(ComPort As String, Bauds As UInt32) As Boolean
        'Validate Parameters
        If Not SerialPort.GetPortNames.Contains(ComPort) Then
            RaiseEvent OnConnectionFail(Me)
            Return False
        End If
        If Bauds = 0 Then
            RaiseEvent OnConnectionFail(Me)
            Return False
        End If

        Me.Comm = ComPort
        Me.Bauds = Bauds

        'Try Connection
        oSerialPort = New SerialPort

        oSerialPort.PortName = ComPort
        oSerialPort.BaudRate = Bauds
        oSerialPort.Handshake = Handshake.None

        _logger.LogInformation($"Opening serial port {ComPort} at {Bauds}...")

        Try
            oSerialPort.Open()
            Flush()
            _logger.LogInformation($"Serial port {ComPort} Opened")
        Catch ex As Exception
            _logger.LogError(ex, $"Fail to Serial port {ComPort}. Cause {ex.Message}")
            RaiseEvent OnConnectionFail(Me)
            Return False
        End Try

        Return True
    End Function
    ''' <summary>
    ''' Write Data to USB
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Write(Data As String) As Boolean Implements IDevice.Write
        Dim Bytes() As Byte = System.Text.Encoding.Default.GetBytes(Data)
        Return Write(Bytes)
    End Function
    ''' <summary>
    ''' Write Data to USB
    ''' </summary>
    ''' <param name="Bytes"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Write(Bytes() As Byte) As Boolean Implements IDevice.Write
        Try
            If oSerialPort IsNot Nothing AndAlso oSerialPort.IsOpen Then
                oSerialPort.Write(Bytes, 0, Bytes.Length)
                Return True
            Else
                'Try to recover
                Return False
            End If
        Catch ex As Exception
            If IO.Ports.SerialPort.GetPortNames.Contains(oSerialPort.PortName) Then
                'oUSBClient.Close()
            End If
            RaiseEvent OnConnectionError(Me)
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Encodes data with protocol and send it
    ''' </summary>
    ''' <param name="Bytes"></param>
    ''' <returns></returns>
    Public Function EncodeAndWrite(Bytes As List(Of Byte)) As Boolean
        Dim Data = Protocol.Encode(Bytes)
        Write(Data.ToArray)
        Return True
    End Function


    ''' <summary>
    ''' Closes Current Communication
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Close() As Boolean Implements IDevice.Close
        If oSerialPort IsNot Nothing AndAlso oSerialPort.IsOpen Then
            Try
                oSerialPort.Close()
            Catch ex As Exception

            End Try

            LocalClose = True
            RaiseEvent OnConnectionClosed(Me, CLOSE_ORIGIN.LOCAL)
        End If
        State = STATES.IDLE
        Return True
    End Function

    Public Sub Flush()
        If oSerialPort IsNot Nothing AndAlso oSerialPort.IsOpen Then
            oSerialPort.ReadExisting()
            oSerialPort.BaseStream.Flush()
        End If
    End Sub

    Private Sub oSerialPort_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles oSerialPort.DataReceived
        Dim Bytes As Int32 = oSerialPort.BytesToRead
        Dim Buffer(Bytes - 1) As Byte
        oSerialPort.Read(Buffer, 0, Bytes)

        If Protocol.Decode(Buffer.ToList) Then
            RaiseEvent OnPacketReceived(Me, Protocol.RxPacket)
            RaiseEvent OnPayloadReceived(Me, Protocol.Payload)
        End If


    End Sub

    Public Function ChangeSpeed(BaudRate As Integer) As Boolean
        oSerialPort.Close()
        oSerialPort.BaudRate = BaudRate
        oSerialPort.Open()
        Return True
    End Function


#End Region

End Class
