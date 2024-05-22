#Region "Microsoft.VisualBasic::ced27b770225ef4247666144a2b8fdb1, Microsoft.VisualBasic.Core\src\Serialization\JSON\AnonymousJSON.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 99
    '    Code Lines: 48 (48.48%)
    ' Comment Lines: 40 (40.40%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 11 (11.11%)
    '     File Size: 3.87 KB


    '     Module AnonymousJSONExtensions
    ' 
    '         Function: AnonymousJSON, (+4 Overloads) GetJson, KeysJson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Serialization.JSON

    ''' <summary>
    ''' Extension helpers for deal with the anonymous type
    ''' </summary>
    ''' 
    <HideModuleName>
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
        ''' get string array json
        ''' </summary>
        ''' <param name="array">a string collection, this function will convert this
        ''' enumerable collection object as array and then serialize as json.
        ''' </param>
        ''' <param name="indent"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 专门为任意字符串集合所创建的json序列化方法，在这个函数之中会自动调用ToArray后再进行json序列化
        ''' </remarks>
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
