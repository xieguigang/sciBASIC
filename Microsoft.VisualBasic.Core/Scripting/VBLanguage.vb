#Region "Microsoft.VisualBasic::5d571b3e592c026eec19bbd5d23261ad, Microsoft.VisualBasic.Core\Scripting\VBLanguage.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Class Patterns
' 
'         Constructor: (+1 Overloads) Sub New
' 
'     Class KeywordProcessor
' 
'         Properties: Words
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: AutoEscapeVBKeyword
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Scripting.SymbolBuilder.VBLanguage

    Public NotInheritable Class Patterns

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 匹配一个合法的标识符，在正则匹配的时候应该不区分大小写
        ''' </summary>
        Public Const Identifer$ = "\[?[_a-z][_a-z0-9]*\]?"

        Public Const Access$ = "((Partial )|(Public )|(Private )|(Friend )|(Protected )|(Shadows )|(Shared )|(Overrides )|(Overloads )|(Overridable )|(MustOverrides )|(NotInheritable )|(MustInherit ))*"
        Public Const Type$ = "^\s*" & Access & "((Class)|(Module)|(Structure)|(Enum)|(Delegate)|(Interface))\s+" & VBLanguage.Patterns.Identifer
        Public Const Property$ = "^\s+" & Access & "\s*((ReadOnly )|(WriteOnly )|(Default ))*\s*Property\s+" & VBLanguage.Patterns.Identifer
        Public Const Method$ = "^\s+" & Access & "\s*((Sub )|(Function )|(Iterator )|(Operator ))+\s*" & VBLanguage.Patterns.Identifer
        Public Const Operator$ = "^\s+" & Access & "\s*Operator\s+(([<]|[>]|\=|\+|\-|\*|/|\^|\\)+|(" & VBLanguage.Patterns.Identifer & "))"
        Public Const Close$ = "^\s+End\s((Sub)|(Function)|(Class)|(Structure)|(Enum)|(Interface)|(Operator)|(Module))"
        Public Const CloseType$ = "^\s*End\s((Class)|(Structure)|(Enum)|(Interface)|(Module))"
        Public Const Indents$ = "^\s+"
        Public Const Attribute$ = "<.+?>\s*"

        ''' <summary>
        ''' The VB.NET type char index
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TypeChar As Index(Of Char) = {"!"c, "@"c, "#"c, "$"c, "%"c, "&"c, "?"c}

        Public Shared Function TypeCharName(c As Char) As String
            Select Case c
                Case "!"c
                    Return "Single"
                Case "@"c
                    Return "Decimal"
                Case "#"c
                    Return "Double"
                Case "$"c
                    Return "String"
                Case "%"c
                    Return "Integer"
                Case "&"c
                    Return "Long"
                Case "?"c
                    Return "Boolean"
                Case Else
                    Throw New InvalidExpressionException($"Character '{c}' is not a valid VB type char!")
            End Select
        End Function

    End Class

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
