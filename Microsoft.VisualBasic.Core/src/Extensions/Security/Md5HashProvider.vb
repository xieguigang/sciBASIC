#Region "Microsoft.VisualBasic::5b52993acfd72454fd2bfbe7c3b1caca, Microsoft.VisualBasic.Core\src\Extensions\Security\Md5HashProvider.vb"

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

    '   Total Lines: 120
    '    Code Lines: 61 (50.83%)
    ' Comment Lines: 41 (34.17%)
    '    - Xml Docs: 51.22%
    ' 
    '   Blank Lines: 18 (15.00%)
    '     File Size: 4.43 KB


    '     Class Md5HashProvider
    ' 
    '         Function: ComputeMD5Hash, GetMd5Bytes, (+2 Overloads) GetMd5Hash, GetMd5hashLong
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

        ''' <summary>
        ''' calculate string md5 hashcode
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns>
        ''' this function may returns empty hashcode string if the given <paramref name="input"/> string is empty.
        ''' </returns>
        Public Function GetMd5Hash(input As String) As String
            If String.IsNullOrEmpty(input) Then
                Return ""
            End If

            ' Convert the input string to a byte array and compute the hash.
            Dim data As Byte() = Encoding.UTF8.GetBytes(input)
            Return GetMd5Hash(input:=data)
        End Function

        Public Function ComputeMD5Hash(input As String) As Long
            If String.IsNullOrEmpty(input) Then
                Return 0
            Else
                Dim inputBytes = Encoding.UTF8.GetBytes(input)
                Dim hashBytes = md5Hash.ComputeHash(inputBytes)
                Return BitConverter.ToInt64(hashBytes, 0)
            End If
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

        ''' <summary>
        ''' Calculate the md5 hashcode based on the given <paramref name="input"/> data.
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
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
