Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' 用于读写tsv/XML文件格式的键值对数据
    ''' </summary>
    Public Class PropertyValue
        Implements INamedValue, IPropertyValue

        ''' <summary>
        ''' ID
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Key As String Implements IKeyedEntity(Of String).Key
        ''' <summary>
        ''' Property Name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [Property] As String Implements IPropertyValue.Property
        ''' <summary>
        ''' Value text
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property Value As String Implements IPropertyValue.Value

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
End Namespace