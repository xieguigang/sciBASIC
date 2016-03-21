Imports Microsoft.VisualBasic.Linq

Public Module Ranks

    Public Class Ranking(Of T)
        Public Property Evaluate As Func(Of T, Double)
        ''' <summary>
        ''' The sort direction
        ''' </summary>
        ''' <returns></returns>
        Public Property Max As Boolean
        ''' <summary>
        ''' 默认不加权重
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight As Double = 1

        Public Function Sort(source As IEnumerable(Of T)) As SeqValue(Of T, Double)()
            Dim Evaluate As Func(Of T, Double) = Me.Evaluate
            Dim LQuery = (From x As T In source Select x, v = Evaluate(x)).ToArray
            Dim result As SeqValue(Of T, Double)()
            Dim weights As Double() = _Weight.CopyVector(LQuery.Length)

            If Max Then   ' 由于后面需要进行加权计算，所以在这里是反过来求最大的
                result = (From x In LQuery Select x Order By x.v Ascending).Select(Function(x) x.x).SeqIterator(weights).ToArray
            Else
                result = (From x In LQuery Select x Order By x.v Descending).Select(Function(x) x.x).SeqIterator(weights).ToArray
            End If

            Return result
        End Function
    End Class

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="Evaluate"></param>
    ''' <returns>Ranks值最小的是认为最好的</returns>
    Public Function Best(Of T)(source As IEnumerable(Of T), Evaluate As IEnumerable(Of Ranking(Of T))) As T
        Dim array As T() = Sort(source, Evaluate).ToArray
        Return array.FirstOrDefault
    End Function

    Public Function Sort(Of T)(source As IEnumerable(Of T), Evaluate As IEnumerable(Of Ranking(Of T))) As IEnumerable(Of T)
        Dim LQuery = (From method As Ranking(Of T) In Evaluate.AsParallel Select method.Sort(source)).MatrixAsIterator
        Dim Groups = (From x In LQuery Select x Group x By x.obj Into Group).ToArray
        Dim Ranks = (From x In Groups.AsParallel
                     Select x.obj,
                         rank = x.Group.Sum(Function(o) o.Pos * o.Follow)  ' 加权重计算
                     Order By rank Descending).ToArray
        Return Ranks.Select(Function(x) x.obj)
    End Function

End Module
