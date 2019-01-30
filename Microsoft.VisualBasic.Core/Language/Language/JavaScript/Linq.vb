Imports System.Runtime.CompilerServices

Namespace Language.JavaScript

    Public Module Linq

        <Extension>
        Public Sub splice(Of T)(array As T(), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        <Extension>
        Public Sub splice(Of T)(list As List(Of T), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">序列的类型</typeparam>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), init As V = Nothing) As V
            For Each x As T In seq
                init = produce(init, x)
            Next

            Return init
        End Function

        <Extension>
        Public Function Sort(Of T)(seq As IEnumerable(Of T), comparer As Comparison(Of T)) As IEnumerable(Of T)
            With New List(Of T)(seq)
                Call .Sort(comparer)
                Return .AsEnumerable
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sort(Of T)(seq As IEnumerable(Of T), comparer As Func(Of T, T, Double)) As IEnumerable(Of T)
            Return seq.Sort(Function(x, y)
                                Dim d As Double = comparer(x, y)

                                If d > 0 Then
                                    Return 1
                                ElseIf d < 0 Then
                                    Return -1
                                Else
                                    Return 0
                                End If
                            End Function)
        End Function
    End Module
End Namespace