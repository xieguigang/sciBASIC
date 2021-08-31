'-------------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class is used to convert some aspects of the Java String class.
'-------------------------------------------------------------------------------------------
Imports System.Runtime.CompilerServices

Friend Module StringHelperClass
    '----------------------------------------------------------------------------------
    '	This method replaces the Java String.substring method when 'start' is a
    '	method call or calculated value to ensure that 'start' is obtained just once.
    '----------------------------------------------------------------------------------
    <Extension()>
    Friend Function SubstringSpecial(ByVal self As String, ByVal start As Integer, ByVal [end] As Integer) As String
        Return self.Substring(start, [end] - start)
    End Function

    '------------------------------------------------------------------------------------
    '	This method is used to replace calls to the 2-arg Java String.startsWith method.
    '------------------------------------------------------------------------------------
    <Extension()>
    Friend Function StartsWith(ByVal self As String, ByVal prefix As String, ByVal toffset As Integer) As Boolean
        Return self.IndexOf(prefix, toffset, StringComparison.Ordinal) = toffset
    End Function

    '------------------------------------------------------------------------------
    '	This method is used to replace most calls to the Java String.split method.
    '------------------------------------------------------------------------------
    <Extension()>
    Friend Function Split(ByVal self As String, ByVal regexDelimiter As String, ByVal trimTrailingEmptyStrings As Boolean) As String()
        Dim splitArray = RegularExpressions.Regex.Split(self, regexDelimiter)

        If trimTrailingEmptyStrings Then
            If splitArray.Length > 1 Then
                For i = splitArray.Length To 0 + 1 Step -1

                    If splitArray(i - 1).Length > 0 Then
                        If i < splitArray.Length Then Array.Resize(splitArray, i)
                        Exit For
                    End If
                Next
            End If
        End If

        Return splitArray
    End Function

    '-----------------------------------------------------------------------------
    '	These methods are used to replace calls to some Java String constructors.
    '-----------------------------------------------------------------------------
    Friend Function NewString(ByVal bytes As SByte()) As String
        Return NewString(bytes, 0, bytes.Length)
    End Function

    Friend Function NewString(ByVal bytes As SByte(), ByVal index As Integer, ByVal count As Integer) As String
        Return Encoding.UTF8.GetString(CType(CObj(bytes), Byte()), index, count)
    End Function

    Friend Function NewString(ByVal bytes As SByte(), ByVal encoding As String) As String
        Return NewString(bytes, 0, bytes.Length, encoding)
    End Function

    Friend Function NewString(ByVal bytes As SByte(), ByVal index As Integer, ByVal count As Integer, ByVal encoding As String) As String
        Return Text.Encoding.GetEncoding(encoding).GetString(CType(CObj(bytes), Byte()), index, count)
    End Function

    '--------------------------------------------------------------------------------
    '	These methods are used to replace calls to the Java String.getBytes methods.
    '--------------------------------------------------------------------------------
    <Extension()>
    Friend Function GetBytes(ByVal self As String) As SByte()
        Return GetSBytesForEncoding(Encoding.UTF8, self)
    End Function

    <Extension()>
    Friend Function GetBytes(ByVal self As String, ByVal encoding As Encoding) As SByte()
        Return GetSBytesForEncoding(encoding, self)
    End Function

    <Extension()>
    Friend Function GetBytes(ByVal self As String, ByVal encoding As String) As SByte()
        Return GetSBytesForEncoding(Text.Encoding.GetEncoding(encoding), self)
    End Function

    Private Function GetSBytesForEncoding(ByVal encoding As Encoding, ByVal s As String) As SByte()
        Dim sbytes = New SByte(encoding.GetByteCount(s) - 1) {}
        encoding.GetBytes(s, 0, s.Length, CType(CObj(sbytes), Byte()), 0)
        Return sbytes
    End Function
End Module
