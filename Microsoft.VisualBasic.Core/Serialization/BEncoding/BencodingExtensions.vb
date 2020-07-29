Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates

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
        Public Function ToBEncodeString(Of T)(obj As T) As String
            Return GetType(T).ToBEncode(obj).ToBencodedString
        End Function

        <Extension>
        Public Function ToBEncode(type As Type, obj As Object) As BElement
            If type.IsArray Then
                Return DirectCast(obj, Array).encodeList(type)
            ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                Dim table As New BDictionary
                Dim raw As IDictionary = DirectCast(obj, IDictionary)
                Dim item As Object

                For Each key As Object In raw.Keys
                    item = raw.Item(key)
                    table.Add(New BString(key.ToString), item.GetType.ToBEncode(item))
                Next

                Return table
            ElseIf type Is GetType(String) OrElse type Is GetType(Double) OrElse type Is GetType(Boolean) OrElse type Is GetType(Date) Then
                Return New BString(obj.ToString)
            ElseIf type Is GetType(Integer) OrElse type Is GetType(Long) OrElse type Is GetType(Short) OrElse type Is GetType(Byte) Then
                Return New BInteger(obj)
            ElseIf type.ImplementInterface(GetType(IList)) Then
                Return DirectCast(obj, IList).encodeList(Nothing)
            Else
                Dim schema As PropertyInfo() = type _
                    .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                    .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
                    .ToArray
                Dim table As New BDictionary
                Dim item As Object

                For Each reader As PropertyInfo In schema
                    item = reader.GetValue(obj, Nothing)
                    table.Add(New BString(reader.Name), item.GetType.ToBEncode(item))
                Next

                Return table
            End If
        End Function

        <Extension>
        Private Function encodeList(sequence As IEnumerable, type As Type) As BElement
            Dim list As New BList

            type = type.GetElementType

            If type Is GetType(Object) Then
                type = Nothing
            End If

            For Each item As Object In sequence
                If type Is Nothing Then
                    Call list.Add(item.GetType.ToBEncode(item))
                Else
                    Call list.Add(type.ToBEncode(item))
                End If
            Next

            Return list
        End Function
    End Module
End Namespace