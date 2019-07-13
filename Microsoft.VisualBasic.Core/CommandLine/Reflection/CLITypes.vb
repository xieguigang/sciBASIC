#Region "Microsoft.VisualBasic::2fc1db2e6208fb90a99353505f436fe9, Microsoft.VisualBasic.Core\CommandLine\Reflection\CLITypes.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum CLITypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The data type enumeration of the target optional parameter switch.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum CLITypes As Byte

        ''' <summary>
        ''' 其他未知的未定义类型
        ''' (如果是未定义的类型，则在自动生成命令行参数值的时候会被忽略掉，这个时候会需要你自己进行
        ''' 手动处理这些未定义类型的参数值)
        ''' </summary>
        Undefined = 0

        ''' <summary>
        ''' File/directory path, is equals most string.
        ''' (对于指定为路径类型的参数值，在生成命令行的时候会自动调用<see cref="CLIPath"/>函数，
        ''' 在Linux系统之中，文件夹也是一种文件)
        ''' </summary>
        File = 1

        ''' <summary>
        ''' ``String``.(对于指定为字符串类型的参数，在调用的时候回自动调用
        ''' <see cref="Utils.CLIToken"/>函数)
        ''' </summary>
        ''' <remarks></remarks>
        [String] = 2
        ''' <summary>
        ''' Int
        ''' </summary>
        ''' <remarks></remarks>
        [Integer] = 3
        ''' <summary>
        ''' Real
        ''' </summary>
        ''' <remarks></remarks>
        [Double] = 4
        ''' <summary>
        ''' This is a flag value, if this flag presents in the CLI, then this named Boolean 
        ''' value is ``TRUE``, otherwise is ``FALSE``.
        ''' </summary>
        [Boolean] = 5

        ' 下面是高级类型

        ''' <summary>
        ''' This cli argument parameter value is kind of binary value, 
        ''' we can decode this value into binary data chunks.
        ''' 
        ''' (这个命令行参数值是base64字符串，可以看作为一个二进制输入参数)
        ''' </summary>
        Base64 = 100
    End Enum
End Namespace
