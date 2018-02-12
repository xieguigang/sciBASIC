Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Development

    ' Here is the example:

#Region "b77a5c561934e089; ./Tools/SoftwareToolkits/LicenseMgr.vb"

    ' Author:                                                                         <-- authors
    '
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) 2016 amethyst.asuka@gcmodeller.org                                <-- copyright
    '
    '
    ' The MIT License (MIT)                                                           <-- title
    '
    ' Permission is hereby granted, free of charge, to any person obtaining a copy    <-- biref
    ' of this software and associated documentation files (the "Software"), to deal
    ' in the Software without restriction, including without limitation the rights
    ' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ' copies of the Software, and to permit persons to whom the Software is
    ' furnished to do so, subject to the following conditions:
    '
    ' The above copyright notice and this permission notice shall be included in
    ' all copies or substantial portions of the Software.
    '
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    ' THE SOFTWARE.

#End Region

    Public Class LicenseInfo : Inherits BaseClass

        Public Property Authors As NamedValue(Of String)()
        Public Property Title As String
        Public Property Copyright As String
        Public Property Brief As String
        Public Property RootDIR As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRelativePath(src As String) As String
            Return ProgramPathSearchTool.RelativePath(RootDIR, src)
        End Function

        Public Overrides Function ToString() As String
            Return Brief
        End Function
    End Class
End Namespace