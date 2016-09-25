
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class SerialData : Implements sIdEnumerable
    Implements IEnumerable(Of PointData)

    ''' <summary>
    ''' 绘图的点的数据，请注意，这里面的点之间是有顺序之分的
    ''' </summary>
    Public pts As PointData()
    Public lineType As DashStyle = DashStyle.Solid
    Public Property title As String Implements sIdEnumerable.Identifier

    ''' <summary>
    ''' 点的半径大小
    ''' </summary>
    Public PointSize As Single = 1
    Public color As Color = Color.Black
    Public width As Single = 1

    ''' <summary>
    ''' 对一系列特定的数据点的注释数据
    ''' </summary>
    ''' <returns></returns>
    Public Property annotations As Annotation()

    Public Function GetPointByX(x As Single) As PointData
        For Each pt As PointData In pts
            If pt.pt.X = x Then
                Return pt
            End If
        Next

        Return Nothing
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of PointData) Implements IEnumerable(Of PointData).GetEnumerator
        For Each x In pts
            Yield x
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class

''' <summary>
''' 绘图的点的数据
''' </summary>
Public Structure PointData

    ''' <summary>
    ''' 坐标数据不需要进行额外的转换，绘图函数内部会自动进行mapping转换的
    ''' </summary>
    Public pt As PointF
    ''' <summary>
    ''' 正误差
    ''' </summary>
    Public errPlus As Double
    ''' <summary>
    ''' 负误差
    ''' </summary>
    Public errMinus As Double
    Public Tag As String
    Public value As Double

    Sub New(x As Single, y As Single)
        pt = New PointF(x, y)
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure