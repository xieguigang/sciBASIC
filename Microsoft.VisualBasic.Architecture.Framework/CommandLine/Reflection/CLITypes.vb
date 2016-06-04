Namespace CommandLine.Reflection

    ''' <summary>
    ''' The data type enumeration of the target optional parameter switch.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CLITypes
        ''' <summary>
        ''' String.(对于指定为字符串类型的参数，在调用的时候回自动调用<see cref="Extensions.CLIToken"/>函数)
        ''' </summary>
        ''' <remarks></remarks>
        [String]
        ''' <summary>
        ''' Int
        ''' </summary>
        ''' <remarks></remarks>
        [Integer]
        ''' <summary>
        ''' Real
        ''' </summary>
        ''' <remarks></remarks>
        [Double]
        ''' <summary>
        ''' This is a flag value, if this flag presents in the CLI, then this named Boolean value is TRUE, otherwise is FALSE.
        ''' </summary>
        [Boolean]
        ''' <summary>
        ''' File path, is equals most string.(对于指定为路径类型的参数值，在生成命令行的时候会自动调用<see cref="CLIPath"/>函数)
        ''' </summary>
        File
    End Enum
End Namespace