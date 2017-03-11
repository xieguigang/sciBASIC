Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BarPlot

    Public Module StyledBarplot

        Public Structure BarSerial
            Dim Label$
            Dim Value#
            ''' <summary>
            ''' 颜色表达式或者图片资源的文件路径
            ''' </summary>
            Dim Brush$

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure

        ''' <summary>
        ''' 进行更加复杂的样式的条形图的绘图操作
        ''' </summary>
        ''' <param name="data">这个数据集之中是没有同组比较数据的</param>
        ''' <param name="size"></param>
        ''' <param name="padding$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As IEnumerable(Of BarSerial),
                             Optional size As Size = Nothing,
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white") As Bitmap

            Return g.GraphicsPlots(
                size, padding,
                bg,
                Sub(ByRef g, region)
                    Call data _
                        .ToArray _
                        .__plotInternal(g, region)
                End Sub)
        End Function

        <Extension>
        Private Sub __plotInternal(data As BarSerial(), g As Graphics, region As GraphicsRegion)

        End Sub

        ''' <summary>
        ''' 使用这个函数进行条形图之中的系列的颜色的渲染
        ''' </summary>
        ''' <param name="data">
        ''' 系列数据，其中的<see cref="BarSerial.Brush"/>将会被填充为颜色谱之中的某一个颜色值
        ''' </param>
        ''' <param name="schema$"><see cref="Designer"/>的颜色谱名称</param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorRendering(data As IEnumerable(Of BarSerial), schema$) As BarSerial()
            Dim array As BarSerial() = data.ToArray
            Dim colors As Color() = Designer.GetColors(schema, array.Length)
            Dim out As BarSerial() = LinqAPI.Exec(Of BarSerial) <=
 _
                From ls As SeqValue(Of BarSerial)
                In array.SeqIterator
                Let color As String = colors(ls).RGB2Hexadecimal
                Let r As BarSerial = (+ls)
                Select New BarSerial With {
                    .Brush = color,
                    .Label = r.Label,
                    .Value = r.Value
                }

            Return out
        End Function
    End Module
End Namespace