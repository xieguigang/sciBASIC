Imports System.Text

Namespace SecurityString

    ''' <summary>
    ''' 并行化的需求
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Md5HashProvider

        ReadOnly md5Hash As Security.Cryptography.MD5 =
            Security.Cryptography.MD5.Create()

        Public Function GetMd5Hash(input As String) As String
            If String.IsNullOrEmpty(input) Then
                Return ""
            End If

            Dim data As Byte() = Encoding.UTF8.GetBytes(input)  ' Convert the input string to a byte array and compute the hash. 
            Return GetMd5Hash(input:=data)
        End Function

        Public Function GetMd5Hash(input As Byte()) As String
            If input.IsNullOrEmpty Then
                Return ""
            End If

            Dim data As Byte() = md5Hash.ComputeHash(input)    ' Convert the input string to a byte array and compute the hash. 
            ' Create a new Stringbuilder to collect the bytes 
            ' and create a string. 
            Dim sBuilder As New StringBuilder()

            ' Loop through each byte of the hashed data  
            ' and format each one as a hexadecimal string. 
            For i As Integer = 0 To data.Length - 1
                sBuilder.Append(data(i).ToString("x2"))
            Next i

            Return sBuilder.ToString() ' Return the hexadecimal string. 
        End Function
    End Class
End Namespace