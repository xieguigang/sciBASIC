Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Public Class GraphWriter

    ReadOnly graph As SoapGraph

    Sub New(type As Type)
        graph = SoapGraph.GetSchema(type, Serializations.XML)
    End Sub

    Public Function Load(xml As XmlElement) As Object
        Return loadGraphTree(xml, graph)
    End Function

    Private Shared Function loadGraphTree(xml As XmlElement, parent As SoapGraph) As Object
        Dim members = xml.elements _
           .SafeQuery _
           .GroupBy(Function(xi) xi.name) _
           .ToDictionary(Function(xi) xi.Key,
                         Function(xi)
                             Return xi.ToArray
                         End Function)
        Dim obj As Object = parent.Activate(parent:=parent, docs:=members.Keys.ToArray, schema:=parent)
        Dim value As Object

        For Each objVal In members
            If parent.writers.ContainsKey(objVal.Key) Then
                Dim docs = objVal.Value
                Dim define = parent.writers(objVal.Key)

                If docs.Length > 1 AndAlso Not define.PropertyType.IsArray Then
                    ' warning
                    Call $"{objVal.Key}(array) -> {define.Name}(scalar) type mis-matched!".Warning

                    value = loadGraphTree(docs(Scan0), SoapGraph.GetSchema(define.PropertyType, Serializations.XML))
                Else
                    Dim array As Array = Array.CreateInstance(define.PropertyType.GetElementType, docs.Length)
                    Dim elementGraph = SoapGraph.GetSchema(define.PropertyType.GetElementType, Serializations.XML)

                    For i As Integer = 0 To array.Length - 1
                        value = loadGraphTree(docs(i), elementGraph)
                        array.SetValue(value, i)
                    Next

                    value = array
                End If

                Call define.SetValue(obj, value)
            Else
                Call $"missing {objVal.Key} from schema {parent.ToString}".Warning
            End If
        Next

        Return obj
    End Function

    Public Shared Function LoadXml(Of T)(xml As String) As T
        Dim doc As XmlElement = XmlElement.ParseXmlText(xml.SolveStream)
        Dim writer As New GraphWriter(GetType(T))
        Dim obj As Object = writer.Load(doc)

        Return obj
    End Function

End Class
