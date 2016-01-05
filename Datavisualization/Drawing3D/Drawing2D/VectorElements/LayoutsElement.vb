Imports System.Drawing

Namespace Drawing2D.VectorElements

    Public MustInherit Class LayoutsElement

        Public Property Location As Point
        Public Property TooltipTag As String

        Public MustOverride ReadOnly Property Size As Size

        Public ReadOnly Property DrawingRegion As Rectangle
            Get
                Return New Rectangle(Location, Size)
            End Get
        End Property

        Protected _GDIDevice As Microsoft.VisualBasic.GDIPlusDeviceHandle

        ''' <summary>
        ''' 默认是允许自动组织布局的
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EnableAutoLayout As Boolean = True

        Sub New(GDI As Microsoft.VisualBasic.GDIPlusDeviceHandle, InitLoci As Point)
            _GDIDevice = GDI
            Location = InitLoci
        End Sub

        Protected MustOverride Sub InvokeDrawing()

        Public Function MoveTo(pt As Point) As LayoutsElement
            Location = pt
            Return Me
        End Function

        Public Function MoveOffset(offset As Point) As LayoutsElement
            Location = New Point(Location.X + offset.X, Location.Y + offset.Y)
            Return Me
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="OverridesLoci">假若需要进行绘制到的时候复写当前的元素的位置，则请使用这个参数</param>
        ''' <returns>函数返回当前元素在绘制之后所占据的区域</returns>
        ''' <remarks></remarks>
        Public Function InvokeDrawing(Optional OverridesLoci As Point = Nothing) As Rectangle

            If Not OverridesLoci.IsEmpty Then
                Me.Location = OverridesLoci
            End If

            Call InvokeDrawing()

            Return New Rectangle(Me.Location, Me.Size)
        End Function

        Public Overrides Function ToString() As String
            Return DrawingRegion.ToString
        End Function
    End Class
End Namespace