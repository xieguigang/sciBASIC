Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports std = System.Math
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 图像加载器 - 从 PNG 文件加载图像并转换为 Tensor
''' Image loader: convert PNG files to Tensor objects.
''' </summary>
Public Class ImageLoader

    ''' <summary>
    ''' 加载 PNG 图像为 Tensor [H, W, 3]，值范围 [0, 1]
    ''' Load a PNG image as a Tensor with shape [H, W, 3], values in [0, 1].
    ''' </summary>
    Public Shared Function Load(path As String) As Tensor
        Using bmp As New Bitmap(path)
            Dim W = bmp.Width
            Dim H = bmp.Height
            Dim result = New Tensor(H, W, 3)

            ' 锁定像素数据
            Dim rect As New Rectangle(0, 0, W, H)
            Dim bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
            Dim stride = bmpData.Stride
            Dim bytes(stride * H - 1) As Byte
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length)
            bmp.UnlockBits(bmpData)

            ' Format24bppRgb: BGR 顺序，行优先，stride 可能大于 W*3
            For y = 0 To H - 1
                For x = 0 To W - 1
                    Dim offset = y * stride + x * 3
                    Dim b = bytes(offset) / 255.0
                    Dim g = bytes(offset + 1) / 255.0
                    Dim r = bytes(offset + 2) / 255.0
                    result(y, x, 0) = r
                    result(y, x, 1) = g
                    result(y, x, 2) = b
                Next
            Next

            Return result
        End Using
    End Function

    ''' <summary>
    ''' 将 Tensor [H, W, 3] 保存为 PNG 图像
    ''' Save a Tensor [H, W, 3] (values in [0,1]) as a PNG image.
    ''' </summary>
    Public Shared Sub Save(image As Tensor, path As String)
        Dim H = image.Shape(0)
        Dim W = image.Shape(1)
        Using bmp As New Bitmap(W, H, PixelFormat.Format24bppRgb)
            Dim rect As New Rectangle(0, 0, W, H)
            Dim bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb)
            Dim stride = bmpData.Stride
            Dim bytes(stride * H - 1) As Byte

            For y = 0 To H - 1
                For x = 0 To W - 1
                    Dim offset = y * stride + x * 3
                    Dim r = image(y, x, 0)
                    Dim g = image(y, x, 1)
                    Dim b = image(y, x, 2)
                    ' clamp 到 [0, 1] 然后转换为 [0, 255]
                    bytes(offset) = CByte(std.Max(0, std.Min(255, CInt(b * 255))))
                    bytes(offset + 1) = CByte(std.Max(0, std.Min(255, CInt(g * 255))))
                    bytes(offset + 2) = CByte(std.Max(0, std.Min(255, CInt(r * 255))))
                Next
            Next

            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length)
            bmp.UnlockBits(bmpData)

            ' 确保目录存在
            Dim dir = path.FileName
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If
            bmp.Save(path, ImageFormat.Png)
        End Using
    End Sub

    ''' <summary>
    ''' 加载一个目录下所有 view_NN.png 图像
    ''' </summary>
    Public Shared Function LoadAll(directory As String, numViews As Integer) As List(Of Tensor)
        Dim images As New List(Of Tensor)()
        For i = 0 To numViews - 1
            Dim path = $"{directory}/view_{i:00}.png"
            If File.Exists(path) Then
                images.Add(Load(path))
                Console.WriteLine($"  Loaded {path}")
            End If
        Next
        Return images
    End Function

End Class


''' <summary>
''' 点云 I/O - 读写 PLY 文件
''' Point cloud I/O: read/write PLY files (ASCII format).
''' </summary>
Public Class PointCloudIO

    ''' <summary>
    ''' 保存点云为 PLY 文件
    ''' Save a point cloud to an ASCII PLY file.
    ''' </summary>
    Public Shared Sub SavePLY(path As String, positions As Tensor, colors As Tensor)
        Dim n = positions.Shape(0)
        Using sw As New StreamWriter(path)
            sw.WriteLine("ply")
            sw.WriteLine("format ascii 1.0")
            sw.WriteLine($"element vertex {n}")
            sw.WriteLine("property float x")
            sw.WriteLine("property float y")
            sw.WriteLine("property float z")
            sw.WriteLine("property uchar red")
            sw.WriteLine("property uchar green")
            sw.WriteLine("property uchar blue")
            sw.WriteLine("property float scale_x")
            sw.WriteLine("property float scale_y")
            sw.WriteLine("property float scale_z")
            sw.WriteLine("property float opacity")
            sw.WriteLine("end_header")
            For i = 0 To n - 1
                Dim x = positions(i, 0)
                Dim y = positions(i, 1)
                Dim z = positions(i, 2)
                Dim r = CInt(std.Max(0, std.Min(255, colors(i, 0) * 255)))
                Dim g = CInt(std.Max(0, std.Min(255, colors(i, 1) * 255)))
                Dim b = CInt(std.Max(0, std.Min(255, colors(i, 2) * 255)))
                sw.WriteLine($"{x:F6} {y:F6} {z:F6} {r} {g} {b} 0.1 0.1 0.1 0.5")
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 保存高斯模型为 PLY 文件（包含所有高斯参数）
    ''' </summary>
    Public Shared Sub SaveGaussianPLY(path As String, model As GaussianModel)
        Dim n = model.Count
        Using sw As New StreamWriter(path)
            sw.WriteLine("ply")
            sw.WriteLine("format ascii 1.0")
            sw.WriteLine($"element vertex {n}")
            sw.WriteLine("property float x")
            sw.WriteLine("property float y")
            sw.WriteLine("property float z")
            sw.WriteLine("property float nx")
            sw.WriteLine("property float ny")
            sw.WriteLine("property float nz")
            sw.WriteLine("property uchar red")
            sw.WriteLine("property uchar green")
            sw.WriteLine("property uchar blue")
            sw.WriteLine("property float scale_0")
            sw.WriteLine("property float scale_1")
            sw.WriteLine("property float scale_2")
            sw.WriteLine("property float rot_0")
            sw.WriteLine("property float rot_1")
            sw.WriteLine("property float rot_2")
            sw.WriteLine("property float rot_3")
            sw.WriteLine("property float opacity")
            sw.WriteLine("end_header")
            For i = 0 To n - 1
                Dim x = model.Positions(i, 0)
                Dim y = model.Positions(i, 1)
                Dim z = model.Positions(i, 2)
                Dim r = CInt(std.Max(0, std.Min(255, model.Colors(i, 0) * 255)))
                Dim g = CInt(std.Max(0, std.Min(255, model.Colors(i, 1) * 255)))
                Dim b = CInt(std.Max(0, std.Min(255, model.Colors(i, 2) * 255)))
                Dim sx = std.Exp(model.Scales(i, 0))
                Dim sy = std.Exp(model.Scales(i, 1))
                Dim sz = std.Exp(model.Scales(i, 2))
                Dim qw = model.Rotations(i, 0)
                Dim qx = model.Rotations(i, 1)
                Dim qy = model.Rotations(i, 2)
                Dim qz = model.Rotations(i, 3)
                Dim op = model.GetOpacity(i)
                sw.WriteLine($"{x:F6} {y:F6} {z:F6} 0 0 0 {r} {g} {b} " &
                             $"{sx:F6} {sy:F6} {sz:F6} " &
                             $"{qw:F6} {qx:F6} {qy:F6} {qz:F6} " &
                             $"{op:F6}")
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 从 PLY 文件加载点云
    ''' </summary>
    Public Shared Function LoadPLY(path As String) As (Positions As Tensor, Colors As Tensor)
        Dim lines = File.ReadAllLines(path)
        Dim headerEnd = -1
        Dim n = 0
        For i = 0 To lines.Length - 1
            If lines(i).StartsWith("element vertex ") Then
                n = Integer.Parse(lines(i).Substring("element vertex ".Length).Trim())
            End If
            If lines(i).Trim() = "end_header" Then
                headerEnd = i
                Exit For
            End If
        Next

        Dim positions = New Tensor(n, 3)
        Dim colors = New Tensor(n, 3)
        For i = 0 To n - 1
            Dim parts = lines(headerEnd + 1 + i).Split(New Char() {" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)
            positions(i, 0) = Double.Parse(parts(0))
            positions(i, 1) = Double.Parse(parts(1))
            positions(i, 2) = Double.Parse(parts(2))
            If parts.Length >= 6 Then
                colors(i, 0) = Double.Parse(parts(3)) / 255.0
                colors(i, 1) = Double.Parse(parts(4)) / 255.0
                colors(i, 2) = Double.Parse(parts(5)) / 255.0
            End If
        Next
        Return (positions, colors)
    End Function

End Class
