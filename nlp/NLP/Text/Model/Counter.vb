#Region "Microsoft.VisualBasic::9fc55aae6e0caf416f3aa455f6b0faf1, Data\Trinity\Model\Counter.vb"

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

    '   Total Lines: 65
    '    Code Lines: 50 (76.92%)
    ' Comment Lines: 3 (4.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (18.46%)
    '     File Size: 2.20 KB


    '     Class Counter
    ' 
    '         Properties: nchar, paragraph, sentences, token, total
    ' 
    '         Function: (+2 Overloads) Count
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Model

    ''' <summary>
    ''' counter for a specific token string
    ''' </summary>
    Public Class Counter

        Public Property token As String

        Public ReadOnly Property nchar As Integer
            Get
                Return token.Length
            End Get
        End Property

        Public Property total As Integer
        Public Property paragraph As Integer
        Public Property sentences As Integer

        Public Shared Iterator Function Count(par As Paragraph) As IEnumerable(Of Counter)
            Dim tokens = par.sentences _
                .SafeQuery _
                .Select(Function(s) s.words.Select(Function(c) (t:=c, s))) _
                .IteratesALL _
                .GroupBy(Function(a) a.t.ToLower) _
                .ToArray

            For Each item In tokens
                Yield New Counter With {
                    .token = item.Key,
                    .total = item.Count,
                    .sentences = item.Select(Function(a) a.s).Distinct.Count,
                    .paragraph = 1
                }
            Next
        End Function

        Public Shared Iterator Function Count(par As IEnumerable(Of Paragraph)) As IEnumerable(Of Counter)
            Dim allTokens As New List(Of (par As Integer, counts As Counter))

            For Each p As Paragraph In par.SafeQuery
                Dim hc As Integer = p.GetHashCode

                For Each t As Counter In Count(p)
                    allTokens.Add((hc, t))
                Next
            Next

            Dim tokens = allTokens _
                .GroupBy(Function(t) t.counts.token.ToLower) _
                .ToArray

            For Each item In tokens
                Yield New Counter With {
                    .token = item.Key,
                    .paragraph = item.Select(Function(a) a.par).Distinct.Count,
                    .sentences = item.Select(Function(a) a.counts.sentences).Sum,
                    .total = item.Select(Function(a) a.counts.total).Sum
                }
            Next
        End Function
    End Class
End Namespace
