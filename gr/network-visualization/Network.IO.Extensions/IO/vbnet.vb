#Region "Microsoft.VisualBasic::7f9e4c7e5ce897361688eb3944ac3c87, gr\network-visualization\Network.IO.Extensions\IO\vbnet.vb"

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

    '   Total Lines: 45
    '    Code Lines: 25 (55.56%)
    ' Comment Lines: 14 (31.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (13.33%)
    '     File Size: 1.55 KB


    '     Class vbnet
    ' 
    '         Function: Load, Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO.Compression
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
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
            Dim tmp = TempFileSystem.GetAppSysTempFile(, sessionID:=App.PID)
            Call UnZip.ImprovedExtractToDirectory(vbnet, tmp, Overwrite.Always)
            Return NetworkFileIO.Load(tmp)
        End Function

        ''' <summary>
        ''' 文件拓展名为``*.vbnet``
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="vbnet$"></param>
        ''' <returns></returns>
        Public Shared Function Save(net As NetworkTables, vbnet$, Optional encoding As Encoding = Nothing) As Boolean
            Dim tmp$ = TempFileSystem.GetAppSysTempFile(, sessionID:=App.PID)

            Call net.Save(tmp, encoding)
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
