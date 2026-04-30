using System.Text.RegularExpressions;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Common.DTOs;

namespace Fourteen.Application.Helpers
{
    public class NaturalLanguageQueryParser
    {
        private static readonly Dictionary<string, string> CountryMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["afghanistan"] = "AF",
            ["albania"] = "AL",
            ["algeria"] = "DZ",
            ["andorra"] = "AD",
            ["angola"] = "AO",
            ["antigua and barbuda"] = "AG",
            ["argentina"] = "AR",
            ["armenia"] = "AM",
            ["australia"] = "AU",
            ["austria"] = "AT",
            ["azerbaijan"] = "AZ",

            ["bahamas"] = "BS",
            ["bahrain"] = "BH",
            ["bangladesh"] = "BD",
            ["barbados"] = "BB",
            ["belarus"] = "BY",
            ["belgium"] = "BE",
            ["belize"] = "BZ",
            ["benin"] = "BJ",
            ["bhutan"] = "BT",
            ["bolivia"] = "BO",
            ["bosnia and herzegovina"] = "BA",
            ["botswana"] = "BW",
            ["brazil"] = "BR",
            ["brunei"] = "BN",
            ["bulgaria"] = "BG",
            ["burkina faso"] = "BF",
            ["burundi"] = "BI",

            ["cambodia"] = "KH",
            ["cameroon"] = "CM",
            ["canada"] = "CA",
            ["cape verde"] = "CV",
            ["central african republic"] = "CF",
            ["chad"] = "TD",
            ["chile"] = "CL",
            ["china"] = "CN",
            ["colombia"] = "CO",
            ["comoros"] = "KM",
            ["congo"] = "CG",
            ["costa rica"] = "CR",
            ["croatia"] = "HR",
            ["cuba"] = "CU",
            ["cyprus"] = "CY",
            ["czech republic"] = "CZ",

            ["denmark"] = "DK",
            ["djibouti"] = "DJ",
            ["dominica"] = "DM",
            ["dominican republic"] = "DO",

            ["ecuador"] = "EC",
            ["egypt"] = "EG",
            ["el salvador"] = "SV",
            ["equatorial guinea"] = "GQ",
            ["eritrea"] = "ER",
            ["estonia"] = "EE",
            ["eswatini"] = "SZ",
            ["ethiopia"] = "ET",

            ["fiji"] = "FJ",
            ["finland"] = "FI",
            ["france"] = "FR",

            ["gabon"] = "GA",
            ["gambia"] = "GM",
            ["georgia"] = "GE",
            ["germany"] = "DE",
            ["ghana"] = "GH",
            ["greece"] = "GR",
            ["grenada"] = "GD",
            ["guatemala"] = "GT",
            ["guinea"] = "GN",
            ["guinea bissau"] = "GW",
            ["guyana"] = "GY",

            ["haiti"] = "HT",
            ["honduras"] = "HN",
            ["hungary"] = "HU",

            ["iceland"] = "IS",
            ["india"] = "IN",
            ["indonesia"] = "ID",
            ["iran"] = "IR",
            ["iraq"] = "IQ",
            ["ireland"] = "IE",
            ["israel"] = "IL",
            ["italy"] = "IT",

            ["jamaica"] = "JM",
            ["japan"] = "JP",
            ["jordan"] = "JO",

            ["kazakhstan"] = "KZ",
            ["kenya"] = "KE",
            ["kiribati"] = "KI",
            ["kuwait"] = "KW",
            ["kyrgyzstan"] = "KG",

            ["laos"] = "LA",
            ["latvia"] = "LV",
            ["lebanon"] = "LB",
            ["lesotho"] = "LS",
            ["liberia"] = "LR",
            ["libya"] = "LY",
            ["liechtenstein"] = "LI",
            ["lithuania"] = "LT",
            ["luxembourg"] = "LU",

            ["madagascar"] = "MG",
            ["malawi"] = "MW",
            ["malaysia"] = "MY",
            ["maldives"] = "MV",
            ["mali"] = "ML",
            ["malta"] = "MT",
            ["marshall islands"] = "MH",
            ["mauritania"] = "MR",
            ["mauritius"] = "MU",
            ["mexico"] = "MX",
            ["micronesia"] = "FM",
            ["moldova"] = "MD",
            ["monaco"] = "MC",
            ["mongolia"] = "MN",
            ["montenegro"] = "ME",
            ["morocco"] = "MA",
            ["mozambique"] = "MZ",
            ["myanmar"] = "MM",

            ["namibia"] = "NA",
            ["nauru"] = "NR",
            ["nepal"] = "NP",
            ["netherlands"] = "NL",
            ["new zealand"] = "NZ",
            ["nicaragua"] = "NI",
            ["niger"] = "NE",
            ["nigeria"] = "NG",
            ["north korea"] = "KP",
            ["north macedonia"] = "MK",
            ["norway"] = "NO",

