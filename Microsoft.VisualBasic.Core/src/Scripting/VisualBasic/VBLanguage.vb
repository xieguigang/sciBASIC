#Region "Microsoft.VisualBasic::0064a6a31284baa503da7845ed505bf0, Microsoft.VisualBasic.Core\src\Scripting\VisualBasic\VBLanguage.vb"

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

    '   Total Lines: 77
    '    Code Lines: 59
    ' Comment Lines: 10
    '   Blank Lines: 8
    '     File Size: 3.38 KB


    '     Class Patterns
    ' 
    '         Properties: TypeChar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CharToType, TypeCharName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Scripting.SymbolBuilder.VBLanguage

    ''' <summary>
    ''' <see cref="Regex"/> pattern of VisualBasic language tokens
    ''' </summary>
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

        Public Shared Function CharToType(c As Char) As Type
            Select Case c
                Case "!"c
                    Return GetType(Single)
                Case "@"c
                    Return GetType(Decimal)
                Case "#"c
                    Return GetType(Double)
                Case "$"c
                    Return GetType(String)
                Case "%"c
                    Return GetType(Integer)
                Case "&"c
                    Return GetType(Long)
                Case "?"c
                    Return GetType(Boolean)
                Case Else
                    Throw New InvalidExpressionException($"Character '{c}' is not a valid VB type char!")
            End Select
        End Function
    End Class
End Namespace
