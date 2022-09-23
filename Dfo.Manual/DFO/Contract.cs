using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dfo_toa_manual.DFO
{
    /**
     {
        "sekvensnr": "5223",
        "kontraktnr": "00001376",
        "ansattId": "05001234",
        "status": "520-Signert",
        "kostnadssted": "12345",
        "organisasjonId": 10001234,
        "firmakode": "1234",
        "dato": "2001-01-01",
        "tidspunkt": "14:00:00",
        "filtype": "application/pdf",
        "filinnhold": "JVBERi0xLjQNMSAwIG9iag08PCAvVHlwZSAvWE9iamVjdCAvU3VidHlwZSAvSW1h"
     }
    */
    internal class Contract
    {
        private string _sequenceNumber;
        public string SequenceNumber => _sequenceNumber;
        private string _contractId;
        public string ContractId => _contractId;
        private string _employeeId;
        public string EmployeeId => _employeeId;
        private string _status;
        public string Status => _status;
        private string _costCentre;
        public string CostCentre => _costCentre;
        private int _organizationId;
        public int OrganizationId => _organizationId;
        private string _companyCode;
        public string CompanyCode => _companyCode;
        private string _date;
        public string Date => _date;
        private string _time;
        public string Time => _time;
        private string _fileType;
        public string FileType => _fileType;
        private string _fileContent;
        public string FileContent => _fileContent;


        public Contract(dynamic contractData)
        {
            this._sequenceNumber = (string)contractData.sekvensnr;
            this._contractId = (string)contractData.kontraktnr;
            this._employeeId = (string)contractData.ansattId;
            this._status = (string)contractData.status;
            this._costCentre = (string)contractData.kostnadssted;
            this._organizationId = contractData.organisasjonsId ?? 0;
            this._companyCode = (string)contractData.firmakode;
            this._date = (string)contractData.dato;
            this._time = (string)contractData.tidspunkt;
            this._fileType = (string)contractData.filtype;
            this._fileContent = (string)contractData.filinnhold;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + _sequenceNumber + ";" + _contractId + ";" + _employeeId + ";" + _status + ";" + _costCentre + ";" + _organizationId.ToString() + ";" + _companyCode + ";" + _date + ";" + _time + ";" + _fileType + ";" + _fileContent;
        }
    }
}
