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
            Return GetType(T).ToBEncode(obj, digest Or theSampleObjectDigest).ToBencodedString
        End Function

        ReadOnly theSampleObjectDigest As New [Default](Of Func(Of Object, Object))(AddressOf theSameObject)

        Private Function theSameObject(obj As Object) As Object
            Return obj
        End Function

        <Extension>
        Public Function ToBEncode(type As Type, obj As Object, digest As Func(Of Object, Object)) As BElement
            If type.IsArray Then
                Return DirectCast(obj, Array).encodeList(type, digest)
            ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                Dim table As New BDictionary
                Dim raw As IDictionary = DirectCast(obj, IDictionary)
                Dim item As Object

                For Each key As Object In raw.Keys
                    item = raw.Item(key)
                    table.Add(New BString(key.ToString), item.GetType.ToBEncode(item, digest))
                Next

                Return table
            ElseIf type Is GetType(String) OrElse type Is GetType(Double) OrElse type Is GetType(Boolean) OrElse type Is GetType(Date) Then
                Return New BString(obj.ToString)
            ElseIf type Is GetType(Integer) OrElse type Is GetType(Long) OrElse type Is GetType(Short) OrElse type Is GetType(Byte) Then
                Return New BInteger(obj)
            ElseIf type.ImplementInterface(GetType(IList)) Then
                Return DirectCast(obj, IList).encodeList(Nothing, digest)
            Else
                Return encodeObject(digest(obj), digest)
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
                table.Add(New BString(reader.Name), item.GetType.ToBEncode(item, digest))
            Next

            Return table
        End Function

        <Extension>
        Private Function encodeList(sequence As IEnumerable, type As Type, digest As Func(Of Object, Object)) As BElement
            Dim list As New BList

            type = type.GetElementType

            If type Is GetType(Object) Then
                type = Nothing
            End If

            For Each item As Object In sequence
                If type Is Nothing Then
                    Call list.Add(item.GetType.ToBEncode(item, digest))
                Else
                    Call list.Add(type.ToBEncode(item, digest))
                End If
            Next

            Return list
        End Function
    End Module
End Namespace