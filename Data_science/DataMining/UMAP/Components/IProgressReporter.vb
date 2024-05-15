#Region "Microsoft.VisualBasic::74243db976226d7956ee82d201cd5f6b, Data_science\DataMining\UMAP\Components\IProgressReporter.vb"

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

    '   Total Lines: 16
    '    Code Lines: 9
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 536 B


    ' Class ProgressReporter
    ' 
    '     Function: Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline

''' <summary>
''' The progress will be a value from 0 to 1 that indicates approximately how much of the processing has been completed
''' </summary>
Friend Class ProgressReporter

    Public report As RunSlavePipeline.SetProgressEventHandler

    Public Function Run(Of T)(action As Func(Of T), progress As Double, msg As String) As T
        Dim result As T = action()
        Call report(progress, msg)
        Return result
    End Function

End Class
