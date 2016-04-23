Imports System.Runtime.CompilerServices

''' <summary>
''' Simple type parser
''' </summary>
Public Module Parser

    ''' <summary>
    ''' <see cref="Integer"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseInteger(s As String) As Integer
        Return CInt(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Long"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseLong(s As String) As Long
        Return CLng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Double"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseDouble(s As String) As Double
        Return Val(Trim(s))
    End Function

    ''' <summary>
    ''' <see cref="Single"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseSingle(s As String) As Single
        Return CSng(Val(Trim(s)))
    End Function

    ''' <summary>
    ''' <see cref="Boolean"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseBoolean(s As String) As Boolean
        Return Trim(s).getBoolean
    End Function

    ''' <summary>
    ''' <see cref="Date"/> text parser
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ParseDate(s As String) As Date
        Return Date.Parse(Trim(s))
    End Function
End Module
