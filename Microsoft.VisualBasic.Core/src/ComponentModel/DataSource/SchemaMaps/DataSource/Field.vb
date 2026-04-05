Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' <see cref="DataFrameColumnAttribute"/>属性的别称
    ''' </summary>
    Public Class Field : Inherits DataFrameColumnAttribute

        ''' <summary>
        ''' Initializes a new instance by name.
        ''' </summary>
        ''' <param name="FieldName">The name.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(fieldName As String)
            Call MyBase.New(fieldName)
        End Sub

        Sub New(ordinal As Integer)
            Call MyBase.New(ordinal)
        End Sub
    End Class
End Namespace