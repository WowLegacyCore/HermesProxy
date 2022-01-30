using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace HermesProxy.Network.BattleNet.REST
{
    public static class Forms
    {
        /// <summary>
        /// Returns a <see cref="FormInputs"/> instance.
        /// </summary>
        public static FormInputs GetFormInput()
        {
            var formInputs = new FormInputs
            {
                Type = "LOGIN_FORM"
            };

            formInputs.Inputs.Add(new()
            {
                ID = "account_name",
                Type = "text",
                Label = "E-mail",
                MaxLength = 320
            });

            formInputs.Inputs.Add(new()
            {
                ID = "password",
                Type = "passowrd",
                Label = "Password",
                MaxLength = 16
            });

            formInputs.Inputs.Add(new()
            {
                ID = "log_in_submit",
                Type = "submit",
                Label = "Log In",
            });

            return formInputs;
        }
    }

    [DataContract]
    public class FormInputs
    {
        [DataMember(Name = "inputs")]
        public List<FormInput> Inputs = new();

        [DataMember(Name = "prompt")]
        public string Prompt;

        [DataMember(Name = "type")]
        public string Type;
    }

    [DataContract]
    public class FormInput
    {
        [DataMember(Name = "input_id")]
        public string ID;

        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "max_length")]
        public int MaxLength;
    }

    [DataContract]
    public class FormInputValue
    {
        [DataMember(Name = "input_id")]
        public string ID;

        [DataMember(Name = "value")]
        public string Value;
    }

    [DataContract]
    public class LogonData
    {
        [DataMember(Name = "version")]
        public string Version;

        [DataMember(Name = "program_id")]
        public string Program;

        [DataMember(Name = "platform_id")]
        public string Platform;

        [DataMember(Name = "inputs")]
        public List<FormInputValue> Inputs = new();

        public string GetDataFromId(string data)
        {
            return Inputs.FirstOrDefault(x => x.ID == data)?.Value ?? string.Empty;
        }
    }

    [DataContract]
    public class LogonResult
    {
        [DataMember(Name = "authentication_state")]
        public string AuthenticationState;

        [DataMember(Name = "login_ticket")]
        public string LoginTicket;

        [DataMember(Name = "error_code")]
        public string ErrorCode;

        [DataMember(Name = "error_message")]
        public string ErrorMessage;

        [DataMember(Name = "support_error_code")]
        public string SupportErrorCode;

        [DataMember(Name = "authenticator_form")]
        public FormInputs AuthenticatorForm = new();
    }

    [DataContract]
    public class ClientVersion
    {
        [DataMember(Name = "versionBuild")]
        public int Build;

        [DataMember(Name = "versionMajor")]
        public int Major;

        [DataMember(Name = "versionMinor")]
        public int Minor;

        [DataMember(Name = "versionRevision")]
        public int Revision;
    }

    [DataContract]
    public class RealmListTicketInformation
    {
        [DataMember(Name = "platform")]
        public int Platform;

        [DataMember(Name = "buildVariant")]
        public string BuildVariant;

        [DataMember(Name = "type")]
        public int Type;

        [DataMember(Name = "timeZone")]
        public string Timezone;

        [DataMember(Name = "currentTime")]
        public int CurrentTime;

        [DataMember(Name = "textLocale")]
        public int TextLocale;

        [DataMember(Name = "audioLocale")]
        public int AudioLocale;

        [DataMember(Name = "versionDataBuild")]
        public int VersionDataBuild;

        [DataMember(Name = "version")]
        public ClientVersion ClientVersion = new();

        [DataMember(Name = "secret")]
        public List<int> Secret;

        [DataMember(Name = "clientArch")]
        public int ClientArch;

        [DataMember(Name = "systemVersion")]
        public string SystemVersion;

        [DataMember(Name = "platformType")]
        public int PlatformType;

        [DataMember(Name = "systemArch")]
        public int SystemArch;
    }

    [DataContract]
    public class RealmListTicketClientInformation
    {
        [DataMember(Name = "info")]
        public RealmListTicketInformation Info = new();
    }

    [DataContract]
    public class RealmEntry
    {
        [DataMember(Name = "wowRealmAddress")]
        public int WowRealmAddress { get; set; }

        [DataMember(Name = "cfgTimezonesID")]
        public int CfgTimezonesID { get; set; }

        [DataMember(Name = "populationState")]
        public int PopulationState { get; set; }

        [DataMember(Name = "cfgCategoriesID")]
        public int CfgCategoriesID { get; set; }

        [DataMember(Name = "version")]
        public ClientVersion Version { get; set; } = new();

        [DataMember(Name = "cfgRealmsID")]
        public int CfgRealmsID { get; set; }

        [DataMember(Name = "flags")]
        public int Flags { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "cfgConfigsID")]
        public int CfgConfigsID { get; set; }

        [DataMember(Name = "cfgLanguagesID")]
        public int CfgLanguagesID { get; set; }
    }

    [DataContract]
    public class RealmListUpdates
    {
        [DataMember(Name = "updates")]
        public IList<RealmListUpdate> Updates = new List<RealmListUpdate>();
    }

    [DataContract]
    public class RealmListUpdate
    {
        [DataMember(Name = "update")]
        public RealmEntry Update = new();

        [DataMember(Name = "deleting")]
        public bool Deleting;
    }

    [DataContract]
    public class RealmCharacterCountList
    {
        [DataMember(Name = "counts")]
        public List<RealmCharacterCountEntry> Counts = new();
    }

    [DataContract]
    public class RealmCharacterCountEntry
    {
        [DataMember(Name = "wowRealmAddress")]
        public int WowRealmAddress;

        [DataMember(Name = "count")]
        public int Count;
    }
}
