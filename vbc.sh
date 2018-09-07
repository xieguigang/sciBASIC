#!/bin/sh

cli="$@";
mono "~/MSBuild/vbc.exe" $cli
