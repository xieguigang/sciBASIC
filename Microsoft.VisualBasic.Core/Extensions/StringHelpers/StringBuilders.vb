Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' <see cref="StringBuilder"/> helpers
''' </summary>
Public Module StringBuilders

    ''' <summary>
    ''' 批量进行替换操作
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <param name="replacements"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Replace(sb As StringBuilder, ParamArray replacements As NamedValue(Of String)()) As StringBuilder
        For Each tuple As NamedValue(Of String) In replacements.SafeQuery
            Call sb.Replace(tuple.Name, tuple.Value)
        Next

        Return sb
    End Function

    ''' <summary>
    ''' 适用于更加复杂的结果值的产生的链式替换
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <param name="find$"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Replace(sb As StringBuilder, find$, value As Func(Of String)) As StringBuilder
        Return sb.Replace(find, value())
    End Function
End Module
