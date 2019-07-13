#Region "Microsoft.VisualBasic::a69aafc7e68b530d3d920e4c7a11b6a4, gr\network-visualization\Datavisualization.Network\IO\FileStream\csv\vbnet.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class vbnet
    ' 
    '         Function: Load, Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices.Zip
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
        Public Shared Function Load(vbnet$) As NetworkTables
            Dim tmp = App.GetAppSysTempFile(, sessionID:=App.PID)
            Call UnZip.ImprovedExtractToDirectory(vbnet, tmp, Overwrite.Always)
            Return NetworkTables.Load(tmp)
        End Function

        ''' <summary>
        ''' 文件拓展名为``*.vbnet``
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="vbnet$"></param>
        ''' <returns></returns>
        Public Shared Function Save(net As NetworkTables, vbnet$) As Boolean
            Dim tmp$ = App.GetAppSysTempFile(, sessionID:=App.PID)

            Call net.Save(tmp)
            Call ZipLib.AddToArchive(
                ls - r - l - "*.*" <= tmp,
                vbnet,
                ArchiveAction.Replace,
                Overwrite.Always,
                CompressionLevel.Fastest)

            Return True
        End Function
    End Class
End Namespace
