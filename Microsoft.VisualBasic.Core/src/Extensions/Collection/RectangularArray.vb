Namespace ComponentModel.Collection

    Public NotInheritable Class RectangularArray

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Create an empty matrix with m row and n cols.
        ''' (生成一个有m行n列的矩阵，但是是使用数组来表示的)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="m">m Rows</param>
        ''' <param name="n">n Cols</param>
        ''' <returns></returns>
        Public Shared Function Matrix(Of T)(m%, n%) As T()()
            Dim x As T()() = New T(m - 1)() {}

            For i As Integer = 0 To m - 1
                x(i) = New T(n - 1) {}
            Next

            Return x
        End Function

        Public Shared Function Cubic(Of T)(size1 As Integer, size2 As Integer, size3 As Integer) As T()()()
            Dim x = New T(size1 - 1)()() {}

            For array1 As Integer = 0 To size1 - 1
                x(array1) = New T(size2 - 1)() {}
                If size3 > -1 Then
                    For array2 As Integer = 0 To size2 - 1
                        x(array1)(array2) = New T(size3 - 1) {}
                    Next
                End If
            Next

            Return x
        End Function

        Public Shared Function Matrix(type As Type, m%, n%) As Array
            Dim newMatrix As Array = Array.CreateInstance(type.MakeArrayType, m)

            For i As Integer = 0 To m - 1
                Call newMatrix.SetValue(Array.CreateInstance(type, n), i)
            Next

            Return newMatrix
        End Function
    End Class
End Namespace