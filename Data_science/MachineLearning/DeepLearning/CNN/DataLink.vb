
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN

    Public MustInherit Class DataLink

        ''' <summary>
        ''' the input and output
        ''' </summary>
        ''' <remarks>
        ''' data object at here for link the current layer and the next layer
        ''' no needs for save into the model file
        ''' </remarks>
        <IgnoreDataMember> Protected in_act As DataBlock
        <IgnoreDataMember> Protected out_act As DataBlock

    End Class
End Namespace