
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace FileIO

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Encoding"/>会和<see cref="Encodings"/>产生冲突，
    ''' 使用这个单独的拓展模块，但是位于不同的命名空间来解决这个问题。
    ''' </remarks>
    Public Module Extensions

        ''' <summary>
        ''' Write all object into a text file by using its <see cref="Object.ToString"/> method.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="saveTo"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension> Public Function FlushAllLines(Of T)(data As IEnumerable(Of T), saveTo$, Optional encoding As Encoding = Nothing) As Boolean
            Dim strings As IEnumerable(Of String) =
                data.Select(AddressOf Scripting.ToString)
            Dim parent$ = FileSystem.GetParentPath(saveTo)

            Call parent.MkDIR

            If encoding Is Nothing Then
                encoding = Encoding.Default
            End If

            Try
                Using writer As StreamWriter = saveTo.OpenWriter(encoding,)
                    For Each line As String In strings
                        Call writer.WriteLine(line)
                    Next
                End Using
            Catch ex As Exception
                Call App.LogException(New Exception(saveTo, ex))
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Open text file writer, this function will auto handle all things.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function OpenWriter(path As String, Optional encoding As Encoding = Nothing, Optional newLine As String = vbLf) As StreamWriter
            Call "".SaveTo(path)

            Dim file As New FileStream(path, FileMode.OpenOrCreate)
            Dim writer As New StreamWriter(file, encoding) With {
                .NewLine =
                If(newLine Is Nothing OrElse newLine.Length = 0,
                vbLf,
                newLine)
            }

            Return writer
        End Function
    End Module
End Namespace