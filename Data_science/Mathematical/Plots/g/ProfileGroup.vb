Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' The plot data group
''' </summary>
Public MustInherit Class ProfileGroup

    ''' <summary>
    ''' The color profile of the plot elements
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Property Serials As NamedValue(Of Color)()

    Public Overrides Function ToString() As String
        Return MyClass.GetJson
    End Function
End Class