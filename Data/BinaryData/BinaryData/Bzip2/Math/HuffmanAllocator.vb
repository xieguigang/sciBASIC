' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports stdNum = System.Math

Namespace Bzip2
    ''' <summary>An in-place, length restricted Canonical Huffman code length allocator</summary>
    ''' <remarks>
	''' Based on the algorithm proposed by R.L.Milidiú, A.A.Pessoa and E.S.Laber 
    ''' in "In-place Length-Restricted Prefix Coding" (see: http://www-di.inf.puc-rio.br/~laber/public/spire98.ps)
	''' and incorporating additional ideas from the implementation of "shcodec" by Simakov Alexander
    ''' (see: http://webcenter.ru/~xander/)
    ''' </remarks>
    Friend Module HuffmanAllocator
#Region "Public methods"
        ''' <summary>Allocates Canonical Huffman code lengths in place based on a sorted frequency array</summary>
        ''' <param name="array">On input, a sorted array of symbol frequencies; On output, an array of Canonical Huffman code lenghts</param>
        ''' <param name="maximumLength">The maximum code length. Must be at least ceil(log2(array.length))</param>
        Public Sub AllocateHuffmanCodeLengths(array As Integer(), maximumLength As Integer)
            Select Case array.Length
                Case 2
                    array(1) = 1
                Case 1
                    array(0) = 1
                    Return
            End Select

            ' Pass 1 : Set extended parent pointers 
            SetExtendedParentPointers(array)

            ' Pass 2 : Find number of nodes to relocate in order to achieve maximum code length 
            Dim nodesToRelocate = FindNodesToRelocate(array, maximumLength)

            ' Pass 3 : Generate code lengths 
            If array(0) Mod array.Length >= nodesToRelocate Then
                AllocateNodeLengths(array)
            Else
                Dim insertDepth = maximumLength - SignificantBits(nodesToRelocate - 1)
                AllocateNodeLengthsWithRelocation(array, nodesToRelocate, insertDepth)
            End If
        End Sub
#End Region

#Region "Private methods"
        ' 
        '  @param array The code length array
        '  @param i The input position
        '  @param nodesToMove The number of internal nodes to be relocated
        '  @return The smallest k such that nodesToMove <= k <= i and i <= (array[k] % array.length)
        ' 
        Private Function First(array As Integer(), i As Integer, nodesToMove As Integer) As Integer
            Dim length = array.Length
            Dim limit = i
            Dim k = array.Length - 2

            While i >= nodesToMove AndAlso array(i) Mod length > limit
                k = i
                i -= limit - i + 1
            End While

            i = stdNum.Max(nodesToMove - 1, i)

            While k > i + 1
                Dim temp = i + k >> 1

                If array(temp) Mod length > limit Then
                    k = temp
                Else
                    i = temp
                End If
            End While

            Return k
        End Function

        ' 
        '  Fills the code array with extended parent pointers
        '  @param array The code length array
        ' 
        Private Sub SetExtendedParentPointers(array As Integer())
            Dim length = array.Length
            array(0) += array(1)
            Dim headNode = 0, tailNode = 1, topNode = 2

            While tailNode < length - 1
                Dim temp As Integer

                If topNode >= length OrElse array(headNode) < array(topNode) Then
                    temp = array(headNode)
                    array(stdNum.Min(Threading.Interlocked.Increment(headNode), headNode - 1)) = tailNode
                Else
                    temp = array(stdNum.Min(Threading.Interlocked.Increment(topNode), topNode - 1))
                End If

                If topNode >= length OrElse headNode < tailNode AndAlso array(headNode) < array(topNode) Then
                    temp += array(headNode)
                    array(stdNum.Min(Threading.Interlocked.Increment(headNode), headNode - 1)) = tailNode + length
                Else
                    temp += array(stdNum.Min(Threading.Interlocked.Increment(topNode), topNode - 1))
                End If

                array(tailNode) = temp
                tailNode += 1
            End While
        End Sub

        ' 
        '  Finds the number of nodes to relocate in order to achieve a given code length limit
        '  @param array The code length array
        '  @param maximumLength The maximum bit length for the generated codes
        '  @return The number of nodes to relocate
        ' 
        Private Function FindNodesToRelocate(array As Integer(), maximumLength As Integer) As Integer
            Dim currentNode = array.Length - 2
            Dim currentDepth = 1

            While currentDepth < maximumLength - 1 AndAlso currentNode > 1
                currentNode = First(array, currentNode - 1, 0)
                currentDepth += 1
            End While

            Return currentNode
        End Function

        ' 
        '  A final allocation pass with no code length limit
        '  @param array The code length array
        ' 
        Private Sub AllocateNodeLengths(array As Integer())
            Dim firstNode = array.Length - 2
            Dim nextNode = array.Length - 1
            Dim currentDepth = 1, availableNodes = 2

            While availableNodes > 0
                Dim lastNode = firstNode
                firstNode = First(array, lastNode - 1, 0)

                For i = availableNodes - (lastNode - firstNode) To 0 + 1 Step -1
                    array(stdNum.Max(Threading.Interlocked.Decrement(nextNode), nextNode + 1)) = currentDepth
                Next

                availableNodes = lastNode - firstNode << 1
                currentDepth += 1
            End While
        End Sub

        ' *
        '  A final allocation pass that relocates nodes in order to achieve a maximum code length limit
        '  @param array The code length array
        '  @param nodesToMove The number of internal nodes to be relocated
        '  @param insertDepth The depth at which to insert relocated nodes
        ' 

        Private Sub AllocateNodeLengthsWithRelocation(array As Integer(), nodesToMove As Integer, insertDepth As Integer)
            Dim firstNode = array.Length - 2
            Dim nextNode = array.Length - 1
            Dim currentDepth = If(insertDepth = 1, 2, 1)
            Dim nodesLeftToMove = If(insertDepth = 1, nodesToMove - 2, nodesToMove)
            Dim availableNodes = currentDepth << 1

            While availableNodes > 0
                Dim lastNode = firstNode
                firstNode = If(firstNode <= nodesToMove, firstNode, First(array, lastNode - 1, nodesToMove))
                Dim offset = 0

                If currentDepth >= insertDepth Then
                    offset = stdNum.Min(nodesLeftToMove, 1 << currentDepth - insertDepth)
                ElseIf currentDepth = insertDepth - 1 Then
                    offset = 1
                    If array(firstNode) = lastNode Then firstNode += 1
                End If

                For i = availableNodes - (lastNode - firstNode + offset) To 0 + 1 Step -1
                    array(stdNum.Max(Threading.Interlocked.Decrement(nextNode), nextNode + 1)) = currentDepth
                Next

                nodesLeftToMove -= offset
                availableNodes = lastNode - firstNode + offset << 1
                currentDepth += 1
            End While
        End Sub

        Private Function SignificantBits(x As Integer) As Integer
            Dim n As Integer
            n = 0

            While x > 0
                x >>= 1
                n += 1
            End While

            Return n
        End Function
#End Region
    End Module
End Namespace
