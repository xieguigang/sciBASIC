Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Emit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus

Namespace MonteCarlo

    Public Class VariableModel : Inherits DoubleRange
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

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New VariableModel(Min, Max) With {
                .Name = Name
            }
        End Function

        Public Function GetRandomModel() As NamedValue(Of INextRandomNumber)
            Return New NamedValue(Of INextRandomNumber) With {
                .Name = Name,
                .x = AddressOf GetValue
            }
        End Function

        Public Overrides Function ToString() As String
            Return Name & " --> " & "{ min:=" & Min & ", max:=" & Max & " }"
        End Function
    End Class
End Namespace