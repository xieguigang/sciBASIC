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
        Public Function RunPlot(driver As [Delegate], CSS As CssBlock, ParamArray args As Argument()) As GraphicsData
            Dim type As MethodInfo = driver.Method
            Dim parameters = type.GetParameters
            Dim values As Dictionary(Of String, Argument) = args.ToDictionary(Function(arg) arg.name)

        End Function

        Sub test()

            With New VB

                RunPlot(Nothing, Nothing, !A = 99, !B = 123, !C = "dertfff")
            End With
        End Sub
    End Module
End Namespace