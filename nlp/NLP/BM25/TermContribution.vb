#Region "Microsoft.VisualBasic::b737f33b396660910da27eb041809d0d, nlp\NLP\BM25\TermContribution.vb"

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

    '   Total Lines: 27
    '    Code Lines: 10 (37.04%)
    ' Comment Lines: 9 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 753 B


    '     Class TermContribution
    ' 
    '         Properties: Contribution, Idf, LengthFactor, Term, Tf
    '                     TfSaturation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BM25

    ''' <summary>
    ''' 单个查询词的贡献明细。
    ''' </summary>
    Public Class TermContribution

        ''' <summary>查询词。</summary>
        Public Property Term As String

        ''' <summary>IDF 值。</summary>
        Public Property Idf As Double

        ''' <summary>词频 TF。</summary>
        Public Property Tf As Integer

        ''' <summary>文档长度因子。</summary>
        Public Property LengthFactor As Double

        ''' <summary>该词的 TF 饱和部分得分。</summary>
        Public Property TfSaturation As Double

        ''' <summary>该词的最终贡献分。</summary>
        Public Property Contribution As Double

    End Class
End Namespace
