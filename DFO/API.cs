using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace dfo_toa_manual.DFO
{
    internal class API
    {
        public static List<string> getContractSequenceList(HttpClient client, string dateFrom, string dateTo)
        {
            HttpResponseMessage response = client.GetAsync("infokontrakterFiler/v2/?q=dato+ge+'" + dateFrom + "'+AND+dato+le+'" + dateTo + "'").Result;
            if (response.IsSuccessStatusCode)
            {
                dynamic contracts = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                contracts = contracts.InfotakFilRespons;
                List<string> listOfContracts = new List<string>();
                foreach (dynamic contractData in contracts)
                {
                    listOfContracts.Add((string)contractData.sekvensnr);
                }
                return listOfContracts;
            }
            else
            {
                throw new Exception("DFO API call failed. " + (int)response.StatusCode + " " + response.ReasonPhrase + " " + response.ToString());
            }
        }

        public static Contract getContract(HttpClient client, string sequenceNumber)
        {
            HttpResponseMessage response = client.GetAsync("infokontrakterFiler/v2/" + sequenceNumber).Result;
            if (response.IsSuccessStatusCode)
            {
                dynamic contractData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return new Contract(contractData.InfotakFilRespons[0]);
            }
            else
            {
                throw new Exception("DFO API call failed. " + (int)response.StatusCode + " " + response.ReasonPhrase + " " + response.ToString());
            }
        }

        public static EmployeeContract getEmployeeContract(HttpClient client, string employeeId, string contractId)
        {            
            HttpResponseMessage response = client.GetAsync("ansatteInfokontrakter/v2/?q=id+eq+" + employeeId + "&kontraktnr+eq+" + contractId).Result;
            if (response.IsSuccessStatusCode)
            {
                dynamic employeeContractData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return new EmployeeContract(employeeContractData.AnsattInfotak[0]);
            }
            else
            {
                throw new Exception("DFO API call failed. " + (int)response.StatusCode + " " + response.ReasonPhrase + " " + response.ToString());
            }
        }

        public static Employee getEmployee(HttpClient client, string id)
        {
            HttpResponseMessage response = client.GetAsync("ansatte/v2/" + id).Result;
            if (response.IsSuccessStatusCode)
            {
                dynamic employeeData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return new Employee(employeeData.ansatt[0]);
            }
            else
            {
                throw new Exception("DFO API call failed. " + (int)response.StatusCode + " " + response.ReasonPhrase + " " + response.ToString());
            }
        }
    }
}
