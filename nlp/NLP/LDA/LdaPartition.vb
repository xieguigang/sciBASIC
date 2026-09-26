Namespace LDA

    ''' <summary>
    ''' A continuous block of documents that is sampled by one single worker thread.
    ''' </summary>
    Friend Structure DocBlock

        ''' <summary>
        ''' the first document index of this block (inclusive)
        ''' </summary>
        Friend docStart As Integer

        ''' <summary>
        ''' the last document index of this block (inclusive)
        ''' </summary>
        ''' <remarks>
        ''' ``docEnd`` will be smaller than <see cref="docStart"/> for an empty 
        ''' padding block, so that a ``For`` loop over the block does nothing.
        ''' </remarks>
        Friend docEnd As Integer

        ''' <summary>
        ''' the total number of the word tokens inside this block, which is used 
        ''' as the workload estimate of the block.
        ''' </summary>
        Friend tokens As Integer

        ''' <summary>
        ''' the number of the documents inside this block
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Count As Integer
            Get
                Return docEnd - docStart + 1
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{docStart}..{docEnd}] tokens={tokens}"
        End Function
    End Structure

    ''' <summary>
    ''' The document partitioner of the parallel gibbs sampling.
    ''' </summary>
    ''' <remarks>
    ''' The partition is created only once before the first sampling iteration 
    ''' and then it is reused by all of the following iterations. Creating the 
    ''' parallel task inside the iteration loop is the major reason of the poor 
    ''' performance of the previous implementation: with M documents and 
    ''' <see cref="LdaGibbsSampler.ITERATIONS"/> iterations, the old code was 
    ''' starting ``M * ITERATIONS`` parallel schedules, whilst the workload of 
    ''' one single document is way too small to pay for that overhead.
    ''' 
    ''' A document is never split across two blocks. This keeps the per document 
    ''' counters ``nd`` and ``ndsum`` under the exclusive ownership of one 
    ''' worker thread, so that they do not need any synchronization at all.
    ''' </remarks>
    Friend Module LdaPartition

        ''' <summary>
        ''' Split the corpus into <paramref name="blocks"/> continuous blocks of 
        ''' documents with a balanced amount of word tokens.
        ''' </summary>
        ''' <param name="docLen">the token count of each document.</param>
        ''' <param name="blocks">the requested number of the blocks, in general 
        ''' this value is equals to the number of the worker threads.</param>
        ''' <returns>
        ''' an array of <paramref name="blocks"/> elements of which the blocks 
        ''' are disjoint and their union covers all of the documents.
        ''' </returns>
        ''' <remarks>
        ''' A greedy bin packing over the accumulated token count is used here 
        ''' instead of a simple ``M / blocks`` split: the length of the documents 
        ''' of a real world corpus is heavily skewed, so that an equal split of 
        ''' the document index produces stragglers.
        ''' </remarks>
        Friend Function Build(docLen As Integer(), blocks As Integer) As DocBlock()
            Dim m As Integer = If(docLen Is Nothing, 0, docLen.Length)

            If m <= 0 Then
                Return New DocBlock() {}
            End If
            If blocks < 1 Then
                blocks = 1
            End If
            If blocks > m Then
                ' never split one document across two worker threads
                blocks = m
            End If

            Dim total As Long = 0

            For i As Integer = 0 To m - 1
                total += docLen(i)
            Next

            Dim target As Double = total / blocks
            Dim result As DocBlock() = New DocBlock(blocks - 1) {}
            Dim made As Integer = 0
            Dim start As Integer = 0
            Dim acc As Integer = 0

            For i As Integer = 0 To m - 1
                acc += docLen(i)

                Dim isLast As Boolean = (i = m - 1)
                Dim remaining As Integer = blocks - made - 1

                If isLast Then
                    ' flush everything that is left into the last block
                    result(made) = New DocBlock With {
                        .docStart = start,
                        .docEnd = i,
                        .tokens = acc
                    }
                    made += 1
                ElseIf remaining > 0 AndAlso acc >= target Then
                    result(made) = New DocBlock With {
                        .docStart = start,
                        .docEnd = i,
                        .tokens = acc
                    }
                    made += 1
                    start = i + 1
                    acc = 0
                End If
            Next

            ' the greedy packing may produce less blocks than requested when the 
            ' tail of the corpus is empty, fill the rest with empty blocks so 
            ' that the caller can index the result by the thread id directly.
            Do While made < blocks
                result(made) = New DocBlock With {
                    .docStart = m,
                    .docEnd = m - 1,
                    .tokens = 0
                }
                made += 1
            Loop

            Return result
        End Function
    End Module
End Namespace
