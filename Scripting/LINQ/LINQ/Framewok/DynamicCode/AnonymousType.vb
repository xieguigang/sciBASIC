Namespace Framework.DynamicCode

    Public Class AnonymousType

        Public Class [PropertyF]
            <Xml.Serialization.XmlAttribute> Public Property Name As String
            <Xml.Serialization.XmlElement> Public Property Value As Object

            Public Overrides Function ToString() As String
                Return String.Format("{0}:={1}", Name, Value.ToString)
            End Function
        End Class

        <Xml.Serialization.XmlArray> Public Property Properties As AnonymousType.[PropertyF]()

        Default Public ReadOnly Property [Property](Name As String) As Object
            Get
                Dim LQuery = From p As PropertyF In Me.Properties Where String.Equals(Name, p.Name, StringComparison.OrdinalIgnoreCase) Select p '
                Dim Result = LQuery.ToArray
                If Result.Count = 0 Then
                    Return Nothing
                Else
                    Return Result.First
                End If
            End Get
        End Property
    End Class
End Namespace