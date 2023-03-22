Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Turtle

    Public Module BuildObject

        ''' <summary>
        ''' build rdf object from the ttl data
        ''' </summary>
        ''' <param name="ttl"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateObjects(ttl As IEnumerable(Of Triple)) As IEnumerable(Of RDFEntity)
            Dim objs = ttl.GroupBy(Function(t) t.subject).ToArray

            For Each group As IGrouping(Of String, Triple) In objs
                Yield group.CreateObject
            Next
        End Function

        <Extension>
        Private Function CreateObject(group As IGrouping(Of String, Triple)) As RDFEntity
            Dim objData As New Dictionary(Of String, RDFEntity)

            For Each tuple As IGrouping(Of String, Relation) In group _
                .Select(Function(t) t.relations) _
                .IteratesALL _
                .GroupBy(Function(r) r.predicate)

                Dim data As New Dictionary(Of String, RDFEntity)
                Dim i As i32 = 1

                For Each val As String In tuple _
                    .Select(Function(ti) ti.objs) _
                    .IteratesALL

                    Call data.Add(++i, New RDFEntity With {.value = val})
                Next

                objData(tuple.Key) = New RDFEntity With {
                    .RDFId = tuple.Key,
                    .Properties = data
                }
            Next

            Return New RDFEntity With {
                .RDFId = group.Key,
                .Properties = objData
            }
        End Function
    End Module
End Namespace