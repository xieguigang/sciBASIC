Namespace CommandLine

    Public Class CliResCommon

        Private ReadOnly CHUNK_BUFFER As Type = GetType(Byte())
        Private ReadOnly Resource As Dictionary(Of String, Func(Of Byte()))

        ReadOnly DataCache As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="DataCache">资源文件的数据缓存文件夹</param>
        Sub New(DataCache As String, ResourceManager As Type)
            Me.DataCache = DataCache
            Resource = (From [Property] As System.Reflection.PropertyInfo
                        In ResourceManager.GetProperties(bindingAttr:=System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Static)
                        Where [Property].PropertyType.Equals(CHUNK_BUFFER)
                        Select [Property]).ToArray.ToDictionary(Of String, Func(Of Byte()))(
 _
                            Function(obj) obj.Name,
                            elementSelector:=Function(obj) New Func(Of Byte())(Function() DirectCast(obj.GetValue(Nothing, Nothing), Byte())))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">使用 NameOf 操作符来获取资源</param>
        ''' <returns></returns>
        Public Function TryRelease(Name As String, Optional ext As String = "exe") As String
            Dim Path As String = $"{DataCache}/{Name}.{ext}"

            If Path.FileExists Then
                Return Path
            End If

            If Not Resource.ContainsKey(Name) Then
                Return ""
            End If

            Dim ChunkBuffer As Byte() = Resource(Name)()
            Try
                If ChunkBuffer.FlushStream(Path) Then
                    Call Console.WriteLine($"Release resource to {Path.ToFileURL} // length={ChunkBuffer.Length} bytes")
                    Return Path
                Else
                    Return ""
                End If
            Catch ex As Exception
                Call Console.WriteLine(ex.ToString)
                Return ""
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return DataCache
        End Function
    End Class
End Namespace