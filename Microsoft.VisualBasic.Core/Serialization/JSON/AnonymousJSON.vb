#Region "Microsoft.VisualBasic::85970f575669d31dd25b44f3643abf94, Microsoft.VisualBasic.Core\Serialization\JSON\AnonymousJSON.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module AnonymousJSONExtensions
    ' 
    '         Function: AnonymousJSON, (+4 Overloads) GetJson, KeysJson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Serialization.JSON

    ''' <summary>
    ''' Extension helpers for deal with the anonymous type
    ''' </summary>
    Public Module AnonymousJSONExtensions

        <Extension>
        Public Function GetJson(obj As String(,)) As String
            With New Dictionary(Of String, String)
                For Each prop As String() In obj.RowIterator
                    Call .Add(prop(0), prop(1))
                Next

                Return .GetJson
            End With
        End Function

        ''' <summary>
        ''' 专门为任意字符串集合所创建的json序列化方法，在这个函数之中会自动调用ToArray后再进行json序列化
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="indent"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetJson(array As IEnumerable(Of String), Optional indent As Boolean = False) As String
            Return array.ToArray.GetJson(indent:=indent)
        End Function

        ''' <summary>
        ''' 专门针对字符串集合的
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <param name="indent"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 任意类型的字符串集合都会被首先转换为字符串数组然后再转换为json字符串
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetJson(keys As Dictionary(Of String, Object).KeyCollection, Optional indent As Boolean = False) As String
            Return GetType(String()).GetObjectJson(keys.ToArray, indent)
        End Function

        ''' <summary>
        ''' 专门针对字符串集合的
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <param name="indent"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 任意类型的字符串集合都会被首先转换为字符串数组然后再转换为json字符串
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetJson(keys As SortedDictionary(Of String, Object).KeyCollection, Optional indent As Boolean = False) As String
            Return GetType(String()).GetObjectJson(keys.ToArray, indent)
        End Function

        <Extension>
        Public Function AnonymousJSON(Of T As Class)(obj As T) As String
            Dim keys = obj.GetType.GetProperties(PublicProperty)

            With New Dictionary(Of String, String)
                For Each key As PropertyInfo In keys
                    Call .Add(key.Name, key.GetValue(obj).ToString)
                Next

                Return .GetJson
            End With
        End Function

        ''' <summary>
        ''' Returns all of the keys in a dictionary in json format
        ''' </summary>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function KeysJson(Of V)(d As Dictionary(Of String, V)) As String
            Return d.Keys.ToArray.GetJson
        End Function
    End Module
End Namespace
