namespace LSSD.Lunch.StudentImporter
{
    public class ConfigFile 
    {
        public string ConnectionString_Base64 { get; set; } = string.Empty;
        public string ConnectionString {
            get {
                return EncodingHelpers.Base64Decode(this.ConnectionString_Base64);
            }
        }        
    }
}