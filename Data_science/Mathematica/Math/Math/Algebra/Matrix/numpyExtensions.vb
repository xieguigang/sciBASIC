
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

<HideModuleName> Public Module NumpyExtensions

    ''' <summary>
    ''' Returns the average of the array elements. The average is taken over the 
    ''' flattened array by default, otherwise over the specified axis. float64 
    ''' intermediate and return values are used for integer inputs.
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <param name="axis%"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Mean(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
        Return matrix.Apply(Function(x) x.Average, axis:=axis, aggregate:=AddressOf AsVector)
    End Function

    <Extension>
    Public Function Apply(Of T, Tout)(matrix As IEnumerable(Of Vector),
                                      math As Func(Of IEnumerable(Of Double), T),
                                      axis%,
                                      aggregate As Func(Of IEnumerable(Of T), Tout)) As Tout

        ' >>> a = np.array([[1, 2], [3, 4]])
        ' >>> np.mean(a)
        ' 2.5
        ' >>> np.mean(a, axis=0)
        ' Array([ 2., 3.])
        ' >>> np.mean(a, axis=1)
        ' Array([ 1.5, 3.5])
        If axis < 0 Then
            Return aggregate({math(matrix.IteratesALL)})
        ElseIf axis = 0 Then
            Return Iterator Function() As IEnumerable(Of T)
                       Dim data As Vector() = matrix.ToArray
                       Dim columns As Integer = data(Scan0).Length
#Disable Warning
                       For i As Integer = 0 To columns - 1
                           Yield math(data.Select(Function(row) row(i)))
                       Next
#Enable Warning
                   End Function().doCall(aggregate)
        ElseIf axis = 1 Then
            Return matrix _
                .Select(Function(r) math(r)) _
                .doCall(aggregate)
        Else
            Throw New NotImplementedException
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function doCall(Of T, Tout)(input As T, aggregate As Func(Of T, Tout)) As Tout
        Return aggregate(input)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Std(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
        Return matrix.Apply(Function(x) x.StdError, axis:=axis, aggregate:=AddressOf AsVector)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Sort(matrix As IEnumerable(Of Vector), Optional axis% = -1) As IEnumerable(Of Vector)
        Return matrix.Apply(Function(x) x.Sort, axis:=axis, aggregate:=Function(collect) collect.Select(AddressOf AsVector))
    End Function
End Module
