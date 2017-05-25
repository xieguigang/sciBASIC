Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Where is the Min() or Max() or first TRUE or FALSE ?
''' (这个模块之中的函数返回来的都是集合之中的符合条件的对象元素的index坐标)
''' </summary>
Public NotInheritable Class Which

    ''' <summary>
    ''' 在这里不适用Module类型，要不然会和其他的Max拓展函数产生冲突的。。
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Determines the location, i.e., index of the (first) minimum or maximum of a numeric (or logical) vector.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x">
    ''' numeric (logical, integer or double) vector or an R object for which the internal coercion to 
    ''' double works whose min or max is searched for.
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Max(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
        Return x.MaxIndex
    End Function

    ''' <summary>
    ''' Determines the location, i.e., index of the (first) minimum or maximum of a numeric (or logical) vector.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x">
    ''' numeric (logical, integer or double) vector or an R object for which the internal coercion to 
    ''' double works whose min or max is searched for.
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Min(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
        Return x.MinIndex
    End Function

    ''' <summary>
    ''' Return all of the indices which is True
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    Public Shared Function IsTrue(v As IEnumerable(Of Boolean)) As Integer()
        Return v _
            .SeqIterator _
            .Where(Function(b) True = +b) _
            .Select(Function(i) CInt(i)) _
            .ToArray
    End Function

    ''' <summary>
    ''' Returns all of the indices which is False
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    Public Shared Function IsFalse(v As IEnumerable(Of Boolean)) As Integer()
        Return v _
           .SeqIterator _
           .Where(Function(b) False = +b) _
           .Select(Function(i) CInt(i)) _
           .ToArray
    End Function

    Public Shared Function IsTrue([operator] As Func(Of Boolean())) As Integer()
        Return Which.IsTrue([operator]())
    End Function

    Public Shared Function IsFalse([operator] As Func(Of Boolean())) As Integer()
        Return Which.IsFalse([operator]())
    End Function

    ''' <summary>
    ''' 枚举出所有大于目标的顶点编号
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="compareTo"></param>
    ''' <returns></returns>
    ''' <remarks>因为这个返回的是一个迭代器，所以可以和First结合产生FirstGreaterThan表达式</remarks>
    Public Shared Function IsGreaterThan(Of T As IComparable)(source As IEnumerable(Of T), compareTo As T) As IEnumerable(Of Integer)
        Return source _
            .SeqIterator _
            .Where(Function(o) Language.GreaterThan(o.value, compareTo)) _
            .Select(Function(i) i.i) ' 因为返回的是linq表达式，所以这里就不加ToArray了
    End Function

    Public Shared Function IsGreaterThan(Of T, C As IComparable)(source As IEnumerable(Of T), getValue As Func(Of T, C), compareTo As C) As IEnumerable(Of Integer)
        Return Which.IsGreaterThan(source.Select(getValue), compareTo)
    End Function
End Class
