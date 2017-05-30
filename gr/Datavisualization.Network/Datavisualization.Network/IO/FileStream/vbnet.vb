#Region "Microsoft.VisualBasic::1628dd6494f1620286f8b1095894e525, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\IO\FileStream\vbnet.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
