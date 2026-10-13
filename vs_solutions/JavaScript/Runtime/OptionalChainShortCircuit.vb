#Region "Microsoft.VisualBasic::23f6d92a19800b4a5ebeb6f4de06a992, vs_solutions\JavaScript\Runtime\OptionalChainShortCircuit.vb"

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
    '    Code Lines: 8 (33.33%)
    ' Comment Lines: 11 (45.83%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 847 B


    '     Class OptionalChainShortCircuit
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' Internal control-flow signal thrown when an optional-chain access
    ''' (<c>a?.b</c> / <c>a?.[i]</c> / <c>a?.()</c>) hits a nullish base.
    ''' <para>
    ''' Caught at the ROOT of the chain (both the interpreter and generated code
    ''' wrap the whole chain), where it becomes the value <c>undefined</c> — so
    ''' <c>a?.b.c</c> never evaluates <c>.c</c> when <c>a</c> is nullish.
    ''' Public so that translated VB programs can catch it too; it is deliberately
    ''' NOT a <see cref="JsRuntimeException"/>, so a JS try/catch cannot swallow it.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class OptionalChainShortCircuit
        Inherits Exception

        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace

