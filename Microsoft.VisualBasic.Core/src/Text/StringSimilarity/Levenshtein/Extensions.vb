#Region "Microsoft.VisualBasic::91fdbd5ab073e0638d3ad0552d11a05c, Microsoft.VisualBasic.Core\src\Text\StringSimilarity\Levenshtein\Extensions.vb"

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

    '   Total Lines: 58
    '    Code Lines: 40 (68.97%)
    ' Comment Lines: 7 (12.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (18.97%)
    '     File Size: 2.19 KB


    '     Module LevExtensions
    ' 
    '         Function: StripSymbol
    ' 
    '         Sub: GetMatches
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Language

Namespace Text.Levenshtein

    <HideModuleName>
    Public Module LevExtensions

        <Extension>
        Public Sub GetMatches(Of T)(edits As DistResult, ref As T(), hyp As T(), ByRef refOUT As T(), ByRef hypOUT As T())
            Dim len As Integer = edits.DistEdits.Count("m"c)
            Dim idx As i32 = Scan0
            Dim iiiii As Integer = 0

            refOUT = New T(len - 1) {}
            hypOUT = New T(len - 1) {}

            For j As Integer = 0 To hyp.Length   ' 参照subject画列
                For i As Integer = 0 To ref.Length  ' 参照query画行
                    If edits.IsPath(i, j) Then
                        Dim ch As String = edits.DistEdits.Get(++idx)

                        If ch = "m"c Then
                            refOUT(iiiii) = ref(i)
                            hypOUT(iiiii) = hyp(j - 1)

                            iiiii += 1
                        End If
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 将字符串之中的标点符号删除，只留下字母
        ''' </summary>
        ''' <param name="input$"></param>
        ''' <param name="replaceAs$">将这些标点符号替换为目标字符串，默认为空格</param>
        ''' <param name="stripMinusSign">是否将减号也替换掉，默认是不进行替换</param>
        ''' <returns></returns>
        <Extension>
        Public Function StripSymbol(input$, Optional replaceAs$ = " ", Optional stripMinusSign As Boolean = False) As String
            Dim sym As Char() = ASCII.Symbols
            Dim sb As New StringBuilder(input$)

            If Not stripMinusSign Then
                sym = sym.Where(Function(c) c <> "-"c).ToArray
            End If

            For Each c As Char In sym
                Call sb.Replace(c.ToString, replaceAs$)
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
