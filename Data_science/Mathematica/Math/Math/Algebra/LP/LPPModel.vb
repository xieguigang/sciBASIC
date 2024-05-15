#Region "Microsoft.VisualBasic::d348c7b105456003ba7aaf1552a70a24, Data_science\Mathematica\Math\Math\Algebra\LP\LPPModel.vb"

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

    '   Total Lines: 130
    '    Code Lines: 93
    ' Comment Lines: 11
    '   Blank Lines: 26
    '     File Size: 4.49 KB


    '     Class LppVariable
    ' 
    '         Properties: coefficient, symbol
    ' 
    '     Class LppEquation
    ' 
    '         Properties: constraintCoefficients, constraintType, constraintValue, symbol
    ' 
    '     Class LPPModel
    ' 
    '         Properties: constraintCoefficients, constraintRightHandSides, constraintTypes, name, objectiveFunctionCoefficients
    '                     objectiveFunctionType, objectiveFunctionValue, variableNames, variables
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ConfigModelName, ConfigSymbols, ParseMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace LinearAlgebra.LinearProgramming

    Public Class LppVariable

        <XmlAttribute> Public Property symbol As String
        <XmlAttribute> Public Property coefficient As Double

    End Class

    Public Class LppEquation

        <XmlAttribute> Public Property symbol As String
        <XmlAttribute> Public Property constraintValue As Double
        <XmlAttribute> Public Property constraintType As String

        <XmlText>
        Public Property constraintCoefficients As String

    End Class

    ''' <summary>
    ''' Linear Programming Model
    ''' </summary>
    Public Class LPPModel : Inherits XmlDataModel

        Public Property objectiveFunctionType As String
        Public Property variables As LppVariable()

        ''' <summary>
        ''' base64 string represented matrix
        ''' </summary>
        ''' <returns></returns>
        <XmlElement>
        Public Property constraintCoefficients As LppEquation()
        Public Property objectiveFunctionValue As Double

        ''' <summary>
        ''' the model name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As NamedValue

        Public ReadOnly Property objectiveFunctionCoefficients As Double()
            Get
                Return variables.Select(Function(v) v.coefficient).ToArray
            End Get
        End Property

        Public ReadOnly Property variableNames As String()
            Get
                Return variables.Select(Function(v) v.symbol).ToArray
            End Get
        End Property

        Public ReadOnly Property constraintRightHandSides As Double()
            Get
                Return constraintCoefficients.Select(Function(v) v.constraintValue).ToArray
            End Get
        End Property

        Public ReadOnly Property constraintTypes As String()
            Get
                Return constraintCoefficients.Select(Function(v) v.constraintType).ToArray
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(matrix As IEnumerable(Of Double()), type As String(), constraints As Double(), symbols As String())
            constraintCoefficients = matrix _
                .Select(Function(v, i)
                            Dim base64 = v _
                                .Select(Function(d) BitConverter.GetBytes(d)) _
                                .IteratesALL _
                                .ToBase64String

                            Return New LppEquation With {
                                .constraintCoefficients = base64,
                                .constraintType = type(i),
                                .constraintValue = constraints(i),
                                .symbol = symbols(i)
                            }
                        End Function) _
                .ToArray
        End Sub

        Public Function ParseMatrix() As Double()()
            Return constraintCoefficients _
                .Select(Function(base64Str)
                            Dim bytes As Byte() = base64Str.constraintCoefficients.Base64RawBytes
                            Dim chunks = bytes.Split(8)

                            Return chunks _
                                .Select(Function(v) BitConverter.ToDouble(v, Scan0)) _
                                .ToArray
                        End Function) _
                .ToArray
        End Function

        Public Function ConfigSymbols(names As String(), value As Double()) As LPPModel
            Me.variables = names _
                .Select(Function(name, i)
                            Return New LppVariable With {
                                .symbol = name,
                                .coefficient = value(i)
                            }
                        End Function) _
                .ToArray

            Return Me
        End Function

        Public Function ConfigModelName(name As String, description As String) As LPPModel
            Me.name = New NamedValue With {
                .name = name,
                .text = description
            }

            Return Me
        End Function

    End Class
End Namespace
