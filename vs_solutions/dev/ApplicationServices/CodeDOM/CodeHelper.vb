#Region "Microsoft.VisualBasic::a932a3b98d366ee02de011c3b4c836eb, sciBASIC#\vs_solutions\dev\ApplicationServices\CodeDOM\CodeHelper.vb"

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

    '   Total Lines: 72
    '    Code Lines: 46
    ' Comment Lines: 12
    '   Blank Lines: 14
    '     File Size: 2.59 KB


    '     Module CodeHelper
    ' 
    '         Function: EnumCodeHelper, EnumMember
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder.VBLanguage
Imports Microsoft.VisualBasic.Text

Namespace Emit.CodeDOM_VBC

    Public Module CodeHelper

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="items"></param>
        ''' <param name="enumName$"></param>
        ''' <param name="type$"></param>
        ''' <param name="pascalStyle"></param>
        ''' <returns></returns>
        <Extension>
        Public Function EnumCodeHelper(items As IEnumerable(Of String), enumName$, Optional type$ = NameOf(Int32), Optional pascalStyle As Boolean = True) As String
            Dim src$ = GetType(DescriptionAttribute).Namespace
            Dim code As New StringBuilder

            Call code.AppendLine($"Imports {src}")
            Call code.AppendLine()
            Call code.AppendLine($"Public Enum {enumName} As {type}")

            ' 将字符串集合转换为枚举成员
            For Each member$ In items
                ' xml code comments
                Call code.AppendLine("''' <summary>")
                Call code.AppendLine("''' " & member)
                Call code.AppendLine("''' </summary>")

                ' add member name
                Call code.AppendLine(vbTab & member.EnumMember(pascalStyle))
            Next

            Call code.AppendLine("End Enum")

            Return code.ToString
        End Function

        <Extension>
        Public Function EnumMember(member$, Optional pascalStyle As Boolean = True, Optional newLine As Boolean = False) As String
            Dim src = $"<Description(""{member}"")>"

            ' generate valid enum member name
            member = member.ReplaceChars(ASCII.Symbols, "_"c)

            If pascalStyle Then
                member = member _
                    .Split("_"c) _
                    .Where(Function(s) Not s.StringEmpty) _
                    .Select(AddressOf UpperCaseFirstChar) _
                    .JoinBy("")
            End If

            If Char.IsSymbol(member.First) AndAlso member.First <> "_"c Then
                member = "_" & Mid(member, 2)
            End If
            If Char.IsDigit(member.First) Then
                member = "_" & member
            End If

            member = KeywordProcessor.AutoEscapeVBKeyword(member)
            member = src & If(newLine, vbCrLf, " ") & member

            Return member
        End Function
    End Module
End Namespace
