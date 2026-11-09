' ---------------------------------------------------------------------------
' 产物路径约定
'
' 所有中间产物都按 output/<runTag>/ 归档，好处是：
'   * 一次失败不会污染上一次成功的产物，便于对比不同档位的结果；
'   * 训练日志、渲染对照图、点云、网格、报告都在同一个“现场”里，
'     排查问题时不需要到处找文件。
' ---------------------------------------------------------------------------

Imports System.IO

Namespace Pipeline

    ''' <summary>
    ''' 一次运行涉及的全部输出路径。
    ''' </summary>
    Public Class RunPaths

        Public Property Root As String
        Public Property PlyDir As String
        Public Property MeshDir As String
        Public Property RenderDir As String
        Public Property LogDir As String
        Public Property ReportDir As String

        ''' <summary>
        ''' 创建（必要时）并返回目录结构。
        ''' </summary>
        Public Shared Function Create(outputRoot As String, runTag As String) As RunPaths
            Dim root As String = Path.Combine(outputRoot, runTag)

            Dim paths As New RunPaths With {
                .Root = root,
                .PlyDir = Path.Combine(root, "ply"),
                .MeshDir = Path.Combine(root, "mesh"),
                .RenderDir = Path.Combine(root, "render"),
                .LogDir = Path.Combine(root, "logs"),
                .ReportDir = Path.Combine(root, "report")
            }

            For Each dir As String In {paths.Root, paths.PlyDir, paths.MeshDir, paths.RenderDir, paths.LogDir, paths.ReportDir}
                If Not Directory.Exists(dir) Then Call Directory.CreateDirectory(dir)
            Next

            Return paths
        End Function

        Public Function Ply(name As String) As String
            Return Path.Combine(PlyDir, name)
        End Function

        Public Function Mesh(name As String) As String
            Return Path.Combine(MeshDir, name)
        End Function

        Public Function Render(name As String) As String
            Return Path.Combine(RenderDir, name)
        End Function

        Public Function Log(name As String) As String
            Return Path.Combine(LogDir, name)
        End Function

        Public Function Report(name As String) As String
            Return Path.Combine(ReportDir, name)
        End Function

    End Class

End Namespace
