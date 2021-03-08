Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 处理错误的工作逻辑的抽象接口
    ''' </summary>
    ''' <param name="ex">Socket的内部错误信息</param>
    ''' <remarks></remarks>
    Public Delegate Sub ExceptionHandler(ex As Exception)

    <HideModuleName>
    Public Module ExceptionExtensions

        ''' <summary>
        ''' Just throw exception, but the exception contains more details information for the debugging
        ''' </summary>
        ''' <param name="msg$"></param>
        ''' <returns></returns>
        Public Function Fail(msg$, <CallerMemberName> Optional caller$ = Nothing) As VisualBasicAppException
            Return VisualBasicAppException.Creates(msg, caller)
        End Function
    End Module
End Namespace