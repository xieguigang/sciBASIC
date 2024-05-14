#Region "Microsoft.VisualBasic::44d970541f46228caec7693e170af01d, Data\DataFrame.Extensions\Property\Properties.vb"

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

    '   Total Lines: 413
    '    Code Lines: 178
    ' Comment Lines: 192
    '   Blank Lines: 43
    '     File Size: 26.23 KB


    '     Class Properties
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, GetEnumerator1, (+2 Overloads) getProperty, Load, propertyNames
    '                   setProperty, stringPropertyNames
    ' 
    '         Sub: (+2 Overloads) Dispose, (+2 Overloads) list, (+2 Overloads) load, loadFromXML, save
    '              (+2 Overloads) store, (+2 Overloads) storeToXML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace IO.Properties

    ''' <summary>
    ''' The Properties class represents a persistent set of properties. The Properties can be saved to a stream or loaded 
    ''' from a stream. Each key and its corresponding value in the property list is a string.
    ''' A property list can contain another property list as its "defaults"; this second property list is searched if the 
    ''' property key is not found in the original property list.
    ''' Because Properties inherits from Hashtable, the put and putAll methods can be applied to a Properties object. Their 
    ''' use is strongly discouraged as they allow the caller to insert entries whose keys or values are not Strings. The 
    ''' setProperty method should be used instead. If the store or save method is called on a "compromised" Properties 
    ''' object that contains a non-String key or value, the call will fail. Similarly, the call to the propertyNames or 
    ''' list method will fail if it is called on a "compromised" Properties object that contains a non-String key.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Properties : Implements IDisposable
        Implements IEnumerable(Of KeyValuePair(Of String, String))

#Region "Field Detail"
        ''' <summary>
        ''' A property list that contains default values for any keys not found in this property list.
        ''' </summary>
        ''' <remarks></remarks>
        Protected defaults As Properties
        Protected _fileName As String
        Protected _innerTable As New Dictionary(Of String, Object)

        Default Public Property [property](keyName As String) As String
            Get
                Return getProperty(keyName)
            End Get
            Set(value As String)
                Call setProperty(keyName, value)
            End Set
        End Property
#End Region

