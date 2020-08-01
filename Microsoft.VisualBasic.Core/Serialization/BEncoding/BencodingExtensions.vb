#Region "Microsoft.VisualBasic::e6097d5ad448fbd355dd78a9bf126127, Microsoft.VisualBasic.Core\Serialization\BEncoding\BencodingExtensions.vb"

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

    '     Module BencodingExtensions
    ' 
    '         Function: BDecode, encodeList, encodeObject, theSameObject, ToBEncode
    '                   ToBEncodeString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
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
        Public Function BDecode(ByVal s As String) As BElement()
            Return Decode(s)
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

        <Extension>
        Public Function ToBEncode(obj As Object, digest As Func(Of Object, Object)) As BElement
            Dim type As Type

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
                    table.Add(New BString(key.ToString), ToBEncode(item, digest))
                Next

                Return table
            ElseIf type Is GetType(String) OrElse type Is GetType(Double) OrElse type Is GetType(Boolean) OrElse type Is GetType(Date) Then
                Return New BString(obj.ToString)
            ElseIf type Is GetType(Integer) OrElse type Is GetType(Long) OrElse type Is GetType(Short) OrElse type Is GetType(Byte) Then
                Return New BInteger(obj)
            ElseIf type.ImplementInterface(GetType(IList)) Then
                Return DirectCast(obj, IList).encodeList(digest)
            Else
                Return encodeObject(obj, digest)
            End If
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
