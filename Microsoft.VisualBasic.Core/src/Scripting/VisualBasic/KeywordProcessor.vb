#Region "Microsoft.VisualBasic::e3def9fdd143b474305f796dd8a75cfb, Microsoft.VisualBasic.Core\src\Scripting\VisualBasic\KeywordProcessor.vb"

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


    ' Code Statistics:

    '   Total Lines: 71
    '    Code Lines: 47
    ' Comment Lines: 18
    '   Blank Lines: 6
    '     File Size: 3.19 KB


    '     Class KeywordProcessor
    ' 
    '         Properties: TokenWords
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AutoEscapeVBKeyword
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
            "|AddHandler|AddressOf|Alias|And|AndAlso|As|Ascending|" &
            "|Boolean|ByRef|Byte|By|" &
            "|Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|" &
            "|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|" &
            "|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|" &
            "|False|Finally|For|Friend|Function|From|" &
            "|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|" &
            "|Handles|" &
            "|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|" &
            "|Let|Lib|Like|Long|Loop|" &
            "|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|My|" &
            "|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|NameOf|" &
            "|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|Order|" &
            "|ParamArray|Partial|Private|Property|Protected|Public|" &
            "|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|" &
            "|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|" &
            "|Then|Throw|To|True|Try|TryCast|TypeOf|" &
            "|Variant|" &
            "|Wend|" &
            "|UInteger|ULong|UShort|Using|Union|" &
            "|When|While|Widening|With|WithEvents|WriteOnly|Where|" &
            "|Xor|" &
            "|Yield|"

        ''' <summary>
        ''' Tokenize of <see cref="VBKeywords"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TokenWords As String()
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
