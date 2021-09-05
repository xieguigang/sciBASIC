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

        <XmlAttribute> Public Property constraintValue As Double
        <XmlAttribute> Public Property constraintType As String

        <XmlText>
        Public Property constraintCoefficients As String

    End Class

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

        Sub New(matrix As IEnumerable(Of Double()), type As String(), constraints As Double())
            constraintCoefficients = matrix _
                .Select(Function(v, i)
                            Dim base64 = v _
                                .Select(Function(d) BitConverter.GetBytes(d)) _
                                .IteratesALL _
                                .ToBase64String

                            Return New LppEquation With {
                                .constraintCoefficients = base64,
                                .constraintType = type(i),
                                .constraintValue = constraints(i)
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