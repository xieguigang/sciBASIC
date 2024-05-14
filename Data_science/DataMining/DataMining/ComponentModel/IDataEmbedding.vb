#Region "Microsoft.VisualBasic::4a99bc9ef812a8f2513d72ef2da03b05, Data_science\DataMining\DataMining\ComponentModel\IDataEmbedding.vb"

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

    '   Total Lines: 14
    '    Code Lines: 6
    ' Comment Lines: 4
    '   Blank Lines: 4
    '     File Size: 357 B


    '     Class IDataEmbedding
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel

    Public MustInherit Class IDataEmbedding

        Public MustOverride ReadOnly Property dimension As Integer

        ''' <summary>
        ''' get projection result
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetEmbedding() As Double()()

    End Class
End Namespace
