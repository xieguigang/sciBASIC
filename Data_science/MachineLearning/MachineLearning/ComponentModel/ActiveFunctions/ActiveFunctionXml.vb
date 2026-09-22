#Region "Microsoft.VisualBasic::6454e483bb6d9710bef256201ddd9939, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\ActiveFunctionXml.vb"

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

    '   Total Lines: 208
    '    Code Lines: 105 (50.48%)
    ' Comment Lines: 91 (43.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (5.77%)
    '     File Size: 9.29 KB


    '     Class ActiveFunction
    ' 
    '         Properties: [Function], Arguments, BipolarSigmoid, name, ReLU
    '                     Sigmoid, Threshold
    ' 
    '         Function: CreateFunction, HasKey, Parse, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.Activations

    ''' <summary>
    ''' 激活函数的存储于XML文档之中的数据模型
    ''' </summary>
    Public Class ActiveFunction

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns>
        ''' The unique name of the activation function, this name should be 
        ''' identical with the class name of the target activation function type.
        ''' </returns>
        <XmlAttribute>
        Public Property name As String
        ''' <summary>
        ''' 函数对象的构造参数列表
        ''' </summary>
        ''' <returns>
        ''' A collection of the <see cref="NamedValue"/> objects which describes 
        ''' the constructor arguments of this activation function.
        ''' </returns>
        ''' <remarks>
        ''' 因为无法将字典对象进行Xml序列化, 所以在这里使用键值对集合来表示
        ''' </remarks>
        <XmlElement>
        Public Property Arguments As NamedValue()

        ''' <summary>
        ''' Gets the value of a specific constructor argument by its name.
        ''' </summary>
        ''' <param name="name">The argument name, the name is case insensitive.</param>
        ''' <returns>
        ''' The argument value in <see cref="Double"/> type; ``0`` will be 
        ''' returned when the target argument is not exists.
        ''' </returns>
        Default Public ReadOnly Property Item(name As String) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Arguments _
                    .FirstOrDefault(Function(tag) tag.name.TextEquals(name)) _
                   ?.text
            End Get
        End Property

        ''' <summary>
        ''' 通过这个只读属性来得到激活函数的对象模型
        ''' </summary>
        ''' <returns>
        ''' The <see cref="IActivationFunction"/> object instance which is 
        ''' created by the <see cref="CreateFunction"/> method.
        ''' </returns>
        Public ReadOnly Property [Function]() As IActivationFunction
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return CreateFunction()
            End Get
        End Property

        ''' <summary>
        ''' Create the activation function object instance from the stored data model.
        ''' </summary>
        ''' <returns>
        ''' An <see cref="IActivationFunction"/> object instance which is 
        ''' described by the <see cref="name"/> and <see cref="Arguments"/>.
        ''' </returns>
        ''' <remarks>
        ''' A default <see cref="Activations.Sigmoid"/> function will be returned 
        ''' when the <see cref="name"/> value is not a valid function name.
        ''' </remarks>
        Public Function CreateFunction() As IActivationFunction
            With Me
                Select Case name
                    Case NameOf(Activations.BipolarSigmoid)
                        If HasKey("alpha") Then
                            Return New BipolarSigmoid(!alpha)
                        Else
                            Return New BipolarSigmoid
                        End If
                    Case NameOf(Activations.SigmoidFunction)
                        Return New SigmoidFunction
                    Case NameOf(Activations.Sigmoid)
                        If HasKey("alpha") Then
                            Return New Sigmoid(!alpha)
                        Else
                            Return New Sigmoid
                        End If
                    Case NameOf(Activations.Threshold)
                        Return New Threshold
                    Case NameOf(Activations.ReLU)
                        Return New ReLU
                    Case NameOf(Activations.HyperbolicTangent)
                        Return New HyperbolicTangent
                    Case NameOf(Activations.Sinc)
                        Return New Sinc
                    Case NameOf(Activations.Softplus)
                        Return New Softplus
                    Case NameOf(Identical)
                        Return New Identical
                    Case NameOf(QLinear)
                        Return New QLinear
                    Case Else
