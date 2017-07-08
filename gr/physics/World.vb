''' <summary>
''' Physics world reactor
''' </summary>
Public Class World

    Public Enum Type As Byte
        Plain2D = 2
        Spatial3D = 3
    End Enum

    Dim objects As List(Of MassPoint)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="time%">迭代的次数</param>
    Public Sub React(time As UInteger)
        For i As UInteger = 0 To time

        Next
    End Sub
End Class
