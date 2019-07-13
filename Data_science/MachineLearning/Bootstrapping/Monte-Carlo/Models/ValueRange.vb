#Region "Microsoft.VisualBasic::94d2667eb5789843b210c4047aceabda, Data_science\MachineLearning\Bootstrapping\Monte-Carlo\Models\ValueRange.vb"

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

    '     Class ValueRange
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Clone, GetRandomModel, GetValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus

Namespace MonteCarlo

    ''' <summary>
    ''' Value range of the variable, like <see cref="var"/>
    ''' </summary>
    Public Class ValueRange : Inherits DoubleRange
        Implements ICloneable

        <XmlAttribute>
        Public Property Name As String

        Sub New(min#, max#)
            MyBase.New(min, max)
        End Sub

        Sub New(name$, min#, max#)
            MyBase.New(min, max)
            _Name = name
        End Sub

        Sub New()
        End Sub

        Public Function GetValue() As Double
            Return GetRandom(Min, Max)()
        End Function

        ''' <summary>
        ''' Copy the range and name property value
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ValueRange(Min, Max) With {
                .Name = Name
            }
        End Function

        ''' <summary>
        ''' Ranged random value provider
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRandomModel() As NamedValue(Of IValueProvider)
            Return New NamedValue(Of IValueProvider) With {
                .Name = Name,
                .Value = AddressOf GetValue
            }
        End Function

        Public Overrides Function ToString() As String
            Return Name & " --> " & "{ min:=" & Min & ", max:=" & Max & " }"
        End Function
    End Class
End Namespace
