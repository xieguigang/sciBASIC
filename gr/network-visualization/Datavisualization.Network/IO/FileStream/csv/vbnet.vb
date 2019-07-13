#Region "Microsoft.VisualBasic::a69aafc7e68b530d3d920e4c7a11b6a4, gr\network-visualization\Datavisualization.Network\IO\FileStream\csv\vbnet.vb"

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
