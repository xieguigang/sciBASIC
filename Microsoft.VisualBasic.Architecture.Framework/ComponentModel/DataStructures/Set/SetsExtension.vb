Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures

Public Module SetsExtension

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function ToArray(Of T)(s As [Set], [ctype] As Func(Of Object, T)) As T()
        Return s.ToArray _
                .Select([ctype]) _
                .ToArray
    End Function

    ''' <summary>
    ''' DirectCast
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function ToArray(Of T)(s As [Set]) As T()
        Return s.ToArray _
                .Select(Function(x) DirectCast(x, T)) _
                .ToArray
    End Function

#Region "API"

    ''' <summary>
    ''' 差集，函数会从<paramref name="s1"/>集合之中删除<paramref name="s2"/>中共同的元素
    ''' </summary>
    ''' <param name="s1"></param>
    ''' <param name="s2"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Except")>
    Public Function Except(s1 As [Set], s2 As [Set]) As [Set]
        Return s1 - s2
    End Function

    ''' <summary>
    ''' 就并集
    ''' </summary>
    ''' <param name="s1"></param>
    ''' <param name="s2"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Union", Info:="Performs a union of two sets.")>
    Public Function Union(s1 As [Set], s2 As [Set]) As [Set]
        Return s1 Or s2
    End Function

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <param name="s1"></param>
    ''' <param name="s2"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Intersection", Info:="Performs an intersection of two sets.")>
    Public Function Intersection(s1 As [Set], s2 As [Set]) As [Set]
        Return s1 And s2
    End Function

    ''' <summary>
    ''' 将任意序列转换为集合类型
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("As.Set")>
    <Extension>
    Public Function AsSet(source As IEnumerable) As [Set]
        Return New [Set](source)
    End Function

    <ExportAPI("As.Array")>
    Public Function ToArray([set] As [Set]) As Object()
        Return [set].ToArray
    End Function
#End Region
End Module
