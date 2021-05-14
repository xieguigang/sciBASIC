#Region "Microsoft.VisualBasic::b52830da536b47a8c6e0f27c10d6a98b, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\SharedORM\Languages\Perl.vb"

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

    '     Class Perl
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetSourceCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 为了构建GCModeller的脚本自动化而构建的源代码生成器，生成``perl module(*.pm)``
    ''' </summary>
    Public Class Perl : Inherits CodeGenerator

        Dim namespace$

        ' #!/usr/bin/perl 
        '
        ' package assemblyName;
        '
        ' sub new {
        '
        '     my $class = shift;
        '     my $self = {
        '
        '         _memberField1 => shift,
        '         _memberField2 => shift,
        '
        '     };
        '
        '     bless $self, $class;
        '     return $self;
        ' }
        '
        ' sub public_function {
        '
        '     my ($self, $arg1, $arg2) = @_;
        '
        '     # using class member
        '     #
        '     # get value
        '     my $m1 = $self->{_memberField1};
        '     
        '     # set value
        '     $self->{_memberField1} = $arg1;
        ' }
        '
        ' sub readonly_property {
        '     my ($self) = @_;
        '
        '     # return field value
        '     return $self->{_memberField2};
        ' }
        '
        '
        ' # usage
        '
        ' #!/usr/bin/perl
        '
        ' use assemblyName;
        '
        ' my $App = new assemblyName();
        '
        ' # invoke
        ' $App->public_function($arg1, $arg2);
        '
        ' # get property value
        ' my $value = $App->readonly_property();

        Public Sub New(CLI As Type, namespace$)
            MyBase.New(CLI)
            Me.namespace = [namespace]
        End Sub

        Public Overrides Function GetSourceCode() As String
            Dim perl As New StringBuilder

            Call perl.AppendLine("#!/usr/bin/perl")
            Call perl.AppendLine()

            Call perl.AppendLine($"package {exe};")

            Call perl.AppendLine("sub new {")
            Call perl.AppendLine("my $class = shift;")
            Call perl.AppendLine("my $self = {")
            Call perl.AppendLine("    path = shift")
            Call perl.AppendLine("};")
            Call perl.AppendLine("bless $self, $class;")
            Call perl.AppendLine("return $self;")
            Call perl.AppendLine("}")

            Return perl.ToString
        End Function
    End Class
End Namespace
