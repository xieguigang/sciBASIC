#Region "Microsoft.VisualBasic::943dc4873e2b69e6c4c0474126f3c2a0, nlp\NLP\BM25\IdfVariant.vb"

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

    '   Total Lines: 12
    '    Code Lines: 6 (50.00%)
    ' Comment Lines: 5 (41.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (8.33%)
    '     File Size: 378 B


    '     Enum IdfVariant
    ' 
    '         Lucene, Okapi
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BM25

    ''' <summary>
    ''' IDF 计算方式。
    ''' </summary>
    Public Enum IdfVariant
        ''' <summary>Lucene/Elasticsearch 变体: log(1 + (N-n+0.5)/(n+0.5))，避免负值。</summary>
        Lucene
        ''' <summary>原始 Okapi 变体: log((N-n+0.5)/(n+0.5))，可能产生负值。</summary>
        Okapi
    End Enum
End Namespace
