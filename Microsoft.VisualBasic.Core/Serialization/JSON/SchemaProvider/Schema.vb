#Region "Microsoft.VisualBasic::0b71e24fa0391addb7022cf3116ed3d6, Microsoft.VisualBasic.Core\Serialization\JSON\SchemaProvider\Schema.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Schema
    ' 
    '         Properties: title
    ' 
    '         Function: ToString
    ' 
    '     Class SchemaProvider
    ' 
    '         Properties: description, properties, required, type
    ' 
    '     Class [Property]
    ' 
    '         Properties: exclusiveMinimum, minimum, name, ref
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
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
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property minimum As Integer
        Public Property exclusiveMinimum As Boolean
        Public Property ref As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
