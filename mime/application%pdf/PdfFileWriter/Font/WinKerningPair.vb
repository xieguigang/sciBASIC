

''' <summary>
''' Kerning pair class
''' </summary>
Public Class WinKerningPair : Implements IComparable(Of WinKerningPair)

    ''' <summary>
    ''' Gets first character
    ''' </summary>
    Public Property First As Char

    ''' <summary>
    ''' Gets second character
    ''' </summary>
    Public Property Second As Char

    ''' <summary>
    ''' Gets kerning amount in design units
    ''' </summary>
    Public Property KernAmount As Integer

    Friend Sub New(DC As FontApi)
        First = DC.ReadChar()
        Second = DC.ReadChar()
        KernAmount = DC.ReadInt32()
        Return
    End Sub

    ''' <summary>
    ''' Kerning pair constructor
    ''' </summary>
    ''' <param name="First">First character</param>
    ''' <param name="Second">Second character</param>
    Public Sub New(First As Char, Second As Char)
        Me.First = First
        Me.Second = Second
        Return
    End Sub

    ''' <summary>
    ''' Compare kerning pairs
    ''' </summary>
    ''' <param name="Other">Other pair</param>
    ''' <returns>Compare result</returns>
    Public Function CompareTo(Other As WinKerningPair) As Integer Implements IComparable(Of WinKerningPair).CompareTo
        Return If(First <> Other.First, AscW(First) - AscW(Other.First), AscW(Second) - AscW(Other.Second))
    End Function
End Class
