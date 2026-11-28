using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;
using Marilog.Ship.Domain.Entities;
using Marilog.Ship.Pages;
using Marilog.Ship.Domain;

namespace Marilog.Ship.Data
{
    /// <summary>
    /// Reference data written on the first run, each table only while it is empty.
    /// Everything stays editable afterwards. Ports are not seeded — the master adds them when needed.
    /// </summary>
    public static class ShipSeedData
    {
        public static void Seed(ShipContext db)
        {
            if (!db.Set<Country>().Any())
                db.Set<Country>().AddRange(Countries.Select(c => Country.Create(c.Code, c.Name)));

            if (!db.Set<Currency>().Any())
                db.Set<Currency>().AddRange(Currencies.Select(c =>
                    Currency.Create(c.Code, c.Name, c.UsdRate, c.Symbol, isBaseCurrency: c.Code == "USD")));

            if (!db.Set<Rank>().Any())
                db.Set<Rank>().AddRange(Ranks.Select(r => Rank.Create(r.Code, r.Name, r.Department)));

            if (!db.Set<DeductionType>().Any())
                db.Set<DeductionType>().AddRange(DeductionTypes.Select(d => DeductionType.Create(d.Code, d.Name)));

            if (!db.Set<DocumentType>().Any())
                db.Set<DocumentType>().AddRange(DocumentTypes.Select(d => DocumentType.Create(d.Code, d.Name, d.SortOrder)));
            db.SaveChanges();
        }

        // USD per 1 unit. USD is the pivot (1) and the default base currency;
        // the others are fixed pegs. The master adds any other currency with its rate.
        private static readonly (string Code, string Name, string? Symbol, decimal UsdRate)[] Currencies =
        [
            ("USD", "US Dollar", "$", 1m),
            ("AED", "UAE Dirham", null, 0.27229408m),
            ("SAR", "Saudi Riyal", null, 0.26666667m),
            ("QAR", "Qatari Riyal", null, 0.27472527m),
            ("OMR", "Omani Rial", null, 2.60078023m),
            ("BHD", "Bahraini Dinar", null, 2.65957447m),
            ("JOD", "Jordanian Dinar", null, 1.41043724m),
        ];

        // RankCode "MASTER" is used by CrewContractService to find the master — keep it.
        private static readonly (string Code, string Name, Department Department)[] Ranks =
        [
            ("MASTER", "Master", Department.DECK),
            ("CO", "Chief Officer", Department.DECK),
            ("2O", "Second Officer", Department.DECK),
            ("3O", "Third Officer", Department.DECK),
            ("DCDT", "Deck Cadet", Department.DECK),
            ("BOSUN", "Bosun", Department.DECK),
            ("AB", "Able Seaman", Department.DECK),
            ("OS", "Ordinary Seaman", Department.DECK),
            ("CE", "Chief Engineer", Department.ENGINE),
            ("2E", "Second Engineer", Department.ENGINE),
            ("3E", "Third Engineer", Department.ENGINE),
            ("4E", "Fourth Engineer", Department.ENGINE),
            ("ETO", "Electro-Technical Officer", Department.ENGINE),
            ("ECDT", "Engine Cadet", Department.ENGINE),
            ("FITTER", "Fitter", Department.ENGINE),
            ("OILER", "Oiler", Department.ENGINE),
            ("WIPER", "Wiper", Department.ENGINE),
            ("COOK", "Cook", Department.CATERING),
            ("MESSMAN", "Messman", Department.CATERING),
            ("STEWARD", "Steward", Department.CATERING),
        ];

