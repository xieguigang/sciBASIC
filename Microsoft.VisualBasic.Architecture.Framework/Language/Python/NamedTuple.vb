Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Python

    ''' <summary>
    ''' ``namedtuple()`` Factory Function for Tuples with Named Fields
    ''' </summary>
    Public Class NamedTuple : Inherits [Property](Of Object)
        Implements sIdEnumerable

        Public Property Type As String Implements sIdEnumerable.Identifier

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace