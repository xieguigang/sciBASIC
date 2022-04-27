
' 
'     zt - Simple and partial Mantel Test - version 1.1
'     copyright (c) Eric Bonnet 2001 - 2007
' 
'     This program is free software; you can redistribute it and/or modify
'     it under the terms of the GNU General Public License as published by
'     the Free Software Foundation; either version 2 of the License, or
'     (at your option) any later version.
' 
'     This program is distributed in the hope that it will be useful,
'     but WITHOUT ANY WARRANTY; without even the implied warranty of
'     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'     GNU General Public License for more details.
' 
'     You should have received a copy of the GNU General Public License
'     along with this program; if not, write to the Free Software
'     Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307
'        

Imports System.Runtime.CompilerServices

Namespace Hypothesis.Mantel

    <HideModuleName>
    Public Module test

        Public Const MIN_MAT_SIZE As Integer = 5
        Public Const MAX_EXACT_SIZE As Integer = 12
        Public Const EXACT_PROC_SIZE As Integer = 8

        <Extension>
        Public Function test(model As Model, matA As Double()(), matB As Double()(), matC As Double()())
            Dim i As Integer
            Dim j As Integer
            Dim c As Integer
            Dim tmem As Integer
            Dim res As Integer
        End Function

    End Module
End Namespace