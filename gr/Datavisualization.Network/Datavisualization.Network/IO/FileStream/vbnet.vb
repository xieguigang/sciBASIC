Imports System.IO.Compression
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileStream

    ''' <summary>
    ''' VB.NET sciBASIC network data file.
    ''' </summary>
    Public Class vbnet

        ''' <summary>
        ''' 文件拓展名为``*.vbnet``
        ''' </summary>
        ''' <param name="vbnet$"></param>
        ''' <returns></returns>
        Public Shared Function Load(vbnet$) As Network
            Dim tmp = App.GetAppSysTempFile(, sessionID:=App.PID)
            Call GZip.ImprovedExtractToDirectory(vbnet, tmp, Overwrite.Always)
            Return Network.Load(tmp)
        End Function

        ''' <summary>
        ''' 文件拓展名为``*.vbnet``
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="vbnet$"></param>
        ''' <returns></returns>
        Public Shared Function Save(net As Network, vbnet$) As Boolean
            Dim tmp$ = App.GetAppSysTempFile(, sessionID:=App.PID)

            Call net.Save(tmp)
            Call GZip.AddToArchive(
                vbnet,
                ls - r - l - "*.*" <= tmp,
                ArchiveAction.Replace,
                Overwrite.Always,
                CompressionLevel.Fastest)

            Return True
        End Function
    End Class
End Namespace