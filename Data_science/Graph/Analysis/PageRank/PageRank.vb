#Region "Microsoft.VisualBasic::20b379ef25f3b7a6a1b1cb3aea4eba5f, Data_science\Graph\Analysis\PageRank\PageRank.vb"

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

    '   Total Lines: 204
    '    Code Lines: 117 (57.35%)
    ' Comment Lines: 50 (24.51%)
    '    - Xml Docs: 68.00%
    ' 
    '   Blank Lines: 37 (18.14%)
    '     File Size: 8.22 KB


    '     Class PageRank
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputePageRank, PageRankGenerator, PageRankLoop1, TransposeLinkMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis.PageRank

    ''' <summary>
    ''' 无权重的``pagerank``计算模块.(https://github.com/jeffersonhwang/pagerank)
    ''' </summary>
    Public Class PageRank

#Region "Private Fields"

        Dim _incomingLinks As List(Of Integer)(), _leafNodes As List(Of Integer)
        Dim _numLinks As Vector
        Dim _alpha As Double, _convergence As Double
        Dim _checkSteps As Integer

#End Region

#Region "Constructor"

        ''' <summary>
        ''' ``outGoingLinks(i)`` contains the indices of the pages pointed to by page i.
        ''' (每一行都是指向第i行的页面的index值的集合)
        ''' </summary>
        ''' <param name="linkMatrix"><see cref="GraphMatrix"/></param>
        ''' <param name="alpha"></param>
        ''' <param name="convergence"></param>
        ''' <param name="checkSteps"></param>
        Public Sub New(linkMatrix As List(Of Integer)(), Optional alpha# = 0.85, Optional convergence# = 0.0001, Optional checkSteps% = 10)
            With TransposeLinkMatrix(linkMatrix)
                _incomingLinks = .incomingLinks
                _numLinks = .numLinks
                _leafNodes = .leafNodes
            End With

            _alpha = alpha
            _convergence = convergence
            _checkSteps = checkSteps
        End Sub

#End Region

#Region "Methods"

        ''' <summary>
        ''' Convenience wrap for the link matrix transpose and the generator.
        ''' See <see cref="PageRankGenerator"/> method for parameter descriptions
        ''' </summary>
        Public Function ComputePageRank() As Vector
            Dim final As Vector = Nothing

            ' Run the page rank iteration
            For Each generator As Vector In PageRankGenerator(
                _incomingLinks,
                _numLinks,
                _leafNodes,
                _alpha,
                _convergence,
                _checkSteps)

                final = generator
            Next

            Return final
        End Function

        ''' <summary>
        ''' Transposes the link matrix which contains the links from each page. 
        ''' Returns a Tuple of:  
        ''' 
        ''' + 1) pages pointing to a given page, 
        ''' + 2) how many links each page contains, and
        ''' + 3) which pages contain no links at all. 
        ''' 
        ''' We want to know is which pages
        ''' </summary>
        ''' <param name="outGoingLinks">``outGoingLinks(i)`` contains the indices of the pages pointed to by page i</param>
        ''' <returns>A tuple of (incomingLinks, numOutGoingLinks, leafNodes)</returns>
        Protected Function TransposeLinkMatrix(outGoingLinks As List(Of Integer)()) As (incomingLinks As List(Of Integer)(), numLinks As Vector, leafNodes As List(Of Integer))
            Dim nPages As Integer = outGoingLinks.Length

            ' incomingLinks[i] will contain the indices jj of the pages
            ' linking to page i
            Dim incomingLinks As New List(Of List(Of Integer))(nPages)

            For i As Integer = 0 To nPages - 1
                incomingLinks.Add(New List(Of Integer)())
            Next

            ' the number of links in each page
            Dim numLinks As Vector = New Vector(nPages)
            ' the indices of the leaf nodes
            Dim leafNodes As New List(Of Integer)

            For i As Integer = 0 To nPages - 1
                Dim values As List(Of Integer) = outGoingLinks(i)

                If values.Count = 0 Then
                    leafNodes.Add(i)
                Else
                    numLinks(i) = values.Count
                    ' transpose the link matrix
                    For Each j As Integer In values
                        Dim list As List(Of Integer) = incomingLinks(j)
                        list.Add(i)
                        incomingLinks(j) = list
                    Next
                End If
            Next

            Return (incomingLinks.ToArray, numLinks, leafNodes)
        End Function

        ''' <summary>
        ''' Computes an approximate page rank vector of N pages to within some convergence factor.
        ''' </summary>
        ''' <param name="at">At a sparse square matrix with N rows. At[i] contains the indices of pages jj linking to i</param>
        ''' <param name="leafNodes">contains the indices of pages without links</param>
        ''' <param name="numLinks">iNumLinks[i] is the number of links going out from i.</param>
        ''' <param name="alpha">a value between 0 and 1. Determines the relative importance of "stochastic" links.</param>
        ''' <param name="convergence">a relative convergence criterion. Smaller means better, but more expensive.</param>
        ''' <param name="checkSteps">check for convergence after so many steps</param>
        Protected Iterator Function PageRankGenerator(at As List(Of Integer)(), numLinks As Vector, leafNodes As List(Of Integer), alpha#, convergence#, checkSteps%) As IEnumerable(Of Vector)
            Dim N As Integer = at.Length
            Dim M As Integer = leafNodes.Count

            Dim iNew As Vector = Vector.Ones(N) / N
            Dim iOld As Vector = Vector.Ones(N) / N
            Dim diff As Vector
            Dim done As Boolean = False
            Dim diffSumMod As Double

            While Not done
                ' normalize every now and then for numerical stability
                iNew /= iNew.Sum
                diff = PageRankLoop1(iNew, iOld, checkSteps, alpha, M, N, leafNodes, at, numLinks)
                diffSumMod = diff.SumMagnitude
                done = diffSumMod < convergence

                Call Console.WriteLine($"{diffSumMod} < {convergence}")

                Yield iNew
            End While
        End Function

        Private Function PageRankLoop1(ByRef iNew As Vector,
                                       ByRef iOld As Vector,
                                       ByRef checkSteps%,
                                       alpha#,
                                       ByRef M%,
                                       ByRef N%,
                                       ByRef leafNodes As List(Of Integer),
                                       at As List(Of Integer)(),
                                       numLinks As Vector) As Vector

            Dim tempVec As Vector

            For i As Integer = 0 To checkSteps - 1
                ' swap arrays
                tempVec = iNew
                iNew = iOld
                iOld = tempVec

                ' an element in the 1 x I vector. 
                ' all elements are identical.
                Dim oneIv As Double = (1 - alpha) * iOld.Sum / N

                ' an element of the A x I vector.
                ' all elements are identical.
                Dim oneAv As Double = 0.0

                If M > 0 Then
                    oneAv = alpha * iOld.Take(leafNodes).Sum / N
                End If

                ' the elements of the H x I multiplication
                Dim eval = N.Sequence _
                    .AsParallel _
                    .Select(Function(j)
                                Dim page As List(Of Integer) = at(j)
                                Dim h As Double = 0

                                If page.Count > 0 Then
                                    ' .DotProduct
                                    h = alpha * tempVec.Take(page).DotProduct(1.0 / numLinks.Take(page))
                                End If

                                Return (h + oneAv + oneIv, j)
                            End Function) _
                    .OrderBy(Function(d) d.j) _
                    .Select(Function(d) d.Item1) _
                    .ToArray

                For j As Integer = 0 To N - 1
                    iNew(j) = eval(j)
                Next
            Next

            Return iNew - iOld
        End Function
#End Region
    End Class
End Namespace
