#Region "Microsoft.VisualBasic::3368ebe9b7c4bdcd8d69e3c4f3fd02ff, Data_science\Mathematica\Math\Math\FuzzyLogic\MembershipFunction.vb"

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

'   Total Lines: 121
'    Code Lines: 44 (36.36%)
' Comment Lines: 54 (44.63%)
'    - Xml Docs: 70.37%
' 
'   Blank Lines: 23 (19.01%)
'     File Size: 3.82 KB


'     Class MembershipFunction
' 
'         Properties: Name, Value, X0, X1, X2
'                     X3
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: Area, Centorid
' 
' 
' /********************************************************************************/

#End Region

#Region "GNU Lesser General Public License"
'
'This file is part of DotFuzzy.
'
'DotFuzzy is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'DotFuzzy is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with DotFuzzy.  If not, see <http://www.gnu.org/licenses/>.
'

#End Region

Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

Namespace Mamdani

    ''' <summary>
    ''' Mamdani 模糊集合：梯形四点表示 <c>(X0, X1, X2, X3)</c>。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本类型现在是 <see cref="TSK.TrapezoidalMembership"/> 的**门面**：隶属度求值、
    ''' 解析导数与参数向量只有一份实现，<see cref="X0"/>..<see cref="X3"/> 是对参数向量的投影。
    ''' 因此历史 FCL/XML 模型（四点格式）与新迁入的 TSK 可微内核共享同一套数学，
    ''' 既不需要写适配器，也不会出现两套求值逻辑各自漂移。
    ''' </para>
    ''' <para>
    ''' 与求值无关的成员保持原有语义：<see cref="Value"/> 仍是规则触发强度累积器
    ''' （由 <c>FuzzyEngine.Defuzzify</c> 写入），<see cref="Centorid"/> 与 <see cref="Area"/>
    ''' 仍是重心法去模糊用的近似式。这两者都**不参与** TSK 推理。
    ''' </para>
    ''' </remarks>
    Public Class MembershipFunction : Inherits TrapezoidalMembership

#Region "Constructors"

        ''' <summary>
        ''' Default constructor（四点全部为 0）。
        ''' </summary>
        Public Sub New()
            MyBase.New(0.0, 0.0, 0.0, 0.0)
        End Sub

        ''' <param name="name">The name that identificates the membership function.</param>
        Public Sub New(name As String)
            MyBase.New(0.0, 0.0, 0.0, 0.0)

            Me.Name = name
        End Sub

        ''' <param name="name">The name that identificates the linguistic variable.</param>
        ''' <param name="x0">The value of the (x0, 0) point.</param>
        ''' <param name="x1">The value of the (x1, 1) point.</param>
        ''' <param name="x2">The value of the (x2, 1) point.</param>
        ''' <param name="x3">The value of the (x3, 0) point.</param>
        Public Sub New(name As String, x0 As Double, x1 As Double, x2 As Double, x3 As Double)
            MyBase.New(x0, x1, x2, x3)

            Me.Name = name
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' The name that identificates the membership function.
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' The value of the (x0, 0) point（参数向量的第 0 个分量）。
        ''' </summary>
        Public Property X0() As Double
            Get
                Return A
            End Get
            Set(value As Double)
                Call SetParameterAt(0, value)
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x1, 1) point（参数向量的第 1 个分量）。
        ''' </summary>
        Public Property X1() As Double
            Get
                Return B
            End Get
            Set(value As Double)
                Call SetParameterAt(1, value)
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x2, 1) point（参数向量的第 2 个分量）。
        ''' </summary>
        Public Property X2() As Double
            Get
                Return C
            End Get
            Set(value As Double)
                Call SetParameterAt(2, value)
            End Set
        End Property

        ''' <summary>
        ''' The value of the (x3, 0) point（参数向量的第 3 个分量）。
        ''' </summary>
        Public Property X3() As Double
            Get
                Return D
            End Get
            Set(value As Double)
                Call SetParameterAt(3, value)
            End Set
        End Property

        ''' <summary>
        ''' 改写单个分量（避免每次写入都为整个参数向量分配临时数组）。
        ''' </summary>
        Private Sub SetParameterAt(index As Integer, value As Double)
            Dim values As Double() = GetParameters()

            values(index) = value
            Call SetParameters(values)
        End Sub

        ''' <summary>
        ''' The value of membership function after evaluation process.
        ''' </summary>
        Public Property Value() As Double

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Calculate the centroid of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of centroid.</returns>
        ''' <remarks>
        ''' 直接读基类的参数投影 <see cref="TSK.TrapezoidalMembership.A"/> 等，避免经属性访问
        ''' 反复分配参数数组（去模糊是 O(术语个数) 的热路径）。
        ''' </remarks>
        Public Function Centorid() As Double
            Dim a As Double = Me.C - Me.B
            Dim b As Double = Me.D - Me.A
            Dim c As Double = Me.B - Me.A

            Return ((2 * a * c) + (a * a) + (c * b) + (a * b) + (b * b)) / (3 * (a + b)) + Me.A
        End Function

        ''' <summary>
        ''' Calculate the area of a trapezoidal membership function.
        ''' </summary>
        ''' <returns>The value of area.</returns>
        Public Function Area() As Double
            Dim a As Double = Me.Centorid() - Me.A
            Dim b As Double = Me.D - Me.A

            Return (Value * (b + (b - (a * Value)))) / 2
        End Function

        ''' <summary>
        ''' 深拷贝（保留术语名称）。
        ''' </summary>
        Public Overrides Function Clone() As TSK.IMembershipFunction
            Return New MembershipFunction(Name, Me.A, Me.B, Me.C, Me.D)
        End Function

#End Region
    End Class
End Namespace
