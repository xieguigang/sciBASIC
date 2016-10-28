Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ValueVector : Inherits int

    Public Property Y As Dictionary(Of NamedValue(Of Double()))

    Default Public Overloads Property Value(name$) As Double
        Get
            Return Y(name).x(MyBase.value)
        End Get
        Set(value As Double)
            Y(name$).x(MyBase.value) = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return $"[{value}] " & Y.Keys.ToArray.GetJson
    End Function

    ''' <summary>
    ''' Move pointer value
    ''' </summary>
    ''' <param name="v"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Overloads Shared Operator +(v As ValueVector, n%) As ValueVector
        v.value += n
        Return v
    End Operator
End Class
