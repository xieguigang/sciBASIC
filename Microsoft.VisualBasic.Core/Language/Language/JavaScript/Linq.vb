Imports System.Runtime.CompilerServices

Namespace Language.JavaScript

    Public Module Linq

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
        Public Function Reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), init As V) As V
            For Each x As T In seq
                init = produce(init, x)
            Next

            Return init
        End Function
    End Module
End Namespace