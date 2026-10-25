#Region "Microsoft.VisualBasic::5f2fa8337d9fe5578f99fac3fc6683d1, Data_science\MachineVision\GaussianSplatting3D\GaussianSplatting\ImageLoader.vb"

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

'   Total Lines: 226
'    Code Lines: 175 (77.43%)
' Comment Lines: 33 (14.60%)
'    - Xml Docs: 87.88%
' 
'   Blank Lines: 18 (7.96%)
'     File Size: 9.28 KB


' Class ImageLoader
' 
'     Function: Load, LoadAll
' 
'     Sub: Save
' 
' Class PointCloudIO
' 
'     Function: LoadPLY
' 
'     Sub: SaveGaussianPLY, SavePLY
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

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
        ' Core 的 BitmapReader 只支持 BMP（校验 "BM" 魔数），PNG 必须走自带编解码器
        Dim png = PngCodec.DecodeFile(path)
        Dim W = png.Width
        Dim H = png.Height
        Dim result = New Tensor(H, W, 3)

        Dim src = png.Rgb
        Dim data = result.Data

        ' 直接写底层数组（Tensor 索引器 setter 每写一次都会 Interlocked 自增版本号）
        System.Threading.Tasks.Parallel.For(0, H,
            Sub(Y)
                Dim s = Y * W * 3
                Dim d = Y * W * 3

                For X As Integer = 0 To W - 1
                    data(d) = src(s) / 255.0
                    data(d + 1) = src(s + 1) / 255.0
                    data(d + 2) = src(s + 2) / 255.0

                    s += 3
                    d += 3
                Next
            End Sub)

        Return result
    End Function

    ''' <summary>
    ''' 将 Tensor [H, W, 3] 保存为 PNG 图像
    ''' Save a Tensor [H, W, 3] (values in [0,1]) as a PNG image.
    ''' </summary>
    Public Shared Sub Save(image As Tensor, path As String)
        Dim H = image.Shape(0)
        Dim W = image.Shape(1)
        Dim rgb(W * H * 3 - 1) As Byte
        Dim data = image.Data

        System.Threading.Tasks.Parallel.For(0, H,
            Sub(Y)
                Dim src = Y * W * 3

                For X As Integer = 0 To W - 1
                    ' clamp 到 [0, 1] 然后转换为 [0, 255]
                    rgb(src) = CByte(std.Max(0, std.Min(255, CInt(data(src) * 255))))
                    rgb(src + 1) = CByte(std.Max(0, std.Min(255, CInt(data(src + 1) * 255))))
                    rgb(src + 2) = CByte(std.Max(0, std.Min(255, CInt(data(src + 2) * 255))))

                    src += 3
                Next
            End Sub)

        Call PngCodec.EncodeFile(path, W, H, rgb)
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
            Dim pos = model.PositionsData
            Dim cols = model.ColorsData
            Dim scales = model.ScalesData
            Dim rots = model.RotationsData

            For i = 0 To n - 1
                Dim o3 = i * 3
                Dim o4 = i * 4

                Dim x = pos(o3)
                Dim y = pos(o3 + 1)
                Dim z = pos(o3 + 2)

                Dim r = CInt(std.Max(0, std.Min(255, cols(o3) * 255)))
                Dim g = CInt(std.Max(0, std.Min(255, cols(o3 + 1) * 255)))
                Dim b = CInt(std.Max(0, std.Min(255, cols(o3 + 2) * 255)))

                Dim sx = std.Exp(scales(o3))
                Dim sy = std.Exp(scales(o3 + 1))
                Dim sz = std.Exp(scales(o3 + 2))

                Dim op = model.GetOpacity(i)

                sw.WriteLine($"{x:F6} {y:F6} {z:F6} 0 0 0 {r} {g} {b} " &
                             $"{sx:F6} {sy:F6} {sz:F6} " &
                             $"{rots(o4):F6} {rots(o4 + 1):F6} {rots(o4 + 2):F6} {rots(o4 + 3):F6} " &
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
