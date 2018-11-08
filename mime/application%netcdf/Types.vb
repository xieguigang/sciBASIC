Imports Microsoft.VisualBasic.Data.IO

Public Enum types As Integer
    undefined = -1

    [BYTE] = 1
    [CHAR] = 2
    [SHORT] = 3
    [INT] = 4
    [FLOAT] = 5
    [DOUBLE] = 6
End Enum

Module TypeExtensions

    ''' <summary>
    ''' Parse a number into their respective type
    ''' </summary>
    ''' <param name="type">type - integer that represents the type</param>
    ''' <returns>parsed value of the type</returns>
    Public Function num2str(type As types) As String
        Select Case type
            Case types.BYTE
                Return "byte"
            Case types.CHAR
                Return "char"
            Case types.SHORT
                Return "short"
            Case types.INT
                Return "int"
            Case types.FLOAT
                Return "float"
            Case types.DOUBLE
                Return "double"
            Case Else
                ' istanbul ignore next 
                Return "undefined"
        End Select
    End Function

    ''' <summary>
    ''' Parse a number type identifier to his size in bytes
    ''' </summary>
    ''' <param name="type">type - integer that represents the type</param>
    ''' <returns>size of the type</returns>
    Public Function num2bytes(type As types) As Integer
        Select Case type
            Case types.BYTE
                Return 1
            Case types.CHAR
                Return 1
            Case types.SHORT
                Return 2
            Case types.INT
                Return 4
            Case types.FLOAT
                Return 4
            Case types.DOUBLE
                Return 8
            Case Else
                ' istanbul ignore next 
                Return -1
        End Select
    End Function

    ''' <summary>
    ''' Reverse search of num2str
    ''' </summary>
    ''' <param name="type">type - string that represents the type</param>
    ''' <returns>parsed value of the type</returns>
    Public Function str2num(type As String) As types
        Select Case LCase(type)
            Case "byte"
                Return types.BYTE
            Case "char"
                Return types.CHAR
            Case "short"
                Return types.SHORT
            Case "int"
                Return types.INT
            Case "float"
                Return types.FLOAT
            Case "double"
                Return types.DOUBLE
            Case Else
                ' istanbul ignore next
                Return types.undefined
        End Select
    End Function

    ''' <summary>
    ''' Auxiliary function to read numeric data
    ''' </summary>
    ''' <param name="size%">size - Size of the element to read</param>
    ''' <param name="bufferReader">bufferReader - Function to read next value</param>
    ''' <returns>{Array&lt;number>|number}</returns>
    Public Function readNumber(size%, bufferReader As Func(Of Object)) As Object
        If (size <> 1) Then
            Dim numbers As New List(Of Object)

            For i As Integer = 0 To size - 1
                numbers.Add(bufferReader())
            Next

            Return numbers
        Else
            Return bufferReader()
        End If
    End Function

    ''' <summary>
    ''' Given a type And a size reads the next element
    ''' </summary>
    ''' <param name="buffer">buffer - Buffer for the file data</param>
    ''' <param name="type">type - Type of the data to read</param>
    ''' <param name="size">size - Size of the element to read</param>
    ''' <returns>``{string|Array&lt;number>|number}``</returns>
    Public Function readType(buffer As BinaryDataReader, type As types, size As Integer) As Object
        Select Case type
            Case types.BYTE
                Return buffer.ReadBytes(size)
            Case types.CHAR
                Return New String(buffer.ReadChars(size)).TrimNull
            Case types.SHORT
                Return readNumber(size, AddressOf buffer.ReadInt16)
            Case types.INT
                Return readNumber(size, AddressOf buffer.ReadInt32)
            Case types.FLOAT
                Return readNumber(size, AddressOf buffer.ReadSingle)
            Case types.DOUBLE
                Return readNumber(size, AddressOf buffer.ReadDouble)

            Case Else
                ' istanbul ignore next
                Return Utils.notNetcdf(True, $"non valid type {type}")
        End Select
    End Function
End Module