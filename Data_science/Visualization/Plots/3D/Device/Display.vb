#Region "Microsoft.VisualBasic::229c33c5df169db52b18da74283506be, Data_science\Visualization\Plots\3D\Device\Display.vb"

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

    '     Module Display
    ' 
    '         Function: NewWindow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Plot3D.Device

    ''' <summary>
    ''' The 3d display device extension
    ''' </summary>
    Public Module Display

        Public Function NewWindow() As FormDevice
            Return New FormDevice
        End Function
    End Module
End Namespace
