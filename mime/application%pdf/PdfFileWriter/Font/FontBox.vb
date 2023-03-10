
''' <summary>
''' Font box class
''' </summary>
''' <remarks>
''' FontBox class is part of OUTLINETEXTMETRIC structure
''' </remarks>

Public Class FontBox

    ''' <summary>
    ''' Gets left side.
    ''' </summary>
    Public Property Left As Integer

    ''' <summary>
    ''' Gets top side.
    ''' </summary>
    Public Property Top As Integer

    ''' <summary>
    ''' Gets right side.
    ''' </summary>
    Public Property Right As Integer

    ''' <summary>
    ''' Gets bottom side.
    ''' </summary>
    Public Property Bottom As Integer

    Friend Sub New(DC As FontApi)
        Left = DC.ReadInt32()
        Top = DC.ReadInt32()
        Right = DC.ReadInt32()
        Bottom = DC.ReadInt32()
        Return
    End Sub
End Class
