#Region "Microsoft.VisualBasic::c5f604560a011d3f4906007f9daf8d48, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\Testing\DBSCAN\MyCustomDatasetItem.vb"

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

Imports Microsoft.VisualBasic.DataMining.DBSCAN

Public Class MyCustomDatasetItem
    Inherits DatasetItemBase
    Public X As Double
    Public Y As Double

    Public Sub New(x__1 As Double, y__2 As Double)
        X = x__1
        Y = y__2
    End Sub
End Class

