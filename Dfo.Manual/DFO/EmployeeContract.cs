namespace dfo_toa_manual.DFO
{
    /**
     {
        "id": "05001234",
        "kontraktnr": "ABC-2019",
        "sekvensnr": "5223",
        "typeKontrakt": "T3",
        "overlapp": true,
        "kontraktstype": "ABCD2",
        "organisasjonId": 10007332,
        "kostnadssted": "67165",
        "kostnadsstedNavn": "UiO - fakultetet",
        "stillingId": 30045705,
        "stillingskode": 20000025,
        "stillingstittel": "1364 Seniorrådgiver",
        "yrkeskode": "40000012",
        "lonnstrinn": "62",
        "kronetillegg": "250",
        "arslonn": "500000",
        "dellonnsprosent": "100",
        "totalTimer": "150",
        "timerFort": "150",
        "totalKontraktssaldo": "5000",
        "startdato": "2001-01-01",
        "sluttdato": "2020-01-01",
        "saksbehandler": "9900KOPA",
        "konteringLinjeniva": [{
            "arbeidsoppgave": "Assistant",
            "arbeidsoppgaveTekst": "assistentarbeid",
            "lonnart": "1234",
            "lonnartTekst": "Ferie med lønn",
            "landkode": "NO",
            "belop": "500,00",
            "kostnadssted": "12345",
            "kElement4": "123456",
            "kElement5": "1122",
            "kElement6": "99001234",
            "kElement7": "ABC123456789",
            "regnskapskonto": "4321"
        }],
        "endretDato": "2018-12-11",
        "endretAv": "5100ABCD"
     }
    */
    internal class EmployeeContract
    {
        private string _id;
        public string Id => _id;
        private string _contractId;
        public string ContractId => _contractId;
        public string _sequenceNumber;
        public string SequenceNumber => _sequenceNumber;

        public EmployeeContract(dynamic employeeContractData)
        {
            this._id = (string)employeeContractData.id;
            this._contractId = (string)employeeContractData.kontraktnr;
            this._sequenceNumber = (string)employeeContractData.sekvensnr;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + _id + ";" + _contractId + ";" + _sequenceNumber;
        }
    }
}
