Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace CommandLine.SharedORM

    ''' <summary>
    ''' CommandLine source code mapper API
    ''' </summary>
    Public Module API

        ' Usage:= /command1 /param1 <value1> /param2 <value2> [/boolean1 /boolean2 /opt <blabla, default=value233>]

        ''' <summary>
        ''' 从<see cref="ExportAPIAttribute.Usage"/>之中解析出命令行的模型
        ''' </summary>
        ''' <param name="usage$">
        ''' + 由于习惯于在命令行的usage说明之中使用``&lt;>``括号对来包裹参数值，所以usage字符串数据还不能直接使用普通的命令行函数进行解析
        ''' + ``[]``方括号所包裹的参数对象都是可选参数
        ''' + 对于可选参数的默认值，默认值的解析形式为``default=...``，如果没有这个表达式，则默认为Nothing空值为默认值
        ''' </param>
        ''' <returns></returns>
        Public Function CommandLineModel(usage$) As CommandLine
            Dim model As New CommandLine

            Return model
        End Function
    End Module
End Namespace