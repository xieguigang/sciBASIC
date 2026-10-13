#Region "Microsoft.VisualBasic::919c0c55b5dfc275525a7ab9edb9a2b2, vs_solutions\JavaScript\Runtime\ClrObject.vb"

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

    '   Total Lines: 101
    '    Code Lines: 43 (42.57%)
    ' Comment Lines: 37 (36.63%)
    '    - Xml Docs: 94.59%
    ' 
    '   Blank Lines: 21 (20.79%)
    '     File Size: 4.05 KB


    '     Class ClrObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BoundMethod, ToString
    ' 
    '     Class ClrType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ClrBoundMethod
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' A .NET object exposed to script through reflection (see <see cref="ClrInterop"/>).
    ''' <para>
    ''' Script member access (<c>obj.Name</c>, <c>obj.Name = v</c>, <c>obj.Method(...)</c>,
    ''' <c>obj[i]</c>) is resolved by <see cref="ClrInterop"/> against the cached
    ''' reflection view of <see cref="TargetType"/>. The proxy is neither a
    ''' <c>Dictionary</c> nor a <c>List</c>, so the engine's native object model keeps
    ''' working unchanged and every dispatch point simply gains one explicit branch.
    ''' </para>
    ''' <para>
    ''' A VALUE-TYPE target lives here as a BOXED COPY: writing a struct member
    ''' (<c>host.point.X = 1</c>) mutates that copy only and never writes back to the
    ''' property/field the copy came from. Reference-type targets are the object
    ''' itself, so their writes are visible to host code.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class ClrObject

        ''' <summary>The wrapped instance (never Nothing).</summary>
        Public ReadOnly Target As Object

        ''' <summary>Runtime type of <see cref="Target"/>.</summary>
        Public ReadOnly TargetType As Type

        ' bound methods are cached per name so that obj.m === obj.m holds (JS prototype
        ' methods have a stable identity) and repeated member reads stay allocation-free
        Private ReadOnly _methods As New Dictionary(Of String, ClrBoundMethod)

        Public Sub New(target As Object)
            Me.Target = target
            Me.TargetType = target.GetType()
        End Sub

        ''' <summary>The bound method value for a member name (cached per name).</summary>
        Friend Function BoundMethod(name As String) As ClrBoundMethod
            SyncLock _methods
                Dim m As ClrBoundMethod = Nothing
                If _methods.TryGetValue(name, m) Then Return m
                m = New ClrBoundMethod(Me, name)
                _methods(name) = m
                Return m
            End SyncLock
        End Function

        ''' <summary>Script rendering — <c>console.log</c> uses <see cref="JsRuntime.JsDisplay"/>.</summary>
        Public Overrides Function ToString() As String
            Return "[object Object]"
        End Function

    End Class

    ''' <summary>
    ''' A .NET <see cref="System.Type"/> exposed to script: its static members are
    ''' reachable by name and <c>new T(...)</c> runs one of its public constructors.
    ''' <c>typeof T</c> reports "function" and <c>x instanceof T</c> tests the CLR
    ''' type relation, mirroring how a JS class behaves.
    ''' </summary>
    Public NotInheritable Class ClrType

        ''' <summary>The exposed type.</summary>
        Public ReadOnly UnderlyingType As Type

        Public Sub New(underlyingType As Type)
            Me.UnderlyingType = underlyingType
        End Sub

        Public Overrides Function ToString() As String
            Return "class " & UnderlyingType.Name
        End Function

    End Class

    ''' <summary>
    ''' A CLR method bound to its receiver (a <see cref="ClrObject"/> or a
    ''' <see cref="ClrType"/>). Member access creates it; <see cref="JsRuntime.JsInvoke"/>
    ''' dispatches it, so overload resolution happens at call time against the actual
    ''' script arguments.
    ''' </summary>
    Public NotInheritable Class ClrBoundMethod

        ''' <summary>Receiver of the call: a <see cref="ClrObject"/> or a <see cref="ClrType"/>.</summary>
        Public ReadOnly Receiver As Object

        ''' <summary>The CLR member name this value was read from.</summary>
        Public ReadOnly Name As String

        Public Sub New(receiver As Object, name As String)
            Me.Receiver = receiver
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return "[function: " & Name & "]"
        End Function

    End Class

End Namespace

