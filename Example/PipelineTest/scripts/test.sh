# file inputs
# read file input as text file and output the processed content as json
rm ./1.txt
./bin/PipelineTest.exe /file /in ./input.sh /out ./1.txt

echo 
cat ./1.txt
echo 

echo Test success 1

# std inputs
rm ./2.txt
./input.sh | ./bin/PipelineTest.exe /std > ./2.txt

echo 
cat ./2.txt
echo 

echo Test success 2

# both supports
# just using standard input/output
rm ./3.txt
./input.sh | ./bin/PipelineTest.exe /pipe.Test > ./3.txt

echo 
cat ./3.txt
echo 

# using file io
rm ./4.txt
./bin/PipelineTest.exe /pipe.Test /in ./input.sh /out ./4.txt

echo 
cat ./4.txt
echo 

echo Test success 3
