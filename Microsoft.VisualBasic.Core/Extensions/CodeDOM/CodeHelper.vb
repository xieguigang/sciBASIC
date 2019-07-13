#Region "Microsoft.VisualBasic::a932a3b98d366ee02de011c3b4c836eb, Microsoft.VisualBasic.Core\Extensions\CodeDOM\CodeHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
