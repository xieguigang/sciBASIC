#Region "Microsoft.VisualBasic::b35535af12689de324279adcac79bf66, Data_science\Graph\Model\Grid.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Grid
    ' 
    '     Properties: layout, steps
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: Index, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging

''' <summary>
''' 网格也可以看作为一种网络
''' </summary>
Public Class Grid

    ReadOnly X, Y As OrderSelector(Of Double)

    Public ReadOnly Property layout As RectangleF
    Public ReadOnly Property steps As SizeF

    Sub New(size As Size, steps As SizeF)
        Call Me.New(New Rectangle(New Point, size), steps)
    End Sub

    Sub New(layout As Rectangle, steps As SizeF)
        Call Me.New(layout.ToFloat, steps)
    End Sub

    Sub New(layout As RectangleF, steps As SizeF)
        X = New OrderSelector(Of Double)(Math.seq(layout.X, layout.Right, steps.Width))
        Y = New OrderSelector(Of Double)(Math.seq(layout.Y, layout.Bottom, steps.Height))

        Me.steps = steps
        Me.layout = layout
    End Sub

    ''' <summary>
    ''' 返回数据点在网格之中的``X,Y``方格的顶点编号
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    Public Function Index(p As PointF) As Point
        Dim xi = X.FirstGreaterThan(p.X)
        Dim yi = Y.FirstGreaterThan(p.Y)

        Return New Point(xi, yi)
    End Function

    Public Overrides Function ToString() As String
        Return $"{layout} @ {steps}"
    End Function
End Class
