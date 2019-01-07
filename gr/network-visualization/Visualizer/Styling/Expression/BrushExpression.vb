
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Parser

Namespace Styling

    Module BrushExpression

        ''' <summary>
        ''' 通过这个函数来进行颜色填充以及图案填充的画笔对象的统一构建
        ''' </summary>
        ''' <param name="expression">
        ''' #### 使用图片作为填充材质
        ''' 
        ''' + url(filepath/data uri) 所有节点都使用统一的图案
        ''' + map(propertyName, val1=url1, val2=url2, val3=url3)  离散映射
        ''' + rgb(x,x,x,x)|#xxxxx|xxxxx 颜色表达式，所有的节点都使用相同的颜色
        ''' + map(propertyName, val1=color1, val2=color2) 离散映射
        ''' + map(propertyName, [patternName, n]) 区间映射
        ''' </param>
        ''' <returns></returns>
        Public Function Evaluate(expression As String) As GetBrush
            If UrlEvaluator.IsURLPattern(expression) Then
                Return expression.unifyImage
            Else
                Throw New SyntaxErrorException(expression)
            End If
        End Function

        ''' <summary>
        ''' 全部都使用同一的图案
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function unifyImage(expression As String) As GetBrush
            Dim image As Image = UrlEvaluator.EvaluateAsImage(expression)
            Dim brush As New TextureBrush(image)

            Return Iterator Function(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush))
                       For Each n As Node In nodes
                           Yield New Map(Of Node, Brush) With {
                               .Key = n,
                               .Maps = brush
                           }
                       Next
                   End Function
        End Function
    End Module
End Namespace