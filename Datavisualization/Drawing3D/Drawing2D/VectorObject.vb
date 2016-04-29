Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization

Public MustInherit Class VectorObject

    Public ReadOnly Property RECT As Rectangle

    Sub New(locat As Point, size As Size)
        RECT = New Rectangle(locat, size)
    End Sub

    Sub New(rect As Rectangle)
        Me.RECT = rect
    End Sub

    Public MustOverride Sub Draw(gdi As GDIPlusDeviceHandle)

    Public Overrides Function ToString() As String
        Return RECT.GetJson
    End Function
End Class
