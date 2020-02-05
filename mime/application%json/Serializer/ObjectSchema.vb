Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates

Friend Class ObjectSchema

    Public ReadOnly addMethod As MethodInfo
    Public ReadOnly isTable As Boolean
    Public ReadOnly writers As IReadOnlyDictionary(Of String, PropertyInfo)
    ''' <summary>
    ''' Value type of the dictionary
    ''' </summary>
    Public ReadOnly valueType As Type
    Public ReadOnly raw As Type

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="addMethod"></param>
    ''' <param name="isTable"></param>
    ''' <param name="writers"></param>
    ''' <param name="valueType"></param>
    Private Sub New(addMethod As MethodInfo,
                    isTable As Boolean,
                    writers As IReadOnlyDictionary(Of String, PropertyInfo),
                    valueType As Type,
                    raw As Type)

        Me.addMethod = addMethod
        Me.isTable = isTable
        Me.writers = writers
        Me.valueType = valueType
        Me.raw = raw
    End Sub

    Public Shared Function GetSchema(type As Type) As ObjectSchema
        Static cache As New Dictionary(Of Type, ObjectSchema)
        Return cache.ComputeIfAbsent(key:=type, lazyValue:=AddressOf CreateSchema)
    End Function

    Private Shared Function CreateSchema(schema As Type) As ObjectSchema
        Dim isTable As Boolean = schema.IsInheritsFrom(GetType(DictionaryBase)) OrElse schema.ImplementInterface(GetType(IDictionary))
        Dim writers = schema.Schema(PropertyAccess.Writeable, PublicProperty, nonIndex:=True)
        Dim addMethod As MethodInfo = schema.GetMethods _
            .Where(Function(m)
                       Dim params = m.GetParameters

                       Return Not m.IsStatic AndAlso
                           Not params.IsNullOrEmpty AndAlso
                           params.Length = 2 AndAlso
                           m.Name = "Add"
                   End Function) _
            .FirstOrDefault
        Dim valueType As Type = Nothing

        If isTable Then
            With schema.GetGenericArguments
                If .Length = 1 Then
                    valueType = .GetValue(Scan0)
                Else
                    valueType = .GetValue(1)
                End If
            End With
        End If

        Return New ObjectSchema(
            raw:=schema,
            addMethod:=addMethod,
            writers:=writers,
            isTable:=isTable,
            valueType:=valueType
        )
    End Function

    Public Overrides Function ToString() As String
        Return $"{raw.Namespace}::{raw.Name}"
    End Function

End Class
