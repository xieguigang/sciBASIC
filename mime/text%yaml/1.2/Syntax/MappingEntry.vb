#Region "Microsoft.VisualBasic::78d6e080aafe97ac231a62e744dafee4, mime\text%yaml\1.2\Syntax\MappingEntry.vb"

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

    '   Total Lines: 24
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 674 B


    '     Class MappingEntry
    ' 
    '         Properties: Key, Name, Value
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Syntax

    Public Class MappingEntry : Implements INamedValue

        Public Property Key As DataItem
        Public Property Value As DataItem

        Private Property Name As String Implements INamedValue.Key
            Get
                Return Scripting.ToString(Key)
            End Get
            Set(value As String)
                Throw New ReadOnlyException
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{Key}: {Value}"
        End Function
    End Class
End Namespace