#If DEBUG Then
                        Call $"Missing model: {name}".Warning
#End If
                        Return New Activations.Sigmoid
                End Select
            End With
        End Function

        ''' <summary>
        ''' Does a specific constructor argument exists in the 
        ''' <see cref="Arguments"/> list?
        ''' </summary>
        ''' <param name="name">The argument name, the name is case insensitive.</param>
        ''' <returns>
        ''' ``True`` when the target argument is exists in the 
        ''' <see cref="Arguments"/> list, otherwise ``False``.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HasKey(name As String) As Boolean
            If Arguments Is Nothing Then
                Return False
            Else
                Return Arguments.Any(Function(tag) tag.name.TextEquals(name))
            End If
        End Function

        ''' <summary>
        ''' Display this activation function data model as a text expression.
        ''' </summary>
        ''' <returns>
        ''' A string in format like ``name(arg1:=value1, arg2:=value2)``, which 
        ''' can be parsed back by the <see cref="Parse"/> function.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{name}({Arguments.Select(Function(a) $"{a.name}:={a.text}").JoinBy(", ")})"
        End Function

        ''' <summary>
        ''' 将激活函数从数据模型转换为对象模型
        ''' </summary>
        ''' <param name="af">The source activation function data model.</param>
        ''' <returns>The activation function object which is created by the <see cref="CreateFunction"/> method.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(af As ActiveFunction) As IActivationFunction
            Return af.Function
        End Operator

#Region "Default Expressions"
        ''' <summary>
        ''' The default text expression of the <see cref="Activations.ReLU"/> function: ``ReLU()``.
        ''' </summary>
        ''' <returns>A default string expression value.</returns>
        Public Shared ReadOnly Property ReLU As [Default](Of String) = "ReLU()"
        ''' <summary>
        ''' The default text expression of the <see cref="Activations.Threshold"/> function: ``Threshold()``.
        ''' </summary>
        ''' <returns>A default string expression value.</returns>
        Public Shared ReadOnly Property Threshold As [Default](Of String) = "Threshold()"
        ''' <summary>
        ''' The default text expression of the <see cref="Activations.Sigmoid"/> function: ``Sigmoid(alpha:=2.0)``.
        ''' </summary>
        ''' <returns>A default string expression value.</returns>
        Public Shared ReadOnly Property Sigmoid As [Default](Of String) = "Sigmoid(alpha:=2.0)"
        ''' <summary>
        ''' The default text expression of the <see cref="Activations.BipolarSigmoid"/> function: ``BipolarSigmoid(alpha:=2.0)``.
        ''' </summary>
        ''' <returns>A default string expression value.</returns>
        Public Shared ReadOnly Property BipolarSigmoid As [Default](Of String) = "BipolarSigmoid(alpha:=2.0)"
#End Region

        ''' <summary>
        ''' 将文本表达式解析为激活函数的模型
        ''' </summary>
        ''' <param name="expression">这个字符串表达式应该是<see cref="IActivationFunction.ToString()"/>的函数输出结果字符串</param>
        ''' <returns>
        ''' An <see cref="ActiveFunction"/> data model which is parsed from the 
        ''' given text <paramref name="expression"/>.
        ''' </returns>
        Public Shared Function Parse(expression As String) As ActiveFunction
            Dim func As NamedValue(Of String) = expression.GetTagValue("(", trim:=True)

            Return New ActiveFunction With {
                .name = func.Name,
                .Arguments = func.Value _
                    .Trim(")"c) _
                    .StringSplit("\s*,\s*") _
                    .Select(Function(a) a.GetTagValue(":=")) _
                    .Select(Function(a)
                                Return New NamedValue With {
                                    .name = a.Name,
                                    .text = a.Value
                                }
                            End Function) _
                    .ToArray
            }
        End Function
    End Class
End Namespace
