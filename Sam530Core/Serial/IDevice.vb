Public Interface IDevice


    ''' <summary>
    ''' Connects this instance.
    ''' </summary>
    ''' <returns></returns>
    Function  Connect() As Boolean
    ''' <summary>
    ''' Writes the specified data.
    ''' </summary>
    ''' <param name="Data">The data.</param>
    ''' <returns></returns>
    Function Write(Data As String) As Boolean
    ''' <summary>
    ''' Writes the specified bytes.
    ''' </summary>
    ''' <param name="Bytes">The bytes.</param>
    ''' <returns></returns>
    Function Write(Bytes() As Byte) As Boolean
    ''' <summary>
    ''' Closes this instance.
    ''' </summary>
    ''' <returns></returns>
    Function Close() As Boolean
    ''' <summary>
    ''' Indicates if Device is connected
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property IsConnected() As Boolean

    ''' <summary>
    ''' Occurs when [incomming data].
    ''' </summary>
    Event OnIncommingData(Data As List(Of Byte))
    ''' <summary>
    ''' Occurs when [connection error].
    ''' </summary>
    Event OnConnectionError(Sender As Object)

End Interface
