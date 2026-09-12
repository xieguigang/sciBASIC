#Region "Microsoft.VisualBasic::78f1a601a09fa665d07d5a02f1611b78, cuda\ILCuda\test\Metrics\MatrixData.vb"

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

    '   Total Lines: 201
    '    Code Lines: 143 (71.14%)
    ' Comment Lines: 21 (10.45%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 37 (18.41%)
    '     File Size: 7.55 KB


    '     Class MatrixData
    ' 
    '         Properties: Cols, Data, RowNames, Rows, Source
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateRandom, DetectSeparator, FromCsv
    ' 
    '     Class MetricResult
    ' 
    '         Properties: Data, Size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class MatrixMetricsResult
    ' 
    '         Properties: CopyMs, Correlation, Distance, KernelMs, TotalMs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 矩阵数据容器：随机合成矩阵 / CSV 载入，以及度量结果的封装
'
' 属于 demo（皮尔逊相关系数矩阵 + 欧氏距离矩阵）的领域模型，
' 与 CUDA 框架本身无关，因此放在 test 工程中。
' ------------------------------------------------------------------------

Imports System.Globalization
Imports System.IO

Namespace Metrics

    ''' <summary>行主序（row-major）的单精度矩阵</summary>
    Public Class MatrixData
        Private ReadOnly _rows As Integer
        Private ReadOnly _cols As Integer
        Private ReadOnly _data As Single()

        Public Sub New(rows As Integer, cols As Integer, data As Single())
            If rows <= 0 OrElse cols <= 0 Then Throw New ArgumentOutOfRangeException("rows/cols 必须为正数")
            If data Is Nothing OrElse data.Length <> rows * cols Then
                Throw New ArgumentException("数据长度与矩阵维度不一致")
            End If

            _rows = rows
            _cols = cols
            _data = data
        End Sub

        Public ReadOnly Property Rows As Integer
            Get
                Return _rows
            End Get
        End Property

        Public ReadOnly Property Cols As Integer
            Get
                Return _cols
            End Get
        End Property

        Public ReadOnly Property Data As Single()
            Get
                Return _data
            End Get
        End Property

        ''' <summary>行名称（CSV 第一列不是数字时自动当作行名）</summary>
        Public Property RowNames As String()

        ''' <summary>数据来源描述</summary>
        Public Property Source As String

        Default Public Property Item(row As Integer, col As Integer) As Single
            Get
                Return _data(row * _cols + col)
            End Get
            Set(value As Single)
                _data(row * _cols + col) = value
            End Set
        End Property

        ''' <summary>生成一个可复现的随机矩阵（标准正态分布）</summary>
        Public Shared Function CreateRandom(rows As Integer, cols As Integer, Optional seed As Integer = 42) As MatrixData
            Dim rnd As New System.Random(seed)
            Dim data(rows * cols - 1) As Single

            For i As Integer = 0 To data.Length - 1
                ' Box-Muller 变换
                Dim u1 = rnd.NextDouble()
                Dim u2 = rnd.NextDouble()
                If u1 < 0.000000000001 Then u1 = 0.000000000001

                Dim z = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2)
                data(i) = CSng(z)
            Next

            Return New MatrixData(rows, cols, data) With {
                .Source = $"随机矩阵 rows={rows}, cols={cols}, seed={seed}"
            }
        End Function

        ''' <summary>
        ''' 从 CSV / TSV 文本载入矩阵。
        ''' 自动识别分隔符；非数字的表头行会被跳过；行首的非数字列会被当作行名。
        ''' </summary>
        Public Shared Function FromCsv(filePath As String) As MatrixData
            If Not File.Exists(filePath) Then Throw New FileNotFoundException("找不到数据文件", filePath)

            Dim lines = File.ReadAllLines(filePath) _
                .Where(Function(l) Not String.IsNullOrWhiteSpace(l)) _
                .ToList()

            If lines.Count = 0 Then Throw New InvalidDataException("数据文件为空")

            Dim separator As Char = DetectSeparator(lines(0))
            Dim rows As New List(Of Single())()
            Dim names As New List(Of String)()

            For Each line In lines
                Dim tokens = line.Split(separator) _
                    .Select(Function(t) t.Trim().Trim(""""c)) _
                    .Where(Function(t) t.Length > 0) _
                    .ToArray()

                If tokens.Length = 0 Then Continue For

                Dim numbers As New List(Of Single)()
                Dim rowName As String = Nothing

                For Each token In tokens
                    Dim value As Single
                    If Single.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                        numbers.Add(value)
                    ElseIf rowName Is Nothing AndAlso numbers.Count = 0 Then
                        rowName = token
                    End If
                Next

                If numbers.Count = 0 Then Continue For ' 表头行

                rows.Add(numbers.ToArray())
                names.Add(If(rowName, $"row_{rows.Count}"))
            Next

            If rows.Count = 0 Then Throw New InvalidDataException("没有解析到任何数值行")

            Dim cols = rows(0).Length
            If rows.Any(Function(r) r.Length <> cols) Then
                Throw New InvalidDataException("各行的列数不一致")
            End If

            Dim flat(rows.Count * cols - 1) As Single
            For r As Integer = 0 To rows.Count - 1
                Array.Copy(rows(r), 0, flat, r * cols, cols)
            Next

            Return New MatrixData(rows.Count, cols, flat) With {
                .RowNames = If(names.All(Function(n) n Is Nothing), Nothing, names.ToArray()),
                .Source = $"CSV: {filePath}"
            }
        End Function

        Private Shared Function DetectSeparator(sample As String) As Char
            If sample.Contains(vbTab) Then Return vbTab(0)
            If sample.Contains(","c) Then Return ","c
            If sample.Contains(";"c) Then Return ";"c
            Return " "c
        End Function
    End Class

    ''' <summary>一个 rows x rows 的结果矩阵（行间相似度矩阵）</summary>
    Public Class MetricResult
        Private ReadOnly _size As Integer
        Private ReadOnly _data As Single()

        Public Sub New(size As Integer, data As Single())
            _size = size
            _data = data
        End Sub

        Public ReadOnly Property Size As Integer
            Get
                Return _size
            End Get
        End Property

        Public ReadOnly Property Data As Single()
            Get
                Return _data
            End Get
        End Property

        Default Public Property Item(row As Integer, col As Integer) As Single
            Get
                Return _data(row * _size + col)
            End Get
            Set(value As Single)
                _data(row * _size + col) = value
            End Set
        End Property
    End Class

    ''' <summary>一次度量计算的完整结果</summary>
    Public Class MatrixMetricsResult
        ''' <summary>皮尔逊相关系数矩阵</summary>
        Public Property Correlation As MetricResult
        ''' <summary>欧氏距离矩阵</summary>
        Public Property Distance As MetricResult
        ''' <summary>核心计算耗时（毫秒）</summary>
        Public Property KernelMs As Double
        ''' <summary>数据上传/回读耗时（毫秒）</summary>
        Public Property CopyMs As Double

        Public ReadOnly Property TotalMs As Double
            Get
                Return KernelMs + CopyMs
            End Get
        End Property
    End Class
End Namespace