            ["oman"] = "OM",

            ["pakistan"] = "PK",
            ["palau"] = "PW",
            ["panama"] = "PA",
            ["papua new guinea"] = "PG",
            ["paraguay"] = "PY",
            ["peru"] = "PE",
            ["philippines"] = "PH",
            ["poland"] = "PL",
            ["portugal"] = "PT",
            ["qatar"] = "QA",

            ["romania"] = "RO",
            ["russia"] = "RU",
            ["rwanda"] = "RW",

            ["saint kitts and nevis"] = "KN",
            ["saint lucia"] = "LC",
            ["saint vincent and the grenadines"] = "VC",
            ["samoa"] = "WS",
            ["san marino"] = "SM",
            ["sao tome and principe"] = "ST",
            ["saudi arabia"] = "SA",
            ["senegal"] = "SN",
            ["serbia"] = "RS",
            ["seychelles"] = "SC",
            ["sierra leone"] = "SL",
            ["singapore"] = "SG",
            ["slovakia"] = "SK",
            ["slovenia"] = "SI",
            ["solomon islands"] = "SB",
            ["somalia"] = "SO",
            ["south africa"] = "ZA",
            ["south korea"] = "KR",
            ["south sudan"] = "SS",
            ["spain"] = "ES",
            ["sri lanka"] = "LK",
            ["sudan"] = "SD",
            ["suriname"] = "SR",
            ["sweden"] = "SE",
            ["switzerland"] = "CH",
            ["syria"] = "SY",

            ["taiwan"] = "TW",
            ["tajikistan"] = "TJ",
            ["tanzania"] = "TZ",
            ["thailand"] = "TH",
            ["timor leste"] = "TL",
            ["togo"] = "TG",
            ["tonga"] = "TO",
            ["trinidad and tobago"] = "TT",
            ["tunisia"] = "TN",
            ["turkey"] = "TR",
            ["turkmenistan"] = "TM",
            ["tuvalu"] = "TV",

            ["uganda"] = "UG",
            ["ukraine"] = "UA",
            ["united arab emirates"] = "AE",
            ["united kingdom"] = "GB",
            ["united states"] = "US",
            ["uruguay"] = "UY",
            ["uzbekistan"] = "UZ",

            ["vanuatu"] = "VU",
            ["vatican city"] = "VA",
            ["venezuela"] = "VE",
            ["vietnam"] = "VN",

            ["yemen"] = "YE",
            ["zambia"] = "ZM",
            ["zimbabwe"] = "ZW",
            ["palestine"] = "PS"
        };
        private static readonly Dictionary<string, string> NationalityMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["afghan"] = "AF",
            ["albanian"] = "AL",
            ["algerian"] = "DZ",
            ["andorran"] = "AD",
            ["angolan"] = "AO",
            ["antiguan"] = "AG",
            ["barbudan"] = "AG",
            ["argentinian"] = "AR",
            ["argentine"] = "AR",
            ["armenian"] = "AM",
            ["australian"] = "AU",
            ["austrian"] = "AT",
            ["azerbaijani"] = "AZ",

            ["bahamian"] = "BS",
            ["bahraini"] = "BH",
            ["bangladeshi"] = "BD",
            ["barbadian"] = "BB",
            ["belarusian"] = "BY",
            ["belgian"] = "BE",
            ["belizean"] = "BZ",
            ["beninese"] = "BJ",
            ["bhutanese"] = "BT",
            ["bolivian"] = "BO",
            ["bosnian"] = "BA",
            ["herzegovinian"] = "BA",
            ["botswanan"] = "BW",
            ["brazilian"] = "BR",
            ["bruneian"] = "BN",
            ["bulgarian"] = "BG",
            ["burkinabe"] = "BF",
            ["burundian"] = "BI",

            ["cambodian"] = "KH",
            ["cameroonian"] = "CM",
            ["canadian"] = "CA",
            ["cape verdean"] = "CV",
            ["central african"] = "CF",
            ["chadian"] = "TD",
            ["chilean"] = "CL",
            ["chinese"] = "CN",
            ["colombian"] = "CO",
            ["comorian"] = "KM",
            ["congolese"] = "CG",
            ["costa rican"] = "CR",
            ["croatian"] = "HR",
            ["cuban"] = "CU",
            ["cypriot"] = "CY",
            ["czech"] = "CZ",

            ["danish"] = "DK",
            ["djiboutian"] = "DJ",
            ["dominican"] = "DO",

