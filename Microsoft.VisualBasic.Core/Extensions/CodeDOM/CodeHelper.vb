#Region "Microsoft.VisualBasic::ea3539d65fe975a02454e73f31350c4f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\CodeDOM\CodeHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Text
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
                src = $"<Description(""{member}"")>"

                ' xml code comments
                Call code.AppendLine("''' <summary>")
                Call code.AppendLine("''' " & member)
                Call code.AppendLine("''' </summary>")

                ' generate valid enum member name
                member = member.ReplaceChars(ASCII.Symbols, "_"c)

                If pascalStyle Then
                    member = member _
                        .Split("_"c) _
                        .Where(Function(s) Not s.StringEmpty) _
                        .Select(AddressOf UpperCaseFirstChar) _
                        .JoinBy("")
                End If

                ' add member name
                Call code.AppendLine(vbTab & src & " " & member)
            Next

            Call code.AppendLine("End Enum")

            Return code.ToString
        End Function
    End Module
End Namespace
