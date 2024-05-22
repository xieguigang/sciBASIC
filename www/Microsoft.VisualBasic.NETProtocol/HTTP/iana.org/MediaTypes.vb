#Region "Microsoft.VisualBasic::2547dc08db30de3978e4d1e053278609, www\Microsoft.VisualBasic.NETProtocol\HTTP\iana.org\MediaTypes.vb"

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

    '   Total Lines: 23
    '    Code Lines: 8 (34.78%)
    ' Comment Lines: 14 (60.87%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 1 (4.35%)
    '     File Size: 1.07 KB


    ' Class MediaTypes
    ' 
    '     Properties: Name, Reference, Template
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Csv file reader for the csv file list on https://www.iana.org/assignments/media-types/media-types.xhtml
''' 
''' + [application](https://www.iana.org/assignments/media-types/application.csv)
''' + [audio](https://www.iana.org/assignments/media-types/audio.csv)
''' + [font](https://www.iana.org/assignments/media-types/font.csv)
''' + [example]()
''' + [image](https://www.iana.org/assignments/media-types/image.csv)
''' + [message](https://www.iana.org/assignments/media-types/message.csv)
''' + [model](https://www.iana.org/assignments/media-types/model.csv)
''' + [multipart](https://www.iana.org/assignments/media-types/multipart.csv)
''' + [text](https://www.iana.org/assignments/media-types/text.csv)
''' + [video](https://www.iana.org/assignments/media-types/video.csv)
''' </summary>
Public Class MediaTypes
    Public Property Name As String
    Public Property Template As String
    Public Property Reference As String

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
