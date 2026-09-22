Namespace Script

    ''' <summary>
    ''' 一条 <c>#include</c> 预编译指令。
    ''' </summary>
    Public Class IncludeDirective

        ''' <summary>指令原文(引号内的目标文本)</summary>
        Public Property Raw As String

        ''' <summary>本地文件绝对路径(仅 Assembly / Script)</summary>
        Public Property ResolvedPath As String

        ''' <summary>引用目标类型</summary>
        Public Property Kind As IncludeKind

        ''' <summary>nuget 包 id(仅 NuGet)</summary>
        Public Property PackageId As String

        ''' <summary>nuget 版本或版本范围(仅 NuGet); Nothing 表示取最新稳定版</summary>
        Public Property Version As String

        ''' <summary>声明该指令的脚本文件绝对路径</summary>
        Public Property DeclaredIn As String

        Public Overrides Function ToString() As String
            Select Case Kind
                Case IncludeKind.NuGet
                    Return $"{PackageId}@{Version}"
                Case Else
                    Return If(ResolvedPath, Raw)
            End Select
        End Function
    End Class
End Namespace
