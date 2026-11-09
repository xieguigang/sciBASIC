' ---------------------------------------------------------------------------
' 网格质量指标
'
' 这些指标是“能不能用”的判据，而不是好看的数字：
'   * IsClosed / BoundaryLoops：水密性（能否直接 3D 打印 / 做体积仿真）；
'   * SurfaceArea / Volume：与真实工件量级对比，明显偏离说明尺度或法向有问题；
'   * 点到网格 RMS / Max：重建表面与原始点云的一致性（Poisson 平滑会抬高 RMS，
'     BPA 更低但可能带孔洞——两个指标需要一起看）。
' ---------------------------------------------------------------------------

Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports CP = Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports CR = Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports std = System.Math

Namespace Cloudy

    ''' <summary>单个网格的统计。</summary>
    Public Class MeshStat

        Public Property Label As String
        Public Property Algorithm As String
        Public Property Vertices As Integer
        Public Property Triangles As Integer
        Public Property Closed As Boolean
        Public Property BoundaryLoops As Integer
        Public Property SurfaceArea As Double
        Public Property Volume As Double
        Public Property Diagonal As Double
        Public Property RmsToCloud As Double
        Public Property MaxToCloud As Double
        Public Property Seconds As Double
        Public Property Note As String

    End Class

    ''' <summary>
    ''' 计算网格指标（含点云到网格的偏差）。
    ''' </summary>
    Public NotInheritable Class MeshMetrics

        ''' <summary>偏差评估时参与计算的点数上限（控制 O(n·k) 的耗时）。</summary>
        Public Const MaxSamplePoints As Integer = 30000

        Private Sub New()
        End Sub

        Public Shared Function Measure(label As String,
                                       mesh As CR.MeshData,
                                       cloud As CC.PointCloudBuffer,
                                       Optional seconds As Double = 0.0,
                                       Optional note As String = Nothing,
                                       Optional log As Action(Of String) = Nothing) As MeshStat
            Dim stat As New MeshStat With {
                .Label = label,
                .Seconds = seconds,
                .Note = note
            }

            If mesh Is Nothing Then
                stat.Algorithm = "none"
                Return stat
            End If

            stat.Vertices = mesh.VertexCount
            stat.Triangles = mesh.TriangleCount

            Try
                stat.Closed = mesh.IsClosed()
            Catch ex As Exception
                stat.Closed = False
            End Try

            Try
                stat.BoundaryLoops = mesh.BoundaryLoops().Count
            Catch ex As Exception
                stat.BoundaryLoops = -1
            End Try

            Try
                stat.SurfaceArea = mesh.SurfaceArea()
                stat.Volume = mesh.Volume()
            Catch ex As Exception
                stat.SurfaceArea = 0.0
                stat.Volume = 0.0
            End Try

            Try
                Dim bounds As CC.Aabb3 = mesh.Bounds()

                stat.Diagonal = bounds.Diagonal
            Catch ex As Exception
                stat.Diagonal = 0.0
            End Try

            ' ---- 点云→网格偏差 ----
            If cloud IsNot Nothing AndAlso cloud.Count > 0 AndAlso stat.Triangles > 0 Then
                Try
                    Dim sample As CC.PointCloudBuffer = cloud

                    If cloud.Count > MaxSamplePoints Then
                        sample = CP.Sampling.UniformCount(cloud, MaxSamplePoints)
                    End If

                    Dim distances As Double() = CR.MeshProcessing.PointToMeshDistance(mesh, sample, 0.0)
                    Dim sumSquares As Double = 0.0
                    Dim maxDistance As Double = 0.0
                    Dim counted As Integer = 0

                    For i As Integer = 0 To distances.Length - 1
                        Dim d As Double = distances(i)

                        If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Continue For

                        d = std.Abs(d)

                        If d > maxDistance Then maxDistance = d

                        sumSquares += d * d
                        counted += 1
                    Next

                    If counted > 0 Then
                        stat.RmsToCloud = std.Sqrt(sumSquares / counted)
                        stat.MaxToCloud = maxDistance
                    End If
                Catch ex As Exception
                    Call Report(log, $"[warn] {label} 的点云偏差计算失败: {ex.Message}")
                End Try
            End If

            Call Report(log, $"{label}: {stat.Vertices:N0} 顶点 / {stat.Triangles:N0} 面，" &
                             $"封闭={stat.Closed}，边界环={stat.BoundaryLoops}，" &
                             $"RMS={stat.RmsToCloud:G4}，Max={stat.MaxToCloud:G4}")

            Return stat
        End Function

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
