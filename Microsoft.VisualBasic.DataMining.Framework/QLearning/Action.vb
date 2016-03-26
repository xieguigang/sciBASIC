Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization

Namespace QLearning

    ''' <summary>
    ''' One specific environment state have some possible actions,
    ''' but there is just one best action on the current environment state based on the accumulate q-values
    ''' </summary>
    Public Class Action : Implements sIdEnumerable

        ''' <summary>
        ''' The environment variables state as inputs for the machine.
        ''' </summary>
        ''' <returns></returns>
        Public Property EnvirState As String Implements sIdEnumerable.Identifier
        ''' <summary>
        ''' Actions for the current state.
        ''' </summary>
        ''' <returns></returns>
        Public Property Qvalues As Single()

        ''' <summary>
        ''' Environment -> actions' Q-values
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"[ {EnvirState} ] --> {Qvalues.GetJson}"
        End Function

    End Class
End Namespace