            ["ecuadorean"] = "EC",
            ["ecuadorian"] = "EC",
            ["egyptian"] = "EG",
            ["salvadoran"] = "SV",
            ["equatorial guinean"] = "GQ",
            ["eritrean"] = "ER",
            ["estonian"] = "EE",
            ["swazi"] = "SZ",
            ["eswatini"] = "SZ",
            ["ethiopian"] = "ET",

            ["fijian"] = "FJ",
            ["finnish"] = "FI",
            ["french"] = "FR",

            ["gabonese"] = "GA",
            ["gambian"] = "GM",
            ["georgian"] = "GE",
            ["german"] = "DE",
            ["ghanaian"] = "GH",
            ["greek"] = "GR",
            ["grenadian"] = "GD",
            ["guatemalan"] = "GT",
            ["guinean"] = "GN",
            ["bissau guinean"] = "GW",
            ["guyanese"] = "GY",

            ["haitian"] = "HT",
            ["honduran"] = "HN",
            ["hungarian"] = "HU",

            ["icelandic"] = "IS",
            ["indian"] = "IN",
            ["indonesian"] = "ID",
            ["iranian"] = "IR",
            ["iraqi"] = "IQ",
            ["irish"] = "IE",
            ["israeli"] = "IL",
            ["italian"] = "IT",

            ["jamaican"] = "JM",
            ["japanese"] = "JP",
            ["jordanian"] = "JO",

            ["kazakh"] = "KZ",
            ["kenyan"] = "KE",
            ["kiribatian"] = "KI",
            ["kuwaiti"] = "KW",
            ["kyrgyz"] = "KG",

            ["laotian"] = "LA",
            ["latvian"] = "LV",
            ["lebanese"] = "LB",
            ["lesotho"] = "LS",
            ["basotho"] = "LS",
            ["liberian"] = "LR",
            ["libyan"] = "LY",
            ["liechtensteiner"] = "LI",
            ["lithuanian"] = "LT",
            ["luxembourgish"] = "LU",

            ["malagasy"] = "MG",
            ["malawian"] = "MW",
            ["malaysian"] = "MY",
            ["maldivian"] = "MV",
            ["malian"] = "ML",
            ["maltese"] = "MT",
            ["marshallese"] = "MH",
            ["mauritanian"] = "MR",
            ["mauritian"] = "MU",
            ["mexican"] = "MX",
            ["micronesian"] = "FM",
            ["moldovan"] = "MD",
            ["monacan"] = "MC",
            ["mongolian"] = "MN",
            ["montenegrin"] = "ME",
            ["moroccan"] = "MA",
            ["mozambican"] = "MZ",
            ["burmese"] = "MM",
            ["myanmarese"] = "MM",

            ["namibian"] = "NA",
            ["nauruan"] = "NR",
            ["nepalese"] = "NP",
            ["nepali"] = "NP",
            ["dutch"] = "NL",
            ["new zealander"] = "NZ",
            ["nicaraguan"] = "NI",
            ["nigerien"] = "NE",
            ["nigerian"] = "NG",
            ["north korean"] = "KP",
            ["macedonian"] = "MK",
            ["norwegian"] = "NO",

            ["omani"] = "OM",

            ["pakistani"] = "PK",
            ["palauan"] = "PW",
            ["panamanian"] = "PA",
            ["papua new guinean"] = "PG",
            ["paraguayan"] = "PY",
            ["peruvian"] = "PE",
            ["philippine"] = "PH",
            ["filipino"] = "PH",
            ["polish"] = "PL",
            ["portuguese"] = "PT",
            ["qatari"] = "QA",

            ["romanian"] = "RO",
            ["russian"] = "RU",
            ["rwandan"] = "RW",

            ["kittitian"] = "KN",
            ["nevisian"] = "KN",
            ["saint lucian"] = "LC",
            ["vincentian"] = "VC",
            ["samoan"] = "WS",
            ["sammarinese"] = "SM",
            ["sao tomean"] = "ST",
            ["saudi"] = "SA",
            ["saudi arabian"] = "SA",
            ["senegalese"] = "SN",
            ["serbian"] = "RS",
            ["seychellois"] = "SC",
            ["sierra leonean"] = "SL",
            ["singaporean"] = "SG",
            ["slovak"] = "SK",
            ["slovenian"] = "SI",
            ["solomon islander"] = "SB",
            ["somali"] = "SO",
            ["south african"] = "ZA",
            ["south korean"] = "KR",
            ["south sudanese"] = "SS",
            ["spanish"] = "ES",
            ["sri lankan"] = "LK",
            ["sudanese"] = "SD",
            ["surinamese"] = "SR",
            ["swedish"] = "SE",
            ["swiss"] = "CH",
            ["syrian"] = "SY",

