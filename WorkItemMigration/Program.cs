using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TFRestApiApp
{
    class Program
    {
        //Used for building configuration file to grab Personal Access Token from Secret Manager 
        private static IConfigurationRoot Configuration;
        
        
        //URL for Azure DevOps Project
        static readonly string devOpsOrgUrl = "https://dev.azure.com/miisaac/";

        //Learn more about Work Item Tracking at https://docs.microsoft.com/en-us/azure/devops/integrate/quickstarts/work-item-quickstart?view=azure-devops
        static WorkItemTrackingHttpClient WitClient;
        

        static void Main(string[] args)
        {
            //Grab Personal Access Token from Secret Manager. Personal Access Token is stored in secret manager in the form "Pat":"<your personal access token here>"
            //Learn about Secret Manager at : https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Program>();
            Configuration = builder.Build();

            try
            {
                //Connect to Azure DevOps Organization using the OrgUrl and PAT
                ConnectWithPAT(devOpsOrgUrl, Configuration["Pat"]);

                //Set parameters (1. work item id to move if moving single work item, for example to test that this works. 2.name of teamProject source & destination, and 
                //3. query path i.e path to your Query created in Azure Dev Ops)
                
                string sourceTeamProject = "interro.us", destinationTeamProject = "PeerTutor.us"; //Replace the team projects names here with the names of your own projects
               
                string queryPath = "Shared Queries/Move"; //Replace this string with the path to your own query. Learn about queries here: https://docs.microsoft.com/en-us/azure/devops/boards/queries/?view=azure-devops

                //Move only one work item
                //int wiIdToMove = 1; //workitem id
                //MoveWorkItem(wiIdToMove, destinationTeamProject);

                //Move work items from a flat query result
                List<int> wis = RunStoredQuery(sourceTeamProject, queryPath);
                foreach (int wiId in wis) MoveWorkItem(wiId, destinationTeamProject);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null) Console.WriteLine("Detailed Info: " + ex.InnerException.Message);
                Console.WriteLine("Stack:\n" + ex.StackTrace);
                
            }
        }


        /// <summary>
        /// Run query and show result (only flat)
        /// </summary>
        /// <param name="wiqlStr">Wiql String</param>
        static List<int> GetQueryResult(string wiqlStr, string teamProject)
        {
            WorkItemQueryResult result = RunQueryByWiql(wiqlStr, teamProject);

            if (result != null)
            {
                if (result.WorkItems != null) // this is Flat List 
                    return (from wis in result.WorkItems select wis.Id).ToList();
                else Console.WriteLine("There is no query result");
            }

            return new List<int>();
        }

        /// <summary>
        /// Run Query with Wiql
        /// </summary>
        /// <param name="wiqlStr">Wiql String</param>
        /// <returns></returns>
        static WorkItemQueryResult RunQueryByWiql(string wiqlStr, string teamProject)
        {
            Wiql wiql = new Wiql();
            wiql.Query = wiqlStr;

            if (teamProject == "") return WitClient.QueryByWiqlAsync(wiql).Result;
            else return WitClient.QueryByWiqlAsync(wiql, teamProject).Result;
        }

        /// <summary>
        /// Run stored query on azure devops service
        /// </summary>
        /// <param name="project">Team Project Name</param>
        /// <param name="queryPath">Path to Query</param>
        static List<int> RunStoredQuery(string project, string queryPath)
        {
            QueryHierarchyItem query = WitClient.GetQueryAsync(project, queryPath, QueryExpand.Wiql).Result;

            string wiqlStr = query.Wiql;

            return GetQueryResult(wiqlStr, project);
        }

        /// <summary>
        /// Move work item to new team project
        /// </summary>
        /// <param name="WIId"></param>
        /// <param name="destinationTeamProject"></param>
        /// <returns></returns>
        static int MoveWorkItem(int WIId, string destinationTeamProject)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();

            //Fields Reference available at : https://docs.microsoft.com/en-us/azure/devops/reference/xml/reportable-fields-reference?view=azure-devops&viewFallbackFrom=vsts
            fields.Add("System.TeamProject", destinationTeamProject);
            fields.Add("System.AreaPath", destinationTeamProject);
            fields.Add("System.IterationPath", destinationTeamProject);

            var editedWI = UpdateWorkItem(WIId, fields);

            Console.WriteLine("Work item has been moved: " + editedWI.Id.Value);

            return editedWI.Id.Value;
        }

        /// <summary>
        /// Update a work item
        /// </summary>
        /// <param name="WIId"></param>
        /// <param name="Fields"></param>
        /// <returns></returns>
        static WorkItem UpdateWorkItem(int WIId, Dictionary<string, object> Fields)
        {
            JsonPatchDocument patchDocument = new JsonPatchDocument();

            foreach (var key in Fields.Keys)
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/" + key,
                    Value = Fields[key]
                });

            return WitClient.UpdateWorkItemAsync(patchDocument, WIId).Result;
        }

        /// <summary>
        /// Get one work item
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        static WorkItem GetWorkItem(int Id)
        {
            return WitClient.GetWorkItemAsync(Id).Result;
        }

        #region create new connections
        static void InitClients(VssConnection Connection)
        {
            WitClient = Connection.GetClient<WorkItemTrackingHttpClient>();
            
        }

        static void ConnectWithDefaultCreds(string ServiceURL)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new VssCredentials());
            InitClients(connection);
        }

        static void ConnectWithCustomCreds(string ServiceURL, string User, string Password)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new WindowsCredential(new NetworkCredential(User, Password)));
            InitClients(connection);
        }

        static void ConnectWithPAT(string ServiceURL, string PAT)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new VssBasicCredential(string.Empty, PAT));
            InitClients(connection);
        }
        #endregion

        
    }
}
