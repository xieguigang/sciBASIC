#Region "Microsoft.VisualBasic::a192acbe9cbd7cb8daca930d506cbc41, Data_science\MachineLearning\MachineLearning\SVM\Parameter\Parameter.vb"

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

    '     Class Parameter
    ' 
    '         Properties: C, CacheSize, Coefficient0, Degree, EPS
    '                     Gamma, KernelType, Nu, P, Probability
    '                     Shrinking, SvmType, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone, Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    ''' <summary>
    ''' This class contains the various parameters which can affect the way in which an SVM
    ''' is learned.  Unless you know what you are doing, chances are you are best off using
    ''' the default values.
    ''' </summary>
    Public Class Parameter : Implements ICloneable

        ''' <summary>
        ''' Contains custom weights for class labels.  Default weight value is 1.
        ''' </summary>
        Dim _Weights As Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Type of SVM (default C-SVC)
        ''' </summary>
        Public Property SvmType As SvmType

        ''' <summary>
        ''' Type of kernel function (default Polynomial)
        ''' </summary>
        Public Property KernelType As KernelType

        ''' <summary>
        ''' Degree in kernel function (default 3).
        ''' </summary>
        Public Property Degree As Integer

        ''' <summary>
        ''' Gamma in kernel function (default 1/k)
        ''' </summary>
        ''' <remarks>
        ''' 这个参数比较重要，千万不可以设置为零，否则将无法进行数据分类
        ''' </remarks>
        Public Property Gamma As Double = 0.5

        ''' <summary>
        ''' Zeroeth coefficient in kernel function (default 0)
        ''' </summary>
        Public Property Coefficient0 As Double

        ''' <summary>
        ''' Cache memory size in MB (default 100)
        ''' </summary>
        Public Property CacheSize As Double

        ''' <summary>
        ''' Tolerance of termination criterion (default 0.001)
        ''' </summary>
        Public Property EPS As Double

        ''' <summary>
        ''' The parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)
        ''' </summary>
        Public Property C As Double

        ''' <summary>
        ''' <see cref="ColorClass.name"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Weights As Dictionary(Of Integer, Double)
            Get
                Return _Weights
            End Get
            Private Set
                _Weights = Value
            End Set
        End Property

        ''' <summary>
        ''' The parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)
        ''' </summary>
        Public Property Nu As Double

        ''' <summary>
        ''' The epsilon in loss function of epsilon-SVR (default 0.1)
        ''' </summary>
        Public Property P As Double

        ''' <summary>
        ''' Whether to use the shrinking heuristics, (default True)
        ''' </summary>
        Public Property Shrinking As Boolean

        ''' <summary>
        ''' Whether to train an SVC or SVR model for probability estimates, (default False)
        ''' </summary>
        Public Property Probability As Boolean

        ''' <summary>
        ''' Default Constructor.  Gives good default values to all parameters.
        ''' </summary>
        Public Sub New()
            SvmType = SvmType.C_SVC
            KernelType = KernelType.RBF
            Degree = 3
            Gamma = 0.5
            Coefficient0 = 0
            Nu = 0.5
            CacheSize = 40
            C = 1
            EPS = 0.001
            P = 0.1
            Shrinking = True
            Probability = False
            Weights = New Dictionary(Of Integer, Double)()
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other As Parameter = TryCast(obj, Parameter)
            If other Is Nothing Then Return False
            Return other.C = C AndAlso other.CacheSize = CacheSize AndAlso other.Coefficient0 = Coefficient0 AndAlso other.Degree = Degree AndAlso other.EPS = EPS AndAlso other.Gamma = Gamma AndAlso other.KernelType = KernelType AndAlso other.Nu = Nu AndAlso other.P = P AndAlso other.Probability = Probability AndAlso other.Shrinking = Shrinking AndAlso other.SvmType = SvmType AndAlso other.Weights.ToArray().IsEqual(Weights.ToArray())
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return C.GetHashCode() + CacheSize.GetHashCode() + Coefficient0.GetHashCode() + Degree.GetHashCode() + EPS.GetHashCode() + Gamma.GetHashCode() + KernelType.GetHashCode() + Nu.GetHashCode() + P.GetHashCode() + Probability.GetHashCode() + Shrinking.GetHashCode() + SvmType.GetHashCode() + Weights.ToArray().ComputeHashcode()
        End Function


#Region "ICloneable Members"
        ''' <summary>
        ''' Creates a memberwise clone of this parameters object.
        ''' </summary>
        ''' <returns>The clone (as type Parameter)</returns>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return MemberwiseClone()
        End Function

#End Region
    End Class
End Namespace
