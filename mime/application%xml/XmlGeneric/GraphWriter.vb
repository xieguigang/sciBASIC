Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Public Class GraphWriter

    ReadOnly graph As SoapGraph

    Sub New(type As Type)
        graph = SoapGraph.GetSchema(type)
    End Sub

    Public Function Load(xml As XmlElement) As Object

    End Function

    Public Shared Function LoadXml(Of T)(xml As String) As T
        Dim doc As XmlElement = XmlElement.ParseXmlText(xml.SolveStream)
        Dim writer As New GraphWriter(GetType(T))
        Dim obj As Object = writer.Load(doc)

        Return obj
    End Function

End Class
