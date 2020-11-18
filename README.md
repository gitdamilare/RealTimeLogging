# Tecan Solution - Real-Time Statistical Dashboard

## Problem  
Our customers are interested in statistical data of their Tecan products. To facilitate this, design a system to gather and display run time data. Our robots write log files in a specific predefined structure. Once a log file reaches a certain size, it is zipped, and logging continues in a new log file. These log files shall be analyzed, and the statistical information shall be displayed on a web application

1. Analysis of multiple log files at the same time (Priority 1)
2. Extraction of the following data:
   - Number of errors and warnings (Priority 1)
   - Overall run time (Priority 3)
   - The complete number of messages which have been parsed (Priority 3)
3. Extendable to analyze more data from the log files in the future (Priority 2)
4. Display the following statistical data:
   - Number of errors and warnings (Priority 1)
   - Overall run time (Priority 3)
   - The complete number of messages which have been parsed (Priority 3)
5. Refresh the UI when new statistical data has been received (Priority 1)

## Solution WorkFlow 
<img alt="portfolio_view" src="https://github.com/gitdamilare/TecanSolution/blob/main/workflow.png">

## Prerequirements 
* Visual Studio 2019
* .NET Core SDK 3.1
* SQL Server
* Angualr CLI 9.0 above
* Azure SDK 

## Running this Project
**All Restful APIs,Azure functions, Database are hosted on Azure already and connected to the Frontend project**
**Running just the frontend gets the application fired up**

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

 ## BackEnd API Documentation
[Swagger UI](https://reportlogapiservices.azurewebsites.net/swagger/index.html)

## Still wants to run Services and Azure Functions Locally 

* Setup up the right connection string from appsetting.json and make changes from startup.cs
* Cd into the ReportLogEntityFrameworkCore project and run the following command
```
Add-Migration "<name>"
Update-Database
```
* Also in the logBlobTriggerFunction, in the host.json or local.setting.json change the connections string

* Run logBlobTriggerFunction and ReportLogAPI project together. 
[How to: Set multiple startup projects](https://docs.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2019)

## Performance Improvement Plan
* When file is uploaded, Process it and then make a Signal R call to notify all connected devices before saving to Database
* Use Azure CosmosDB or MongoDB, a little performance bump up 
