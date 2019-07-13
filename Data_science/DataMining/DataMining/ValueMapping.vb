#Region "Microsoft.VisualBasic::15cfcf1029c042c0477ffb2ecc8cd008, Data_science\DataMining\DataMining\ValueMapping.vb"

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

    ' Module ValueMapping
    ' 
    '     Function: Discretization, ModalNumber
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion

Public Module ValueMapping

    ''' <summary>
    ''' Gets the modal number of the ranking mapping data set.(求取众数)
    ''' </summary>
    ''' <param name="data">The ranked mapping encoding value.(经过Rank Mapping处理过后的编码值)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 当不存在相同的分组元素数目的时候，会直接取第一个元素的值作为众数
    ''' 当存在相同的分组元素数目的时候，会取最大的元素值作为众数
    ''' </remarks>
    Public Function ModalNumber(data As Integer()) As Integer
        Dim Avg As Double = data.Average
        Dim Min = (From n In data Where n < Avg Select n).ToArray
        Dim Max = (From n In data Where n >= Avg Select n).ToArray
        Dim Mdn As Integer

        If Min.Length > Max.Length Then
            Mdn = Min.Average
        Else
            Mdn = Max.Average
        End If

        Return Mdn
    End Function

    ''' <summary>
    ''' 执行连续数值类型的数据的离散化操作，这个操作常用于决策树的构建
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Discretization(data As IEnumerable(Of Double), levels As Integer) As Discretizer
        Return New Discretizer(data, levels)
    End Function
End Module
