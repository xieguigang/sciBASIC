Namespace Scripting.SymbolBuilder

    Public NotInheritable Class VBLanguage

        Public Const VBKeywords$ =
            "|AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|" &
            "|CType|CUInt|CULng|CUShort|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For|Friend|Function|Get|" &
            "|GetType|GetXMLNamespace|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|" &
            "|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|Private|Property|" &
            "|Protected|Public|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|" &
            "|Try|TryCast|TypeOf|Variant|Wend|UInteger|ULong|UShort|Using|When|While|Widening|With|WithEvents|WriteOnly|Xor|NameOf|Yield|"

        Private Sub New()
        End Sub

        Public Shared Function AutoEscapeVBKeyword(name$) As String
            If InStr(VBKeywords, $"|{name}|", CompareMethod.Text) > 0 Then
                Return $"[{name}]"
            Else
                Return name
            End If
        End Function
    End Class
End Namespace