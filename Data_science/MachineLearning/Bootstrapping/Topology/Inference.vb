#Region "Microsoft.VisualBasic::d8eab898b3c0f07ecdf4ff5b3cc3993a, Data_science\MachineLearning\Bootstrapping\Topology\Inference.vb"

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

    '     Module Inference
    ' 
    '         Function: GAFInference
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Topology

    ''' <summary>
    ''' dy = A - B
    ''' </summary>
    Public Module Inference

        ''' <summary>
        ''' 使用遗传算法来进行网络拓扑结构的估算
        ''' </summary>
        ''' <param name="obs"></param>
        ''' <param name="popSize%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GAFInference(obs As ODEsOut, Optional popSize% = 500) As NamedValue(Of (alpha As Double(), beta As Double()))
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
