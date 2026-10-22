#Region "Microsoft.VisualBasic::2b7e9d41a0c38f4e8d6b5c2a91f0e374, vs_solutions\JavaScript\TypeDefinitions\TsOptions.vb"

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

    '     Class TypeDefinitionOptions

    '         Properties: ExplicitOnly, Globals, IncludeNonPublic, XmlDocPath

    ' /********************************************************************************/

#End Region

Namespace TypeDefinitions

    ''' <summary>
    ''' Options of the TypeScript ``.d.ts`` generation pipeline.
    ''' </summary>
    Public Class TypeDefinitionOptions

        ''' <summary>
        ''' Path of the XML documentation file. When <see langword="Nothing"/> (default)
        ''' the generator looks for ``&lt;assembly file name&gt;.xml`` next to the dll
        ''' and silently continues without comments when there is none.
        ''' </summary>
        Public Property XmlDocPath As String

        ''' <summary>
        ''' Emit a global alias per public type (<c>type X = Ns.X;</c> plus, for
        ''' constructible kinds, <c>declare const X: typeof Ns.X;</c>), so a script can
        ''' write <c>new X()</c> / <c>X.StaticMember</c> directly — matching how a host
        ''' publishes types through <see cref="Runtime.ClrInterop.Register"/>. Default True.
        ''' </summary>
        Public Property Globals As Boolean = True

        ''' <summary>
        ''' Whitelist mode for every type, mirroring <see cref="Runtime.ClrInterop.ExplicitOnly"/>:
        ''' only members marked with <see cref="Runtime.JsExposeAttribute"/> stay visible.
        ''' Default False.
        ''' </summary>
        Public Property ExplicitOnly As Boolean = False

        ''' <summary>
        ''' Also emit non-public (Friend/internal) types. Default False — the script
        ''' bridge can only reach public members anyway.
        ''' </summary>
        Public Property IncludeNonPublic As Boolean = False

    End Class

End Namespace
