
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
        Return matrix.Apply(Function(x) x.Average, axis:=axis)
    End Function

    <Extension>
    Public Function Apply(matrix As IEnumerable(Of Vector), math As Func(Of IEnumerable(Of Double), Double), axis%) As Vector
        ' >>> a = np.array([[1, 2], [3, 4]])
        ' >>> np.mean(a)
        ' 2.5
        ' >>> np.mean(a, axis=0)
        ' Array([ 2., 3.])
        ' >>> np.mean(a, axis=1)
        ' Array([ 1.5, 3.5])
        If axis < 0 Then
            Return New Double() {math(matrix.IteratesALL)}
        ElseIf axis = 0 Then
            Return Iterator Function() As IEnumerable(Of Double)
                       Dim data As Vector() = matrix.ToArray
                       Dim columns As Integer = data(Scan0).Length
#Disable Warning
                       For i As Integer = 0 To columns - 1
                           Yield math(data.Select(Function(row) row(i)))
                       Next
#Enable Warning
                   End Function().AsVector
        ElseIf axis = 1 Then
            Return matrix _
                .Select(Function(r) math(r)) _
                .AsVector
        Else
            Throw New NotImplementedException
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Std(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
        Return matrix.Apply(Function(x) x.StdError, axis:=axis)
    End Function
End Module
