
Imports Microsoft.VisualBasic.Linq

Namespace LinearAlgebra

    Partial Class Vector

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="condition">
        ''' 当conditon的某个位置的为true时，输出x的对应位置的元素，否则选择y对应位置的元素；
        ''' </param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Vector, y As Vector) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x(index)
                           Else
                               Yield y(index)
                           End If
                       Next
                   End Function().AsVector
        End Function

        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Double, y As Double) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x
                           Else
                               Yield y
                           End If
                       Next
                   End Function().AsVector
        End Function

        ''' <summary>
        ''' Perform an indirect sort along the given axis using the algorithm specified
        ''' by the `kind` keyword. It returns an array Of indices Of the same shape As
        ''' `a` that index data along the given axis in sorted order.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns>Returns the indices that would sort an array.</returns>
        ''' <example>
        ''' x = np.array([3, 1, 2])
        ''' np.argsort(x)
        ''' array([1, 2, 0])
        ''' </example>
        Public Shared Function argsort(data As IEnumerable(Of Double)) As Integer()
            Dim sort = From x In data.SeqIterator Select x Order By x.value
            Dim index = sort.Select(Function(x) x.i).ToArray

            Return index
        End Function

        Public Shared Iterator Function column_stack(ParamArray vectors As Vector()) As IEnumerable(Of Double())
            Dim maxL = vectors.Max(Function(vec) vec.Length)

#Disable Warning
            For i As Integer = 0 To maxL - 1
                Yield vectors _
                    .Select(Function(vec) vec.ElementAtOrDefault(i)) _
                    .ToArray
            Next
#Enable Warning
        End Function
    End Class
End Namespace