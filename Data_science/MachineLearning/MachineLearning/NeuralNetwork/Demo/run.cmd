@echo off

SET ANN="../../../../../CLI_tools/Apps/ANN.exe"
SET trainingSet="./Demo_data.Xml"
SET configuration="./config.ini"

%ANN% /training /samples %trainingSet% /config %configuration% /debug