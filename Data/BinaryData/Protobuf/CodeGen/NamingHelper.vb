Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\CodeGen\NamingHelper.vb
' /********************************************************************************/

Imports System.Text

Namespace CodeGen

    ''' <summary>
    ''' 标识符命名转换与关键字转义辅助。
    ''' </summary>
    Friend Module NamingHelper

        ''' <summary>
        ''' Visual Basic 的保留关键字集合（用于生成 <c>[Name]</c> 形式的转义标识符）。
        ''' </summary>
        Friend ReadOnly VbKeywords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "AddHandler", "AddressOf", "Alias", "And", "AndAlso", "As", "Boolean", "ByRef", "Byte", "ByVal",
            "Call", "Case", "Catch", "CBool", "CByte", "CChar", "CDate", "CDbl", "CDec", "Char", "CInt",
            "Class", "CLng", "CObj", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr", "CType",
            "CUInt", "CULng", "CUShort", "Date", "Decimal", "Declare", "Default", "Delegate", "Dim",
            "DirectCast", "Do", "Double", "Each", "Else", "ElseIf", "End", "EndIf", "Enum", "Erase",
            "Error", "Event", "Exit", "False", "Finally", "For", "Friend", "Function", "Get", "GetType",
            "GetXmlNamespace", "Global", "GoSub", "GoTo", "Handles", "If", "Implements", "Imports", "In",
            "Inherits", "Integer", "Interface", "Is", "IsNot", "Let", "Lib", "Like", "Long", "Loop", "Me",
            "Mod", "Module", "MustInherit", "MustOverride", "MyBase", "MyClass", "Namespace", "Narrowing",
            "New", "Next", "Not", "Nothing", "NotInheritable", "NotOverridable", "Object", "Of", "On",
            "Operator", "Option", "Optional", "Or", "OrElse", "Out", "Overloads", "Overridable", "Overrides",
            "ParamArray", "Partial", "Private", "Property", "Protected", "Public", "RaiseEvent", "ReadOnly",
            "ReDim", "REM", "RemoveHandler", "Resume", "Return", "SByte", "Select", "Set", "Shadows",
            "Shared", "Short", "Single", "Static", "Step", "Stop", "String", "Structure", "Sub", "SyncLock",
            "Then", "Throw", "To", "True", "Try", "TryCast", "TypeOf", "UInteger", "ULong", "UShort",
            "Using", "Variant", "Wend", "When", "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor"
        }

        ''' <summary>
        ''' C# 的保留关键字集合（用于生成 <c>@name</c> 形式的转义标识符）。
        ''' </summary>
        Friend ReadOnly CSharpKeywords As New HashSet(Of String)(StringComparer.Ordinal) From {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
            "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
            "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        }

        ''' <summary>
        ''' 把 snake_case / camelCase 标识符转换为 PascalCase。
        ''' </summary>
        Friend Function ToPascalCase(name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return name
            End If

            Dim builder As New StringBuilder()
            Dim upperNext As Boolean = True

            For Each ch As Char In name
                If ch = "_"c OrElse ch = "-"c OrElse ch = " "c OrElse ch = "."c Then
                    upperNext = True
                    Continue For
                End If

                If upperNext Then
                    Call builder.Append(Char.ToUpperInvariant(ch))
                    upperNext = False
                Else
                    Call builder.Append(ch)
                End If
            Next

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' 把标识符转换为 camelCase。
        ''' </summary>
        Friend Function ToCamelCase(name As String) As String
            Dim pascal As String = ToPascalCase(name)

            If String.IsNullOrEmpty(pascal) Then
                Return pascal
            End If

            If pascal.Length = 1 Then
                Return pascal.ToLowerInvariant()
            End If

            Return Char.ToLowerInvariant(pascal(0)) & pascal.Substring(1)
        End Function

        ''' <summary>
        ''' 按语言规则转义标识符（VB 用方括号，C# 用 @ 前缀）。
        ''' </summary>
        Friend Function Escape(name As String, language As CodeLanguage) As String
            If String.IsNullOrEmpty(name) Then
                Return name
            End If

            If language = CodeLanguage.VB Then
                If VbKeywords.Contains(name) Then
                    Return $"[{name}]"
                Else
                    Return name
                End If
            Else
                If CSharpKeywords.Contains(name) Then
                    Return $"@{name}"
                Else
                    Return name
                End If
            End If
        End Function

        ''' <summary>
        ''' 把 package 名称转换为目标语言的命名空间名称（各段转 PascalCase）。
        ''' </summary>
        Friend Function ToNamespace(package As String) As String
            If String.IsNullOrWhiteSpace(package) Then
                Return ""
            End If

            Dim parts As String() = package.Split("."c)
            Dim builder As New StringBuilder()

            For Each part As String In parts
                If part.Length = 0 Then
                    Continue For
                End If

                If builder.Length > 0 Then
                    Call builder.Append("."c)
                End If

                Call builder.Append(ToPascalCase(part))
            Next

            Return builder.ToString()
        End Function

    End Module

End Namespace
