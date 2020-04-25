Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace My.JavaScript

    Public Module [String]

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function parseInt(s As String, Optional radix% = 10) As Integer
            Return Convert.ToInt32(s, radix)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function substr(str$, start%) As String
            Return str.Substring(start)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function substr(str$, start%, length%) As String
            Return str.Substring(start, length)
        End Function

        <Extension>
        Public Function includes(str$, part$) As Boolean
            If str.StringEmpty Then
                Return False
            ElseIf part.StringEmpty Then
                Return True
            Else
                Return str.Contains(part)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function test(pattern$, target$) As Boolean
            Return r.Match(target, pattern).Success
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function test(r As Regex, target$) As Boolean
            Return r.Match(target).Success
        End Function

        <Extension>
        Public Sub match(text$, pattern$, ByRef a$, ByRef b$, Optional ByRef c$ = Nothing)
            Dim parts = text.Match(pattern)

            a = parts.ElementAtOrNull(0)
            b = parts.ElementAtOrNull(1)
            c = parts.ElementAtOrNull(2)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function match(text$, pattern$) As String()
            Return r.Match(text, pattern).Captures.AsQueryable.Cast(Of String).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function match(text$, pattern As r) As String()
            Return pattern.Match(text).Captures.AsQueryable.Cast(Of String).ToArray
        End Function
    End Module
End Namespace