            ["taiwanese"] = "TW",
            ["tajik"] = "TJ",
            ["tanzanian"] = "TZ",
            ["thai"] = "TH",
            ["timorese"] = "TL",
            ["togolese"] = "TG",
            ["tongan"] = "TO",
            ["trinidadian"] = "TT",
            ["tobagonian"] = "TT",
            ["tunisian"] = "TN",
            ["turkish"] = "TR",
            ["turkmen"] = "TM",
            ["tuvaluan"] = "TV",

            ["ugandan"] = "UG",
            ["ukrainian"] = "UA",
            ["emirati"] = "AE",
            ["british"] = "GB",
            ["american"] = "US",
            ["uruguayan"] = "UY",
            ["uzbek"] = "UZ",

            ["ni vanuatu"] = "VU",
            ["vatican"] = "VA",
            ["venezuelan"] = "VE",
            ["vietnamese"] = "VN",

            ["yemeni"] = "YE",
            ["zambian"] = "ZM",
            ["zimbabwean"] = "ZW",
            ["palestinian"] = "PS"
        };
        private static readonly Dictionary<string, string> GenderMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["male"] = "male", ["males"] = "male", ["man"] = "male", ["men"] = "male",
            ["boy"] = "male", ["boys"] = "male", ["guy"] = "male", ["guys"] = "male",
            ["female"] = "female", ["females"] = "female", ["woman"] = "female", ["women"] = "female",
            ["girl"] = "female", ["girls"] = "female", ["lady"] = "female", ["ladies"] = "female",
        };
        private static readonly Dictionary<string, (int Min, int Max)> AgeGroupMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["child"]       = (0,  12),
            ["children"]    = (0,  12),
            ["teen"]        = (13, 19),
            ["teens"]       = (13, 19),
            ["teenager"]    = (13, 19),
            ["teenagers"]   = (13, 19),
            ["young"]       = (16, 24),
            ["youth"]       = (16, 24),
            ["adult"]       = (20, 59),
            ["adults"]      = (20, 59),
            ["middleaged"]  = (40, 59),
            ["elderly"]     = (60, 120),
            ["senior"]      = (60, 120),
            ["seniors"]     = (60, 120),
        };
        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
            { "and", "or", "the", "from", "people", "persons", "users", "age", "aged", "with", "who", "are", "of" };
        public static ParsedProfileFilter Parse(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new ParsedProfileFilter();

        var normalized = query
            .ToLowerInvariant()
            .Replace("-", "").Replace("_", "")
            .Replace(",", " ").Replace(".", " ")
            .Replace("middle aged",   "middleaged")
            .Replace("south africa",  "southafrica")
            .Replace("united states", "unitedstates")
            .Replace("united kingdom","unitedkingdom");

        var explicitAge = ParseExplicitAge(normalized);

        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !Stopwords.Contains(t))
            .ToArray();

        string? gender = null;
        (int Min, int Max)? ageGroup = null;
        string? countryId = null;
        var genderTokensFound = new HashSet<string>();

        foreach (var token in tokens)
        {
            if (GenderMap.TryGetValue(token, out var g))
                genderTokensFound.Add(g);

            if (ageGroup is null && AgeGroupMap.TryGetValue(token, out var range))
                ageGroup = range;

            if (countryId is null && CountryMap.TryGetValue(token, out var cid))
                countryId = cid;
        }

        if (genderTokensFound.Count == 1)
            gender = genderTokensFound.First();

        int? ageMin = explicitAge.Min;
        int? ageMax = explicitAge.Max;

        if (ageGroup is not null)
        {
            ageMin ??= ageGroup.Value.Min;
            ageMax ??= ageGroup.Value.Max;
        }

        return new ParsedProfileFilter
        {
            Gender    = gender,
            AgeMin    = ageMin,
            AgeMax    = ageMax,
            CountryId = countryId,
        };
    }

        private static (int? Min, int? Max) ParseExplicitAge(string normalized)
        {
            var rangeMatch = Regex.Match(normalized, @"(\d+)\s*(?:to|and|-)\s*(\d+)");
            if (rangeMatch.Success)
                return (int.Parse(rangeMatch.Groups[1].Value), int.Parse(rangeMatch.Groups[2].Value));

            var aboveMatch = Regex.Match(normalized, @"(?:above|over|older than|at least)\s+(\d+)");
            if (aboveMatch.Success)
                return (int.Parse(aboveMatch.Groups[1].Value), null);

            var belowMatch = Regex.Match(normalized, @"(?:below|under|younger than|at most)\s+(\d+)");
            if (belowMatch.Success)
                return (null, int.Parse(belowMatch.Groups[1].Value));

            return (null, null);
        }

    }
}