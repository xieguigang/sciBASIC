
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

        End Function
    End Module
End Namespace