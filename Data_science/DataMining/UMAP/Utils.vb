Imports System.Linq
Friend Module Utils
    ''' <summary>
    ''' Creates an empty array
    ''' </summary>
    Public Function Empty(ByVal n As Integer) As Single()
        Return New Single(n - 1) {}
    End Function

    ''' <summary>
    ''' Creates an array filled with index values
    ''' </summary>
    Public Function Range(ByVal n As Integer) As Single()
        Return Enumerable.Range(0, n).[Select](Function(i) CSng(i)).ToArray()
    End Function

    ''' <summary>
    ''' Creates an array filled with a specific value
    ''' </summary>
    Public Function Filled(ByVal count As Integer, ByVal value As Single) As Single()
        Return Enumerable.Range(0, count).[Select](Function(i) value).ToArray()
    End Function

    ''' <summary>
    ''' Returns the mean of an array
    ''' </summary>
    Public Function Mean(ByVal input As Single()) As Single
        Return input.Sum() / input.Length
    End Function

    ''' <summary>
    ''' Returns the maximum value of an array
    ''' </summary>
    Public Function Max(ByVal input As Single()) As Single
        Return input.Max()
    End Function

    ''' <summary>
    ''' Generate nSamples many integers from 0 to poolSize such that no integer is selected twice.The duplication constraint is achieved via rejection sampling.
    ''' </summary>
    Public Function RejectionSample(ByVal nSamples As Integer, ByVal poolSize As Integer, ByVal random As Umap.IProvideRandomValues) As Integer()
        Dim result = New Integer(nSamples - 1) {}

        For i = 0 To nSamples - 1
            Dim rejectSample = True

            While rejectSample
                Dim j = random.Next(0, poolSize)
                Dim broken = False

                For k = 0 To i - 1

                    If j = result(k) Then
                        broken = True
                        Exit For
                    End If
                Next

                If Not broken Then rejectSample = False
                result(i) = j
            End While
        Next

        Return result
    End Function
End Module
