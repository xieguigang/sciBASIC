Imports System.Drawing

Namespace Drawing2D.VectorElements

    Public Class Circle : Inherits LayoutsElement

        Dim _Size As Size
        Dim Brush As SolidBrush

        Public Property FillColor As Color
            Get
                Return Brush.Color
            End Get
            Set(value As Color)
                Brush = New SolidBrush(value)
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LeftTop">左上角</param>
        ''' <param name="D">圆的直径</param>
        ''' <remarks></remarks>
        Friend Sub New(LeftTop As Point, D As Integer, GDI As GDIPlusDeviceHandle, FillColor As Color)
            Call MyBase.New(GDI, LeftTop)
            _Size = New Size(D, D)
            Me.FillColor = FillColor
        End Sub

        Protected Overloads Overrides Sub InvokeDrawing()
            Call Me._GDIDevice.Gr_Device.FillPie(Me.Brush, Me.DrawingRegion, 0, 360)
        End Sub

        Public Overrides ReadOnly Property Size As Size
            Get
                Return _Size
            End Get
        End Property
    End Class
End Namespace