
''' <summary>
''' Standard Selba Protocol
''' Encodes as single address/length
''' Decodes as single/double address/length
''' </summary>
''' <seealso cref="GenIOLib.IProtocol" />
Public Class ProtocolSelba
    Implements IProtocol

    Public Property RxPacket As List(Of Byte) Implements IProtocol.RxPacket

    Public Property Payload As List(Of Byte) Implements IProtocol.Payload

    Public Property HasDummyData As Boolean Implements IProtocol.HasDummyData

    Private _DummyData As String
    Public Property DummyData As String Implements IProtocol.DummyData
        Get
            'If HasDummyData Then
            '    HasDummyData = False
            '    Dim Data As String = _DummyData
            '    _DummyData = ""
            '    Return Data
            'Else
            '    Return ""
            'End If
            Return _DummyData
        End Get
        Set(value As String)
            _DummyData = value
        End Set
    End Property





    Private State As Integer = 0
    Private OutAddress As UInt16 = 1
    Private InAddress As UInt16
    Private InLen As UInt16

    Private InCrc As Byte




    Public Sub SetAddress(Address As UShort) Implements IProtocol.SetAddress
        Me.OutAddress = Address
    End Sub
    ''' <summary>
    ''' Decodes the specified data in.
    ''' </summary>
    ''' <param name="DataIn">The data in.</param>
    ''' <returns></returns>
    Public Function Decode(DataIn As List(Of Byte)) As Boolean Implements IProtocol.Decode

        For Each b In DataIn
            Select Case State
                Case 0
                    If b = IProtocol.CONTROL_CHARS.STX Then
                        RxPacket = New List(Of Byte)
                        RxPacket.Add(b)
                        State += 1
                    ElseIf b = IProtocol.CONTROL_CHARS.SOH Then
                        RxPacket = New List(Of Byte)
                        RxPacket.Add(b)
                        State = 10
                    Else
                        DummyData += ChrW(b)
                        'If b = &HD Then HasDummyData = True
                        HasDummyData = True
                    End If
                    'Single address/length protocol
                Case 1
                    RxPacket.Add(b)
                    InAddress = b
                    State += 1
                Case 2
                    RxPacket.Add(b)
                    InLen = b
                    State += 1
                    Payload = New List(Of Byte)
                    InCrc = 0
                    If InLen = 0 Then
                        State += 1
                    End If
                Case 3
                    RxPacket.Add(b)
                    InCrc = (InCrc Xor b)
                    Payload.Add(b)
                    InLen -= 1
                    If InLen = 0 Then
                        State += 1
                    End If
                Case 4
                    RxPacket.Add(b)
                    If InCrc = b Then
                        State += 1
                    Else
                        State = 0
                        Return False
                    End If
                Case 5
                    RxPacket.Add(b)
                    If b = IProtocol.CONTROL_CHARS.EOT Then
                        State = 0
                        Return True
                    End If

                    'Double Protocol
                Case 10
                    RxPacket.Add(b)
                    InAddress = b
                    InAddress <<= 8
                    State += 1
                Case 11
                    RxPacket.Add(b)
                    InAddress = (InAddress Or b)
                    State += 1
                Case 12
                    RxPacket.Add(b)
                    InLen = b
                    InLen <<= 8
                    State += 1
                Case 13
                    RxPacket.Add(b)
                    InLen = (InLen Or b)
                    State += 1
                    Payload = New List(Of Byte)
                    InCrc = 0
                    If InLen = 0 Then
                        State += 1
                    End If
                Case 14
                    RxPacket.Add(b)
                    InCrc = (InCrc Xor b)
                    Payload.Add(b)
                    InLen -= 1
                    If InLen = 0 Then
                        State += 1
                    End If
                Case 15
                    RxPacket.Add(b)
                    If InCrc = b Then
                        State += 1
                    Else
                        State = 0
                    End If
                Case 16
                    RxPacket.Add(b)
                    If b = IProtocol.CONTROL_CHARS.EOT Then
                        State = 0
                        Return True
                    End If

            End Select
        Next

        Return False
    End Function
    ''' <summary>
    ''' Encodes the specified data in.
    ''' </summary>
    ''' <param name="DataIn">The data in.</param>
    ''' <returns></returns>
    Public Function Encode(DataIn As List(Of Byte)) As List(Of Byte) Implements IProtocol.Encode
        Dim Packet As New List(Of Byte)

        Packet.Add(IProtocol.CONTROL_CHARS.STX)
        Packet.Add(OutAddress)

        If DataIn.Count = 256 Then
            Packet.Add(0)
        Else
            Packet.Add(DataIn.Count)
        End If

        Dim Crc As Byte = 0
        For Each b In DataIn
            Crc = (Crc Xor b)
            Packet.Add(b)
        Next
        Packet.Add(Crc)
        Packet.Add(IProtocol.CONTROL_CHARS.EOT)

        Return Packet
    End Function
End Class
