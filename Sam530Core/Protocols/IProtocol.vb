Public Interface IProtocol
    Enum CONTROL_CHARS
        NUL = 0
        SOH = 1
        STX = 2
        ETX = 3
        EOT = 4
        ENQ = 5
        ACK = 6
        BEL = 7
        BS = 8
        HT = 9
        LF = 10
        VT = 11
        FF = 12
        CR = 13
        SO = 14
        SI = 15
        DLE = 16
        DC1 = 17
        DC2 = 18
        DC3 = 19
        DC4 = 20
        NAK = 21
        SIN = 22
        ETB = 23
        CAN = 24
        EM = 25
        [SUB] = 26
        ESC = 27
        FS = 28
        GS = 29
        RS = 30
        US = 31
    End Enum

    Enum PROTOCOLS As Byte
        OCPP
        SELBA
        SELBA_EX
        MODBUS
    End Enum

    ''' <summary>
    ''' Gets the complete Decoded Packet
    ''' </summary>
    ''' <value>
    ''' The payload.
    ''' </value>
    Property RxPacket As List(Of Byte)
    ''' <summary>
    ''' Gets or sets the payload.
    ''' </summary>
    ''' <value>
    ''' The payload.
    ''' </value>
    Property Payload As List(Of Byte)
    ''' <summary>
    ''' Sets the address.
    ''' </summary>
    ''' <param name="Address">The address.</param>
    Sub SetAddress(Address As UInt16)
    ''' <summary>
    ''' Encodes the specified data in.
    ''' </summary>
    ''' <param name="DataIn">The data in.</param>
    ''' <returns></returns>
    Function Encode(DataIn As List(Of Byte)) As List(Of Byte)
    ''' <summary>
    ''' Decodes the specified data in.
    ''' </summary>
    ''' <param name="DataIn">The data in.</param>
    ''' <returns></returns>
    Function Decode(DataIn As List(Of Byte)) As Boolean
    ''' <summary>
    ''' Gets or sets a value indicating whether this instance has dummy data.
    ''' </summary>
    ''' <value>
    ''' <c>true</c> if this instance has dummy data; otherwise, <c>false</c>.
    ''' </value>
    Property HasDummyData As Boolean
    ''' <summary>
    ''' Gets or sets the dummy data.
    ''' </summary>
    ''' <value>
    ''' The dummy data.
    ''' </value>
    Property DummyData As String
End Interface
