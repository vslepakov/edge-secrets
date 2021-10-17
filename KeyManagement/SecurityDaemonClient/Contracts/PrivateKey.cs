// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    public class PrivateKey
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PrivateKeyType? Type { get; set; }

        public string Ref { get; set; }

        public string Bytes { get; set; }
    }
}
