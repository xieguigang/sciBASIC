Imports System.Runtime.CompilerServices

Namespace Model

    Public Class Word

        ''' <summary>
        ''' the word text
        ''' </summary>
        ''' <returns></returns>
        Public Property str As String
        ''' <summary>
        ''' the reference count of current word
        ''' </summary>
        ''' <returns></returns>
        Public Property num As Integer

        Public Sub New(s As String)
            Me.str = s
            Me.num = 1
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function StartsWith(token As String) As Boolean
            Return str.StartsWith(token)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToLower() As String
            Return Strings.LCase(str)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return str
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(w As Word) As String
            Return w.str
        End Operator
    End Class
End Namespace