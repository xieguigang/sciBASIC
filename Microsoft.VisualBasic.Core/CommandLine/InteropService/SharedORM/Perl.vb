#Region "Microsoft.VisualBasic::b52830da536b47a8c6e0f27c10d6a98b, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\Perl.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



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
