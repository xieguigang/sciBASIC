Imports System.Runtime.CompilerServices

Namespace Scripting.SymbolBuilder.VBLanguage

    ''' <summary>
    ''' Keyword processor of the VB.NET language
    ''' </summary>
    Public NotInheritable Class KeywordProcessor

        ''' <summary>
        ''' List of VB.NET language keywords
        ''' </summary>
        Public Const VBKeywords$ =
            "|AddHandler|AddressOf|Alias|And|AndAlso|As|" &
            "|Boolean|ByRef|Byte|" &
            "|Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|" &
            "|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|" &
            "|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|" &
            "|False|Finally|For|Friend|Function|" &
            "|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|" &
            "|Handles|" &
            "|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|" &
            "|Let|Lib|Like|Long|Loop|" &
            "|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|" &
            "|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|NameOf|" &
            "|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|" &
            "|ParamArray|Partial|Private|Property|Protected|Public|" &
            "|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|" &
            "|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|" &
            "|Then|Throw|To|True|Try|TryCast|TypeOf|" &
            "|Variant|" &
            "|Wend|" &
            "|UInteger|ULong|UShort|Using|" &
            "|When|While|Widening|With|WithEvents|WriteOnly|" &
            "|Xor|" &
            "|Yield|"

        ''' <summary>
        ''' Tokenize of <see cref="VBKeywords"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Words As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return VBKeywords _
                    .Split("|"c) _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
            End Get
        End Property

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Escaping the vb variable name when it conflicts with VB keywords name, 
        ''' this function can be using for the VB.NET related code generator.
        ''' </summary>
        ''' <param name="name$">The identifier name.</param>
        ''' <returns>If the identifier is a VB.NET keyword, then it will be escaping and returns, 
        ''' otherwise, will do nothing, function returns the raw input identifier.
        ''' </returns>
        Public Shared Function AutoEscapeVBKeyword(name$) As String
            If InStr(VBKeywords, $"|{name}|", CompareMethod.Text) > 0 Then
                Return $"[{name}]"
            Else
                Return name
            End If
        End Function
    End Class
End Namespace