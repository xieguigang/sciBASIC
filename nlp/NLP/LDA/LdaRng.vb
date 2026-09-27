Namespace LDA

    ''' <summary>
    ''' A light weight xorshift64 pseudo random number generator.
    ''' </summary>
    ''' <remarks>
    ''' The sampler requires one random stream per worker thread. Using the 
    ''' shared <see cref="RandomExtensions"/> generator here is a bad choice 
    ''' for two reasons:
    ''' 
    ''' 1. the global generator is created from 
    '''    ``New Random(Now.Millisecond * Now.Second + 1)``, so that all of the 
    '''    worker threads, which are created almost at the same time, gets a 
    '''    highly correlated random seed.
    ''' 2. the <see cref="ThreadLocal(Of T)"/> lookup adds overheads on the hot 
    '''    path of the sampling loop, and the result is not reproducible.
    ''' 
    ''' This generator only depends on shift and xor operations, so that it 
    ''' never overflows: VB is doing integer overflow checking unless the 
    ''' ``/removeintchecks`` flag is set, which makes the usual 
    ''' ``xorshift64*``/``splitmix64`` multiply based mixers unsafe here.
    ''' Only the high 53 bits of the state are consumed, which is the part of 
    ''' the xorshift64 state with the best statistical quality.
    ''' </remarks>
    Friend NotInheritable Class LdaRng

        ''' <summary>
        ''' the internal state, must never be zero
        ''' </summary>
        Private state As ULong

        Friend Sub New(seed As ULong)
            state = If(seed = 0UL, 88172645463325252UL, seed)
        End Sub

        ''' <summary>
        ''' Create one generator for each worker thread.
        ''' </summary>
        ''' <param name="baseSeed">a user supplied seed value, which makes the 
        ''' sampling result reproducible.</param>
        ''' <param name="workers">the number of the worker threads.</param>
        ''' <returns>
        ''' an array of ``workers`` elements; the random stream of each element 
        ''' is de-correlated from the others.
        ''' </returns>
        Friend Shared Function CreateWorkers(baseSeed As Integer, workers As Integer) As LdaRng()
            If workers < 1 Then
                workers = 1
            End If

            Dim master As New Random(baseSeed)
            Dim buf As Byte() = New Byte(workers * 8 - 1) {}
            Dim rng As LdaRng() = New LdaRng(workers - 1) {}

            ' draw the initial state of each worker from one single master 
            ' generator, so that the seed value stays reproducible while the 
            ' streams of the worker threads are independent from each other.
            Call master.NextBytes(buf)

            For i As Integer = 0 To workers - 1
                rng(i) = New LdaRng(BitConverter.ToUInt64(buf, i * 8))
            Next

            Return rng
        End Function

        ''' <summary>
        ''' Draws the next uniform random value from the range [0, 1).
        ''' </summary>
        ''' <returns></returns>
        Friend Function NextDouble() As Double
            Dim x As ULong = state

            x = x Xor (x >> 12)
            x = x Xor (x << 25)
            x = x Xor (x >> 27)

            state = x

            ' take the high 53 bits, they are always inside the range of Int64,
            ' so that this conversion can not overflow.
            Return CLng(x >> 11) * (1.0R / 9007199254740992.0R)
        End Function

        ''' <summary>
        ''' Draws the next uniform random integer from the range [0, upper).
        ''' </summary>
        ''' <param name="upper">the exclusive upper bound.</param>
        ''' <returns></returns>
        Friend Function NextInteger(upper As Integer) As Integer
            If upper <= 1 Then
                Return 0
            End If

            ' CInt is doing banker's rounding, so the floor is required here.
            ' note that ``Math`` resolves to the Microsoft.VisualBasic.Math 
            ' namespace in this project, so System.Math has to be qualified.
            Return CInt(System.Math.Floor(NextDouble() * upper))
        End Function
    End Class
End Namespace
