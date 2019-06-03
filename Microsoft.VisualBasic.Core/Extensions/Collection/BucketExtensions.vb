Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

<HideModuleName> Public Module BucketExtensions

    ''' <summary>
    ''' Data partitioning function.
    ''' (将目标集合之中的数据按照<paramref name="parTokens"></paramref>参数分配到子集合之中，
    ''' 这个函数之中不能够使用并行化Linq拓展，以保证元素之间的相互原有的顺序，
    ''' 每一个子集和之中的元素数量为<paramref name="parTokens"/>)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="parTokens">每一个子集合之中的元素的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Split(Of T)(source As IEnumerable(Of T), parTokens As Integer) As T()()
        Return source.SplitIterator(parTokens).ToArray
    End Function

    ''' <summary>
    ''' Performance the partitioning operation on the input sequence.
    ''' (请注意，这个函数只适用于数量较少的序列。对所输入的序列进行分区操作，<paramref name="parTokens"/>函数参数是每一个分区里面的元素的数量)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="parTokens"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function SplitIterator(Of T)(source As IEnumerable(Of T), parTokens As Integer) As IEnumerable(Of T())
        Dim buffer As New List(Of T)(capacity:=parTokens)

        For Each item As T In source
            Call buffer.Add(item)

            If buffer >= parTokens Then
                Yield buffer.ToArray

                buffer *= 0
            End If
        Next

        If buffer > 0 Then
            Yield buffer.ToArray
        End If
    End Function

    ''' <summary>
    ''' Merge two type specific collection.(函数会忽略掉空的集合，函数会构建一个新的集合，原有的集合不受影响)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), target As IEnumerable(Of T)) As List(Of T)
        Dim srcList As List(Of T) = If(source Is Nothing, New List(Of T), source.AsList)
        If Not target Is Nothing Then
            Call srcList.AddRange(target)
        End If
        Return srcList
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), ParamArray data As T()) As List(Of T)
        Return source.Join(target:=data)
    End Function

    ''' <summary>
    ''' Source list join a new <paramref name="data"/> element.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), data As T) As List(Of T)
        Return source.Join({data})
    End Function

    ''' <summary>
    ''' ``X, ....``
    ''' 
    ''' (这个函数是一个安全的函数，当<paramref name="collection"/>为空值的时候回忽略掉<paramref name="collection"/>，
    ''' 只返回包含有一个<paramref name="obj"/>元素的列表)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="collection"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(obj As T, collection As IEnumerable(Of T)) As List(Of T)
        With New List(Of T) From {obj}
            If Not collection Is Nothing Then
                Call .AddRange(collection)
            End If

            Return .ByRef
        End With
    End Function
End Module
