Imports System.Runtime.CompilerServices

Public Module AnonymousTypeHelper

#Disable Warning

    ''' <summary>
    ''' You can using this method to create a empty list for the specific type of anonymous type object.
    ''' (使用这个方法获取得到匿名类型的列表数据集合对象)
    ''' </summary>
    ''' <typeparam name="TAnonymousType"></typeparam>
    ''' <param name="typedef">The temp object which was created anonymous.
    ''' (匿名对象的集合，这个是用来复制匿名类型的，虽然没有引用这个参数，但是却可以直接通过拓展来得到匿名类型生成列表对象)
    ''' </param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CopyTypeDef(Of TAnonymousType As Class)(typedef As IEnumerable(Of TAnonymousType)) As List(Of TAnonymousType)
        Return New List(Of TAnonymousType)
    End Function
#Enable Warning

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="source">仅仅是起到类型复制的作用</param>
    ''' <returns></returns>
    <Extension> Public Function CopyTypeDef(Of TKey, TValue)(source As Dictionary(Of TKey, TValue)) As Dictionary(Of TKey, TValue)
        Dim table As New Dictionary(Of TKey, TValue)
        Return table
    End Function
End Module
