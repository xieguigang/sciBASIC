#!/bin/sh

# file inputs
# read file input as text file and output the processed content as json
./bin/PipelineTest.exe /file /in ./input.sh /out ./1.txt

# std inputs
./input.sh | ./bin/PipelineTest.exe /std > ./2.txt

# both supports
# just using standard input/output
./input.sh | ./bin/PipelineTest.exe /pipe.Test > ./3.txt
# using file io
./bin/PipelineTest.exe /pipe.Test /in ./input.sh /out ./4.txt