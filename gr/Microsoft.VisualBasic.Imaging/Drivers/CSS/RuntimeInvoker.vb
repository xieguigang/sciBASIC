Imports System.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Render
Imports VB = Microsoft.VisualBasic.Language.Runtime

Namespace Driver.CSS

    ''' <summary>
    ''' 因为绘图的函数一般有很多的CSS样式参数用来调整图形上面的元素的样式，
    ''' 通过命令行传递这么多的参数不现实，故而在这里通过CSS文件加反射的形式
    ''' 来传递这些绘图参数，并且同时也保留对函数式编程的兼容性
    ''' </summary>
    Public Module RuntimeInvoker

        ' CSS文件说明
        ' 
        ' selector为函数参数的名称
        ' 样式属性则是具体的参数值
        '
        ' 例如
        ' #tickFont {
        '     font-size: 14px;
        '     color: red;
        ' }
        '
        ' 定义了绘图函数的tickFont参数的字体大小为14个像素点，并且在进行绘图的时候字体颜色为红色
        ' 如果没有定义字体名称的话，则是使用默认字体

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="driver">绘图函数</param>
        ''' <param name="CSS">
        ''' 定义了该函数之中的CSS元素的样式值，假若没有某一个可选参数的值，则使用默认参数值；
        ''' 或者参数值出现在了<paramref name="args"/>列表之中，则会被<paramref name="args"/>的值所覆盖
        ''' </param>
        ''' <param name="args">必须要包含有所有的必须参数，可选参数可以不包含在其中</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为考虑到手动输入参数可能会出现大小写不匹配的问题，故而在这里会首先尝试使用字典查找，
        ''' 没有找到键名的时候才会进行字符串大小写不敏感的字符串比较
        ''' </remarks>
        Public Function RunPlot(driver As [Delegate], CSS As CSSFile, ParamArray args As ArgumentReference()) As GraphicsData
            Dim type As MethodInfo = driver.Method
            Dim parameters = type.GetParameters
            Dim values As Dictionary(Of String, ArgumentReference) = args.ToDictionary(Function(arg) arg.name)
            Dim arguments As New List(Of Object)

            ' 因为args是必须参数，所以要首先进行赋值遍历
            For Each arg As ParameterInfo In parameters
                If values.ContainsKey(arg.Name) Then
                    arguments += values(arg.Name)
                Else
                    With values.Keys.Where(Function(s) s.TextEquals(arg.Name)).FirstOrDefault
                        If .StringEmpty Then

                            ' 在values参数列表之中查找不到，则可能是在CSS之中定义的样式，查看CSS样式文件之中是否存在？
                            Dim style As Selector = CSS("#" & arg.Name)

                            If style Is Nothing Then
                                ' 在CSS之中没有定义，则判断这个参数是否为可选参数，如果不是可选参数，则抛出错误
                                If Not arg.IsOptional Then
                                    Throw New ArgumentNullException(String.Format(RequiredArgvNotFound, arg.Name))
                                Else
                                    arguments += arg.DefaultValue
                                End If

                            Else

                                ' 因为绘图的样式值都是使用CSS字符串来完成的，所以
                                ' 在这里就直接调用CSS样式的ToString方法来得到参数值了
                                Dim cssValue$ = style.ToString
                                arguments += cssValue

                            End If

                        Else
                            arguments += values(.ref)
                        End If
                    End With
                End If
            Next

            Return type.Invoke(driver.Target, arguments.ToArray)
        End Function

        Const RequiredArgvNotFound$ = "Parameter '{0}' which is required by the graphics driver function is not found!"
    End Module
End Namespace