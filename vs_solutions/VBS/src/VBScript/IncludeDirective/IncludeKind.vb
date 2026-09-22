Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet

Namespace Script

    ''' <summary>
    ''' <c>#include</c> 指令所引用的目标类型。
    ''' </summary>
    Public Enum IncludeKind

        ''' <summary>外部 CLR 程序集(.dll)</summary>
        [Assembly]

        ''' <summary>其它 VB.NET 脚本(.vb) —— 其代码会被复制到主脚本的顶层命名空间</summary>
        Script

        ''' <summary>nuget 程序包(由 <see cref="NuGetResolver"/> 解析并缓存到本地)</summary>
        NuGet

        ''' <summary>无法解析的目标(保持历史宽松语义: 仅告警)</summary>
        Unresolved
    End Enum
End Namespace