Imports System.Runtime.CompilerServices

Namespace IO

    Public Module Extensions

        <Extension>
        Public Function DataFrame(data As IEnumerable(Of PropertyValue)) As EntityObject()
            Dim objects = data.GroupBy(Function(k) k.Key).ToArray
            Dim out As EntityObject() = objects _
                .Select(AddressOf CreateObject) _
                .ToArray
            Return out
        End Function

        <Extension>
        Public Function CreateObject(g As IGrouping(Of String, PropertyValue)) As EntityObject
            Return New EntityObject With {
                .ID = g.Key,
                .Properties = g.ToDictionary(Function(k) k.Property,
                                             Function(v) v.Value)
            }
        End Function
    End Module
End Namespace