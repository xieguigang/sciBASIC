Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace HTML.CSS.Render

    Public Class CSSFile

        Public Property Selectors As Selector()

    End Class

    Public Class Selector : Inherits [Property](Of String)
        Implements INamedValue

        Public Property Selector As String Implements IKeyedEntity(Of String).Key
    End Class
End Namespace