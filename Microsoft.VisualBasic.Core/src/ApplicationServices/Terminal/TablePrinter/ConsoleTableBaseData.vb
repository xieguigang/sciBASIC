#Region "Microsoft.VisualBasic::602ba58a0f9b41e3b4d78bd903b088c3, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\ConsoleTableBaseData.vb"

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

    '   Total Lines: 13
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 417.00 B


    '     Class ConsoleTableBaseData
    ' 
    '         Properties: Column, Rows
    ' 
    '         Function: AppendLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.TablePrinter

    Public Class ConsoleTableBaseData

        Public Property Column As List(Of Object)
        Public Property Rows As List(Of List(Of Object))

        Public Function AppendLine(line As IEnumerable) As ConsoleTableBaseData
            Rows.Add((From x In line Select x).ToList)
            Return Me
        End Function
    End Class
End Namespace
