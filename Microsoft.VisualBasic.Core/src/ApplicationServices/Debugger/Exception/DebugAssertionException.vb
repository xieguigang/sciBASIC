Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 调试断言失败异常：当 <see cref="DebugAudit.Require"/> 调用失败，或者 <see cref="DebugAudit.ThrowOnFailure"/>
    ''' 开关被打开之后 <see cref="DebugAudit.Check"/> / <see cref="DebugAudit.Test"/> 检查失败的时候抛出这个异常类型。
    ''' </summary>
    ''' <remarks>
    ''' 这个异常类型只是用于将失败位置(表达式原文、源文件、行号、函数名)结构化地传递给上层调用者，
    ''' 因此并不需要额外的内部异常作为<see cref="Exception.InnerException"/>。
    ''' </remarks>
    Public Class DebugAssertionException : Inherits Exception

        ''' <summary>
        ''' 检查失败的表达式原文
        ''' </summary>
        ''' <returns></returns>
        Public Property Expression As String
            Get
                Return _expression
            End Get
            Private Set(value As String)
                _expression = value
            End Set
        End Property

        ''' <summary>
        ''' 表达式所在的源代码文件路径
        ''' </summary>
        ''' <returns></returns>
        Public Property SourceFile As String
            Get
                Return _sourceFile
            End Get
            Private Set(value As String)
                _sourceFile = value
            End Set
        End Property

        ''' <summary>
        ''' 表达式所在的源代码行号，未知的时候为-1
        ''' </summary>
        ''' <returns></returns>
        Public Property Line As Integer
            Get
                Return _line
            End Get
            Private Set(value As Integer)
                _line = value
            End Set
        End Property

        ''' <summary>
        ''' 表达式所在的函数名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Member As String
            Get
                Return _member
            End Get
            Private Set(value As String)
                _member = value
            End Set
        End Property

        Private _expression As String
        Private _sourceFile As String
        Private _line As Integer
        Private _member As String

        ''' <summary>
        ''' 使用一条已经格式化好的失败消息来构造断言失败异常
        ''' </summary>
        ''' <param name="message">断言失败的完整消息文本</param>
        ''' <param name="expression">检查失败的表达式原文</param>
        ''' <param name="sourceFile">表达式所在的源代码文件路径</param>
        ''' <param name="line">表达式所在的源代码行号</param>
        ''' <param name="member">表达式所在的函数名称</param>
        Public Sub New(message As String, expression As String, sourceFile As String, line As Integer, member As String)
            Call MyBase.New(message)

            _expression = expression
            _sourceFile = sourceFile
            _line = line
            _member = member
        End Sub

        ''' <summary>
        ''' 从一个调试审计事件构造断言失败异常
        ''' </summary>
        ''' <param name="failure">失败的审计事件</param>
        ''' <returns></returns>
        Public Shared Function Create(failure As DebugAuditEvent) As DebugAssertionException
            If failure Is Nothing Then
                Return New DebugAssertionException("Assertion failed: <unknown>", Nothing, Nothing, -1, Nothing)
            Else
                Return New DebugAssertionException(
                    message:=failure.Describe(),
                    expression:=failure.Expression,
                    sourceFile:=failure.SourceFile,
                    line:=failure.Line,
                    member:=failure.Member
                )
            End If
        End Function
    End Class
End Namespace
