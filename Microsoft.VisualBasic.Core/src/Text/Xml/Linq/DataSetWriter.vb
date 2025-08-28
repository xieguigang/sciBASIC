#Region "Microsoft.VisualBasic::f9dc30f5ca3fe7670a67c574f5b8d493, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\DataSetWriter.vb"

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

    '   Total Lines: 122
    '    Code Lines: 57 (46.72%)
    ' Comment Lines: 48 (39.34%)
    '    - Xml Docs: 43.75%
    ' 
    '   Blank Lines: 17 (13.93%)
    '     File Size: 4.93 KB


    '     Class DataSetWriter
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, Flush, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel

Namespace Text.Xml.Linq

    ''' <summary>
    ''' Write a very large dataset in Xml format
    ''' </summary>
    ''' <remarks>
    ''' usually be used for dump the database table data to a xml file
    ''' 
    ''' the output xml file format is compatible with the .NET DataSet xml format
    ''' 
    ''' see also: https://docs.microsoft.com/en-us/dotnet/api/system.data.dataset.writexml?view=net-5.0
    ''' 
    ''' example:
    ''' 
    ''' ```vb
    ''' Dim writer As New DataSetWriter(Of biocad_registryModel.molecule)("molecules.xml")
    ''' 
    ''' For Each mol As biocad_registryModel.molecule In registry.molecule.all(Of biocad_registryModel.molecule)()
    '''     Call writer.Write(mol)
    ''' Next
    ''' 
    ''' Call writer.Dispose()
    ''' ```
    ''' </remarks>
    Public Class DataSetWriter(Of T) : Implements IDisposable

        Dim file As StreamWriter
        Dim indentBlank$ = "   "
        Dim leaveOpen As Boolean = True

        Public Const DataSetPrefix$ = "XmlDataSetOf"

        Sub New(file As StreamWriter, Optional leaveOpen As Boolean = True)
            Me.leaveOpen = leaveOpen

            Me.file = file
            Me.file.WriteLine(NodeIterator.XmlDeclare.Replace("utf-16", XmlDeclaration.ToEncoding(file.Encoding).Description.ToLower))
            Me.file.WriteLine($"<{DataSetPrefix}{GetType(T).Name} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
            Me.file.WriteLine(indentBlank & "<!--")
            Me.file.WriteLine(XmlDataModel.GetTypeReferenceComment(GetType(T), 6))
            Me.file.WriteLine(indentBlank & "-->")
        End Sub

        ''' <summary>
        ''' Create a new xml dataset writer
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="encoding"></param>
        Sub New(file As String, Optional encoding As XmlEncodings = XmlEncodings.UTF16)
            ' 20190419 因为VB.NET生成的Xml文件默认是unicode编码的
            ' 但是文本编码默认是utf8的, 所以可能会出现下面的错误
            ' 
            ' System.Xml.XmlException: 'There is no Unicode byte order mark. Cannot switch to Unicode.'
            '
            ' 下面的两行代码是专门用来处理编码问题来避免出现上面的错误
            '
            Call Me.New(file.OpenWriter(encoding.TextEncoding), leaveOpen:=False)
        End Sub

        Public Sub Write(data As T)
            Dim xml As String() = data.GetXml.LineTokens.Skip(1).ToArray

            xml(0) = xml(0) _
                .Replace("xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "") _
                .Replace("xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "") _
                .Replace("  ", "")

            For Each line As String In xml
                Call file.Write(indentBlank)
                Call file.WriteLine(line)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Flush()
            Call file.Flush()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call file.WriteLine($"</{DataSetPrefix}{GetType(T).Name}>")
                    Call file.Flush()

                    If Not leaveOpen Then
                        Call file.Close()
                        Call file.Dispose()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
