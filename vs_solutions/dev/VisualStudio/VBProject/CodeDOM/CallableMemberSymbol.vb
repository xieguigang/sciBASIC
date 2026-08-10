#Region "Microsoft.VisualBasic::b8ecdba7ec49800388d1418b08966faa, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\CallableMemberSymbol.vb"

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

    '   Total Lines: 39
    '    Code Lines: 19 (48.72%)
    ' Comment Lines: 13 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (17.95%)
    '     File Size: 1.32 KB


    '     Class CallableMemberSymbol
    ' 
    '         Properties: Locals, Parameters, ReturnType
    ' 
    '     Class MethodSymbol
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

    ''' <summary>
    ''' base for callable members that own parameters, a return type and a body
    ''' with local variables: <see cref="MethodSymbol"/> and <see cref="PropertySymbol"/>.
    ''' </summary>
    Public MustInherit Class CallableMemberSymbol : Inherits MemberSymbol
        Public Property Parameters As Dictionary(Of String, TypeInfo)
        ''' <summary>
        ''' return type; Nothing for Sub / Sub New / Property without an explicit As clause.
        ''' </summary>
        Public Property ReturnType As TypeInfo
        ''' <summary>
        ''' local variables declared inside the member body (Dim XXX As XXX).
        ''' </summary>
        Public Property Locals As Dictionary(Of String, VariableSymbol)
    End Class

    ''' <summary>
    ''' function / sub / operator / sub new (constructor).
    ''' </summary>
    Public Class MethodSymbol : Inherits CallableMemberSymbol

        Private _type As SymbolType

        Public Overrides ReadOnly Property Type As SymbolType
            Get
                Return _type
            End Get
        End Property

        Public Sub New(type As SymbolType)
            _type = type
        End Sub

    End Class
End Namespace
