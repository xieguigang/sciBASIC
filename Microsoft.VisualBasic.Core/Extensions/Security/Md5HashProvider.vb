#Region "Microsoft.VisualBasic::c81fb0eed5a2e47aa2f57020a9833ef1, Microsoft.VisualBasic.Core\Extensions\Security\Md5HashProvider.vb"

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

    '     Class Md5HashProvider
    ' 
    '         Function: GetMd5Bytes, (+2 Overloads) GetMd5Hash, GetMd5hashLong
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports System.Text

Namespace SecurityString

    ''' <summary>
    ''' 并行化的需求
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Md5HashProvider : Implements IDisposable

        ReadOnly md5Hash As MD5 = Security.Cryptography.MD5.Create()

        Public Function GetMd5Hash(input As String) As String
            If String.IsNullOrEmpty(input) Then
                Return ""
            End If

            ' Convert the input string to a byte array and compute the hash.
            Dim data As Byte() = Encoding.UTF8.GetBytes(input)
            Return GetMd5Hash(input:=data)
        End Function

        Public Function GetMd5Bytes(input As Byte()) As Byte()
            If input.IsNullOrEmpty Then
                Return {}
            End If

            Dim data As Byte() = md5Hash.ComputeHash(input)
            Return data
        End Function

        ''' <summary>
        ''' <see cref="MD5.ComputeHash"/> -> <see cref="ToLong"/>
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMd5hashLong(input As Byte()) As Long
            Return GetMd5Bytes(input).ToLong
        End Function

        Public Function GetMd5Hash(input As Byte()) As String
            If input.IsNullOrEmpty Then
                Return ""
            End If

            ' Convert the input string to a byte array and compute the hash.
            Dim data As Byte() = md5Hash.ComputeHash(input)
            ' Create a new Stringbuilder to collect the bytes
            ' and create a string.
            Dim sBuilder As New StringBuilder()

            ' Loop through each byte of the hashed data
            ' and format each one as a hexadecimal string.
            For i As Integer = 0 To data.Length - 1
                sBuilder.Append(data(i).ToString("x2"))
            Next i

            ' Return the hexadecimal string.
            Return sBuilder.ToString()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call md5Hash.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
