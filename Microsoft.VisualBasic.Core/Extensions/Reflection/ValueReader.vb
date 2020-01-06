Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Module ValueReader

    ''' <summary>
    ''' 出错会返回空集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TProperty"></typeparam>
    ''' <param name="collection"></param>
    ''' <param name="name">使用System.NameOf()操作符来获取</param>
    ''' <returns></returns>
    <Extension>
    Public Function [Get](Of T, TProperty)(collection As ICollection(Of T), name As String, Optional trimNull As Boolean = True) As TProperty()
        Dim properties = DataFramework.Schema(Of T)(PropertyAccess.Readable, nonIndex:=True)

        If properties.IsNullOrEmpty OrElse Not properties.ContainsKey(name) Then
            Return New TProperty() {}
        End If

        Dim [property] As PropertyInfo = properties(name)
        Dim resultBuffer As TProperty()
        Dim LQuery = From obj As T In collection.AsParallel
                     Let value As Object = [property].GetValue(obj, Nothing)
                     Let cast = If(value Is Nothing, Nothing, DirectCast(value, TProperty))
                     Select cast

        If trimNull Then
            resultBuffer = LQuery _
                .Where(Function(item) Not item Is Nothing) _
                .ToArray
        Else
            resultBuffer = LQuery.ToArray
        End If

        Return resultBuffer
    End Function

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

    <Extension>
    Public Function GetDouble(field As FieldInfo, Optional obj As Object = Nothing) As Double
        Return CType(field.GetValue(obj), Double)
    End Function

    <Extension>
    Public Function GetInt(field As FieldInfo, Optional obj As Object = Nothing) As Integer
        Return CType(field.GetValue(obj), Integer)
    End Function
End Module
