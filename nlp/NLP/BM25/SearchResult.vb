#Region "Microsoft.VisualBasic::66ba65df774da8540ffc756e0696e6fe, nlp\NLP\BM25\SearchResult.vb"

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

    '   Total Lines: 22
    '    Code Lines: 10 (45.45%)
    ' Comment Lines: 6 (27.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 622 B


    '     Class SearchResult
    ' 
    '         Properties: DocId, Score, TermContributions
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BM25

    ''' <summary>
    ''' 单条检索结果。
    ''' </summary>
    Public Class SearchResult

        ''' <summary>文档 ID。</summary>
        Public Property DocId As Integer

        ''' <summary>BM25 得分。</summary>
        Public Property Score As Double

        ''' <summary>各查询词的贡献明细（用于可解释性）。</summary>
        Public Property TermContributions As List(Of TermContribution)

        Public Overrides Function ToString() As String
            Return $"Doc#{DocId}  Score={Score:F6}"
        End Function

    End Class
End Namespace
