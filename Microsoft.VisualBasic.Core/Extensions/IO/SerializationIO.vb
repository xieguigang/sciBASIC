#Region "Microsoft.VisualBasic::e3f88acaa3aee62820bf10f83811c764, Microsoft.VisualBasic.Core\Extensions\IO\SerializationIO.vb"

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

    ' Module SerializationIO
    ' 
    '     Function: SolveListStream
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Module SerializationIO

    <Extension>
    Public Function SolveListStream(path$, Optional encoding As Encoding = Nothing) As IEnumerable(Of String)
        Select Case path.ExtensionSuffix.ToLower
            Case "", "txt"
                Return path.IterateAllLines
            Case "json"
                Return path.LoadObject(Of String())
            Case "xml"
                Return path.LoadXml(Of String())
            Case Else
                Throw New NotImplementedException(path)
        End Select
    End Function
End Module

