#Region "Microsoft.VisualBasic::0748f70313fc01e2b6809c5f8a59e771, Data_science\DataMining\DataMining\Interface\DFL\I_FactorElement.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 120
    '    Code Lines: 62
    ' Comment Lines: 42
    '   Blank Lines: 16
    '     File Size: 4.56 KB


    '     Class I_FactorElement
    ' 
    '         Properties: FunctionalState, Quantity, Weight
    ' 
    '         Function: get_InteractionQuantity, Internal_getEventProbabilities, set_Quantity, ShadowCopy, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace DFL_Driver

    ''' <summary>
    ''' This object represents the factor which decides the node state changes.(决定<see cref="dflNode"></see>的状态的因素)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class I_FactorElement

        Protected _InteractionTarget As dflNode
        Dim _InternalQuantityValue As Double
        ''' <summary>
        ''' <see cref="_Weight"></see>越大,则<see cref="_ABS_Weight"></see>越小，即事件发生的阈值越小
        ''' </summary>
        ''' <remarks></remarks>
        Dim _Weight As Double
        ''' <summary>
        '''  1 - <see cref="stdNum.Abs"></see>(<see cref="_Weight"></see>)
        ''' </summary>
        ''' <remarks></remarks>
        Protected _ABS_Weight As Double

        ''' <summary>
        ''' Weight = [-1,1]. (可以带有符号，介于-1到1之间)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Weight As Double
            Get
                Return _Weight
            End Get
            Set(value As Double)
                _Weight = value
                _ABS_Weight = 1 - stdNum.Abs(value)
            End Set
        End Property

        Public Overridable ReadOnly Property Quantity As Double
            Get
                Return _InternalQuantityValue
            End Get
        End Property

        ''' <summary>
        ''' Does this factor effects on the node states changes? value zero is no effects.
        ''' (当前的这个因素是否会影响目标节点的状态值的改变，0表示不影响)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FunctionalState As Double
            Get
                Dim n As Double = Rnd()
                Dim p As Double = Internal_getEventProbabilities()

                If n >= p Then
                    Return get_InteractionQuantity()
                Else
                    Return 0 '事件发生的阈值不满足条件，无法起作用
                End If
            End Get
        End Property

        ''' <summary>
        ''' 计算公式为 (1-w)， 即本函数返回的值越低，则事件越容易发生，请注意使用 rnd >= Internal_getEventProbabilities() 来描述事件发生
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function Internal_getEventProbabilities() As Double
            If _InternalQuantityValue = 0.0R Then
                Return 1  '当调控因子的数量为0的时候，rnd必须要大于1才会发生调控事件，即没有调控因子的时候，很明显不能够从rnd>=1发生调控事件
            End If

            Dim qw As Double = _InteractionTarget.get_FactorsCollectionWeight

            If qw = 0.0R Then 'q/qw=1  '如果总量为0，则说明q也为0
                qw = 0  '1-1=0
            Else
                qw = (1 - _InternalQuantityValue / qw)
            End If

            Dim value As Double = _ABS_Weight * qw

            Return value
        End Function

        ''' <summary>
        ''' 假若事件发生的话，这个函数决定了<see cref="FunctionalState"></see>所返回的计算值
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function get_InteractionQuantity() As Double
            Return _InternalQuantityValue * _Weight
        End Function

        Public Overrides Function ToString() As String
            Return 100 * Quantity / _InteractionTarget.get_FactorsCollectionWeight
        End Function

        Public Function set_Quantity(value As Double) As DFL_Driver.I_FactorElement
            _InternalQuantityValue = value
            Return Me
        End Function

        ''' <summary>
        ''' <see cref="dflNode"></see>对象初始化的时候所使用的方法
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="Target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ShadowCopy(data As I_FactorElement, Target As dflNode) As I_FactorElement
            Dim Element = data
            Element._InteractionTarget = Target
            Return Element
        End Function
    End Class
End Namespace
