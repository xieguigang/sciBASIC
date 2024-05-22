#Region "Microsoft.VisualBasic::6660c748be09594936710401bc64f5cf, Data_science\MachineLearning\MachineLearning\SVM\Parameter\Parameter.vb"

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

    '   Total Lines: 198
    '    Code Lines: 98 (49.49%)
    ' Comment Lines: 74 (37.37%)
    '    - Xml Docs: 78.38%
    ' 
    '   Blank Lines: 26 (13.13%)
    '     File Size: 6.87 KB


    '     Class Parameter
    ' 
    '         Properties: c, cacheSize, coefficient0, degree, EPS
    '                     gamma, kernelType, nu, P, probability
    '                     shrinking, svmType, weights
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
        Dim m_Weights As Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Type of SVM (default C-SVC)
        ''' </summary>
        Public Property svmType As SvmType

        ''' <summary>
        ''' Type of kernel function (default Polynomial)
        ''' </summary>
        Public Property kernelType As KernelType

        ''' <summary>
        ''' Degree in kernel function (default 3).
        ''' </summary>
        Public Property degree As Integer

        ''' <summary>
        ''' Gamma in kernel function (default 1/k)
        ''' </summary>
        ''' <remarks>
        ''' 这个参数比较重要，千万不可以设置为零，否则将无法进行数据分类
        ''' </remarks>
        Public Property gamma As Double = 0.5

        ''' <summary>
        ''' Zeroeth coefficient in kernel function (default 0)
        ''' </summary>
        Public Property coefficient0 As Double

        ''' <summary>
        ''' Cache memory size in MB (default 100)
        ''' </summary>
        Public Property cacheSize As Double

        ''' <summary>
        ''' Tolerance of termination criterion (default 0.001)
        ''' </summary>
        Public Property EPS As Double

        ''' <summary>
        ''' The parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)
        ''' </summary>
        Public Property c As Double

        ''' <summary>
        ''' <see cref="ColorClass.name"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property weights As Dictionary(Of Integer, Double)
            Get
                Return m_Weights
            End Get
            Private Set
                m_Weights = Value
            End Set
        End Property

        ''' <summary>
        ''' The parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)
        ''' </summary>
        Public Property nu As Double

        ''' <summary>
        ''' The epsilon in loss function of epsilon-SVR (default 0.1)
        ''' </summary>
        Public Property P As Double

        ''' <summary>
        ''' Whether to use the shrinking heuristics, (default True)
        ''' </summary>
        Public Property shrinking As Boolean

        ''' <summary>
        ''' Whether to train an SVC or SVR model for probability estimates, (default False)
        ''' </summary>
        Public Property probability As Boolean

        ''' <summary>
        ''' Default Constructor.  Gives good default values to all parameters.
        ''' </summary>
        Public Sub New()
            svmType = SvmType.C_SVC
            kernelType = KernelType.RBF
            degree = 3
            gamma = 0.5
            coefficient0 = 0
            nu = 0.5
            cacheSize = 40
            c = 1
            EPS = 0.001
            P = 0.1
            shrinking = True
            probability = False
            weights = New Dictionary(Of Integer, Double)()
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other As Parameter = TryCast(obj, Parameter)

            If other Is Nothing Then
                Return False
            End If

            Return other.c = c AndAlso
                other.cacheSize = cacheSize AndAlso
                other.coefficient0 = coefficient0 AndAlso
                other.degree = degree AndAlso
                other.EPS = EPS AndAlso
                other.gamma = gamma AndAlso
                other.kernelType = kernelType AndAlso
                other.nu = nu AndAlso
                other.P = P AndAlso
                other.probability = probability AndAlso
                other.shrinking = shrinking AndAlso
                other.svmType = svmType AndAlso
                other.weights.ToArray().IsEqual(weights.ToArray())
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return c.GetHashCode() +
                cacheSize.GetHashCode() +
                coefficient0.GetHashCode() +
                degree.GetHashCode() +
                EPS.GetHashCode() +
                gamma.GetHashCode() +
                kernelType.GetHashCode() +
                nu.GetHashCode() +
                P.GetHashCode() +
                probability.GetHashCode() +
                shrinking.GetHashCode() +
                svmType.GetHashCode() +
                weights.ToArray().ComputeHashcode()
        End Function

#Region "ICloneable Members"

        ''' <summary>
        ''' Creates a memberwise clone of this parameters object.
        ''' </summary>
        ''' <returns>The clone (as type Parameter)</returns>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Parameter With {
                .c = c,
                .cacheSize = cacheSize,
                .coefficient0 = coefficient0,
                .degree = degree,
                .EPS = EPS,
                .gamma = gamma,
                .kernelType = kernelType,
                .nu = nu,
                .P = P,
                .probability = probability,
                .shrinking = shrinking,
                .svmType = svmType,
                .weights = New Dictionary(Of Integer, Double)(weights)
            }
        End Function

#End Region
    End Class
End Namespace
