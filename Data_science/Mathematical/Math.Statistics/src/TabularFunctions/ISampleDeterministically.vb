#Region "Microsoft.VisualBasic::037f383bcd5b645afd8fd0f925f47cbd, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\src\TabularFunctions\ISampleDeterministically.vb"

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

Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace TabularFunctions


	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Interface ISampleDeterministically
		Inherits IWriteToXML

		Function GetYFromX( x As Double) As Double
		Function GetYValues() As List(Of Double?)
	End Interface

End Namespace
