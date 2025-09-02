Imports Microsoft.VisualBasic.Serialization.JSON
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public Class Tensor

    ReadOnly nrValues As Integer
    ''' <summary>
    ''' dimension size
    ''' </summary>
    ReadOnly sizes As Integer()
    ReadOnly values As Rev()

    Public ReadOnly Property Dimension As Integer
        Get
            Return sizes.Length
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{sizes.GetJson}"
    End Function

    Public Shared Operator *(T1 As Tensor, T2 As Tensor) As Tensor
        Return MatMulElementWise(T1, T2)
    End Operator

    Public Shared Operator *(T As Tensor, d As Double) As Tensor
        Return T.Scale(d)
    End Operator

    Public Shared Operator *(d As Double, T As Tensor) As Tensor
        Return T * d
    End Operator

    Public Shared Operator /(T1 As Tensor, T2 As Tensor) As Tensor
        Return MatDivElementWise(T1, T2)
    End Operator

    Public Shared Operator /(T As Tensor, d As Double) As Tensor
        Return T.Scale(1.0 / d)
    End Operator

    Public Shared Operator +(T1 As Tensor, T2 As Tensor) As Tensor
        Return MatAdd(T1, T2)
    End Operator

    Public Shared Operator +(T As Tensor, d As Double) As Tensor
        Return T.Add(d)
    End Operator

    Public Shared Operator +(d As Double, T As Tensor) As Tensor
        Return T + d
    End Operator

    Public Shared Operator -(T1 As Tensor, T2 As Tensor) As Tensor
        Return MatAdd(T1, T2 * -1)
    End Operator

    Public Shared Operator -(T As Tensor, d As Double) As Tensor
        Return T.Add(-d)
    End Operator

    Public Shared Operator -(d As Double, T As Tensor) As Tensor
        Return d + -1 * T
    End Operator

    ''' <summary>
    ''' Index parameter implementation
    ''' </summary>
    ''' <param name="keys"></param>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Default Public Property Item(keys As Integer()) As Rev
        Get
            Return getItem(keys)
        End Get
        Set(value As Rev)
            Call setItem(value, keys)
        End Set
    End Property

    ''' <summary>
    ''' Index parameter implementation
    ''' </summary>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Default Public Property Item(key As Integer) As Rev
        Get
            Return getItem(key)
        End Get
        Set(value As Rev)
            Call setItem(value, key)
        End Set
    End Property

    ''' <summary>
    ''' Index parameter implementation
    ''' </summary>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Default Public Property Item(a As Integer, b As Integer) As Rev
        Get
            Return getItem(a, b)
        End Get
        Set(value As Rev)
            Call setItem(value, a, b)
        End Set
    End Property

    ''' <summary>
    ''' Index parameter implementation
    ''' </summary>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Default Public Property Item(a As Integer, b As Integer, c As Integer) As Rev
        Get
            Return getItem(a, b, c)
        End Get
        Set(value As Rev)
            Call setItem(value, a, b, c)
        End Set
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="sizes"></param>
    ''' <exception cref="ArgumentNullException"></exception>
    ''' <exception cref="ArgumentException"></exception>
    Public Sub New(ParamArray sizes As Integer())
        If sizes Is Nothing Then Throw New ArgumentNullException(NameOf(sizes))
        If sizes.Length = 0 Then Throw New ArgumentException(NameOf(sizes) & "Must have at least one dimension")

        Me.sizes = New Integer(sizes.Length - 1) {}
        sizes.CopyTo(Me.sizes, 0)

        nrValues = 1
        For [dim] = 0 To Dimension - 1
            If sizes([dim]) < 1 Then Throw New ArgumentException(NameOf(sizes) & "Size of dimension must be > 0")
            nrValues *= sizes([dim])
        Next

        values = New Rev(nrValues - 1) {}
        For c = 0 To nrValues - 1
            values(c) = New Rev(0.0)
        Next
    End Sub

    ''' <summary>
    ''' Copy constructor. Removes the derivatives and actions.
    ''' </summary>
    ''' <param name="T"></param>
    Public Sub New(T As Tensor, Optional copy As Boolean = False)
        sizes = New Integer(T.Dimension - 1) {}
        Array.Copy(T.sizes, sizes, Dimension)

        nrValues = 1
        For [dim] = 0 To Dimension - 1
            nrValues *= sizes([dim])
        Next

        values = New Rev(nrValues - 1) {}
        For c = 0 To nrValues - 1
            If copy Then
                values(c) = T.values(c)
            Else
                values(c) = New Rev(T.values(c).Magnitude)
            End If
        Next
    End Sub

    Private Sub setItem(value As Rev, ParamArray keys As Integer())
        If keys.Length <> Dimension Then Throw New ArgumentException("Wrong number of dimensions")
        For [dim] = 0 To Dimension - 1
            If keys([dim]) < 0 Then Throw New ArgumentException("key must be nonnegative")
            If keys([dim]) >= sizes([dim]) Then Throw New ArgumentException("key is outside the dimension of the tensor")
        Next

        Dim ind = 0
        Dim blocksize = 1
        For [dim] = Dimension - 1 To 0 Step -1
            ind += keys([dim]) * blocksize
            blocksize *= sizes([dim])
        Next

        values(ind) = value
    End Sub

    Private Function getItem(ParamArray keys As Integer()) As Rev
        If keys.Length <> Dimension Then Throw New ArgumentException("Wrong number of dimensions")
        For [dim] = 0 To Dimension - 1
            If keys([dim]) < 0 Then Throw New ArgumentException("key must be nonnegative")
            If keys([dim]) >= sizes([dim]) Then Throw New ArgumentException("key is outside the dimension of the tensor")
        Next

        Dim ind = 0
        Dim blocksize = 1
        For [dim] = Dimension - 1 To 0 Step -1
            ind += keys([dim]) * blocksize
            blocksize *= sizes([dim])
        Next

        Return values(ind)
    End Function

    ''' <summary>
    ''' Transpose the last two dimensions
    ''' </summary>
    ''' <returns></returns>
    Public Function Transpose() As Tensor
        If Dimension < 2 Then Throw New ArgumentException("Tensor must have dimension >= 2")

        Dim T As Tensor = New Tensor(sizes)

        T.sizes(Dimension - 2) = sizes(Dimension - 1)
        T.sizes(Dimension - 1) = sizes(Dimension - 2)

        Dim imax = T.sizes(Dimension - 2)
        Dim jmax = T.sizes(Dimension - 1)

        Dim nrBlocks As Integer = nrValues / (imax * jmax)
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax * jmax
            For i = 0 To imax - 1
                For j = 0 To jmax - 1
                    T.values(offset + jmax * i + j) = values(offset + imax * j + i)
                Next
            Next
        Next

        Return T
    End Function

    ''' <summary>
    ''' Scale all elements with a number
    ''' </summary>
    ''' <param name="s"></param>
    Public Function Scale(s As Double) As Tensor
        Dim T As Tensor = New Tensor(Me, True)

        For c = 0 To nrValues - 1
            T.values(c) = values(c) * s
        Next

        Return T
    End Function

    ''' <summary>
    ''' Add a number to all elements
    ''' </summary>
    ''' <param name="s"></param>
    Public Function Add(s As Double) As Tensor
        Dim T As Tensor = New Tensor(Me, True)

        For c = 0 To nrValues - 1
            T.values(c) = values(c) + s
        Next

        Return T
    End Function

    ''' <summary>
    ''' Element-wise power 
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    Public Function Pow(e As Double) As Tensor
        Dim T As Tensor = New Tensor(Me, True)

        For c = 0 To nrValues - 1
            T.values(c) = values(c).Pow(e)
        Next

        Return T
    End Function

    ''' <summary>
    ''' Generate normal random values with He scaling over the second last dimension of the tensor.
    ''' </summary>
    Public Sub GenerateNormalRandomValues()
        If Dimension < 2 Then Throw New ArgumentException("Tensor must have dimension >= 2")

        Dim imax = sizes(Dimension - 2)
        For c = 0 To nrValues - 1
            values(c) = New Rev(CDbl((randf.GetNextNormalNumber() * std.Sqrt(2.0 / imax))))
        Next
    End Sub

    ''' <summary>
    ''' Softmax over last dimension
    ''' </summary>
    Public Function Softmax() As Tensor
        Dim T As Tensor = New Tensor(Me, True)

        Dim imax = sizes(Dimension - 1)
        Dim nrBlocks As Integer = nrValues / imax
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax
            Dim normalization As Rev = New Rev(0)
            For i = 0 To imax - 1
                normalization += values(offset + i).Exp()
            Next
            For i = 0 To imax - 1
                T.values(offset + i) = values(offset + i).Exp() / normalization
            Next
        Next

        Return T
    End Function

    ''' <summary>
    ''' Mask upper triangular part of the last two dimensions of a tensor
    ''' </summary>
    ''' <exception cref="ArgumentException"></exception>
    Public Sub Mask()
        If Dimension < 2 Then Throw New ArgumentException("Tensor must have dimension >= 2")

        Dim imax = sizes(Dimension - 2)
        Dim jmax = sizes(Dimension - 1)

        Dim nrBlocks As Integer = nrValues / (imax * jmax)
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax * jmax
            For i = 0 To imax - 1
                For j = i + 1 To jmax - 1
                    values(offset + jmax * i + j) = New Rev(Double.NegativeInfinity)
                Next
            Next
        Next

    End Sub

    ''' <summary>
    ''' Zero out elemens of the last dimension of a tensor based on a masking array
    ''' </summary>
    ''' <param name="dropoutMask"></param>
    ''' <exception cref="ArgumentException"></exception>
    Public Function Dropout(dropoutMask As Boolean(), dropoutRate As Double) As Tensor
        Dim imax = sizes(Dimension - 1)
        If imax <> dropoutMask.Length Then Throw New ArgumentException("Wrong length of fropout vector")

        Dim T As Tensor = New Tensor(Me, True)
        T *= 1.0 / (1.0 - dropoutRate)

        Dim nrBlocks As Integer = nrValues / imax
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax
            For i = 0 To imax - 1
                If dropoutMask(i) Then T.values(offset + i) = New Rev(0.0)
            Next
        Next

        Return T

    End Function

    ''' <summary>
    ''' Set all elements
    ''' </summary>
    Public Sub ReLU()
        For c = 0 To nrValues - 1
            If values(c).Magnitude < 0 Then values(c) = New Rev(0.0)
        Next
    End Sub

    ''' <summary>
    ''' Flatten last two dimensions. 
    ''' </summary>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Public Function Flatten() As Tensor
        If Dimension < 2 Then Throw New ArgumentException("Tensor must have dimension >= 2")

        Dim sizes = New Integer(Dimension - 1) {}
        If Dimension > 2 Then Array.Copy(Me.sizes, sizes, Me.sizes.Length - 2)
        Dim imax = Me.sizes(Dimension - 2)
        Dim jmax = Me.sizes(Dimension - 1)
        sizes(Dimension - 2) = 1
        sizes(Dimension - 1) = imax * jmax
        Dim C As Tensor = New Tensor(sizes)

        Dim nrBlocks As Integer = nrValues / (imax * jmax)
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax * jmax
            Dim ind = 0
            For i = 0 To imax - 1
                For j = 0 To jmax - 1
                    C.values(offset + std.Min(Threading.Interlocked.Increment(ind), ind - 1)) = values(offset + i * jmax + j)
                Next
            Next
        Next

        Return C
    End Function

    ''' <summary>
    ''' Only used for final output layer
    ''' </summary>
    ''' <returns></returns>
    ''' <exception cref="InvalidOperationException"></exception>
    Public Function GetMaxIndex() As Integer()
        If Dimension <> 3 Then Throw New InvalidOperationException("Tensor must have dimension = 3")
        If sizes(1) <> 1 Then Throw New InvalidOperationException("Second dimension must have size = 1")

        Dim imax = sizes(0)
        Dim jmax = sizes(2)
        Dim maxind = New Integer(imax - 1) {}

        For i = 0 To imax - 1
            Dim maxval As Double = 0
            For j = 0 To jmax - 1
                If values(i * jmax + j).Magnitude > maxval Then
                    maxind(i) = j
                    maxval = values(i * jmax + j).Magnitude
                End If
            Next
        Next

        Return maxind
    End Function



    ''' <summary>
    ''' Add a vector to the last dimension of the matrix
    ''' </summary>
    ''' <param name="v"></param>
    Public Function VecAdd(v As Tensor) As Tensor
        Dim T As Tensor = New Tensor(Me, True)

        If v.Dimension <> 1 Then Throw New ArgumentException("Vector must have dimension 1")
        If v.sizes(0) <> sizes(Dimension - 1) Then Throw New ArgumentException("Wrong size of vector")

        Dim imax = sizes(Dimension - 1)
        Dim nrBlocks As Integer = nrValues / imax
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax
            For i = 0 To imax - 1
                T.values(offset + i) += v.values(i)
            Next
        Next

        Return T

    End Function

    ''' <summary>
    ''' In place element-wise addition of the elemens of two tensors with identical shape.
    ''' Only used for optimizer.
    ''' </summary>
    ''' <param name="T"></param>
    ''' <exception cref="ArgumentException"></exception>
    Public Sub MatAdd(T As Tensor)
        If Dimension <> T.Dimension Then Throw New ArgumentException("Tensors must have >= 2 dimensions")
        For [dim] = 0 To Dimension - 1
            If sizes([dim]) <> T.sizes([dim]) Then Throw New ArgumentException("Dimensions of tensors must be identical")
        Next

        For c = 0 To nrValues - 1
            values(c) += T.values(c)
        Next

    End Sub

    ''' <summary>
    ''' Remove derivatives and derivative actions
    ''' </summary>
    Public Sub ClearDerivatives()
        For c = 0 To nrValues - 1
            values(c) = New Rev(values(c).Magnitude)
        Next
    End Sub

    ''' <summary>
    ''' Extract derivatives of a tensor and return as a new tensor
    ''' </summary>
    ''' <returns></returns>
    Public Function GetDerivatives() As Tensor
        Dim T As Tensor = New Tensor(sizes)

        For c = 0 To nrValues - 1
            T.values(c) = New Rev(values(c).Derivative)
        Next

        Return T
    End Function

    ''' <summary>
    ''' Perform derivative calculations for all elements in a tensor with input values from 
    ''' the derivatives of another tensor with the same shape
    ''' </summary>
    ''' <param name="T"></param>
    ''' <exception cref="ArgumentException"></exception>
    Public Sub TransferDerivatives(T As Tensor)
        If Dimension <> T.Dimension Then Throw New ArgumentException("Tensors must have the same number of dimensions")
        For [dim] = 0 To Dimension - 1
            If sizes([dim]) <> T.sizes([dim]) Then Throw New ArgumentException("Size of dimensions must be equal")
        Next

        For c = 0 To nrValues - 1
            If T.values(c).Derivative <> 0 Then values(c).CalculateDerivative(T.values(c).Derivative)
        Next

    End Sub

    ''' <summary>
    ''' Element-wise addition of the elemens of two tensors with identical shape
    ''' </summary>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    ''' <exception cref="ArgumentException"></exception>
    Public Shared Function MatAdd(A As Tensor, B As Tensor) As Tensor
        If A.Dimension <> B.Dimension Then Throw New ArgumentException("Tensors must have >= 2 dimensions")
        For [dim] = 0 To A.Dimension - 1
            If A.sizes([dim]) <> B.sizes([dim]) Then Throw New ArgumentException("Dimensions of tensors must be identical")
        Next

        Dim lC As Tensor = New Tensor(A.sizes)
        For c = 0 To lC.nrValues - 1
            lC.values(c) = A.values(c) + B.values(c)
        Next

        Return lC

    End Function

    ''' <summary>
    ''' Element-wise division of the elemens of two tensors with identical shape
    ''' </summary>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function MatDivElementWise(A As Tensor, B As Tensor) As Tensor
        If A.Dimension <> B.Dimension Then Throw New ArgumentException("Tensors must have >= 2 dimensions")
        For [dim] = 0 To A.Dimension - 1
            If A.sizes([dim]) <> B.sizes([dim]) Then Throw New ArgumentException("Dimensions of tensors must be identical")
        Next

        Dim lC As Tensor = New Tensor(A.sizes)
        For c = 0 To lC.nrValues - 1
            lC.values(c) = A.values(c) / B.values(c)
        Next

        Return lC

    End Function

    ''' <summary>
    ''' Element-wise multiplication of the elemens of two tensors with identical shape
    ''' </summary>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function MatMulElementWise(A As Tensor, B As Tensor) As Tensor
        If A.Dimension <> B.Dimension Then Throw New ArgumentException("Tensors must have >= 2 dimensions")
        For [dim] As Integer = 0 To A.Dimension - 1
            If A.sizes([dim]) <> B.sizes([dim]) Then Throw New ArgumentException("Dimensions of tensors must be identical")
        Next

        Dim lC As New Tensor(A.sizes)
        For c = 0 To lC.nrValues - 1
            lC.values(c) = A.values(c) * B.values(c)
        Next

        Return lC

    End Function

    ''' <summary>
    ''' Matrix multiplication over the last two dimensions of two tensors
    ''' </summary>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function MatMul(A As Tensor, B As Tensor) As Tensor
        If A.Dimension < 2 OrElse B.Dimension < 2 Then Throw New ArgumentException("Tensors must have >= 2 dimensions")
        If A.sizes(A.Dimension - 1) <> B.sizes(B.Dimension - 2) Then Throw New ArgumentException("Wrong dimensions for matrix multiplication")

        Dim imax = A.sizes(A.Dimension - 2)
        Dim jmax = B.sizes(B.Dimension - 1)
        Dim kmax = A.sizes(A.Dimension - 1)
        Dim blockSizeA = imax * kmax
        Dim blockSizeB = jmax * kmax
        Dim blockSizeC = jmax * imax

        If B.Dimension > 2 AndAlso A.nrValues / blockSizeA <> B.nrValues / blockSizeB Then Throw New ArgumentException("Wrong dimensions for matrix multiplication")

        Dim sizes = New Integer(A.Dimension - 1) {}
        A.sizes.CopyTo(sizes, 0)
        sizes(A.Dimension - 2) = imax
        sizes(A.Dimension - 1) = jmax
        Dim C As New Tensor(sizes)

        If B.Dimension = 2 Then blockSizeB = 0

        Dim nrblocks As Integer = C.nrValues / (imax * jmax)
        For block = 0 To nrblocks - 1
            Dim offsetA = block * blockSizeA
            Dim offsetB = block * blockSizeB
            Dim offsetC = block * blockSizeC

            For i = 0 To imax - 1
                For j = 0 To jmax - 1
                    For k = 0 To kmax - 1
                        C.values(offsetC + i * jmax + j) += A.values(offsetA + i * kmax + k) * B.values(offsetB + k * jmax + j)
                    Next
                Next
            Next
        Next

        C = Checkpoints.Instance.AddCheckpoint(C)

        Return C

    End Function

    ''' <summary>
    ''' Concatenate an array of tensors of identical shape along its last dimension
    ''' </summary>
    ''' <param name="Tensors"></param>
    ''' <returns></returns>
    Public Shared Function Concat(Tensors As Tensor()) As Tensor
        For t = 1 To Tensors.Length - 1
            If Tensors(t).Dimension <> Tensors(0).Dimension Then Throw New ArgumentException("All tensors in array must have the same dimension")
            For [dim] = 0 To Tensors(0).Dimension - 1
                If Tensors(t).sizes([dim]) <> Tensors(0).sizes([dim]) Then Throw New ArgumentException("All tensors dimensions in array must be equal")
            Next
        Next

        Dim sizes = New Integer(Tensors(0).Dimension - 1) {}
        Tensors(0).sizes.CopyTo(sizes, 0)
        Dim lastdim = sizes(sizes.Length - 1)
        sizes(sizes.Length - 1) *= Tensors.Length
        Dim C As Tensor = New Tensor(sizes)

        Dim nrBlocks As Integer = Tensors(0).nrValues / lastdim
        For block = 0 To nrBlocks - 1
            Dim offsetT = block * lastdim
            Dim offsetC = block * lastdim * Tensors.Length
            For t = 1 To Tensors.Length - 1
                For i = 0 To lastdim - 1
                    C.values(offsetC + t * lastdim + i) = Tensors(t).values(offsetT + i)
                Next
            Next
        Next

        Return C

    End Function

    ''' <summary>
    ''' Add two tensors and normalize over last dimension
    ''' </summary>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function AddNorm(A As Tensor, B As Tensor) As Tensor
        If A.Dimension <> B.Dimension Then Throw New ArgumentException("Tensors must have the same number of dimensions")

        Dim eps = 0.001

        Dim C As New Tensor(A)

        Dim imax = C.sizes(C.Dimension - 1)
        Dim nrBlocks As Integer = C.nrValues / imax
        For block = 0 To nrBlocks - 1
            Dim offset = block * imax
            Dim mean As Rev = Rev.Zero
            For i = 0 To imax - 1
                mean += (A.values(offset + i) + B.values(offset + i)) / imax
            Next

            Dim var As Rev = Rev.Zero
            For i = 0 To imax - 1
                var += (A.values(offset + i) + B.values(offset + i) - mean).Pow(2) / imax
            Next

            For i = 0 To imax - 1
                C.values(offset + i) = (A.values(offset + i) + B.values(offset + i) - mean) / (var + eps).Pow(0.5)
            Next
        Next

        C = Checkpoints.Instance.AddCheckpoint(C)

        Return C
    End Function

End Class
