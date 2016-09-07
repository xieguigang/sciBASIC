Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Serialization.JSON

    ''' <summary>
    ''' Here is a basic example of a JSON Schema:
    ''' 
    ''' ```json
    ''' {
    '''	   "title": "Example Schema",
    '''	   "type": "object",
    '''	   "properties": {
    '''	       "firstName": {
    '''	          "type": "string"
    '''	       },
    '''	       "lastName": {
    '''	          "type": "string"
    '''	       },
    '''	       "age": {
    '''	          "description": "Age in years",
    '''	          "type": "integer",
    '''	          "minimum": 0
    '''	       }
    '''	   },
    '''	   "required": ["firstName", "lastName"]
    ''' }
    ''' ```
    ''' </summary>
    Public Class Schema : Inherits SchemaProvider

        Public Property title As String

        Public Overrides Function ToString() As String
            Return Me.GetJson(simpleDict:=True)
        End Function
    End Class

    Public MustInherit Class SchemaProvider
        Public Property description As String
        Public Property type As String
        Public Property properties As Dictionary(Of [Property])
        Public Property required As String()
    End Class

    Public Class [Property] : Inherits SchemaProvider
        Implements sIdEnumerable

        Public Property name As String Implements sIdEnumerable.Identifier
        Public Property minimum As Integer
        Public Property exclusiveMinimum As Boolean
        Public Property ref As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace