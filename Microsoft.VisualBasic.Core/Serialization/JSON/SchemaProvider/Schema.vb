#Region "Microsoft.VisualBasic::0b71e24fa0391addb7022cf3116ed3d6, Microsoft.VisualBasic.Core\Serialization\JSON\SchemaProvider\Schema.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



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
