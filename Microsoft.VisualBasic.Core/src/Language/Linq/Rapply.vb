#Region "Microsoft.VisualBasic::91f1a64cf3bb254ca0f2bc9bee362f2d, Microsoft.VisualBasic.Core\src\Language\Linq\Rapply.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 3 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1.19 KB


    '     Module Rapply
    ' 
    '         Function: lapply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Language

    ''' <summary>
    ''' Helper for implements ``lapply`` and ``sapply`` liked operations from R language
    ''' </summary>
    Public Module Rapply

        <Extension>
        Public Function lapply(Of Tin As INamedValue, TOut)(sequence As IEnumerable(Of Tin), apply As [Delegate], ParamArray args As Object()) As Dictionary(Of String, TOut)
            Dim result As New Dictionary(Of String, TOut)
            Dim key$
            Dim value As Object
            Dim arguments As Object() = New Object(args.Length) {}
            Dim method As MethodInfo = apply.Method
            Dim obj As Object = apply.Target

            Call Array.ConstrainedCopy(args, Scan0, arguments, 1, args.Length)

            For Each item As Tin In sequence
                key = item.Key
                arguments(Scan0) = item
                value = method.Invoke(obj, arguments)
                result.Add(key, value)
            Next

            Return result
        End Function
    End Module
End Namespace
