Namespace Debugging

    ''' <summary>
    ''' 默认的参数值是<see cref="DebuggerLevels.On"/>
    ''' </summary>
    Public Enum DebuggerLevels
        ''' <summary>
        ''' 是否输出调试信息有程序代码来控制，这个是默认的参数
        ''' </summary>
        [On]
        ''' <summary>
        ''' 不会输出任务调试信息
        ''' </summary>
        Off
        ''' <summary>
        ''' 强制覆盖掉<see cref="[On]"/>的设置，输出所有类型的信息
        ''' </summary>
        All
        ''' <summary>
        ''' 只会输出警告或者错误类型的信息
        ''' </summary>
        Warning
        ''' <summary>
        ''' 只会输出错误类型的信息
        ''' </summary>
        [Error]
    End Enum
End Namespace