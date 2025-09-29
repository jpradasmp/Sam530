Imports System.Device.Gpio
Imports System.Runtime.InteropServices

''' <summary>
''' Requires 'sudo apt install libgpiod2' en dispositiu a controlar.
''' </summary>
Public Class GPIO
    ''' <summary>
    ''' Set GPIO High
    ''' </summary>
    ''' <param name="Pin"></param>
    ''' <returns></returns>
    Public Shared Function SetHigh(Pin As Integer) As Boolean
        If (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Then
            Return True
        End If
        Try
            Using Controller As New GpioController
                Controller.OpenPin(Pin)
                Controller.Write(Pin, PinValue.High)
                Controller.ClosePin(Pin)
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Set GPIO Low
    ''' </summary>
    ''' <param name="Pin"></param>
    ''' <returns></returns>
    Public Shared Function SetLow(Pin As Integer) As Boolean
        If (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Then
            Return True
        End If
        Try
            Using Controller As New GpioController
                Controller.OpenPin(Pin)
                Controller.Write(Pin, PinValue.Low)
                Controller.ClosePin(Pin)
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