        private static readonly (string Code, string Name)[] DeductionTypes =
        [
            ("OVERPAYMENT", "Overpayment recovery"),
            ("SIGNOFF_COST", "Sign-off cost recovery (agent, ticket, visa, transport)"),
            ("PHONE", "Mobile / SIM card"),
            ("MISC", "Miscellaneous"),
        ];
        // The master adds more on the Document Types page.
        // CIGARETTES (ShipCodes.CigarettesDocType) is used by the MGA — keep it.
        private static readonly (string Code, string Name, int SortOrder)[] DocumentTypes =
        [
            ("INVOICE", "Invoice", 1),
            ("RECEIPT", "Receipt", 2),
            ("CASH_VOUCHER", "Cash Voucher", 3),
            (ShipCodes.CigarettesDocType, "Cigarettes Purchase", 4),
        ];
        // ISO 3166-1 alpha-2.
        private static readonly (string Code, string Name)[] Countries =
        [
            ("AF", "Afghanistan"), ("AL", "Albania"), ("DZ", "Algeria"), ("AS", "American Samoa"), ("AD", "Andorra"),
            ("AO", "Angola"), ("AI", "Anguilla"), ("AQ", "Antarctica"), ("AG", "Antigua and Barbuda"),
            ("AR", "Argentina"), ("AM", "Armenia"), ("AW", "Aruba"), ("AU", "Australia"), ("AT", "Austria"),
            ("AZ", "Azerbaijan"), ("BS", "Bahamas"), ("BH", "Bahrain"), ("BD", "Bangladesh"), ("BB", "Barbados"),
            ("BY", "Belarus"), ("BE", "Belgium"), ("BZ", "Belize"), ("BJ", "Benin"), ("BM", "Bermuda"),
            ("BT", "Bhutan"), ("BO", "Bolivia"), ("BQ", "Bonaire, Sint Eustatius and Saba"),
            ("BA", "Bosnia and Herzegovina"), ("BW", "Botswana"), ("BV", "Bouvet Island"), ("BR", "Brazil"),
            ("IO", "British Indian Ocean Territory"), ("BN", "Brunei Darussalam"), ("BG", "Bulgaria"),
            ("BF", "Burkina Faso"), ("BI", "Burundi"), ("CV", "Cabo Verde"), ("KH", "Cambodia"),
            ("CM", "Cameroon"), ("CA", "Canada"), ("KY", "Cayman Islands"), ("CF", "Central African Republic"),
            ("TD", "Chad"), ("CL", "Chile"), ("CN", "China"), ("CX", "Christmas Island"),
            ("CC", "Cocos (Keeling) Islands"), ("CO", "Colombia"), ("KM", "Comoros"), ("CG", "Congo"),
            ("CD", "DR Congo"), ("CK", "Cook Islands"), ("CR", "Costa Rica"), ("HR", "Croatia"),
            ("CU", "Cuba"), ("CW", "Curaçao"), ("CY", "Cyprus"), ("CZ", "Czechia"), ("CI", "Côte d'Ivoire"),
            ("DK", "Denmark"), ("DJ", "Djibouti"), ("DM", "Dominica"), ("DO", "Dominican Republic"),
            ("EC", "Ecuador"), ("EG", "Egypt"), ("SV", "El Salvador"), ("GQ", "Equatorial Guinea"),
            ("ER", "Eritrea"), ("EE", "Estonia"), ("SZ", "Eswatini"), ("ET", "Ethiopia"),
            ("FK", "Falkland Islands (Malvinas)"), ("FO", "Faroe Islands"), ("FJ", "Fiji"), ("FI", "Finland"),
            ("FR", "France"), ("GF", "French Guiana"), ("PF", "French Polynesia"),
            ("TF", "French Southern Territories"), ("GA", "Gabon"), ("GM", "Gambia"), ("GE", "Georgia"),
            ("DE", "Germany"), ("GH", "Ghana"), ("GI", "Gibraltar"), ("GR", "Greece"), ("GL", "Greenland"),
            ("GD", "Grenada"), ("GP", "Guadeloupe"), ("GU", "Guam"), ("GT", "Guatemala"), ("GG", "Guernsey"),
            ("GN", "Guinea"), ("GW", "Guinea-Bissau"), ("GY", "Guyana"), ("HT", "Haiti"),
            ("HM", "Heard Island and McDonald Islands"), ("VA", "Holy See (Vatican City State)"),
            ("HN", "Honduras"), ("HK", "Hong Kong"), ("HU", "Hungary"), ("IS", "Iceland"), ("IN", "India"),
            ("ID", "Indonesia"), ("IR", "Iran"), ("IQ", "Iraq"), ("IE", "Ireland"), ("IM", "Isle of Man"),
            ("IL", "Israel"), ("IT", "Italy"), ("JM", "Jamaica"), ("JP", "Japan"), ("JE", "Jersey"),
            ("JO", "Jordan"), ("KZ", "Kazakhstan"), ("KE", "Kenya"), ("KI", "Kiribati"), ("KW", "Kuwait"),
            ("KG", "Kyrgyzstan"), ("LA", "Laos"), ("LV", "Latvia"), ("LB", "Lebanon"), ("LS", "Lesotho"),
            ("LR", "Liberia"), ("LY", "Libya"), ("LI", "Liechtenstein"), ("LT", "Lithuania"),
            ("LU", "Luxembourg"), ("MO", "Macao"), ("MG", "Madagascar"), ("MW", "Malawi"), ("MY", "Malaysia"),
            ("MV", "Maldives"), ("ML", "Mali"), ("MT", "Malta"), ("MH", "Marshall Islands"),
            ("MQ", "Martinique"), ("MR", "Mauritania"), ("MU", "Mauritius"), ("YT", "Mayotte"),
            ("MX", "Mexico"), ("FM", "Micronesia"), ("MD", "Moldova"), ("MC", "Monaco"),
            ("MN", "Mongolia"), ("ME", "Montenegro"), ("MS", "Montserrat"), ("MA", "Morocco"),
            ("MZ", "Mozambique"), ("MM", "Myanmar"), ("NA", "Namibia"), ("NR", "Nauru"), ("NP", "Nepal"),
            ("NL", "Netherlands"), ("NC", "New Caledonia"), ("NZ", "New Zealand"), ("NI", "Nicaragua"),
            ("NE", "Niger"), ("NG", "Nigeria"), ("NU", "Niue"), ("NF", "Norfolk Island"),
            ("KP", "North Korea"), ("MK", "North Macedonia"), ("MP", "Northern Mariana Islands"), ("NO", "Norway"),
            ("OM", "Oman"), ("PK", "Pakistan"), ("PW", "Palau"), ("PS", "Palestine"), ("PA", "Panama"),
            ("PG", "Papua New Guinea"), ("PY", "Paraguay"), ("PE", "Peru"), ("PH", "Philippines"),
            ("PN", "Pitcairn"), ("PL", "Poland"), ("PT", "Portugal"), ("PR", "Puerto Rico"), ("QA", "Qatar"),
            ("RO", "Romania"), ("RU", "Russia"), ("RW", "Rwanda"), ("RE", "Réunion"),
            ("BL", "Saint Barthélemy"), ("SH", "Saint Helena, Ascension and Tristan da Cunha"),
            ("KN", "Saint Kitts and Nevis"), ("LC", "Saint Lucia"), ("MF", "Saint Martin (French part)"),
            ("PM", "Saint Pierre and Miquelon"), ("VC", "Saint Vincent and the Grenadines"), ("WS", "Samoa"),
            ("SM", "San Marino"), ("ST", "Sao Tome and Principe"), ("SA", "Saudi Arabia"), ("SN", "Senegal"),
            ("RS", "Serbia"), ("SC", "Seychelles"), ("SL", "Sierra Leone"), ("SG", "Singapore"),
            ("SX", "Sint Maarten (Dutch part)"), ("SK", "Slovakia"), ("SI", "Slovenia"),
            ("SB", "Solomon Islands"), ("SO", "Somalia"), ("ZA", "South Africa"),
            ("GS", "South Georgia and the South Sandwich Islands"),
            ("KR", "South Korea"), ("SS", "South Sudan"), ("ES", "Spain"), ("LK", "Sri Lanka"), ("SD", "Sudan"),
            ("SR", "Suriname"), ("SJ", "Svalbard and Jan Mayen"), ("SE", "Sweden"), ("CH", "Switzerland"),
            ("SY", "Syria"), ("TW", "Taiwan"), ("TJ", "Tajikistan"), ("TZ", "Tanzania"), ("TH", "Thailand"),
            ("TL", "Timor-Leste"), ("TG", "Togo"), ("TK", "Tokelau"), ("TO", "Tonga"),
            ("TT", "Trinidad and Tobago"), ("TN", "Tunisia"), ("TM", "Turkmenistan"),
            ("TC", "Turks and Caicos Islands"), ("TV", "Tuvalu"), ("TR", "Türkiye"), ("UG", "Uganda"),
            ("UA", "Ukraine"), ("AE", "United Arab Emirates"), ("GB", "United Kingdom"), ("US", "United States"),
            ("UM", "United States Minor Outlying Islands"), ("UY", "Uruguay"), ("UZ", "Uzbekistan"),
            ("VU", "Vanuatu"), ("VE", "Venezuela"), ("VN", "Vietnam"), ("VG", "British Virgin Islands"),
            ("VI", "U.S. Virgin Islands"), ("WF", "Wallis and Futuna"), ("EH", "Western Sahara"), ("YE", "Yemen"),
            ("ZM", "Zambia"), ("ZW", "Zimbabwe"), ("AX", "Åland Islands")
        ];
    }
}