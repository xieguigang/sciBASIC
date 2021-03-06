﻿#Region "Microsoft.VisualBasic::5a0c96638bc8e8079395582facca1e33, Data_science\DataMining\UMAP\Components\IProgressReporter.vb"

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

    ' Delegate Sub
    ' 
    ' 
    ' Class ProgressReporter
    ' 
    '     Function: Run
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' The progress will be a value from 0 to 1 that indicates approximately how much of the processing has been completed
''' </summary>
Public Delegate Sub IProgressReporter(progress As Double)

Friend Class ProgressReporter

    Public report As IProgressReporter

    Public Function Run(Of T)(action As Func(Of T), progress As Double) As T
        Dim result As T = action()
        Call report(progress)
        Return result
    End Function

End Class
