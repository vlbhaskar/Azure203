# Azure 203 usefull links

To run .sh files in linux/unix/mac
bash filename.sh

In windows platoform rename the .sh files to .cmd and run cmd files using command promot
windows+Run --> cmd --> in command line enter path of cmd file and enter
https://ss64.com/nt/syntax-run.html

Azure CLI reference: https://docs.microsoft.com/en-us/cli/azure/reference-index?view=azure-cli-latest

Powershell
https://docs.microsoft.com/en-us/powershell/azure/get-started-azureps?view=azps-3.6.1


Overview of Azure Cloud Shell
https://docs.microsoft.com/en-us/azure/cloud-shell/overview?view=azps-3.6.1

ARM Templates reference:
https://docs.microsoft.com/en-us/azure/templates/

Begineer Sample:
https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/template-tutorial-create-first-template?tabs=azure-powershell

Sample Templates
https://docs.microsoft.com/en-us/azure/app-service/samples-resource-manager-templates


Azure Rest APIs
https://docs.microsoft.com/en-us/rest/api/azure/

Manage APP service with REST API
https://docs.microsoft.com/en-us/rest/api/appservice/

Create or update APP service with REST API
https://docs.microsoft.com/en-us/rest/api/appservice/webapps/createorupdate

EventGrid overview: https://docs.microsoft.com/en-us/azure/event-grid/overview

# OMS.Products (Setup instructions)

Step-1:

Login to Azure --> Create Cosmos DB Account(API option as Core(SQL))  --> Open newly created cosmos DB Account --> click on "Data Explorer" Create "New Database" with Name as OMSDB --> Create "New Container" with Name byCategoryID --> Open the container and add below JSON document.

{
     "pid": 3,
    "name": "Samsung 1000 GB HDD",
    "price": 100,
    "brandName": "Samsung",
    "series": "860 EVO",
    "os": "Windows 8/Windows 7/Windows Server 2003 (32-bit and 64-bit), Vista (SP1 and above), XP (SP2 and above), MAC OSX, and Linux",
    "harddisk": "500GB",
    "CategoryID": 1,
    }
    
Step-2:

Go to newly created Azure Cosmos DB account blade --> Click on Keys --> Note down Cosmos DB URL, Primarykey fields

Step-3; 

Open OMS.sln file in VStudio --> Go to OMS.Products Project --> open ProcuctContrller.cs --> replace below two properties with URL, Key notedown in Step-2. 
   private static string CosmosEndpoint = "https://cosmosdb.documents.azure.com:443/";
        private static string CosmosMasterKey = "<enter your primary key in azure portal>";

Step-4: 

In Vstudio --> Right click on project OMS.Products --> and click on "Set as start project" --> click on F5 or Run --> Start debugging --> It will open swagger page

Step-5: 

You can post the documents from Swagger with the JSON format as mentioned below so code will connect to COSMOSDB and isert document. Use get method in Swagger to fetch all doucments.

  # OMS Draft Design
![OMS Design](https://github.com/vlbhaskar/Azure203/blob/master/DesignDiagramlatest.jpg)

