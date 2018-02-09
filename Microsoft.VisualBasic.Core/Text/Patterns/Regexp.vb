Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Text.Patterns

    Public Class Regexp

        ReadOnly r As Regex
        ReadOnly pattern$

        Public ReadOnly Property Regex As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return pattern
            End Get
        End Property

        Sub New(pattern$, Optional opts As RegexOptions = RegexICSng)
            Me.r = New Regex(pattern, opts)
            Me.pattern = pattern
        End Sub

        Public Overrides Function ToString() As String
            Return pattern & " @ " & r.Options.ToString
        End Function

        ''' <summary>
        ''' Grep target input <paramref name="text"/> by a specific regex <paramref name="pattern"/>.
        ''' Match all of the sub string in target <paramref name="text"/> that matched the input regex <paramref name="pattern"/>.
        ''' (即从目标字符串之中匹配出所有符合目标模式的字符串)
        ''' </summary>
        ''' <param name="pattern"></param>
        ''' <param name="text$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <=(pattern As Regexp, text$) As String()
            Return pattern.r.Matches(text).ToArray
        End Operator

        Public Shared Operator >=(pattern As Regexp, text$) As String()
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' 求出目标字符串<paramref name="text"/>和<paramref name="pattern"/>的交集，即使用正则表达式来匹配目标字符串之中的子串
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="pattern"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator And(text$, pattern As Regexp) As String
            Return pattern.r.Match(text).Value
        End Operator

        ''' <summary>
        ''' 求出目标字符串<paramref name="text"/>和<paramref name="pattern"/>的交集，即使用正则表达式来匹配目标字符串之中的子串
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="pattern"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator And(pattern As Regexp, text$) As String
            Return pattern.r.Match(text).Value
        End Operator

        ''' <summary>
        ''' Target input <paramref name="text"/> is equals to the regex <paramref name="pattern"/>?
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="pattern"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(text$, pattern As Regexp) As Boolean
            Return (text And pattern) = text
        End Operator

        ''' <summary>
        ''' Target input <paramref name="text"/> is equals to the regex <paramref name="pattern"/>?
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="pattern"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(pattern As Regexp, text$) As Boolean
            Return text Like pattern
        End Operator
    End Class
End Namespace