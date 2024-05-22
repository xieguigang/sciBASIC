#Region "Microsoft.VisualBasic::f17af4f5e6c17eb7842f13a607157478, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BencodingExtensions.vb"

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

    '   Total Lines: 157
    '    Code Lines: 88 (56.05%)
    ' Comment Lines: 48 (30.57%)
    '    - Xml Docs: 85.42%
    ' 
    '   Blank Lines: 21 (13.38%)
    '     File Size: 6.31 KB


    '     Module BencodingExtensions
    ' 
    '         Function: BDecode, encodeList, encodeObject, encodePrimitive, theSameObject
    '                   ToBEncode, ToBEncodeString, ToList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.Default

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A class with extension methods for use with Bencoding.
    ''' </summary>
    <HideModuleName>
    Public Module BencodingExtensions

        ''' <summary>
        ''' Decode the current instance.
        ''' </summary>
        ''' <param name="s">The current instance.</param>
        ''' <returns>The root elements of the decoded string.</returns>
        <Extension()>
        Public Function BDecode(s As String) As BElement()
            Return Decode(s)
        End Function

        ''' <summary>
        ''' cast object to a object array
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToList(b As BElement) As BElement()
            Return DirectCast(b, BList).ToArray
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the element.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToBEncodeString(Of T)(obj As T, Optional digest As Func(Of Object, Object) = Nothing) As String
            Return ToBEncode(obj, digest Or theSampleObjectDigest).ToBencodedString
        End Function

        ReadOnly theSampleObjectDigest As New [Default](Of Func(Of Object, Object))(AddressOf theSameObject)

        Private Function theSameObject(obj As Object) As Object
            Return obj
        End Function

        ''' <summary>
        ''' bencode有4种数据类型:string,integer,list和dictionary。
        ''' 
        ''' string：字符是以这种方式编码的: &lt;字符串长度>:&lt;字符串>。
        ''' 如，"hello"：5:hello
        ''' 
        ''' 2. integer：整数是以这种方式编码的: i&lt;整数>e。
        ''' 
        ''' 如，1234：i1234e
        ''' 
        ''' 3. list：列表是以这种方式编码的: l[数据1][数据2][数据3][…]e。
        ''' 
        ''' 如，["hello","world",1234]
        ''' 1. "hello"编码：5:hello
        ''' 2. "world"编码：5:world
        ''' 3. 1234编码：i1234e
        ''' 4. 最终编码：l5:hello5:worldi1234ee
        ''' 
        ''' 4. dictionary：字典是以这种方式编码的: d[key1][value1][key2][value2][…]e，其中key必须是string而且按照字母顺序排序。
        ''' 
        ''' 如，{"name":"jisen","coin":"btc","balance":1000}
        ''' 1. "name":"jisen"编码：4:name5:jisen
        ''' 2. "coin":"btc"编码：4:coin3:btc
        ''' 3. "balance":1000编码：7:balancei1000e
        ''' 4. 最终编码，按key的字母排序：d7:balancei1000e4:coin3:btc4:name5:jisene
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="digest"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToBEncode(obj As Object, Optional digest As Func(Of Object, Object) = Nothing) As BElement
            Dim type As Type

            digest = If(digest, theSampleObjectDigest.DefaultValue)
            obj = digest(obj)
            type = obj.GetType

            If type.IsArray Then
                Return DirectCast(obj, Array).encodeList(digest)
            ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                Dim table As New BDictionary
                Dim raw As IDictionary = DirectCast(obj, IDictionary)
                Dim item As Object

                For Each key As Object In raw.Keys
                    item = raw.Item(key)

                    If item Is Nothing Then
                        Continue For
                    End If

                    table.Add(New BString(key.ToString), ToBEncode(item, digest))
                Next

                Return table
            ElseIf type.ImplementInterface(GetType(IList)) Then
                Return DirectCast(obj, IList).encodeList(digest)
            ElseIf DataFramework.IsPrimitive(type) Then
                Return encodePrimitive(obj, type)
            Else
                Return encodeObject(obj, digest)
            End If
        End Function

        Private Function encodePrimitive(value As Object, type As Type) As BElement
            Select Case type
                Case GetType(String), GetType(Double), GetType(Boolean), GetType(Date), GetType(Single)
                    Return New BString(value.ToString)
                Case GetType(Integer), GetType(Short), GetType(UShort), GetType(Byte), GetType(SByte)
                    Return New BInteger(CInt(value))
                Case GetType(Long), GetType(UInteger), GetType(ULong)
                    Return New BInteger(CLng(value))
                Case Else
                    Throw New NotImplementedException(type.FullName & ": " & value.ToString)
            End Select
        End Function

        Private Function encodeObject(obj As Object, digest As Func(Of Object, Object)) As BElement
            Dim type As Type = obj.GetType
            Dim table As New BDictionary
            Dim item As Object
            Dim schema As PropertyInfo() = type _
                 .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                 .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
                 .ToArray

            For Each reader As PropertyInfo In schema
                item = reader.GetValue(obj, Nothing)
                table.Add(New BString(reader.Name), ToBEncode(item, digest))
            Next

            Return table
        End Function

        <Extension>
        Private Function encodeList(sequence As IEnumerable, digest As Func(Of Object, Object)) As BElement
            Dim list As New BList

            For Each item As Object In sequence
                Call list.Add(ToBEncode(item, digest))
            Next

            Return list
        End Function
    End Module
End Namespace
