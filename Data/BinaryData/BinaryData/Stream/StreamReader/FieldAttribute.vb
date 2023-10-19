Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Public Class FieldAttribute : Inherits Field

    Public Property N As Integer

    Public ReadOnly Property ReadArray As Boolean
        Get
            Return N > 0
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ordinal">The ordinal position in the rawdata layout</param>
    ''' <param name="n">for array used only, means array length to read, default negative means scalar</param>
    Sub New(ordinal As Integer, Optional n As Integer = -1)
        Call MyBase.New(ordinal)
        Me.N = n
    End Sub

    Public Function Read(buf As BinaryDataReader, p As PropertyInfo) As Object
        Dim type As Type = p.PropertyType
        Dim code As TypeCode = Type.GetTypeCode(type)

        If type.IsArray Then
        Else

        End If
    End Function
End Class