namespace dfo_toa_manual.DFO
{
    /**
     {
        "id": "05001234",
        "brukerident": "olanor",
        "dfoBrukerident": "9900olanor",
        "eksternIdent": "abcdefe23456@uit.no",
        "fornavn": "Ola",
        "etternavn": "Nordmann",
        "fnr": "01017044123",
        "annenId": [{
            "idType": "02",
            "idBeskrivelse": "Passnummer",
            "idNr": "123456789",
            "idStartdato": "2019-01-01",
            "idSluttdato": "2023-12-31",
            "idLand": "Norge"
        }],
        "fdato": "1970-01-01",
        "kjonn": "M",
        "sakarkivnr": "20/09876543",
        "landkode": "NO",
        "medarbeidergruppe": "1",
        "medarbeiderundergruppe": "01",
        "startdato": "2010-05-01",
        "sluttdato": "2019-01-31",
        "sluttarsak": "Oppsig.fra ans - slutt i stat",
        "stillingId": 30000468,
        "hjemmelKode": "90",
        "hjemmelTekst": "A§14.9-2bVikar",
        "dellonnsprosent": "100",
        "kostnadssted": "67165",
        "organisasjonId": 10000048,
        "jurBedriftsnummer": 976362832,
        "pdo": "STVG",
        "tilleggsstilling": [{
            "stillingId": 30000468,
            "startdato": "2019-05-01",
            "sluttdato": "2020-06-01",
            "dellonnsprosent": "100",
            "ekstraStilling": "MP"
        }],
        "lederflagg": false,
        "portaltilgang": false,
        "turnustilgang": false,
        "eksternbruker": true,
        "reservasjonPublisering": false,
        "epost": "ola.nordmann@uib.no",
        "tjenestetelefon": "45011222",
        "privatTelefonnummer": "33112233",
        "telefonnummer": "56231245",
        "mobilnummer": "90011222",
        "mobilPrivat": "90543572",
        "privatTlfUtland": "+46 123456789",
        "privaEpost": "abc@gmail.com",
        "privatPostadresse": "Stolpelyktveien 1",
        "privatPostnr": "5231",
        "privatPoststed": "Paradis",
        "endretDato": "2018-12-19",
        "endretKlokkeslett": "10:13:31",
        "endretInfotype": "0509",
        "endretAv": "3-ABCD / 9900ABCD"
     }
    */
    internal class Employee
    {
        private string _socialSecurityNumber;
        public string SocialSecurityNumber => _socialSecurityNumber;
        private string _firstName;
        public string FirstName => _firstName;
        private string _lastName;
        public string LastName => _lastName;
        private string _address;
        public string Address => _address;
        private string _zipcode;
        public string Zipcode => _zipcode;
        private string _city;
        public string City => _city;
        private string _phonenumber;
        public string PhoneNumber => _phonenumber;
        private string _email;
        public string Email => _email;
        public Employee(dynamic employeeData)
        {
            this._socialSecurityNumber = (string)employeeData.fnr;
            this._firstName = (string)employeeData.fornavn;
            this._lastName = (string)employeeData.etternavn;
            this._address = (string)employeeData.privatPostadresse;
            this._zipcode = (string)employeeData.privatPostnr;
            this._city = (string)employeeData.privatPoststed;
            this._phonenumber = (string)employeeData.mobilPrivat;
            this._email = (string)employeeData.epost;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + _firstName + ";" + _lastName;
        }
    }
}
