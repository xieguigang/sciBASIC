Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class BarDataSample

    Public Property Tag As String
    Public Property data As Double()

    Public ReadOnly Property StackedSum As Double
        Get
            Return data.Sum
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class BarDataGroup : Inherits ProfileGroup

    ''' <summary>
    ''' 与<see cref="BarDataSample.data"/>里面的数据顺序是一致的
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Property Serials As NamedValue(Of Color)()
    Public Property Samples As BarDataSample()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function FromDistributes(data As IEnumerable(Of Double), Optional base! = 10.0F, Optional color$ = "darkblue") As BarDataGroup
        Dim source = data.Distributes(base!)
        Dim bg As Color = color.ToColor(onFailure:=Drawing.Color.DarkBlue)
        Dim values As New List(Of Double)
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
            From lv As Integer
            In source.Keys
            Select New NamedValue(Of Color) With {
                .Name = lv.ToString,
                .x = bg
            }

        For Each x In serials
            values += source(CInt(x.Name)).value
        Next

        Return New BarDataGroup With {
            .Serials = serials,
            .Samples = {
                New BarDataSample With {
                    .Tag = "Distribution",
                    .data = values
                }
            }
        }
    End Function
End Class

Public MustInherit Class ProfileGroup
    Public Overridable Property Serials As NamedValue(Of Color)()

    Public Overrides Function ToString() As String
        Return MyClass.GetJson
    End Function
End Class