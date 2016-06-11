Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language

Public Module RegexExtensions

    Public Structure [NameOf]

        '
        ' Summary:
        '     Specifies that no options are set. For more information about the default behavior
        '     of the regular expression engine, see the "Default Options" section in the Regular
        '     Expression Options topic.
        Public Const None As String = NameOf(None)
        '
        ' Summary:
        '     Specifies case-insensitive matching. For more information, see the "Case-Insensitive
        '     Matching " section in the Regular Expression Options topic.
        Public Const IgnoreCase As String = NameOf(IgnoreCase)
        '
        ' Summary:
        '     Multiline mode. Changes the meaning of ^ and $ so they match at the beginning
        '     and end, respectively, of any line, and not just the beginning and end of the
        '     entire string. For more information, see the "Multiline Mode" section in the
        '     Regular Expression Options topic.
        Public Const Multiline As String = NameOf(Multiline)
        '
        ' Summary:
        '     Specifies that the only valid captures are explicitly named or numbered groups
        '     of the form (?<name>…). This allows unnamed parentheses to act as noncapturing
        '     groups without the syntactic clumsiness of the expression (?:…). For more information,
        '     see the "Explicit Captures Only" section in the Regular Expression Options topic.
        Public Const ExplicitCapture As String = NameOf(ExplicitCapture)
        '
        ' Summary:
        '     Specifies that the regular expression is compiled to an assembly. This yields
        '     faster execution but increases startup time. This value should not be assigned
        '     to the System.Text.RegularExpressions.RegexCompilationInfo.Options property when
        '     calling the System.Text.RegularExpressions.Regex.CompileToAssembly(System.Text.RegularExpressions.RegexCompilationInfo[],System.Reflection.AssemblyName)
        '     method. For more information, see the "Compiled Regular Expressions" section
        '     in the Regular Expression Options topic.
        Public Const Compiled As String = NameOf(Compiled)
        '
        ' Summary:
        '     Specifies single-line mode. Changes the meaning of the dot (.) so it matches
        '     every character (instead of every character except \n). For more information,
        '     see the "Single-line Mode" section in the Regular Expression Options topic.
        Public Const Singleline As String = NameOf(Singleline)
        '
        ' Summary:
        '     Eliminates unescaped white space from the pattern and enables comments marked
        '     with #. However, this value does not affect or eliminate white space in character
        '     classes, numeric quantifiers, or tokens that mark the beginning of individual
        '     regular expression language elements. For more information, see the "Ignore White
        '     Space" section of the Regular Expression Options topic.
        Public Const IgnorePatternWhitespace As String = NameOf(IgnorePatternWhitespace)
        '
        ' Summary:
        '     Specifies that the search will be from right to left instead of from left to
        '     right. For more information, see the "Right-to-Left Mode" section in the Regular
        '     Expression Options topic.
        Public Const RightToLeft As String = NameOf(RightToLeft)
        '
        ' Summary:
        '     Enables ECMAScript-compliant behavior for the expression. This value can be used
        '     only in conjunction with the System.Text.RegularExpressions.RegexOptions.IgnoreCase,
        '     System.Text.RegularExpressions.RegexOptions.Multiline, and System.Text.RegularExpressions.RegexOptions.Compiled
        '     values. The use of this value with any other values results in an exception.For
        '     more information on the System.Text.RegularExpressions.RegexOptions.ECMAScript
        '     option, see the "ECMAScript Matching Behavior" section in the Regular Expression
        '     Options topic.
        Public Const ECMAScript As String = NameOf(ECMAScript)
        '
        ' Summary:
        '     Specifies that cultural differences in language is ignored. For more information,
        '     see the "Comparison Using the Invariant Culture" section in the Regular Expression
        '     Options topic.
        Public Const CultureInvariant As String = NameOf(CultureInvariant)
    End Structure

    <Extension>
    Public Function EachValue(Of T)(m As MatchCollection, parser As Func(Of String, T)) As IEnumerable(Of T)
        Return From s As Match In m Select parser(s.Value)
    End Function

    <ExportAPI("As.Array")>
    <Extension> Public Function ToArray(source As MatchCollection) As String()
        Dim LQuery As String() =
            LinqAPI.Exec(Of String) <= From m As Match
                                       In source
                                       Select m.Value
        Return LQuery
    End Function

    <Extension>
    Public Function ToArray(Of T)(source As MatchCollection, [CType] As Func(Of String, T)) As T()
        Dim LQuery As T() =
            LinqAPI.Exec(Of T) <= From m As Match
                                  In source
                                  Let s As String = m.Value
                                  Select [CType](s)
        Return LQuery
    End Function
End Module
