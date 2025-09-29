Imports System.Timers
Imports Microsoft.Extensions.Logging
Public Class StateManager
    Const API_REQUEST As Byte = &H38

    Const BOARD_H As Byte = &H4
    Const BOARD_L As Byte = &HFE
    Const ACK As Byte = 6

    Enum API_COMMANDS

        'Request
        GET_STATUS = &H1

        'Request
        ANS_STATUS = &H81

    End Enum

    ''' <summary>
    ''' The logger
    ''' </summary>
    Public Shared logger As ILogger


    ''' <summary>
    ''' If true StateManager can send otherwise is disabled (A TCP client is connected to system)
    ''' </summary>
    ''' <returns></returns>
    Public Property Enabled As Boolean = True

    ''' <summary>
    ''' Manages H563 Status and Send Command
    ''' </summary>
    Public WithEvents TmsStatus As Timer

    ''' <summary>
    ''' Define Interval to get status
    ''' </summary>
    Private ReadOnly _Interval As Integer

    Private Sequence As Byte

#Region "Events"
    Public Event OnSendCommand(Sender As Object, Packet As List(Of Byte))
#End Region

    ''' <summary>
    ''' ms between calls
    ''' </summary>
    ''' <param name="Interval"></param>
    Sub New(Interval As Integer)
        _Interval = Interval

        TmsStatus = New Timer
        TmsStatus.Interval = _Interval
        TmsStatus.AutoReset = False
    End Sub

    ''' <summary>
    ''' Start the manager
    ''' </summary>
    ''' <returns></returns>
    Public Function Start() As Boolean
        If Not TmsStatus.Enabled Then
            TmsStatus.Start()
        End If
        Return True
    End Function

    ''' <summary>
    ''' Stops the manager
    ''' </summary>
    ''' <returns></returns>
    Public Function [Stop]() As Boolean
        TmsStatus.Stop()
        Return True
    End Function


    Public Sub OnSerialPacketReceived(Payload As List(Of Byte))
        logger.LogInformation("Receive API status")


        'logger.LogInformation(String.Join(", ", Payload.Select(Function(b) b.ToString("X2"))))

        'Ignore API Command and BoardId
        If CType(Payload(0), API_COMMANDS) = API_REQUEST Then
            'Ignora BoardId (2 bytes)
            'Command is Acknowledge
            Select Case CType(Payload(3), API_COMMANDS)
                Case API_COMMANDS.ANS_STATUS
                    If Payload(4) = ACK Then
                        Sequence = Payload(5)
                    End If

            End Select

        End If

    End Sub

#Region "Private Methods"
    ''' <summary>
    ''' Get Packet Header
    ''' </summary>
    ''' <returns></returns>
    Private Function PacketHeader() As List(Of Byte)
        Return New List(Of Byte) From {API_REQUEST, BOARD_H, BOARD_L}
    End Function
    ''' <summary>
    ''' Get Status Package
    ''' </summary>
    ''' <returns></returns>
    Private Function GetStatus() As List(Of Byte)
        Dim Packet As List(Of Byte) = PacketHeader()
        Packet.Add(API_COMMANDS.GET_STATUS)
        Packet.Add(Sequence)
        Return Packet
    End Function

#End Region


#Region "Timer Event"

    Private Sub TmsStatus_Elapsed(sender As Object, e As ElapsedEventArgs) Handles TmsStatus.Elapsed
        If Enabled Then
            logger.LogInformation($"Sent API status Seq {Sequence}")
            RaiseEvent OnSendCommand(Me, GetStatus)
        End If
        TmsStatus.Start()
    End Sub

#End Region

End Class
