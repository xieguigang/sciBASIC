' ---------------------------------------------------------------------------
' COLMAP 二进制稀疏重建读取器
'
' 仅读取本流水线需要的部分：
'   cameras.bin  → 内参（模型/尺寸/参数，保留原始数组）
'   images.bin   → 每张照片的世界→相机位姿（四元数 + 平移）
'   points3D.bin → 稀疏三维点 + RGB + 重投影误差
'
' 性能要点：images.bin 在本数据集上是 73 MB（每张照片约 3000 个 2D 观测），
' 观测块（每点 8+8+8 = 24 字节）用 Seek 跳过而不是逐条 ReadDouble——
' 否则会多出上百万次无谓的读取调用。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Text

Namespace Colmap

    ''' <summary>
    ''' COLMAP 二进制稀疏模型读取器。
    ''' </summary>
    Public NotInheritable Class ColmapSparse

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 读取一个 COLMAP 稀疏目录（例如 <c>sparse/0</c>）。
        ''' </summary>
        ''' <param name="sparseDir">包含 cameras.bin / images.bin / points3D.bin 的目录</param>
        Public Shared Function Read(sparseDir As String) As ColmapScene
            If String.IsNullOrEmpty(sparseDir) Then
                Throw New ArgumentNullException(NameOf(sparseDir))
            End If

            Dim dir As String = ResolveSparseDir(sparseDir)
            Dim camerasPath As String = Path.Combine(dir, "cameras.bin")
            Dim imagesPath As String = Path.Combine(dir, "images.bin")
            Dim pointsPath As String = Path.Combine(dir, "points3D.bin")

            For Each path As String In {camerasPath, imagesPath, pointsPath}
                If Not File.Exists(path) Then
                    Throw New FileNotFoundException(
                        $"缺少 COLMAP 文件: {path}{vbCrLf}" &
                        "本读取器只支持二进制格式（cameras.bin/images.bin/points3D.bin）；" &
                        "如果你手上是 .txt 版本，请先用 COLMAP 的 model_converter 转换。", path)
                End If
            Next

            Dim scene As New ColmapScene With {.SparseDir = dir}

            Call ReadCameras(camerasPath, scene)
            Call ReadImages(imagesPath, scene)
            Call ReadPoints(pointsPath, scene)
            Call scene.SortImages()

            Return scene
        End Function

        ''' <summary>
        ''' 允许传入 <c>sparse</c>（自动下探 <c>0</c>）或直接传入 <c>sparse/0</c>。
        ''' </summary>
        Private Shared Function ResolveSparseDir(sparseDir As String) As String
            If File.Exists(Path.Combine(sparseDir, "cameras.bin")) Then Return sparseDir

            Dim nested As String = Path.Combine(sparseDir, "0")

            If File.Exists(Path.Combine(nested, "cameras.bin")) Then Return nested

            Return sparseDir
        End Function

        Private Shared Sub ReadCameras(path As String, scene As ColmapScene)
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read)
                Using reader As New BinaryReader(stream, Encoding.UTF8)
                    Dim count As ULong = reader.ReadUInt64()

                    For i As ULong = 0 To count - 1
                        Dim camera As New ColmapCamera()

                        camera.CameraId = reader.ReadInt32()
                        camera.ModelId = reader.ReadInt32()
                        camera.Width = CInt(reader.ReadUInt64())
                        camera.Height = CInt(reader.ReadUInt64())

                        Dim parameterCount As Integer = ParameterCountOf(camera.ModelId)
                        Dim parameters(parameterCount - 1) As Double

                        For k As Integer = 0 To parameterCount - 1
                            parameters(k) = reader.ReadDouble()
                        Next

                        camera.Parameters = parameters
                        scene.Cameras(camera.CameraId) = camera
                    Next
                End Using
            End Using
        End Sub

        Private Shared Sub ReadImages(path As String, scene As ColmapScene)
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read)
                Using reader As New BinaryReader(stream, Encoding.UTF8)
                    Dim count As ULong = reader.ReadUInt64()

                    For i As ULong = 0 To count - 1
                        Dim image As New ColmapImage()

                        image.ImageId = reader.ReadInt32()
                        image.Qw = reader.ReadDouble()
                        image.Qx = reader.ReadDouble()
                        image.Qy = reader.ReadDouble()
                        image.Qz = reader.ReadDouble()
                        image.Tx = reader.ReadDouble()
                        image.Ty = reader.ReadDouble()
                        image.Tz = reader.ReadDouble()
                        image.CameraId = reader.ReadInt32()
                        image.Name = ReadNullTerminatedString(reader)

                        ' 跳过 2D 观测：每个观测 = double x + double y + uint64 point3D_id
                        Dim observations As ULong = reader.ReadUInt64()
                        Call stream.Seek(CLng(observations) * 24L, SeekOrigin.Current)

                        Call scene.Images.Add(image)
                    Next
                End Using
            End Using
        End Sub

        Private Shared Sub ReadPoints(path As String, scene As ColmapScene)
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read)
                Using reader As New BinaryReader(stream, Encoding.UTF8)
                    Dim count As Integer = CInt(reader.ReadUInt64())
                    Dim xyz(count * 3 - 1) As Double
                    Dim rgb(count * 3 - 1) As Double
                    Dim errors(count - 1) As Double

                    For i As Integer = 0 To count - 1
                        Call reader.ReadUInt64() ' point3D_id

                        Dim o3 As Integer = i * 3

                        xyz(o3) = reader.ReadDouble()
                        xyz(o3 + 1) = reader.ReadDouble()
                        xyz(o3 + 2) = reader.ReadDouble()

                        rgb(o3) = reader.ReadByte() / 255.0
                        rgb(o3 + 1) = reader.ReadByte() / 255.0
                        rgb(o3 + 2) = reader.ReadByte() / 255.0

                        errors(i) = reader.ReadDouble()

                        ' 跳过 track（每元素 = int32 image_id + int32 point2D_idx）
                        Dim trackLength As ULong = reader.ReadUInt64()
                        Call stream.Seek(CLng(trackLength) * 8L, SeekOrigin.Current)
                    Next

                    scene.Points = xyz
                    scene.Colors = rgb
                    scene.Errors = errors
                End Using
            End Using
        End Sub

        Private Shared Function ReadNullTerminatedString(reader As BinaryReader) As String
            Dim buffer As New List(Of Byte)(64)

            While True
                Dim b As Integer = reader.BaseStream.ReadByte()

                If b <= 0 Then Exit While

                Call buffer.Add(CByte(b))
            End While

            Return Encoding.UTF8.GetString(buffer.ToArray())
        End Function

        ''' <summary>
        ''' 各 COLMAP 相机模型的参数个数（与 colmap/src/colmap/sensor/models.h 一致）。
        ''' </summary>
        Private Shared Function ParameterCountOf(modelId As Integer) As Integer
            Select Case modelId
                Case 0  ' SIMPLE_PINHOLE
                    Return 3
                Case 1  ' PINHOLE
                    Return 4
                Case 2  ' SIMPLE_RADIAL
                    Return 4
                Case 3  ' RADIAL
                    Return 5
                Case 4  ' OPENCV
                    Return 8
                Case 5  ' OPENCV_FISHEYE
                    Return 8
                Case 6  ' FULL_OPENCV
                    Return 12
                Case 7  ' FOV
                    Return 5
                Case 8  ' SIMPLE_RADIAL_FISHEYE
                    Return 4
                Case 9  ' RADIAL_FISHEYE
                    Return 5
                Case 10 ' THIN_PRISM_FISHEYE
                    Return 12
                Case 11 ' RAD_TAN_THIN_PRISM_FISHEYE
                    Return 16
                Case Else
                    Throw New NotSupportedException($"未知的 COLMAP 相机模型编号: {modelId}")
            End Select
        End Function

    End Class

End Namespace
