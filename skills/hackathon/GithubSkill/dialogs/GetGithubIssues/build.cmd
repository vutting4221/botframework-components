
@echo off
setlocal

set region=%1
if "%region%" EQU "" set region=westus

set key=%2
if "%2" NEQ "" set key=--authoringKey %2

set alias=%3

set bot=%4

echo Building LUIS models
bf luis:build --luConfig luconfig.json --region=%region% %key% --suffix %alias% --botName %bot% --dialog

goto done

:help
echo build.cmd [region] [authoringKey]
echo Region defaults to westus.
echo Must have an explicit key or set it using "bf config:set:luis --authoringKey <LUISKEY>"
:done