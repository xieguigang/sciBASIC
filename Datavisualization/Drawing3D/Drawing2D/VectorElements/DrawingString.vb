Imports System.Drawing

Namespace Drawing2D.VectorElements

    Public Class DrawingString : Inherits LayoutsElement

        Dim _InternalText As String
        Dim _DrawingFont As Font
        Dim _Size As Size
        Dim _TextBrush As Brush

        Public Property Font As Font
            Get
                Return _DrawingFont
            End Get
            Set(value As Font)
                _DrawingFont = value
                Dim Size = Me._GDIDevice.Gr_Device.MeasureString(Me._InternalText, value)
                _Size = New Size(Size.Width, Size.Height)
            End Set
        End Property

        Friend Sub New(str As String, Color As Color, GDI As Microsoft.VisualBasic.GDIPlusDeviceHandle, Optional InitLocation As Point = Nothing)
            Call MyBase.New(GDI, InitLocation)
            _InternalText = str
            _TextBrush = New SolidBrush(Color)
        End Sub

        Protected Overloads Overrides Sub InvokeDrawing()
            Call Me._GDIDevice.Gr_Device.DrawString(_InternalText, Font, _TextBrush, Me.Location)
        End Sub

        Public Overrides ReadOnly Property Size As System.Drawing.Size
            Get
                Return _Size
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _InternalText
        End Function
    End Class
End Namespace