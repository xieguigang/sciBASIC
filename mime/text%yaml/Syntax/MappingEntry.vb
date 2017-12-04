#Region "Microsoft.VisualBasic::13104db3a3ee0ac77538902faca29f28, ..\sciBASIC#\mime\text%yaml\Syntax\MappingEntry.vb"

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
