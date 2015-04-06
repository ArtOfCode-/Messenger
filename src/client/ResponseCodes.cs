using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessengerClient
{
    /// <summary>
    ///     Details the possible response codes.
    /// </summary>
    public enum ResponseCode
    {
        Success,
        ServerError,
        NoDateFound,
        BadDateFormat,
        NoMessageFound,
        NoHandlingProtocol,
        NoCode,
        NoResponse
    }

    /// <summary>
    ///     Deals with the handling of response codes
    /// </summary>
    class ResponseCodes
    {
        /// <summary>
        ///     Maps response code strings to their respective values.
        /// </summary>
        public static Dictionary<string, ResponseCode> CodeStrings = new Dictionary<string, ResponseCode>
        {
            {"[600]", ResponseCode.Success},
            {"[100]", ResponseCode.ServerError},
            {"[200]", ResponseCode.NoDateFound},
            {"[201]", ResponseCode.BadDateFormat},
            {"[300]", ResponseCode.NoMessageFound},
            {"[400]", ResponseCode.NoHandlingProtocol},
        };

        /// <summary>
        ///     Gives the response code equivalent for a string
        /// </summary>
        /// <param name="code">The code in string format</param>
        /// <returns>A ResponseCode representation of the string</returns>
        public static ResponseCode GetResponse(string code)
        {
            if (CodeStrings.ContainsKey(code))
            {
                return CodeStrings[code];
            }
            else
            {
                return ResponseCode.NoCode;
            }
        }
    }
}