#Region "Constructor Detail"
        ''' <summary>
        ''' Creates an empty property list with no default values.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates an empty property list with the specified defaults.
        ''' </summary>
        ''' <param name="defaults">defaults - the defaults.</param>
        ''' <remarks></remarks>
        Public Sub New(defaults As Properties)
        End Sub

        Public Shared Function Load(fileName As String) As Properties
            Dim Reader = fileName.OpenReader
            Dim Properties As Properties = New Properties
            Call Properties.Load(Reader)
            Return Properties
        End Function
#End Region

#Region "Method Detail"
        ''' <summary>
        ''' Calls the Hashtable method put. Provided for parallelism with the getProperty method. Enforces use of strings for property keys and values. The value returned is the result of the Hashtable call to put.
        ''' </summary>
        ''' <param name="key">the key to be placed into this property list.</param>
        ''' <param name="value">the value corresponding to key.</param>
        ''' <returns>the previous value of the specified key in this property list, or null if it did not have one.</returns>
        ''' <remarks></remarks>
        Public Function setProperty(key As String, value As String) As Object
            If _innerTable.ContainsKey(key) Then
                Dim previousValue As String = _innerTable(key)
                _innerTable(key) = value
                Return previousValue
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Reads a property list (key and element pairs) from the input character stream in a simple line-oriented format.
        ''' Properties are processed in terms of lines. There are two kinds of line, natural lines and logical lines. A natural line is defined as a line of characters that is terminated either by a set of line terminator characters (\n or \r or \r\n) or by the end of the stream. A natural line may be either a blank line, a comment line, or hold all or some of a key-element pair. A logical line holds all the data of a key-element pair, which may be spread out across several adjacent natural lines by escaping the line terminator sequence with a backslash character \. Note that a comment line cannot be extended in this manner; every natural line that is a comment must have its own comment indicator, as described below. Lines are read from input until the end of the stream is reached.
        ''' A natural line that contains only white space characters is considered blank and is ignored. A comment line has an ASCII '#' or '!' as its first non-white space character; comment lines are also ignored and do not encode key-element information. In addition to line terminators, this format considers the characters space (' ', '\u0020'), tab ('\t', '\u0009'), and form feed ('\f', '\u000C') to be white space.
        ''' If a logical line is spread across several natural lines, the backslash escaping the line terminator sequence, the line terminator sequence, and any white space at the start of the following line have no affect on the key or element values. The remainder of the discussion of key and element parsing (when loading) will assume all the characters constituting the key and element appear on a single natural line after line continuation characters have been removed. Note that it is not sufficient to only examine the character preceding a line terminator sequence to decide if the line terminator is escaped; there must be an odd number of contiguous backslashes for the line terminator to be escaped. Since the input is processed from left to right, a non-zero even number of 2n contiguous backslashes before a line terminator (or elsewhere) encodes n backslashes after escape processing.
        ''' The key contains all of the characters in the line starting with the first non-white space character and up to, but not including, the first unescaped '=', ':', or white space character other than a line terminator. All of these key termination characters may be included in the key by escaping them with a preceding backslash character; for example,
        ''' \:\=
        ''' would be the two-character key ":=". Line terminator characters can be included using \r and \n escape sequences. Any white space after the key is skipped; if the first non-white space character after the key is '=' or ':', then it is ignored and any white space characters after it are also skipped. All remaining characters on the line become part of the associated element string; if there are no remaining characters, the element is the empty string "". Once the raw character sequences constituting the key and element are identified, escape processing is performed as described above.
        ''' As an example, each of the following three lines specifies the key "Truth" and the associated element value "Beauty":
        '''             Truth = Beauty
        ''' Truth:      Beauty()
        ''' Truth:      Beauty()
        ''' As another example, the following three lines specify a single property:
        ''' fruits                           apple, banana, pear, \
        '''                          cantaloupe, watermelon, \
        '''                  kiwi, mango
        ''' 
        ''' The key is "fruits" and the associated element is:
        ''' "apple, banana, pear, cantaloupe, watermelon, kiwi, mango"
        ''' Note that a space appears before each \ so that a space will appear after each comma in the final result; the \, line terminator, and leading white space on the continuation line are merely discarded and are not replaced by one or more other characters.
        ''' As a third example, the line:
        ''' 
        '''        cheeses()
        ''' 
        ''' specifies that the key is "cheeses" and the associated element is the empty string "".
        ''' Characters in keys and elements can be represented in escape sequences similar to those used for character and string literals (see §3.3 and §3.10.6 of the Java Language Specification). The differences from the character escape sequences and Unicode escapes used for characters and strings are:
        ''' 
        ''' Octal escapes are not recognized.
        ''' The character sequence \b does not represent a backspace character.
        ''' The method does not treat a backslash character, \, before a non-valid escape character as an error; the backslash is silently dropped. For example, in a Java string the sequence "\z" would cause a compile time error. In contrast, this method silently drops the backslash. Therefore, this method treats the two character sequence "\b" as equivalent to the single character 'b'.
        ''' Escapes are not necessary for single and double quotes; however, by the rule above, single and double quote characters preceded by a backslash still yield single and double quote characters, respectively.
        ''' Only a single 'u' character is allowed in a Uniocde escape sequence.
        ''' The specified stream remains open after this method returns.
        ''' </summary>
        ''' <param name="reader">the input character stream.</param>
        ''' <remarks></remarks>
        Public Sub load(reader As StreamReader)
            Dim strLines As String() = reader.ReadToEnd.LineTokens
            Dim propertyDatas As KeyValuePair(Of String, String)() = (From strLine As String In strLines.AsParallel
                                                                      Let strItem As String = strLine.Trim
                                                                      Where Not String.IsNullOrEmpty(strItem) AndAlso Not strItem.First = "#"c
                                                                      Let p = InStr(strItem, "=")
                                                                      Let key As String = Mid(strItem, 1, p - 1)
                                                                      Let value As String = Mid(strItem, p + 1)
                                                                      Select New KeyValuePair(Of String, String)(key, value)).ToArray
            For Each item As KeyValuePair(Of String, String) In propertyDatas
                Call Me._innerTable.Add(item.Key, item.Value)
            Next
        End Sub

        ''' <summary>
        ''' Reads a property list (key and element pairs) from the input byte stream. The input stream is in a simple line-oriented format as specified in load(Reader) and is assumed to use the ISO 8859-1 character encoding; that is each byte is one Latin1 character. Characters not in Latin1, and certain special characters, are represented in keys and elements using Unicode escapes.
        ''' The specified stream remains open after this method returns.
        ''' </summary>
        ''' <param name="inStream">the input stream.</param>
        ''' <remarks></remarks>
        Public Sub load(inStream As Stream)
            Call Load(New StreamReader(inStream))
        End Sub

        ''' <summary>
        ''' Deprecated. This method does not throw an IOException if an I/O error occurs while saving the property list. The preferred way to save a properties list is via the store(OutputStream out, String comments) method or the storeToXML(OutputStream os, String comment) method.
        ''' Calls the store(OutputStream out, String comments) method and suppresses IOExceptions that were thrown.
        ''' </summary>
        ''' <param name="out">an output stream.</param>
        ''' <param name="comments">a description of the property list.</param>
        ''' <remarks></remarks>
        Public Sub save(out As Stream, comments As String)
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            If Not String.IsNullOrEmpty(comments) Then
                Dim Tokens As String() = Strings.Split(comments, vbLf)
                For Each strItem In Tokens
                    Call sBuilder.AppendLine("# " & strItem)
                Next
            End If

            For Each itemKey In _innerTable.Keys
                Call sBuilder.AppendLine(String.Format("{0}={1}", itemKey.ToString, _innerTable(itemKey).ToString))
            Next

            Dim s As New StreamWriter(out)
            Call s.WriteLine(sBuilder)
            Call s.Flush()
        End Sub

        ''' <summary>
        ''' Writes this property list (key and element pairs) in this Properties table to the output character stream in a format suitable for using the load(Reader) method.
        ''' Properties from the defaults table of this Properties table (if any) are not written out by this method.
        ''' 
        ''' If the comments argument is not null, then an ASCII # character, the comments string, and a line separator are first written to the output stream. Thus, the comments can serve as an identifying comment. Any one of a line feed ('\n'), a carriage return ('\r'), or a carriage return followed immediately by a line feed in comments is replaced by a line separator generated by the Writer and if the next character in comments is not character # or character ! then an ASCII # is written out after that line separator.
        ''' 
        ''' Next, a comment line is always written, consisting of an ASCII # character, the current date and time (as if produced by the toString method of Date for the current time), and a line separator as generated by the Writer.
        ''' 
        ''' Then every entry in this Properties table is written out, one per line. For each entry the key string is written, then an ASCII =, then the associated element string. For the key, all space characters are written with a preceding \ character. For the element, leading space characters, but not embedded or trailing space characters, are written with a preceding \ character. The key and element characters #, !, =, and : are written with a preceding backslash to ensure that they are properly loaded.
        ''' 
        ''' After the entries have been written, the output stream is flushed. The output stream remains open after this method returns.
        ''' </summary>
        ''' <param name="writer">an output character stream writer.</param>
        ''' <param name="comments">a description of the property list.</param>
        ''' <remarks></remarks>
        Public Sub store(writer As TextWriter, comments As String)
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            If Not String.IsNullOrEmpty(comments) Then
                Dim Tokens As String() = Strings.Split(comments, vbLf)
                For Each strItem In Tokens
                    Call sBuilder.AppendLine("# " & strItem)
                Next
            End If

            For Each itemKey In _innerTable.Keys
                Call sBuilder.AppendLine(String.Format("{0}={1}", itemKey.ToString, _innerTable(itemKey).ToString))
            Next

            Call writer.WriteLine(sBuilder.ToString)
            Call writer.Flush()
        End Sub

        ''' <summary>
        ''' Writes this property list (key and element pairs) in this Properties table to the output stream in a format suitable for loading into a Properties table using the load(InputStream) method.
        ''' Properties from the defaults table of this Properties table (if any) are not written out by this method.
        ''' 
        ''' This method outputs the comments, properties keys and values in the same format as specified in store(Writer), with the following differences:
        ''' 
        ''' The stream is written using the ISO 8859-1 character encoding.
        ''' Characters not in Latin-1 in the comments are written as \uxxxx for their appropriate unicode hexadecimal value xxxx.
        ''' Characters less than \u0020 and characters greater than \u007E in property keys or values are written as \uxxxx for the appropriate hexadecimal value xxxx.
        ''' After the entries have been written, the output stream is flushed. The output stream remains open after this method returns.
        ''' </summary>
        ''' <param name="out">an output stream.</param>
        ''' <param name="comments">a description of the property list.</param>
        ''' <remarks></remarks>
        Public Sub store(out As Stream, comments As String)
            Dim sb As New StringBuilder(1024)

            If Not String.IsNullOrEmpty(comments) Then
                Dim Tokens As String() = Strings.Split(comments, vbLf)
                For Each strItem In Tokens
                    Call sb.AppendLine("# " & strItem)
                Next
            End If

            For Each itemKey In _innerTable.Keys
                Call sb.AppendLine(String.Format("{0}={1}", itemKey.ToString, _innerTable(itemKey).ToString))
            Next

            Dim s As New StreamWriter(out)
            Call s.WriteLine(sb.ToString)
            Call s.Flush()
        End Sub

        ''' <summary>
        ''' Loads all of the properties represented by the XML document on the specified input stream into this properties table.
        ''' The XML document must have the following DOCTYPE declaration:
        ''' 
        ''' !DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"
        ''' 
        ''' Furthermore, the document must satisfy the properties DTD described above.
        ''' The specified stream is closed after this method returns.
        ''' </summary>
        ''' <param name="in">the input stream from which to read the XML document.</param>
        ''' <remarks></remarks>
        Public Sub loadFromXML([in] As Stream)

        End Sub

        ''' <summary>
        ''' Emits an XML document representing all of the properties contained in this table.
        ''' An invocation of this method of the form props.storeToXML(os, comment) behaves in exactly the same way as the invocation props.storeToXML(os, comment, "UTF-8");.
        ''' </summary>
        ''' <param name="os">the output stream on which to emit the XML document.</param>
        ''' <param name="comment">a description of the property list, or null if no comment is desired.</param>
        ''' <remarks></remarks>
        Public Sub storeToXML(os As Stream, comment As String)
            Dim Xml = (From itemKey In _innerTable
                       Select New KeyValuePair With {
                           .Key = itemKey.ToString,
                           .Value = Scripting.ToString(itemKey.Value)}).ToArray.GetXml

            If Not String.IsNullOrEmpty(comment) Then
                Dim xmlDocument As Xml.XmlDocument = New Xml.XmlDocument()
                xmlDocument.LoadXml(Xml)
                Call xmlDocument.CreateComment(comment)

            Else
                Dim s As New StreamWriter(os)
                Call s.WriteLine(Xml)
                Call s.Flush()
            End If
        End Sub

        ''' <summary>
        ''' Emits an XML document representing all of the properties contained in this table, using the specified encoding.
        ''' The XML document will have the following DOCTYPE declaration:
        ''' 
        ''' !DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"
        ''' 
        ''' If the specified comment is null then no comment will be stored in the document.
        ''' 
        ''' The specified stream remains open after this method returns.
        ''' </summary>
        ''' <param name="os">the output stream on which to emit the XML document.</param>
        ''' <param name="comment">a description of the property list, or null if no comment is desired.</param>
        ''' <param name="encoding"></param>
        ''' <remarks></remarks>
        Public Sub storeToXML(os As Stream, comment As String, encoding As String)

        End Sub

        ''' <summary>
        ''' Searches for the property with the specified key in this property list. If the key is not found in this property list, the default property list, and its defaults, recursively, are then checked. The method returns null if the property is not found.
        ''' </summary>
        ''' <param name="key">the property key.</param>
        ''' <returns>the value in this property list with the specified key value.</returns>
        ''' <remarks></remarks>
        Public Function getProperty(key As String) As String
            If _innerTable.ContainsKey(key) Then
                Return _innerTable(key).ToString
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Searches for the property with the specified key in this property list. If the key is not found in this property list, the default property list, 
        ''' and its defaults, recursively, are then checked. The method returns the default value argument if the property is not found.
        ''' </summary>
        ''' <param name="key">the hashtable key.</param>
        ''' <param name="defaultValue">a default value.</param>
        ''' <returns>the value in this property list with the specified key value.</returns>
        ''' <remarks></remarks>
        Public Function getProperty(key As String, defaultValue As String) As String
            If _innerTable.ContainsKey(key) Then
                Return _innerTable(key).ToString
            Else
                Return defaultValue
            End If
        End Function

        ''' <summary>
        ''' Returns an enumeration of all the keys in this property list, including distinct keys in the default property list if a key of the same name has not 
        ''' already been found from the main properties list.
        ''' </summary>
        ''' <returns>an enumeration of all the keys in this property list, including the keys in the default property list.</returns>
        ''' <remarks></remarks>
        Public Function propertyNames() As String()
            Return _innerTable.Keys.ToArray
        End Function

        ''' <summary>
        ''' Returns a set of keys in this property list where the key and its corresponding value are strings, including distinct keys in the default property list 
        ''' if a key of the same name has not already been found from the main properties list. Properties whose key or value is not of type String are omitted.
        ''' The returned set is not backed by the Properties object. Changes to this Properties are not reflected in the set, or vice versa.
        ''' </summary>
        ''' <returns>a set of keys in this property list where the key and its corresponding value are strings, including the keys in the default property list.</returns>
        ''' <remarks></remarks>
        Public Function stringPropertyNames() As String()
            Return propertyNames()
        End Function

        ''' <summary>
        ''' Prints this property list out to the specified output stream. This method is useful for debugging.
        ''' </summary>
        ''' <param name="out">an output stream.</param>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub list(out As Stream)
            Call list(New StreamWriter(out))
        End Sub

        ''' <summary>
        ''' Prints this property list out to the specified output stream. This method is useful for debugging.
        ''' </summary>
        ''' <param name="out">an output stream.</param>
        ''' <remarks></remarks>
        Public Sub list(out As TextWriter)
            For Each keyItem As String In _innerTable.Keys
                Call out.WriteLine(String.Format("{0} = {1}", keyItem.ToString, _innerTable(keyItem).ToString))
            Next
        End Sub
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。

                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String)) Implements IEnumerable(Of KeyValuePair(Of String, String)).GetEnumerator
            For Each x In (From key As String
                           In _innerTable.Keys
                           Let value As Object = _innerTable(key)
                           Select New KeyValuePair(Of String, String)(key, Scripting.ToString(value)))
                Yield x
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
