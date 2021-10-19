﻿// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;

namespace EdgeSecrets.CryptoProvider.SecurityDaemon.Contracts
{
    public class ServerCertificateRequest
    {
        public string CommonName { get; set; }

        public DateTime Expiration { get; set; }
    }
}
