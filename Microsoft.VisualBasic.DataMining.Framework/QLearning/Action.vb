Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization

Namespace QLearning

    Public Class Action : Implements sIdEnumerable

        Public Property Action As String Implements sIdEnumerable.Identifier
        Public Property Qvalues As Single()

        Public Overrides Function ToString() As String
            Return $"[ {Action} ] --> {Qvalues.GetJson}"
        End Function

    End Class
End Namespace