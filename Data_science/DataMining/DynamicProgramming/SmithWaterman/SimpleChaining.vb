#Region "Microsoft.VisualBasic::1386717a718ecb9c6c2da87df742d753, Data_science\DataMining\DynamicProgramming\SmithWaterman\SimpleChaining.vb"

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

    '   Total Lines: 197
    '    Code Lines: 135
    ' Comment Lines: 35
    '   Blank Lines: 27
    '     File Size: 7.69 KB


    '     Module SimpleChaining
    ' 
    '         Properties: FromAComparator
    ' 
    '         Function: Chaining, ChainingImpl, populateChains, sort
    ' 
    '         Sub: printLowerMatrix
    '         Structure ComparatorHelper
    ' 
    '             Function: Compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace SmithWaterman

    Public Module SimpleChaining

        ReadOnly Property FromAComparator As IComparer(Of Match) = New ComparatorHelper()

        Private Structure ComparatorHelper : Implements IComparer(Of Match)

            Public Function Compare(x As Match, y As Match) As Integer Implements IComparer(Of Match).Compare
                Return x.fromA - y.fromA
            End Function
        End Structure

        ''' <summary>
        ''' Identify the best chain from given list of match
        ''' </summary>
        ''' <param name="matches"> a list of match </param>
        ''' <param name="debug">  if true, print list of input match, adjacency, score matrix, best chain found. </param>
        ''' <returns> the optimal chain as a list of match </returns>
        Public Function Chaining(matches As Match(), debug As Boolean) As IEnumerable(Of Match)
            If matches.Length <= 1 Then
                Return matches
            Else
                Return matches.ChainingImpl(debug)
            End If
        End Function

        <Extension>
        Private Function sort(matches As Match()) As Match()
            Dim list = matches.ToList
            list.Sort(FromAComparator)
            Return list.ToArray
        End Function

        ''' <summary>
        ''' Identify the best chain from given list of match
        ''' </summary>
        ''' <param name="matches"> a list of match </param>
        ''' <param name="debug">  if true, print list of input match, adjacency, score matrix, best chain found. </param>
        ''' <returns> the optimal chain as a list of match </returns>
        ''' 
        <Extension>
        Private Function ChainingImpl(matches As Match(), debug As Boolean) As IEnumerable(Of Match)
            Dim size As Integer = matches.Count
            ' Hold adjaency matrix as a double [] the (i,j)= i*(i-1)/2+j
            'with sink
            Dim adjMatrix As Double() = New Double(size * (size - 1) \ 2 + size - 2) {}
            ' Hold score matrix as a double [] the (i,j)= i*(i-1)/2+j
            Dim sMatrix As Double() = New Double(size * (size - 1) \ 2 + size - 2) {}
            'Hold max score of chain end at match i
            Dim sMax As Double() = New Double(size - 1) {}
            ' Hold the previous match index point to match i
            Dim prevIndex As Integer() = New Integer(size - 1) {}

            For i As Integer = 0 To size - 1
                prevIndex(i) = -1
            Next

            'sort the matches based on the occurance in sequence A
            matches = matches.sort

            If debug Then
                Console.WriteLine("The list of Matches {[fromA, toA, fromB, toB, score]...}")

                For Each item In matches
                    Console.WriteLine(item.ToString)
                Next
            End If
            'initialize the adjancey matrix and scre matrx from top left to bottom right
            'for each match i=1..size-1
            ' 	compare to rest match j= 0,...i-1
            For i As Integer = 1 To size - 1
                'if ( i !=size-1)
                Dim mr = matches(i)

                For j As Integer = 0 To i - 1
                    Dim mc As Match = matches(j)
                    Dim i_j As Integer = i * (i - 1) \ 2 + j

                    If mc.isChainable(mr) Then
                        adjMatrix(i_j) = mc.score
                        'update score matrix
                        sMatrix(i_j) = adjMatrix(i_j) + sMax(j)
                        'update sMax if necessary
                        If sMatrix(i_j) > sMax(i) Then
                            sMax(i) = sMatrix(i_j)
                            prevIndex(i) = j
                        End If
                    End If
                Next
            Next

            'now backtrace to construct the chain	  
            'get the max score
            Dim max As Double = 0
            Dim maxIndex As Integer = 0

            For i As Integer = 0 To size - 1
                sMax(i) += DirectCast(matches(i), Match).score
                If sMax(i) > max Then
                    max = sMax(i)
                    maxIndex = i
                End If
            Next

            If debug Then
                Console.WriteLine("The adjacency matrix is:")
                printLowerMatrix(adjMatrix, size)
                Console.Write("sink" & vbTab)
                For i As Integer = 0 To size - 1
                    Console.Write(DirectCast(matches(i), Match).score & vbTab)
                Next
                Console.WriteLine()
                Console.WriteLine("The score matrix is:")
                printLowerMatrix(sMatrix, size)
                Console.Write("sink" & vbTab)
                For i As Integer = 0 To size - 1
                    Console.Write(CSng(sMax(i)) & vbTab)
                Next
                Console.WriteLine()
            End If

            'now the chain end with match at maxIndex
            'the score is max;
            'trace back to the begining of the chain;

            If maxIndex = 0 Then
                Return {
                    matches(Scan0)
                }
            Else
                Return matches.populateChains(prevIndex, maxIndex, max, debug)
            End If
        End Function

        <Extension>
        Private Iterator Function populateChains(matches As Match(), prevIndex As Integer(), maxIndex As Integer, max As Double, debug As Boolean) As IEnumerable(Of Match)
            Dim chainIndex As Integer() = New Integer(maxIndex - 1) {}
            Dim ii As Integer = 1

            For i As Integer = 0 To chainIndex.Length - 1
                chainIndex(i) = -1
            Next

            chainIndex(0) = maxIndex

            While prevIndex(chainIndex(ii - 1)) >= 0
                If chainIndex.Length = ii Then
                    Exit While
                Else
                    chainIndex(ii) = prevIndex(chainIndex(ii - 1))
                End If

                ii += 1
            End While

            'now revese the chain 
            'and put the matches in a list;	 
            For i As Integer = chainIndex.Length - 1 To 0 Step -1
                If chainIndex(i) >= 0 Then
                    Yield matches(chainIndex(i))
                End If
            Next

            If debug Then
                Console.WriteLine("The best chain with score " & max)
                For i As Integer = chainIndex.Length - 1 To 0 Step -1
                    If chainIndex(i) >= 0 Then
                        Console.Write(chainIndex(i) & "---->")
                    End If
                Next
                Console.WriteLine("sink")
            End If
        End Function

        ''' <summary>
        ''' System out the input array as an strict lower diagonal matrix
        ''' </summary>
        Public Sub printLowerMatrix(m As Double(), size As Integer)
            Console.Write(vbTab)
            For i As Integer = 0 To size - 1
                Console.Write(i & vbTab)
            Next
            Console.WriteLine()
            For i As Integer = 0 To size - 1
                Console.Write(i & vbTab)
                For j As Integer = 0 To i - 1
                    Dim i_j As Integer = i * (i - 1) \ 2 + j
                    Console.Write(CSng(m(i_j)) & vbTab)
                Next
                Console.WriteLine()
            Next
        End Sub
    End Module
End Namespace
