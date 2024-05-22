#Region "Microsoft.VisualBasic::0c0c6cdbf3c27738d9dcadaf89831893, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Serialization\NamespaceDoc.vb"

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

    '   Total Lines: 44
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 6 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (9.09%)
    '     File Size: 1.97 KB


    '     Module NamespaceDocExtensions
    ' 
    '         Function: IsNamespaceDoc, ScanAnnotations
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Public Module NamespaceDocExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsNamespaceDoc(type As ProjectType) As Boolean
            Return type.Name = APIExtensions.NamespaceDoc
        End Function

        ''' <summary>
        ''' 一般而言，一个命名空间之下只需要有一个注释类型即可，如果存在多个注释类型的话
        ''' 注释信息字符串会被合并在一起
        ''' </summary>
        ''' <param name="project"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScanAnnotations(project As ProjectSpace) As Dictionary(Of String, String)
            Dim annotations = project _
                .Select(Function(proj) proj.Namespaces) _
                .IteratesALL _
                .Select(Function(ns) ns.Types) _
                .IteratesALL _
                .Where(AddressOf IsNamespaceDoc) _
                .GroupBy(Function(doc) doc.Namespace.fullName) _
                .ToDictionary(Function(ns) ns.Key,
                              Function(nsGroup)
                                  Return nsGroup _
                                      .Select(Function(doc) doc.Summary) _
                                      .Where(Function(s)
                                                 Return Not s.StringEmpty
                                             End Function) _
                                      .Distinct _
                                      .JoinBy(ASCII.LF & ASCII.LF)
                              End Function)
            Return annotations
        End Function
    End Module
End Namespace
