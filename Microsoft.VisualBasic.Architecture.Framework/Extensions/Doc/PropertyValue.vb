Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Interface IPropertyValue : Inherits INamedValue, Value(Of String).IValueOf
    Property [Property] As String
End Interface

Public Class PropertyValue
    Implements INamedValue
    Implements IPropertyValue

    Public Property Key As String Implements IKeyedEntity(Of String).Key
    Public Property [Property] As String Implements IPropertyValue.Property
    Public Property Value As String Implements IPropertyValue.value

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    ''' <summary>
    ''' Imports the tsv file like:
    ''' 
    ''' ```
    ''' &lt;ID>&lt;tab>&lt;PropertyName>&lt;tab>&lt;Value>
    ''' ```
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    Public Shared Function ImportsTsv(path$, Optional header As Boolean = True) As PropertyValue()
        Dim lines$() = path.ReadAllLines

        If header Then
            lines = lines.Skip(1).ToArray
        End If

        Return ImportsLines(
            data:=lines,
            delimiter:=ASCII.TAB)
    End Function

    Public Shared Function ImportsLines(data As IEnumerable(Of String), Optional delimiter As Char = ASCII.TAB) As PropertyValue()
        Return data _
            .Select(Function(t) t.Split(delimiter)) _
            .Select(Function(row) New PropertyValue With {
                .Key = row(0),
                .Property = row(1),
                .Value = row(2)
            }).ToArray
    End Function
End Class
