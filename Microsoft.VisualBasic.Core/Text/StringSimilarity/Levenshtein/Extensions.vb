#Region "Microsoft.VisualBasic::05d9ecdacf1a7d59575c150f068399d1, Microsoft.VisualBasic.Core\Text\StringSimilarity\Levenshtein\Extensions.vb"

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
Imports Microsoft.VisualBasic.Language

Namespace Text.Levenshtein

    Public Module LevExtensions

        <Extension> Public Sub GetMatches(Of T)(edits As DistResult, ref As T(), hyp As T(), ByRef refOUT As T(), ByRef hypOUT As T())
            Dim len As Integer = edits.DistEdits.Count("m"c)
            Dim idx As VBInteger = Scan0
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
