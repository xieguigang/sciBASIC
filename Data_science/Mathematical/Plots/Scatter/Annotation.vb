
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure Annotation

    ''' <summary>
    ''' [<see cref="PointF.X"/>] from <see cref="SerialData.pts"/>::<see cref="PointData.pt"/>
    ''' </summary>
    Public X As Single
    Public Text As String
    ''' <summary>
    ''' Font style for <see cref="Text"/>
    ''' </summary>
    Public Font As String
    Public Legend As LegendStyles
    ''' <summary>
    ''' Size region for <see cref="Legend"/> Drawing
    ''' </summary>
    Public size As SizeF
    Public color As String

    ''' <summary>
    ''' The target annotation data point is null!
    ''' </summary>
    Const PointNull As String = "The target annotation data point is null!"

    Public Sub Draw(ByRef g As Graphics, scaler As Scaling, s As SerialData, r As GraphicsRegion)
        Dim pt As PointData = s.GetPointByX(X)

        If pt.pt.IsEmpty Then
            Call PointNull.PrintException
            Return
        Else
            If size.IsEmpty Then
                size = New SizeF(120, 45)
            End If
        End If

        ' 得到转换坐标
        Dim point As PointF = scaler.PointScaler(r, pt.pt)
        ' 将坐标置于区域大小的中间
        point = New PointF(point.X - size.Width / 2, point.Y - size.Height / 2)

        Dim legend As New Legend With {
            .color = If(String.IsNullOrEmpty(color), $"rgb({s.color.R},{s.color.G},{s.color.B})", color),
            .fontstyle = Font,
            .style = Me.Legend,
            .title = Text
        }
        Dim border As New Border With {
            .color = Drawing.Color.Black,
            .style = DashStyle.Solid,
            .width = 3
        }

        Call DrawLegend(
            g,
            New Point(point.X, point.Y),
            size,
            legend,
            border)
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure