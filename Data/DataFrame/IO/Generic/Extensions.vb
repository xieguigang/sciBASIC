Imports System.Runtime.CompilerServices

Namespace IO

    Public Module Extensions

        ''' <summary>
        ''' 使用这个函数请确保相同编号的对象集合之中是没有相同的属性名称的，
        ''' 但是假若会存在重复的名称的话，这些重复的名称的值会被<see cref="JoinBy"/>操作，分隔符为``分号``
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DataFrame(data As IEnumerable(Of IPropertyValue)) As EntityObject()
            Dim objects = data.GroupBy(Function(k) k.Key).ToArray
            Dim out As EntityObject() = objects _
                .Select(AddressOf CreateObject) _
                .ToArray
            Return out
        End Function

        <Extension>
        Public Function CreateObject(g As IGrouping(Of String, IPropertyValue)) As EntityObject
            Dim data As IPropertyValue() = g.ToArray
            Dim props As Dictionary(Of String, String) = data _
                .GroupBy(Function(p) p.Property) _
                .ToDictionary(Function(k) k.Key,
                              Function(v) v.Select(
                              Function(s) s.value).JoinBy("; "))

            Return New EntityObject With {
                .ID = g.Key,
                .Properties = props
            }
        End Function

        ''' <summary>
        ''' 批量的从目标对象集合之中选出目标属性值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="key$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Values(data As IEnumerable(Of EntityObject), key$) As String()
            Return data.Select(Function(r) r(key$)).ToArray
        End Function
    End Module
End Namespace