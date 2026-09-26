Imports System.Threading

Namespace LDA

    ''' <summary>
    ''' The shared state of the gibbs sampling kernel.
    ''' </summary>
    ''' <remarks>
    ''' All of the worker threads share the same count vectors, but each of them 
    ''' owns one instance of this state object, so that the scratch buffer of the 
    ''' topic probability is thread local and is reused by every token.
    ''' 
    ''' The ownership of the count vectors is:
    ''' 
    ''' + ``nd`` (M * K) is owned by the worker thread that owns the document, 
    '''   a document is never split across two blocks, so that no synchronization 
    '''   is required here.
    ''' + ``ndsum`` does not exist at all: the very same topic is removed and 
    '''   then added back for one token, so the token count of a document is a 
    '''   constant during the whole sampling.
    ''' + ``nw`` (V * K) and ``nwsum`` are global, they are updated through 
    '''   <see cref="Interlocked"/> so that no count is ever lost.
    ''' </remarks>
    Friend NotInheritable Class SamplingState

        ''' <summary>
        ''' the padding width of the <see cref="nwsumP"/> vector, given in the 
        ''' number of the Int32 elements: 16 * 4 bytes = 64 bytes = one cache 
        ''' line on x86 and x64.
        ''' </summary>
        ''' <remarks>
        ''' ``nwsum`` only has K elements, so that without the padding all of 
        ''' the topics are living inside three or four cache lines, which makes 
        ''' the vector the most contended false sharing hot spot of the whole 
        ''' sampler.
        ''' </remarks>
        Friend Const CACHE_LINE_INTS As Integer = 16

        Friend ReadOnly K As Integer
        Friend ReadOnly alpha As Double
        Friend ReadOnly beta As Double
        Friend ReadOnly Vbeta As Double

        ''' <summary>
        ''' number of instances of the word w assigned to the topic k, indexed 
        ''' by ``w * K + k``. Shared by all of the worker threads.
        ''' </summary>
        Friend ReadOnly nw As Integer()

        ''' <summary>
        ''' total number of the words assigned to the topic k, indexed by 
        ''' ``k * CACHE_LINE_INTS``. Shared by all of the worker threads.
        ''' </summary>
        Friend ReadOnly nwsumP As Integer()

        ''' <summary>
        ''' number of the words of the document m assigned to the topic k, 
        ''' indexed by ``m * K + k``. Exclusive to the owner worker thread.
        ''' </summary>
        Friend ReadOnly nd As Integer()

        ''' <summary>
        ''' all of the documents of the corpus, concatenated into one single vector
        ''' </summary>
        Friend ReadOnly docs As Integer()

        ''' <summary>
        ''' the offset of the document m inside <see cref="docs"/>, the vector 
        ''' has M + 1 elements so that ``docOff(m + 1) - docOff(m)`` is the 
        ''' token count of the document m.
        ''' </summary>
        Friend ReadOnly docOff As Integer()

        ''' <summary>
        ''' the token count of the document m
        ''' </summary>
        Friend ReadOnly docLen As Integer()

        ''' <summary>
        ''' the topic assignment of every token, aligned with <see cref="docs"/>
        ''' </summary>
        Friend ReadOnly z As Integer()

        ''' <summary>
        ''' a scratch buffer of the topic probability, one buffer per worker thread
        ''' </summary>
        Friend ReadOnly pBuf As Double()

        Friend Sub New(K As Integer, alpha As Double, beta As Double, V As Integer,
                       nw As Integer(), nwsumP As Integer(), nd As Integer(),
                       docs As Integer(), docOff As Integer(), docLen As Integer(),
                       z As Integer())

            Me.K = K
            Me.alpha = alpha
            Me.beta = beta
            Me.Vbeta = V * beta
            Me.nw = nw
            Me.nwsumP = nwsumP
            Me.nd = nd
            Me.docs = docs
            Me.docOff = docOff
            Me.docLen = docLen
            Me.z = z
            Me.pBuf = If(K > 0, New Double(K - 1) {}, New Double() {})
        End Sub

        ''' <summary>
        ''' read the padded global topic count of the topic k
        ''' </summary>
        Friend Function NwSum(k As Integer) As Integer
            Return nwsumP(k * CACHE_LINE_INTS)
        End Function
    End Class

    ''' <summary>
    ''' The gibbs sampling kernel that runs on the flattened (CSR) corpus layout.
    ''' </summary>
    ''' <remarks>
    ''' Replaces the old <c>GibbsSamplingTask</c>, which was creating a parallel 
    ''' schedule for each single document and was copying the whole ``ndsum`` 
    ''' vector (the length of that vector is the document count of the corpus!) 
    ''' for every single token.
    ''' </remarks>
    Friend Module LdaSamplingKernel

        ''' <summary>
        ''' Run one gibbs sampling sweep over all of the documents of the block.
        ''' </summary>
        ''' <param name="block">the continuous block of documents that is owned 
        ''' by the current worker thread.</param>
        ''' <param name="st">the shared sampling state.</param>
        ''' <param name="rng">the thread local random generator.</param>
        Friend Sub SampleBlock(block As DocBlock, st As SamplingState, rng As LdaRng)
            Dim nTopics As Integer = st.K
            Dim alpha As Double = st.alpha
            Dim beta As Double = st.beta
            Dim Vbeta As Double = st.Vbeta
            Dim stride As Integer = SamplingState.CACHE_LINE_INTS

            Dim nw As Integer() = st.nw
            Dim nwsumP As Integer() = st.nwsumP
            Dim nd As Integer() = st.nd
            Dim docs As Integer() = st.docs
            Dim docOff As Integer() = st.docOff
            Dim docLen As Integer() = st.docLen
            Dim z As Integer() = st.z
            Dim p As Double() = st.pBuf
            Dim k As Integer

            If nTopics <= 0 OrElse docs Is Nothing Then
                Return
            End If

            For m As Integer = block.docStart To block.docEnd
                Dim len As Integer = docLen(m)

                If len <= 0 Then
                    ' an empty document has nothing to sample
                    Continue For
                End If

                Dim off As Integer = docOff(m)
                Dim ndBase As Integer = m * nTopics

                For n As Integer = 0 To len - 1
                    Dim w As Integer = docs(off + n)
                    Dim nwBase As Integer = w * nTopics
                    Dim oldTopic As Integer = z(off + n)

                    ' remove z_i from the count variables
                    ' the document counters are exclusive to this thread, the
                    ' global topic word counters are updated atomically
                    nd(ndBase + oldTopic) -= 1
                    Interlocked.Add(nw(nwBase + oldTopic), -1)
                    Interlocked.Add(nwsumP(oldTopic * stride), -1)

                    ' sample from p(z_i | z_-i, w):
                    '
                    '   p(k) = (nw(w,k) + beta) / (nwsum(k) + V * beta)
                    '        * (nd(m,k) + alpha) / (ndsum(m) + K * alpha)
                    '
                    ' the factor 1 / (ndsum(m) + K * alpha) does not depend on 
                    ' the topic, so that it cancels out in the comparison of the 
                    ' cumulative distribution below and is skipped here.
                    Dim total As Double = 0.0R

                    For k = 0 To nTopics - 1
                        Dim q As Double = (nw(nwBase + k) + beta) * (nd(ndBase + k) + alpha) / (nwsumP(k * stride) + Vbeta)

                        p(k) = q
                        total += q
                    Next

                    ' do the multinomial sampling via the cumulative method
                    Dim u As Double = rng.NextDouble() * total
                    Dim acc As Double = 0.0R
                    Dim newTopic As Integer = nTopics - 1

                    For k = 0 To nTopics - 1
                        acc += p(k)

                        If u < acc Then
                            newTopic = k
                            Exit For
                        End If
                    Next

                    z(off + n) = newTopic

                    ' add the newly estimated z_i to the count variables
                    nd(ndBase + newTopic) += 1
                    Interlocked.Add(nw(nwBase + newTopic), 1)
                    Interlocked.Add(nwsumP(newTopic * stride), 1)
                Next
            Next
        End Sub
    End Module
End Namespace
