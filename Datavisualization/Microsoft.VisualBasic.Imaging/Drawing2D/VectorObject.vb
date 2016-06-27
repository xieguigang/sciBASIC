Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Public MustInherit Class VectorObject

    Dim __rectangle As Rectangle

    Public Property RECT As Rectangle
        Get
            Return __rectangle
        End Get
        Protected Set(value As Rectangle)
            __rectangle = value
        End Set
    End Property

    Sub New(locat As Point, size As Size)
        RECT = New Rectangle(locat, size)
    End Sub

    Sub New(rect As Rectangle)
        Me.RECT = rect
    End Sub

    Public Overridable Sub Draw(gdi As GDIPlusDeviceHandle)
        Call Draw(gdi, RECT)
    End Sub

    Public MustOverride Sub Draw(gdi As GDIPlusDeviceHandle, loci As Rectangle)

    Public Overrides Function ToString() As String
        Return RECT.GetJson
    End Function
End Class
