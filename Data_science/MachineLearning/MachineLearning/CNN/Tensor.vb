Imports System
Imports System.Runtime.InteropServices

Namespace Convolutional
    Public Class Tensor

        Private totalLengthField As Integer
        Private dims As Integer()
        Public memPtr As Single()
        Public consumedMem As Integer
        Private dimProds As Integer()

        Public ReadOnly Property TotalLength As Integer
            Get
                Return totalLengthField
            End Get
        End Property

        Public ReadOnly Property Dimensions As Integer()
            Get
                Return dims
            End Get
        End Property

        Shared ReadOnly sizeofFloat As Integer = Marshal.SizeOf(CSng(1.0))

        Public Sub New(dims As Integer())
            totalLengthField = multAll(dims)
            Me.dims = CType(dims.Clone(), Integer())
            updateDimProds()

            Me.consumedMem = totalLengthField * sizeofFloat

            memPtr = New Single(totalLengthField - 1) {}

            For i = 0 To totalLengthField - 1
                memPtr(i) = 0.0F
            Next
        End Sub

        Private Sub updateDimProds()
            dimProds = New Integer(dims.Length - 1) {}
            dimProds(0) = 1

            For i = 1 To dims.Length - 1
                dimProds(i) = dimProds(i - 1) * dims(i - 1)
            Next
        End Sub

        Default Public Property Item(indexes As Integer) As Single
            Get
                Dim ind = get1DInd(indexes)
                Return memPtr(ind)
            End Get
            Set(value As Single)
                Dim ind = get1DInd(indexes)
                memPtr(ind) = value
            End Set
        End Property

        Default Public Property Item(indexes As Integer()) As Single
            Get
                Dim ind = get1DInd(indexes)
                Return memPtr(ind)
            End Get
            Set(value As Single)
                Dim ind = get1DInd(indexes)
                memPtr(ind) = value
            End Set
        End Property

        Private Function get1DInd(ParamArray indexes As Integer()) As Integer
            Dim ind = indexes(0)

            For i = 1 To indexes.Length - 1
                ind += dimProds(i) * indexes(i)
            Next

            If ind >= totalLengthField Then Throw New IndexOutOfRangeException()
            Return ind
        End Function

        Private Shared Function multAll(array As Integer()) As Integer
            Dim mul = 1

            For i = 0 To array.Length - 1
                mul *= array(i)
            Next

            Return mul
        End Function

        Public Function reshape(newDims As Integer()) As Boolean
            If multAll(dims) <> multAll(newDims) Then Return False
            dims = CType(newDims.Clone(), Integer())
            updateDimProds()
            Return True
        End Function

        Private Shared Function dimsEqual(t1 As Tensor, t2 As Tensor) As Boolean
            If t1.dims.Length <> t2.dims.Length Then Return False

            For i = 0 To t1.dims.Length - 1
                If t1.dims(i) <> t2.dims(i) Then Return False
            Next

            Return True
        End Function

        Public Function clone() As Tensor
            Dim t As Tensor = New Tensor(dims)

            For i = 0 To totalLengthField - 1
                t.memPtr(i) = memPtr(i)
            Next

            Return t
        End Function

        Public Shared Operator *(t1 As Tensor, f As Single) As Tensor
            Dim t As Tensor = New Tensor(t1.dims)

            For i = 0 To t.totalLengthField - 1
                t.memPtr(i) = t1.memPtr(i) * f
            Next

            Return t
        End Operator

        Public Shared Operator +(t1 As Tensor, f As Single) As Tensor
            Dim t As Tensor = New Tensor(t1.dims)

            For i = 0 To t.totalLengthField - 1
                t.memPtr(i) = t1.memPtr(i) + f
            Next

            Return t
        End Operator

        Public Shared Operator -(t1 As Tensor, f As Single) As Tensor
            Return t1 + -f
        End Operator

        Public Shared Operator +(t1 As Tensor, t2 As Tensor) As Tensor
            If t1.dims.Length = 2 AndAlso t2.dims.Length = 2 AndAlso (t1.dims(0) = 1 AndAlso t2.dims(1) = 1 OrElse t1.dims(1) = 1 AndAlso t2.dims(0) = 1) Then Return broadcastedAddition(t1, t2)
            If Not dimsEqual(t1, t2) Then Return Nothing
            Dim t As Tensor = New Tensor(t1.dims)

            For i = 0 To t1.totalLengthField - 1
                t.memPtr(i) = t1.memPtr(i) + t2.memPtr(i)
            Next

            Return t
        End Operator

        Public Shared Operator *(t1 As Tensor, t2 As Tensor) As Tensor
            If t1.dims.Length <> 2 OrElse t2.dims.Length <> 2 OrElse t1.dims(1) <> t2.dims(0) Then
                Return Nothing
            End If

            Dim t As New Tensor(New Integer() {t1.dims(0), t2.dims(1)})
            Dim sum As Single
            Dim ind1 = New Integer() {0, 0}
            Dim ind2 = New Integer() {0, 0}
            Dim ind3 = New Integer() {0, 0}

            For i = 0 To t1.dims(0) - 1
                ind1(0) = i
                ind3(0) = i

                For k = 0 To t2.dims(1) - 1
                    ind2(1) = k
                    ind3(1) = k
                    sum = 0

                    For j = 0 To t1.dims(1) - 1
                        ind1(1) = j
                        ind2(0) = j
                        sum += t1(ind1) * t2(ind2)
                    Next

                    t(ind3) = sum
                Next
            Next

            Return t
        End Operator

        Private Shared Function broadcastedAddition(t1 As Tensor, t2 As Tensor) As Tensor
            Dim dim1 = t1.totalLengthField
            Dim dim2 = t2.totalLengthField
            Dim t As Tensor = New Tensor(New Integer() {dim1, dim2})
            Dim ind = New Integer() {0, 0}
            ind(0) = 0

            While ind(0) < dim1
                ind(1) = 0

                While ind(1) < dim2
                    t(ind) = t1.memPtr(ind(0)) + t2.memPtr(ind(1))
                    ind(1) += 1
                End While

                ind(0) += 1
            End While

            Return t
        End Function

        Public Sub Dispose()
            If memPtr IsNot Nothing Then
                Erase memPtr
                memPtr = Nothing
            End If
        End Sub

        Protected Overrides Sub Finalize()
            Dispose()
        End Sub
    End Class
End Namespace
