#Region "Microsoft.VisualBasic::8f3a1c07d29b4e5fa6c0d1b3478e5a21, vs_solutions\JavaScript\TypeDefinitions\NamespaceDoc.vb"

    ' Author:
    ' 
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

    '     Module NamespaceDoc

    ' /********************************************************************************/

#End Region

Namespace TypeDefinitions

    ''' <summary>
    ''' TypeScript ``.d.ts`` definition file generation for CLR assemblies reflected
    ''' through the LiteJs interop bridge.
    ''' <para>
    ''' The pipeline is: <see cref="AssemblyScanner"/> reflects the exported types
    ''' through the very same <see cref="Runtime.ClrMembers"/> views the script bridge
    ''' uses (so visibility rules can never drift), <see cref="TypeMapper"/> renders
    ''' each CLR type as a TypeScript type following the <see cref="Runtime.ClrConvert"/>
    ''' conversion semantics, <see cref="XmlDocIndex"/> / <see cref="JsDocWriter"/>
    ''' turn the assembly XML documentation into JSDoc comments, and
    ''' <see cref="TsEmitter"/> writes the single self-contained ``.d.ts`` file.
    ''' </para>
    ''' </summary>
    Module NamespaceDoc

    End Module
End Namespace
