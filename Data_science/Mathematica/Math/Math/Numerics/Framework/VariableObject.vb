Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Class VariableObject : Inherits f64
        Implements INamedValue

        ''' <summary>
        ''' the unique id of current object variable 
        ''' </summary>
        ''' <returns></returns>
        Public Property Id As String Implements IKeyedEntity(Of String).Key

        Public Overrides Function ToString() As String
            Return $"Dim {Id} As f64 = {Value}"
        End Function
    End Class
End Namespace