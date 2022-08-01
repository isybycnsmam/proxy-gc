using System.Text.RegularExpressions;

namespace ProxyService.Getting.HttpsSpysOne
{
    public sealed class FuckingKeyDecryptor
    {
        private const string DECRYPTING_SECRETS_REGEX = @"'(?<mappings>[0-9A-Za-z=\^;]+)',60,60,'(?<initialValues>[0-9A-Za-z=\^;]+)'";
        private const string INITIAL_DICTIONARY_KEYS = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWX";
        private const string ENCRYPTED_PROXIES_PORTS_REGEX = @"<script type=""text\/javascript"">document.write\(""<font class=spy2>:<\\\/font>""\+(?<encryptedPort>[\(\)A-Za-z0-9\^\+]+)<\/script>";

        public static string DecryptPortNumbers(string source)
        {
            var secretsMatch = Regex.Match(source, DECRYPTING_SECRETS_REGEX, RegexOptions.None, TimeSpan.FromSeconds(10));

            var initialValues = secretsMatch.Groups["initialValues"].Value;
            var mappings = secretsMatch.Groups["mappings"].Value;

            if (string.IsNullOrEmpty(initialValues) ||
                string.IsNullOrEmpty(mappings))
            {
                throw new NullReferenceException("Fucking keys decryptor missing initialValues or mappings");
            }

            var initialValuesDictionary = new Dictionary<string, string>();
            var initialValuesArray = initialValues.Split('^');
            for (int i = 0; i < INITIAL_DICTIONARY_KEYS.Length; i++)
            {
                if (string.IsNullOrEmpty(initialValuesArray[i]))
                    initialValuesDictionary.Add(INITIAL_DICTIONARY_KEYS[i].ToString(), INITIAL_DICTIONARY_KEYS[i].ToString());
                else
                    initialValuesDictionary.Add(INITIAL_DICTIONARY_KEYS[i].ToString(), initialValuesArray[i]);
            }

            var decryptedMappings = Regex.Replace(mappings, @"\b\w+\b", match => initialValuesDictionary[match.Value]);
            var decryptedMappingsArray = decryptedMappings.Split(';').Where(e => !string.IsNullOrEmpty(e));
            var decryptingDictionary = new Dictionary<string, int>();
            foreach (var decryptedMapping in decryptedMappingsArray)
            {
                var parts = decryptedMapping.Split(new char[] { '=', '^' });
                var value = 0;
                if (parts.Length == 2)
                {
                    value = decryptingDictionary.ContainsKey(parts[1]) ?
                        decryptingDictionary[parts[1]] : Convert.ToInt32(parts[1]);
                }
                else if (parts.Length == 3)
                {
                    var left = decryptingDictionary.ContainsKey(parts[1]) ?
                        decryptingDictionary[parts[1]] : Convert.ToInt32(parts[1]);
                    var right = decryptingDictionary.ContainsKey(parts[2]) ?
                        decryptingDictionary[parts[2]] : Convert.ToInt32(parts[2]);
                    value = left ^ right;
                }
                else
                {
                    throw new InvalidOperationException("Cannot decrypt ports due to invalid number of opreatios when creating final decrypting dictionary");
                }

                decryptingDictionary.Add(parts[0], value);
            }

            source = Regex.Replace(source, ENCRYPTED_PROXIES_PORTS_REGEX, match =>
            {
                var decryptedPort = "";
                var encryptedPort = match.Groups["encryptedPort"].Value;
                var parts = encryptedPort.Replace("(", "").Replace(")", "").Split('+');
                foreach (var part in parts)
                {
                    var numberKeys = part.Split('^');
                    decryptedPort += decryptingDictionary[numberKeys[0]] ^ decryptingDictionary[numberKeys[1]];
                }
                return $":{decryptedPort}";
            }, RegexOptions.None, TimeSpan.FromSeconds(10));

            return source;
        }
    }
}
