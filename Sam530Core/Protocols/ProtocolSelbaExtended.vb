
Public Class ProtocolSelbaExtended
    Implements IProtocol

    Public Property RxPacket As List(Of Byte) Implements IProtocol.RxPacket
    Public Property Payload As List(Of Byte) Implements IProtocol.Payload

    Public Property HasDummyData As Boolean Implements IProtocol.HasDummyData

    Private _DummyData As String
    Public Property DummyData As String Implements IProtocol.DummyData
        Get
            If HasDummyData Then
                HasDummyData = False
                Dim Data As String = _DummyData
                _DummyData = ""
                Return Data
            Else
                Return ""
            End If
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

    Public Function Decode(DataIn As List(Of Byte)) As Boolean Implements IProtocol.Decode
        Dim Processed As Integer

        For Each b In DataIn
            Processed += 1
            Select Case State
                Case 0
                    If b = IProtocol.CONTROL_CHARS.SOH Then
                        State += 1
                    Else
                        DummyData += ChrW(b)
                        'If b = &HD Then HasDummyData = True
                        HasDummyData = True
                    End If
                Case 1
                    InAddress = b
                    InAddress <<= 8
                    State += 1
                Case 2
                    InAddress = (InAddress Or b)
                    State += 1
                Case 3
                    InLen = b
                    InLen <<= 8
                    State += 1
                Case 4
                    InLen = (InLen Or b)
                    State += 1
                    Payload = New List(Of Byte)
                    InCrc = 0
                    If InLen = 0 Then
                        State += 1
                    End If
                    If InLen > 1024 + 128 Then
                        State = 0
                    End If
                Case 5
                    InCrc = (InCrc Xor b)
                    Payload.Add(b)
                    InLen -= 1
                    If InLen = 0 Then
                        State += 1
                    End If
                Case 6
                    If InCrc = b Then
                        State += 1
                    Else
                        State = 0
                    End If
                Case 7
                    If b = IProtocol.CONTROL_CHARS.EOT Then
                        State = 0
                        DataIn.RemoveRange(0, Processed)
                        Return True
                    End If
            End Select
        Next
        DataIn.RemoveRange(0, DataIn.Count)
        Return False
    End Function

    Public Function Encode(DataIn As List(Of Byte)) As List(Of Byte) Implements IProtocol.Encode
        Dim Packet As New List(Of Byte)

        Packet.Add(IProtocol.CONTROL_CHARS.SOH)

        Packet.Add(OutAddress >> 8)
        Packet.Add((OutAddress And &HFF))

        Packet.Add(DataIn.Count >> 8)
        Packet.Add((DataIn.Count And &HFF))

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
