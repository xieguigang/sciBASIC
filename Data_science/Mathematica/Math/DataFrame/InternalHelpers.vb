Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 这个模块是在删除了csv文件模块的依赖之后的进行重复实现某些csv文件模块的逻辑函数的帮助模块
''' </summary>
Friend Module InternalHelpers

    <Extension>
    Friend Function PropertyNames(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As DataSet()) As String()
        Return data.Select(Function(r) r.Properties.Keys).IteratesALL.Distinct.ToArray
    End Function

    <Extension>
    Friend Function Vector(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), name As String) As IEnumerable(Of Double)
        Return data.Select(Function(r) r.Properties(name))
    End Function
End Module
