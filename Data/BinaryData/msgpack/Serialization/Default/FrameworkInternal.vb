Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Serialization.Default

    Friend Class NamedValueSchema : Inherits SchemaProvider(Of NamedValue)

        Protected Friend Overrides Iterator Function GetObjectSchema() As IEnumerable(Of (obj As Type, schema As Dictionary(Of String, NilImplication)))
            Yield (GetType(NamedValue), getSchema())
        End Function

        Private Shared Function getSchema() As Dictionary(Of String, NilImplication)
            Return New Dictionary(Of String, NilImplication) From {
                {NameOf(NamedValue.name), NilImplication.MemberDefault},
                {NameOf(NamedValue.text), NilImplication.MemberDefault}
            }
        End Function
    End Class

End Namespace
