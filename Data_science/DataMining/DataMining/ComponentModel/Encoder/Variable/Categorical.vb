Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace ComponentModel.Encoder.Variable

    Public Class Categorical : Inherits EntityBase(Of String)
        Implements INamedValue

        Public Property id As String Implements IKeyedEntity(Of String).Key

    End Class
End Namespace