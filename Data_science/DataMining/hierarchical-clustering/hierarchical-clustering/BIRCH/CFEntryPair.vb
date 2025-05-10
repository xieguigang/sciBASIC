#Region "Microsoft.VisualBasic::b085c4bf5f7bc3415ab037a0a53c604c, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\BIRCH\CFEntryPair.vb"

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

    '   Total Lines: 76
    '    Code Lines: 34 (44.74%)
    ' Comment Lines: 26 (34.21%)
    '    - Xml Docs: 11.54%
    ' 
    '   Blank Lines: 16 (21.05%)
    '     File Size: 2.21 KB


    '     Class CFEntryPair
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Equals, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

' 
'   This file is part of JBIRCH.
' 
'   JBIRCH is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
' 
'   JBIRCH is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
' 
'   You should have received a copy of the GNU General Public License
'   along with JBIRCH.  If not, see <http://www.gnu.org/licenses/>.
' 
' 

' 
'   CFEntryPair.java
'   Copyright (C) 2009 Roberto Perdisci (roberto.perdisci@gmail.com)
' 

Namespace BIRCH

    ''' 
    ''' <summary>
    ''' @author Roberto Perdisci (roberto.perdisci@gmail.com)
    ''' 
    ''' </summary>
    Public Class CFEntryPair

        Private Shared ReadOnly LINE_SEP As String = vbLf

        Public e1 As CFEntry
        Public e2 As CFEntry

        Public Sub New()
        End Sub

        Public Sub New(e1 As CFEntry, e2 As CFEntry)
            Me.e1 = e1
            Me.e2 = e2
        End Sub

        Public Overrides Function Equals(o As Object) As Boolean
            Dim p = CType(o, CFEntryPair)

            If e1.Equals(p.e1) AndAlso e2.Equals(p.e2) Then
                Return True
            End If

            If e1.Equals(p.e2) AndAlso e2.Equals(p.e1) Then
                Return True
            End If

            Return False
        End Function

        Public Overrides Function ToString() As String
            Dim buff As StringBuilder = New StringBuilder()

            buff.Append("---- CFEntryPiar ----" & LINE_SEP)
            buff.Append("---- e1 ----" & LINE_SEP)
            buff.Append(e1.ToString() & LINE_SEP)
            buff.Append("---- e2 ----" & LINE_SEP)
            buff.Append(e2.ToString() & LINE_SEP)
            buff.Append("-------- end --------" & LINE_SEP)

            Return buff.ToString()
        End Function
    End Class

End Namespace

