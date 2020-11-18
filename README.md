# Tecan Solution - Real-Time Statistical Dashboard

## Problem  
Our customers are interested in statistical data of their Tecan products. To facilitate this, design a system to gather and display run time data. Our robots write log files in a specific predefined structure. Once a log file reaches a certain size, it is zipped, and logging continues in a new log file. These log files shall be analyzed, and the statistical information shall be displayed on a web application ...

## Solution WorkFlow 
<img alt="portfolio_view" src="https://github.com/gitdamilare/TecanSolution/blob/main/workflow.PNG">

## Prerequirements 
* Visual Studio 2019
* .NET Core SDK 3.1
* SQL Server
* Angualr CLI 9.0 above
* Azure SDK 

## How To Run 
*All Restful APIs,Azure functions, Database are hosted on Azure already and connected to the Frontend project. 
*Running just the frontend gets the application fired up

* Clone this project 
* Open the ReportLogFrontend folder in VS Code and then run the following command to get all the packages in your project.
```
npm install 
```
* Now you can run the frontend project with following command. *Note: if it not running on port 4200, make changes in package.json and angular.json // or add --port 4200 to command
```
ng serve
```
if you are still facing some issues check the following command and resolve the issues if you have any.
```
npm audix fix
```
