#Region "Microsoft.VisualBasic::94ae4e0b491c5d095a6e2a53002a395b, Microsoft.VisualBasic.Core\ApplicationServices\Debugger\VisualBasicAppException.vb"

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

    '     Class VisualBasicAppException
    ' 
    '         Properties: args, Environment, System
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Creates
    ' 
    '     Module ExceptionExtensions
    ' 
    ' 
    '         Delegate Sub
    ' 
    '             Function: Fail
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' VisualBasic application exception wrapper
    ''' </summary>
    Public Class VisualBasicAppException : Inherits Exception

        ''' <summary>
        ''' The CLI arguments string
        ''' </summary>
        ''' <returns></returns>
        Public Property args As String
        ''' <summary>
        ''' The internal App environment variables
        ''' </summary>
        ''' <returns></returns>
        Public Property Environment As Dictionary(Of String, String)
        ''' <summary>
        ''' The system version information
        ''' </summary>
        ''' <returns></returns>
        Public Property System As Dictionary(Of String, String)

        ''' <summary>
        ''' <see cref="Exception"/> inner wrapper
        ''' </summary>
        ''' <param name="ex">The exception details</param>
        ''' <param name="calls">Method name where occurs this exception.</param>
        Sub New(ex As Exception, calls As String)
            MyBase.New("@" & calls, ex)
        End Sub

        Public Shared Function Creates(msg As String, calls As String) As VisualBasicAppException
            Return New VisualBasicAppException(New Exception(msg), calls)
        End Function
    End Class

    Public Module ExceptionExtensions

        ''' <summary>
        ''' 处理错误的工作逻辑的抽象接口
        ''' </summary>
        ''' <param name="ex">Socket的内部错误信息</param>
        ''' <remarks></remarks>
        Public Delegate Sub ExceptionHandler(ex As Exception)

        ''' <summary>
        ''' Just throw exception, but the exception contains more details information for the debugging
        ''' </summary>
        ''' <param name="msg$"></param>
        ''' <returns></returns>
        Public Function Fail(msg$, <CallerMemberName> Optional caller$ = "") As VisualBasicAppException
            Return VisualBasicAppException.Creates(msg, caller)
        End Function
    End Module
End Namespace
