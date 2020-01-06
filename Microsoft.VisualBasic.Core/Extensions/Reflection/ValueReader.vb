Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Module ValueReader

    ''' <summary>
    ''' 只对属性有效，出错会返回空值
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("GetValue")>
    <Extension> Public Function GetValue(Type As Type, obj As Object, Name As String) As Object
        Try
            Return getValueInternal(Type, obj, Name)
        Catch ex As Exception
            Return App.LogException(ex, $"{GetType(Extensions).FullName}::{NameOf(GetValue)}")
        End Try
    End Function

    Private Function getValueInternal(type As Type, obj As Object, Name As String) As Object
        Dim [property] = type.GetProperty(Name, PublicProperty)

        If [property] Is Nothing Then
            Return Nothing
        Else
            Dim value = [property].GetValue(obj, Nothing)
            Return value
        End If
    End Function

    ''' <summary>
    ''' 只对属性有效，出错会返回空值
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    <Extension> Public Function GetValue(Of T)(Type As Type, obj As Object, Name As String) As T
        Dim value = Type.GetValue(obj, Name)
        If value Is Nothing Then
            Return Nothing
        End If
        Dim cast As T = DirectCast(value, T)
        Return cast
    End Function
End Module
