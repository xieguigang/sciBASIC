my $i = 0;

while ( $i != 10 ) {
   print "current --> $i\n";
   sleep(1);
   
   $i = $i+1;
}

die "test";