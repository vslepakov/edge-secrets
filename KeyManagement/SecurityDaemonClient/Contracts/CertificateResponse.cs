// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    public class CertificateResponse
    {
        public PrivateKey PrivateKey { get; set; }

        public string Certificate { get; set; }

        public DateTime? Expiration { get; set; }
    }
}
