Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    Public Module DebugAudit

        Dim captures As New Dictionary(Of String, Object)

        Public Function GetCaptures() As Dictionary(Of String, Object)
            Return captures
        End Function

        <Conditional("DEBUG")>
        Public Sub Snapshot(ParamArray vars As (name$, value As Object)())
            Call captures.Clear()

            For Each var As (name$, value As Object) In vars
                captures(var.name) = var.value
            Next
        End Sub

        <Conditional("DEBUG")>
        Public Sub Check(condition As Boolean,
                         Optional msg As String = Nothing,
                         <CallerArgumentExpression("condition")> Optional expr As String = Nothing,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = -1,
                         <CallerMemberName> Optional callee As String = Nothing)

            If Not condition Then
                captures(expr) = If(msg, $"条件不满足：{expr}") & $" at {callee} {file}:line {line}"
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <example>
        ''' DebugAudit.V(user.Profile.Age > 18)
        ''' ' will generates tuple output
        ''' ' ( name,                  value) 
        ''' ' ("user.Profile.Age > 18", True)
        ''' </example>
        Public Function V(value As Object, <CallerArgumentExpression("value")> Optional name As String = Nothing) As (name As String, value As Object)
            Return (If(name, "<unknown>"), value)
        End Function
    End Module
End Namespace