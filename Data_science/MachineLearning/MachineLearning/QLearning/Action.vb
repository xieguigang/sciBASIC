#Region "Microsoft.VisualBasic::568b3b0ba65c7d59340a3bca768edafe, Data_science\MachineLearning\MachineLearning\QLearning\Action.vb"

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

    '     Class Action
    ' 
    '         Properties: EnvirState, Qvalues
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace QLearning

    ''' <summary>
    ''' One specific environment state have some possible actions,
    ''' but there is just one best action on the current environment state based on the accumulate q-values
    ''' </summary>
    Public Class Action : Implements INamedValue

        ''' <summary>
        ''' The environment variables state as inputs for the machine.
        ''' </summary>
        ''' <returns></returns>
        Public Property EnvirState As String Implements INamedValue.Key
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
            Return $"[ {EnvirState} ] {vbTab}--> {Qvalues.GetJson}"
        End Function

    End Class
End Namespace
