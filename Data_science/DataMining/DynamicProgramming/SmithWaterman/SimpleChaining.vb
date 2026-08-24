#Region "Microsoft.VisualBasic::fd86bca2eaf339f53226d41078f6810d, Data_science\DataMining\DynamicProgramming\SmithWaterman\SimpleChaining.vb"

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

    '   Total Lines: 251
    '    Code Lines: 157 (62.55%)
    ' Comment Lines: 62 (24.70%)
    '    - Xml Docs: 54.84%
    ' 
    '   Blank Lines: 32 (12.75%)
    '     File Size: 10.42 KB


    '     Module SimpleChaining
    ' 
    '         Properties: FromAComparator
    ' 
    '         Function: Chaining, ChainingImpl, populateChains, sort, topScoreMatch
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
        ''' 链化算法所允许的最大 match 数量。
        ''' </summary>
        ''' <remarks>
        ''' <see cref="ChainingImpl"/> 需要分配 adjMatrix 与 sMatrix 两个长度约为
        ''' size*(size-1)/2 的 <see cref="Double"/> 数组,内存开销是 O(n^2):
        ''' 
        ''' * size = 4096  -> 2 * 64MB   = 128 MB
        ''' * size = 8192  -> 2 * 256MB  = 512 MB
        ''' * size = 46340 -> 2 * 8.2GB  = 16 GB(此前仅在此处才会触发溢出保护)
        ''' 
        ''' 这两个数组远大于 85KB,会直接进入大对象堆(LOH)。LOH 默认不做压缩,
        ''' 因此在两两比对的 O(n^2) 外层循环中反复分配/丢弃会造成进程常驻内存
        ''' 持续攀升且不归还,外部观察即为“内存泄漏”。
        ''' 
        ''' 取 4096 作为上限:低于该规模时单次链化的临时内存不超过约 128MB;
        ''' 超过该规模时链化对最终“最佳比对”的贡献极小(候选片段已高度碎片化),
        ''' 直接返回得分最高的单条 match 即可,收益/开销比更合理。
        ''' </remarks>
        Const maxChainingSize As Integer = 4096

        ''' <summary>
        ''' 在候选 match 中直接线性挑选得分最高的一条,作为退化的“最佳链”。
        ''' </summary>
        Private Function topScoreMatch(matches As Match()) As Match
            Dim top As Match = matches(Scan0)

            For i As Integer = 1 To matches.Length - 1
                If matches(i).score > top.score Then
                    top = matches(i)
                End If
            Next

            Return top
        End Function

        ''' <summary>
        ''' Identify the best chain from given list of match
        ''' </summary>
        ''' <param name="matches"> a list of match </param>
        ''' <param name="debug">  if true, print list of input match, adjacency, score matrix, best chain found. </param>
        ''' <returns> the optimal chain as a list of match </returns>
        <Extension>
        Private Function ChainingImpl(matches As Match(), debug As Boolean) As IEnumerable(Of Match)
            Dim size As Integer = matches.Length
            ' Hold adjacency matrix as a double [] the (i,j)= i*(i-1)/2+j
            'with sink
            '
            ' 规模保护:链化需要 O(n^2) 的 adjMatrix/sMatrix(见 maxChainingSize 说明)。
            ' 当候选 match 数量过大时,这两个数组会进入 LOH 并造成内存持续膨胀,
            ' 因此退化为“直接取最高分 match”,既避免巨量分配也不影响最佳比对的选取。
            ' 该阈值同时覆盖了 size>46340 时 i*(i-1) 在 32 位 Integer 下溢出的问题。
            If size > maxChainingSize Then
                Return {topScoreMatch(matches)}
            End If

            Dim sizeL As Long = size
            Dim dims As Integer = CInt(sizeL * (sizeL - 1) \ 2 + sizeL - 2)
            Dim adjMatrix As Double() = New Double(dims) {}
            ' Hold score matrix as a double [] the (i,j)= i*(i-1)/2+j
            Dim sMatrix As Double() = New Double(dims) {}
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
            Erase adjMatrix
            Erase sMatrix
            Erase sMax

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
            ' size*(size-1) 在 32 位 Integer 下于 size>46340 时溢出,此处同样用 Long 计算索引
            If CLng(size) * (size - 1) > Integer.MaxValue Then
                Console.WriteLine("[printLowerMatrix] size 过大,跳过矩阵打印。")
                Return
            End If
            Console.Write(vbTab)
            For i As Integer = 0 To size - 1
                Console.Write(i & vbTab)
            Next
            Console.WriteLine()
            For i As Integer = 0 To size - 1
                Console.Write(i & vbTab)
                For j As Integer = 0 To i - 1
                    Dim i_j As Integer = CInt(CLng(i) * (i - 1) \ 2 + j)
                    Console.Write(CSng(m(i_j)) & vbTab)
                Next
                Console.WriteLine()
            Next
        End Sub
    End Module
End Namespace
