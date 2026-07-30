#Region "Microsoft.VisualBasic::cf58a965fcec93e90afb5dfc2a725a94, vs_solutions\dev\vs_PDB\Models\TypeRecord.vb"

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

    '   Total Lines: 38
    '    Code Lines: 12 (31.58%)
    ' Comment Lines: 19 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (18.42%)
    '     File Size: 1.20 KB


    '     Class TypeRecord
    ' 
    '         Properties: Fields, Kind, Name, Size, TypeId
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models

    ''' <summary>
    ''' A type record decoded from the TPI stream (classic PDB) or the metadata type tables
    ''' (Portable PDB).
    ''' </summary>
    Public Class TypeRecord

        ''' <summary>
        ''' Type id (leaf index for classic PDB, metadata token for Portable PDB).
        ''' </summary>
        Public Property TypeId As UInteger

        ''' <summary>
        ''' Type name, e.g. ``System.Int32`` or ``MyNamespace.MyClass``.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Category of the type. See <see cref="TypeKind"/>.
        ''' </summary>
        Public Property Kind As TypeKind

        ''' <summary>
        ''' Size of the type in bytes, 0 when unknown / not applicable.
        ''' </summary>
        Public Property Size As UInteger

        ''' <summary>
        ''' Field / member names of the type (for structs / classes).
        ''' </summary>
        Public Property Fields As New List(Of String)()

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} (#{TypeId})"
        End Function
    End Class
End Namespace
