Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' Schema for <see cref="Attribute"/> and its bind <see cref="PropertyInfo"/> object target
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure BindProperty(Of T As Attribute)
        Implements IReadOnlyId

        ''' <summary>
        ''' The property object that bind with its custom attribute <see cref="Column"/> of type <typeparamref name="T"/>
        ''' </summary>
        Public [Property] As PropertyInfo
        Public Column As T

        ''' <summary>
        ''' Gets the type of this property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Type
            Get
                Return [Property].PropertyType
            End Get
        End Property

        ''' <summary>
        ''' The map name or the <see cref="PropertyInfo.Name"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Identity As String Implements IReadOnlyId.Identity
            Get
                Return [Property].Name
            End Get
        End Property

        ''' <summary>
        ''' Is this map data is null on its attribute or property data?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNull As Boolean
            Get
                Return [Property] Is Nothing OrElse Column Is Nothing
            End Get
        End Property

        Sub New(attr As T, prop As PropertyInfo)
            Column = attr
            [Property] = prop
        End Sub

        Public Sub SetValue(obj As Object, value As Object)
            Call [Property].SetValue(obj, value, Nothing)
        End Sub

        Public Function GetValue(x As Object) As Object
            Return [Property].GetValue(x, Nothing)
        End Function

        ''' <summary>
        ''' Display this schema maps in Visualbasic style.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"Dim {[Property].Name} As {[Property].PropertyType.ToString}"
        End Function

        Public Shared Function FromHash(x As KeyValuePair(Of T, PropertyInfo)) As BindProperty(Of T)
            Return New BindProperty(Of T) With {
                .Column = x.Key,
                .Property = x.Value
            }
        End Function
    End Structure
End Namespace