Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The value object collection that have a name string.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedCollection(Of T) : Implements INamedValue
        Implements IKeyValuePairObject(Of String, T())
        Implements Value(Of T()).IValueOf

        Public Property Name As String Implements IKeyedEntity(Of String).Key, IKeyValuePairObject(Of String, T()).Identifier
        Public Property Value As T() Implements IKeyValuePairObject(Of String, T()).Value, Value(Of T()).IValueOf.value
        Public Property Description As String

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Name Is Nothing AndAlso Value Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">名称属性<see cref="NamedValue(Of T).Name"/></param>必须是相同的
        Sub New(source As IEnumerable(Of NamedValue(Of T)))
            Dim array = source.ToArray

            Name = array(Scan0).Name
            Value = array.Select(Function(x) x.Value).ToArray
            Description = array _
                .Select(Function(x) x.Description) _
                .Where(Function(s) Not s.StringEmpty) _
                .Distinct _
                .JoinBy("; ")
        End Sub

        Public Function GetValues() As NamedValue(Of T)()
            Dim name$ = Me.Name
            Dim describ$ = Description

            Return Value.ToArray(
                Function(v) New NamedValue(Of T) With {
                    .Name = name,
                    .Description = describ,
                    .Value = v
                })
        End Function

        Public Overrides Function ToString() As String
            Return Name & " --> " & Value.GetJson
        End Function
    End Structure
End Namespace