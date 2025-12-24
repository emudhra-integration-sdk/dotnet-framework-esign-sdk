using System;
using System.IO;
using System.Text;

namespace eSignASPLibrary
{
    internal static class eSignSettings
    {
        internal static string pfxAlias { get; set; }
        internal static string ASPID { get; set; }
        internal static string eSignURL { get; set; }
        internal static string eSignCheckStatusURL { get; set; }
        internal static double SessionTimeOut { get; set; }
        internal static bool isProxyRequired { get; set; }
        internal static string proxyIP { get; set; }
        internal static int proxyPort { get; set; }
        internal static bool isDefaultCredentials { get; set; }
        internal static string userName { get; set; }
        internal static string password { get; set; }
        internal static string pfxPath { get; set; }
        internal static string pfxPassword { get; set; }
        internal static string eSignURLV2 { get; set; }
        internal static int SignatureContents { get; set; }

    }
}
