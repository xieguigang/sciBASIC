#Region "Microsoft.VisualBasic::4a0c43b209b7d3a4047aab292c2980bc, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//test/DBSCAN/MyCustomDatasetItem.vb"

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

'   Total Lines: 12
'    Code Lines: 9
' Comment Lines: 0
'   Blank Lines: 3
'     File Size: 247 B


' Class MyCustomDatasetItem
' 
'     Constructor: (+1 Overloads) Sub New
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Public Class MyCustomDatasetItem : Implements IReadOnlyId

    Public X As Double
    Public Y As Double

    Public Sub New(x__1 As Double, y__2 As Double)
        X = x__1
        Y = y__2
    End Sub

    Public ReadOnly Property Identity As String Implements IReadOnlyId.Identity
End Class
