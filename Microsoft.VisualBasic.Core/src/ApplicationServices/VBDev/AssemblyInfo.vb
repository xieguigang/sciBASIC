#Region "Microsoft.VisualBasic::0ce5aaa98206b83d5235fd6a69ca136f, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\AssemblyInfo.vb"

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

    '   Total Lines: 102
    '    Code Lines: 46 (45.10%)
    ' Comment Lines: 36 (35.29%)
    '    - Xml Docs: 58.33%
    ' 
    '   Blank Lines: 20 (19.61%)
    '     File Size: 3.75 KB


    '     Class AssemblyInfo
    ' 
    '         Properties: AssemblyCompany, AssemblyCopyright, AssemblyDescription, AssemblyFileVersion, AssemblyFullName
    '                     AssemblyInformationalVersion, AssemblyProduct, AssemblyTitle, AssemblyTrademark, AssemblyVersion
    '                     BuiltTime, ComVisible, Guid, Name, TargetFramework
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [GetType], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
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

        ''' <summary>
        ''' Gets the <see cref="Type"/> object with the specified name in the assembly instance.
        ''' </summary>
        ''' <param name="fullName">The full name of the type.</param>
        ''' <returns>An object that represents the specified class, or null if the class is not found.</returns>
        Public Overloads Shared Function [GetType](fullName As String) As Type
            Static cache As New Dictionary(Of String, Type)

            SyncLock cache
                If Not cache.ContainsKey(fullName) Then
                    Dim t As Type

                    For Each a As Assembly In AppDomain.CurrentDomain.GetAssemblies
                        t = a.GetType(fullName)

                        If Not t Is Nothing Then
                            cache(fullName) = t
                            Return t
                        End If
                    Next

                    Return Nothing
                Else
                    Return cache(fullName)
                End If
            End SyncLock
        End Function
    End Class
End Namespace
