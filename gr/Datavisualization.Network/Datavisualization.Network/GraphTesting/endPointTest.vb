#Region "Microsoft.VisualBasic::8b5ac593f2ae351f607a28162b8751ea, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\GraphTesting\endPointTest.vb"

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

Imports Microsoft.VisualBasic.Data.visualize.Network.FindPath

Module endPointTest

    Sub Main()
        Dim g = ExampleNetwork()


        For Each subNet In g.IteratesSubNetworks

            Dim endPoints = subNet.EndPoints

            Pause()

        Next

        Pause()
    End Sub
End Module

