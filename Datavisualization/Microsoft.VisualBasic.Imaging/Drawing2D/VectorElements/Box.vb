Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.VectorElements

    Public Class Box : Inherits LayoutsElement

        Sub New(Location As Point, Size As Size, GDI As GDIPlusDeviceHandle, Color As Color)
            Call MyBase.New(GDI, Location)
        End Sub

        Protected Overloads Overrides Sub InvokeDrawing()

        End Sub

        Public Overrides ReadOnly Property Size As Size
            Get

            End Get
        End Property
    End Class

    ''' <summary>
    ''' 按照任意角度旋转的箭头对象
    ''' </summary>
    Public Class Arrow : Inherits LayoutsElement

        ''' <summary>
        ''' 箭头的头部占据整个长度的百分比
        ''' </summary>
        ''' <returns></returns>
        Public Property HeadLengthPercentage As Double = 0.15
        ''' <summary>
        ''' 箭头的主体部分占据整个高度的百分比
        ''' </summary>
        ''' <returns></returns>
        Public Property BodyHeightPercentage As Double = 0.85

        Public Property Color As Color
        Public Property BodySize As Size
        Public Property DirectionLeft As Boolean

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Location">箭头头部的位置</param>
        ''' <param name="Size">高度和宽度</param>
        ''' <param name="GDI"></param>
        ''' <param name="Color">填充的颜色</param>
        Sub New(Location As Point, Size As Size, GDI As GDIPlusDeviceHandle, Color As Color)
            Call MyBase.New(GDI, Location)
            Me.Color = Color
            Me.BodySize = Size
        End Sub

        Sub New(source As Arrow, GDI As GDIPlusDeviceHandle)
            Call MyBase.New(GDI, source.Location)
            Call Microsoft.VisualBasic.Serialization.ShadowCopy(source, Me)
        End Sub

        ''' <summary>
        ''' 返回图形上面的绘图的大小，而非箭头本身的大小
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Size As Size
            Get
                Return BodySize
            End Get
        End Property

        Protected ReadOnly Property HeadLength As Integer
            Get
                Return HeadLengthPercentage * BodySize.Width
            End Get
        End Property

        Protected ReadOnly Property HeadSemiHeight As Integer
            Get
                Return (BodySize.Height * (1 - BodyHeightPercentage)) / 2
            End Get
        End Property

        ''' <summary>
        ''' 忽略了箭头的方向，本箭头对象存粹的在进行图形绘制的时候的左右的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Left As Integer
            Get
                Return {Location.X, Location.X + If(Not DirectionLeft, 1, -1) * BodySize.Width}.Min
            End Get
        End Property
        ''' <summary>
        ''' 忽略了箭头的方向，本箭头对象存粹的在进行图形绘制的时候的左右的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Right As Integer
            Get
                Return {Location.X, Location.X + If(Not DirectionLeft, 1, -1) * BodySize.Width}.Max
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Left} ==> {Right}; // length={BodySize.ToString}"
        End Function

        ''' <summary>
        '''  /|_____
        ''' /       |
        ''' \       |
        '''  \|-----
        ''' </summary>
        Protected Overrides Sub InvokeDrawing()
            Dim Path As System.Drawing.Drawing2D.GraphicsPath = New GraphicsPath

            Dim Direction As Integer = If(DirectionLeft, 1, -1)
            Dim Top As Integer = Me.Location.Y - BodySize.Height / 2
            Dim Left = Me.Location.X
            Dim Right = Left + Direction * BodySize.Width
            Dim Bottom = Top + BodySize.Height
            Dim prePoint As Point


            Call Path.AddLine(Me.Location, New Point(Left + Direction * HeadLength, Top).ShadowCopy(prePoint))                  '/
            Call Path.AddLine(prePoint, New Point(Left + Direction * HeadLength, Top + HeadSemiHeight).ShadowCopy(prePoint))    ' |
            Call Path.AddLine(prePoint, New Point(Right, Top + HeadSemiHeight).ShadowCopy(prePoint))                            '  ----
            Call Path.AddLine(prePoint, New Point(Right, Bottom - HeadSemiHeight).ShadowCopy(prePoint))                         '      |
            Call Path.AddLine(prePoint, New Point(Left + Direction * HeadLength, Bottom - HeadSemiHeight).ShadowCopy(prePoint)) '  ----
            Call Path.AddLine(prePoint, New Point(Left + Direction * HeadLength, Bottom).ShadowCopy(prePoint))                  ' |
            Call Path.AddLine(prePoint, Me.Location)                                                                            '\
            Call Path.CloseFigure()

            Call Me._GDIDevice.Graphics.FillPath(New SolidBrush(Me.Color), Path)
        End Sub
    End Class
End Namespace