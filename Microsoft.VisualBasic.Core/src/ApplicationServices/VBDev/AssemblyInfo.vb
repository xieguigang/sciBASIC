#Region "Microsoft.VisualBasic::e6a929b5c8c3773280f44496b75bc3d2, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\AssemblyInfo.vb"

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

    '   Total Lines: 75
    '    Code Lines: 28
    ' Comment Lines: 31
    '   Blank Lines: 16
    '     File Size: 2.65 KB


    '     Class AssemblyInfo
    ' 
    '         Properties: AssemblyCompany, AssemblyCopyright, AssemblyDescription, AssemblyFileVersion, AssemblyFullName
    '                     AssemblyInformationalVersion, AssemblyProduct, AssemblyTitle, AssemblyTrademark, AssemblyVersion
    '                     BuiltTime, ComVisible, Guid, Name, TargetFramework
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ApplicationServices.Development

    ''' <summary>
    ''' ``My Project\AssemblyInfo.vb``
    ''' </summary>
    Public Class AssemblyInfo : Implements INamedValue

        ' General Information about an assembly is controlled through the following
        ' set of attributes. Change these attribute values to modify the information
        ' associated with an assembly.

        ' Review the values of the assembly attributes

        Public Property AssemblyTitle As String
        Public Property AssemblyDescription As String
        Public Property AssemblyCompany As String
        Public Property AssemblyProduct As String
        Public Property AssemblyCopyright As String
        Public Property AssemblyTrademark As String

        Public Property AssemblyInformationalVersion As String

        Public Property TargetFramework As String

        Public Property ComVisible As Boolean
        Public Property Name As String Implements INamedValue.Key

        ''' <summary>
        ''' The following GUID is for the ID of the typelib if this project is exposed to COM
        ''' </summary>
        ''' <returns></returns>
        Public Property Guid As String

        ' Version information for an assembly consists of the following four values:
        '
        '      Major Version
        '      Minor Version
        '      Build Number
        '      Revision
        '
        ' You can specify all the values or you can default the Build and Revision Numbers
        ' by using the '*' as shown below:
        ' <Assembly: AssemblyVersion("1.0.*")>

        Public Property AssemblyVersion As String
        Public Property AssemblyFileVersion As String

        <XmlAttribute>
        Public Property AssemblyFullName As String

        ''' <summary>
        ''' The compile date and time of the assembly file.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ##### 20210715
        ''' 
        ''' 当这个值为空值的时候，JSON序列化会出错；
        ''' 所以假若需要进行JSON序列化，请及时赋值一个时间值，或者直接使用<see cref="Now"/>进行初始化
        ''' </remarks>
        Public Property BuiltTime As Date

        Public Const ProjectFile As String = "My Project\AssemblyInfo.vb"

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return AssemblyTitle
        End Function
    End Class
End Namespace
